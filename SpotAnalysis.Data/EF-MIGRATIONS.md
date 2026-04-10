# EF Core Migrations

Das Data-Projekt kennt keinen Connection String. Stattdessen wird über `--startup-project` bzw.
`-StartupProject` die DI-Konfiguration aus `SpotAnalysis.Web/Program.cs` genutzt, die den Connection
String aus `SpotAnalysis.Web/appsettings.json` liest.

## CLI (`dotnet ef`)

Aus dem Solution-Verzeichnis (`SpotAnalysis/`) ausführen.

### Migration erstellen

```bash
dotnet ef migrations add <MigrationName> --project SpotAnalysis.Data --startup-project SpotAnalysis.Web
```

### Datenbank aktualisieren

```bash
dotnet ef database update --project SpotAnalysis.Data --startup-project SpotAnalysis.Web
```

## Package Manager Console (Visual Studio)

### Migration erstellen

```powershell
Add-Migration <MigrationName> -Project SpotAnalysis.Data -StartupProject SpotAnalysis.Web
```

### Datenbank aktualisieren

```powershell
Update-Database -Project SpotAnalysis.Data -StartupProject SpotAnalysis.Web
```
