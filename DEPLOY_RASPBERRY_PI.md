# Raspberry Pi Deployment — SpotAnalysis behind nginx

Target setup:
- The Pi creates its own WiFi hotspot — it does not join an existing network.
- SpotAnalysis runs in Docker on the Pi, bound to `127.0.0.1:8080` (HTTP only, not exposed externally).
- nginx on the Pi host terminates TLS (self-signed via mkcert) and reverse-proxies to the container.
- A systemd service starts Docker Compose on boot automatically.
- Reachable from hotspot clients at `https://spotanalysis-pi.local` (or `https://10.42.0.1`).

## 1. Prerequisites on the Pi

- Raspberry Pi OS Bookworm or later (64-bit recommended) — ships with NetworkManager.
- Hostname set to `spotanalysis-pi` (so mDNS resolves as `spotanalysis-pi.local`):
  ```bash
  sudo hostnamectl set-hostname spotanalysis-pi
  ```
- `avahi-daemon` running (default on Raspberry Pi OS):
  ```bash
  sudo apt install -y avahi-daemon
  sudo systemctl enable --now avahi-daemon
  ```
- Docker + compose plugin installed.
- nginx installed:
  ```bash
  sudo apt install -y nginx
  ```
- mkcert installed (arm64 binary) — **do this while the Pi still has internet access**:
  ```bash
  sudo apt install -y libnss3-tools
  curl -JLO "https://dl.filippo.io/mkcert/latest?for=linux/arm64"
  chmod +x mkcert-v*-linux-arm64
  sudo mv mkcert-v*-linux-arm64 /usr/local/bin/mkcert
  ```

## 2. WiFi Hotspot

The Pi uses NetworkManager to create a WiFi access point on `wlan0`. Once created, the connection profile persists and auto-starts on every boot — no extra systemd service needed.

### 2a. Create the hotspot

```bash
sudo nmcli connection add \
  type wifi \
  ifname wlan0 \
  con-name hotspot \
  autoconnect yes \
  ssid SpotAnalysis

sudo nmcli connection modify hotspot \
  802-11-wireless.mode ap \
  802-11-wireless.band bg \
  802-11-wireless.channel 6 \
  ipv4.method shared \
  ipv4.addresses 10.42.0.1/24 \
  wifi-sec.key-mgmt wpa-psk \
  wifi-sec.psk "CHANGE_THIS_PASSWORD"
```

Change the SSID and password to whatever you like. Channel 6 is a safe default for 2.4 GHz; adjust if there's interference.

### 2b. DNS resolution for hotspot clients

NetworkManager's `shared` mode runs a built-in dnsmasq for DHCP. To make `spotanalysis-pi.local` resolve to the Pi's IP for all clients (even those without mDNS support), drop the config file from `deploy/`:

```bash
sudo mkdir -p /etc/NetworkManager/dnsmasq-shared.d
sudo cp deploy/dnsmasq-spotanalysis.conf /etc/NetworkManager/dnsmasq-shared.d/
```

### 2c. Activate and disable old WiFi connections

```bash
sudo nmcli connection up hotspot
sudo nmcli connection down "preconfigured"  # or whatever the existing WiFi connection is named
```

After reboot, NetworkManager will bring up the `hotspot` connection automatically.

### 2d. Verify

From the Pi:
```bash
nmcli connection show hotspot | grep -E 'GENERAL.STATE|ipv4.addresses'
# should show activated and 10.42.0.1/24
```

From a phone or laptop, look for the `SpotAnalysis` WiFi network. After connecting, `ping 10.42.0.1` should work.

## 3. Application code

The repo already contains the proxy-aware changes:
- `SpotAnalysis.Web/Program.cs` uses `UseForwardedHeaders` and no longer calls `UseHsts()` / `UseHttpsRedirection()` — nginx owns TLS + HSTS.
- `compose.yaml` binds the web port to `127.0.0.1:8080:8080` only.

Clone the repo to `/opt/spotanalysis` on the Pi:

```bash
sudo mkdir -p /opt/spotanalysis
sudo chown $USER:$USER /opt/spotanalysis
git clone <repo-url> /opt/spotanalysis
cp /path/to/.env /opt/spotanalysis/.env
```

## 4. Generate the self-signed cert

```bash
mkcert -install
sudo mkdir -p /etc/nginx/certs
cd /etc/nginx/certs
sudo mkcert spotanalysis-pi.local
```

This produces:
- `/etc/nginx/certs/spotanalysis-pi.local.pem`
- `/etc/nginx/certs/spotanalysis-pi.local-key.pem`

Note where the local CA lives (needed for client trust later):
```bash
mkcert -CAROOT
# → e.g. /home/pi/.local/share/mkcert
```

## 5. nginx configuration

### 5a. Add the WebSocket upgrade map

Edit `/etc/nginx/nginx.conf` and, inside the existing `http { … }` block, add:

```nginx
map $http_upgrade $connection_upgrade {
    default upgrade;
    ''      close;
}
```

Required for Blazor Server / SignalR over WebSockets.

### 5b. Site config

Create `/etc/nginx/sites-available/spotanalysis`:

```nginx
server {
    listen 80;
    server_name spotanalysis-pi.local;
    return 301 https://$host$request_uri;
}

server {
    listen 443 ssl http2;
    server_name spotanalysis-pi.local;

    ssl_certificate     /etc/nginx/certs/spotanalysis-pi.local.pem;
    ssl_certificate_key /etc/nginx/certs/spotanalysis-pi.local-key.pem;
    ssl_protocols TLSv1.2 TLSv1.3;

    add_header Strict-Transport-Security "max-age=31536000; includeSubDomains" always;
    add_header X-Content-Type-Options nosniff always;
    add_header X-Frame-Options DENY always;

    client_max_body_size 50m;

    location / {
        proxy_pass http://127.0.0.1:8080;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection $connection_upgrade;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_read_timeout 100s;
    }
}
```

### 5c. Enable and reload

```bash
sudo ln -s /etc/nginx/sites-available/spotanalysis /etc/nginx/sites-enabled/
sudo rm -f /etc/nginx/sites-enabled/default
sudo nginx -t
sudo systemctl reload nginx
```

## 6. Auto-start on boot (systemd)

Install the systemd service from `deploy/`:

```bash
sudo cp deploy/spotanalysis.service /etc/systemd/system/
sudo systemctl daemon-reload
sudo systemctl enable spotanalysis.service
```

This will run `docker compose up -d` on every boot after Docker and the network are ready, and `docker compose down` on shutdown.

### First build

The first start needs to build the images:

```bash
cd /opt/spotanalysis
docker compose up -d --build
```

After this, the systemd service handles subsequent boots. To verify it works:

```bash
sudo reboot
# after reboot:
systemctl status spotanalysis.service
docker compose -f /opt/spotanalysis/compose.yaml ps
```

## 7. Verify end-to-end

From the Pi:
```bash
curl -I http://127.0.0.1:8080          # app direct (200)
curl -kI https://spotanalysis-pi.local # through nginx (200)
```

From a client connected to the `SpotAnalysis` WiFi:
```bash
ping spotanalysis-pi.local             # should resolve to 10.42.0.1
curl -kI https://spotanalysis-pi.local # through nginx (200)
```

## 8. Trust the mkcert CA on each client device

Copy `rootCA.pem` from the path printed by `mkcert -CAROOT` on the Pi to each client, then import:

- **Windows**: double-click the file → *Install Certificate* → *Local Machine* → *Place all certificates in the following store* → *Trusted Root Certification Authorities*.
- **macOS**: open in Keychain Access → *System* keychain → double-click the imported cert → *Trust* → *Always Trust*.
- **Linux (Chrome/Firefox)**: import via browser settings → Certificates → Authorities.
- **Android**: Settings → Security → Encryption & credentials → Install a certificate → CA certificate.
- **iOS**: AirDrop/email the file → Settings → General → VPN & Device Management → install the profile → then Settings → General → About → Certificate Trust Settings → enable full trust for the mkcert CA.

After this, `https://spotanalysis-pi.local` opens without browser warnings.

## Boot sequence summary

On every Pi power-on, this happens automatically:
1. NetworkManager brings up the `hotspot` WiFi AP on `wlan0` (with DHCP + DNS for clients).
2. Docker daemon starts.
3. `spotanalysis.service` runs `docker compose up -d` (PostgreSQL + web app + Adminer).
4. nginx is already enabled and proxies HTTPS to the container.

No manual intervention needed — just power on the Pi and connect to the WiFi.

## Troubleshooting

- **Redirect loop / "too many redirects"** → forwarded headers aren't reaching the app. Confirm nginx sends `X-Forwarded-Proto $scheme` and that `UseForwardedHeaders` is before auth/authorization in `Program.cs`.
- **UI loads but interactive components freeze** → WebSocket upgrade missing. Recheck the `map $http_upgrade` block and the two `proxy_set_header Upgrade/Connection` lines.
- **502 Bad Gateway** → container not up, or not bound to `127.0.0.1:8080`. Check `docker compose ps` and `ss -tlnp | grep 8080`.
- **Hotspot not visible** → check `nmcli connection show hotspot` and `journalctl -u NetworkManager`. Make sure no other connection is competing for `wlan0`.
- **Clients can't resolve `spotanalysis-pi.local`** → verify the dnsmasq config is in place: `cat /etc/NetworkManager/dnsmasq-shared.d/dnsmasq-spotanalysis.conf`. Alternatively, clients can use `https://10.42.0.1` directly.
- **`spotanalysis.service` fails on boot** → check `journalctl -u spotanalysis.service`. Common cause: Docker not ready yet (the `After=docker.service` dependency should prevent this, but check `systemctl status docker`).
- **Browser still warns about the cert** → the client hasn't imported `rootCA.pem`, or imported it into the wrong store (must be *Trusted Root* / *System*, not *Personal*).
