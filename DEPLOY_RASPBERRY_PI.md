# Raspberry Pi Deployment — SpotAnalysis behind nginx

Target setup:
- SpotAnalysis runs in Docker on the Pi, bound to `127.0.0.1:8080` (HTTP only, not exposed externally).
- nginx on the Pi host terminates TLS (self-signed via mkcert) and reverse-proxies to the container.
- Reachable on the LAN at `https://spotanalysis-pi.local`.

## 1. Prerequisites on the Pi

- Raspberry Pi OS (64-bit recommended)
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
- mkcert installed (arm64 binary):
  ```bash
  sudo apt install -y libnss3-tools
  curl -JLO "https://dl.filippo.io/mkcert/latest?for=linux/arm64"
  chmod +x mkcert-v*-linux-arm64
  sudo mv mkcert-v*-linux-arm64 /usr/local/bin/mkcert
  ```

## 2. Application code

The repo already contains the proxy-aware changes:
- `SpotAnalysis.Web/Program.cs` uses `UseForwardedHeaders` and no longer calls `UseHsts()` / `UseHttpsRedirection()` — nginx owns TLS + HSTS.
- `compose.yaml` binds the web port to `127.0.0.1:8080:8080` only.

Just pull the latest code onto the Pi — no further edits needed.

## 3. Generate the self-signed cert

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

## 4. nginx configuration

### 4a. Add the WebSocket upgrade map

Edit `/etc/nginx/nginx.conf` and, inside the existing `http { … }` block, add:

```nginx
map $http_upgrade $connection_upgrade {
    default upgrade;
    ''      close;
}
```

Required for Blazor Server / SignalR over WebSockets.

### 4b. Site config

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

### 4c. Enable and reload

```bash
sudo ln -s /etc/nginx/sites-available/spotanalysis /etc/nginx/sites-enabled/
sudo rm -f /etc/nginx/sites-enabled/default
sudo nginx -t
sudo systemctl reload nginx
```

## 5. Start the app

From the repo root on the Pi:

```bash
docker compose up -d --build
```

Verify locally on the Pi:
```bash
curl -I http://127.0.0.1:8080          # app direct (200)
curl -kI https://spotanalysis-pi.local # through nginx (200)
```

## 6. Verify mDNS from a client

From another machine on the LAN:
```bash
ping spotanalysis-pi.local
```

If it doesn't resolve on Windows, install **Bonjour Print Services** (Apple) or enable the mDNS Windows feature. macOS, iOS, Android, and most Linux desktops resolve `.local` out of the box.

## 7. Trust the mkcert CA on each client device

Copy `rootCA.pem` from the path printed by `mkcert -CAROOT` on the Pi to each client, then import:

- **Windows**: double-click the file → *Install Certificate* → *Local Machine* → *Place all certificates in the following store* → *Trusted Root Certification Authorities*.
- **macOS**: open in Keychain Access → *System* keychain → double-click the imported cert → *Trust* → *Always Trust*.
- **Linux (Chrome/Firefox)**: import via browser settings → Certificates → Authorities.
- **Android**: Settings → Security → Encryption & credentials → Install a certificate → CA certificate.
- **iOS**: AirDrop/email the file → Settings → General → VPN & Device Management → install the profile → then Settings → General → About → Certificate Trust Settings → enable full trust for the mkcert CA.

After this, `https://spotanalysis-pi.local` opens without browser warnings.

## Troubleshooting

- **Redirect loop / "too many redirects"** → forwarded headers aren't reaching the app. Confirm nginx sends `X-Forwarded-Proto $scheme` and that `UseForwardedHeaders` is before auth/authorization in `Program.cs`.
- **UI loads but interactive components freeze** → WebSocket upgrade missing. Recheck the `map $http_upgrade` block and the two `proxy_set_header Upgrade/Connection` lines.
- **502 Bad Gateway** → container not up, or not bound to `127.0.0.1:8080`. Check `docker compose ps` and `ss -tlnp | grep 8080`.
- **`.local` resolves on the Pi but not from clients** → `avahi-daemon` may not be advertising on the LAN interface. Check `systemctl status avahi-daemon` and firewall rules (UDP 5353).
- **Browser still warns about the cert** → the client hasn't imported `rootCA.pem`, or imported it into the wrong store (must be *Trusted Root* / *System*, not *Personal*).
