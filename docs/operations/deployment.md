# Operations — Deployment

## Prerequisites

### Runtime
- .NET 10.0 Runtime (or SDK for building)
- Supported OS: Linux, Windows, macOS

### Browser
- Chrome, Chromium, or Firefox installed
- Compatible WebDriver managed by `NuciWeb.Automation.Selenium`

### Steam Account
- Valid Steam credentials
- Steam Guard enabled with TOTP (shared secret)
- Identity secret for trade confirmations

### Network
- Internet access to `steamcommunity.com`, `steampowered.com`
- Access to image URLs (imgur.com, etc.) for profile pictures

---

## Build

### From Source

```bash
# Restore dependencies
dotnet restore SteamProfileManager.slnx

# Build Release
dotnet build SteamProfileManager.slnx -c Release

# Output: SteamProfileManager/bin/Release/net10.0/
```

### Publish (Self-Contained)

```bash
# Linux x64
dotnet publish SteamProfileManager/SteamProfileManager.csproj -c Release -r linux-x64 --self-contained true -o ./publish/linux-x64

# Windows x64
dotnet publish SteamProfileManager/SteamProfileManager.csproj -c Release -r win-x64 --self-contained true -o ./publish/win-x64

# macOS x64
dotnet publish SteamProfileManager/SteamProfileManager.csproj -c Release -r osx-x64 --self-contained true -o ./publish/osx-x64
```

**Output:** Single executable + dependencies in publish folder

---

## Configuration Deployment

### appsettings.json

Deploy `appsettings.json` alongside executable:

```
publish/
├── SteamProfileManager (or .exe)
├── appsettings.json
├── names.txt (optional)
├── pictures.txt (optional)
└── ... dependencies
```

**Required sections:**
- `botSettings.steamAccount` — credentials
- `botSettings.pageLoadTimeout` — timeout
- `nuciLoggerSettings` — logging config

**Optional sections:**
- `botSettings.randomiseProfileName` + `profileNamesList`
- `botSettings.randomiseProfileIdentifier`
- `botSettings.randomiseProfilePicture` + `profilePicturesList`
- `debugSettings.crashScreenshotFileName`
- `debugSettings.isDebugMode`

### File Lists

If using file-based names/pictures:

```
publish/
├── names.txt          # One name per line
├── pictures.txt       # One URL per line
└── appsettings.json   # References: "profileNamesList": "names.txt"
```

Paths in `appsettings.json` are relative to working directory.

---

## Execution

### Direct Run

```bash
cd publish/linux-x64
./SteamProfileManager
```

### With Custom Config Path

Not currently supported — always reads `appsettings.json` from current directory.

### Environment Variables

Not currently supported for configuration override.

---

## Container Deployment

### Dockerfile Example

```dockerfile
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
WORKDIR /app

# Install browser (Chromium)
RUN apt-get update && apt-get install -y \
    chromium \
    && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["SteamProfileManager/SteamProfileManager.csproj", "SteamProfileManager/"]
RUN dotnet restore "SteamProfileManager/SteamProfileManager.csproj"
COPY . .
WORKDIR "/src/SteamProfileManager"
RUN dotnet publish -c Release -r linux-x64 --self-contained false -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
COPY appsettings.json .
COPY names.txt .
COPY pictures.txt .
ENTRYPOINT ["./SteamProfileManager"]
```

### Build Image

```bash
docker build -t steam-profile-manager .
```

### Run Container

```bash
docker run -d \
  --name steam-profile-manager \
  -v /host/config:/app \
  steam-profile-manager
```

**Volume Mount:** Mount config directory to `/app` for `appsettings.json`, `names.txt`, `pictures.txt`, and log output.

---

## CI/CD Pipeline

### GitHub Actions (Existing)

**File:** `.github/workflows/dotnet.yml`

```yaml
name: .NET

on:
  push:
    branches: [ master ]
  pull_request:
    branches: [ master ]

jobs:
  build:
    name: Build
    runs-on: ubuntu-latest

    steps:
    - uses: actions/checkout@v7

    - name: Setup .NET
      uses: actions/setup-dotnet@v5
      with:
        dotnet-version: 10.0.x

    - name: Restore dependencies
      run: dotnet restore

    - name: Build
      run: dotnet build --no-restore

    - name: Test
      run: dotnet test --no-build --verbosity normal
```

**File:** `.github/workflows/security.yml`

```yaml
name: Security

on:
  push:
    branches: [ master ]
  pull_request:
    branches: [ master ]
  schedule:
    - cron: '0 0 * * 0'

jobs:
  security-scan:
    name: Security Scan
    runs-on: ubuntu-latest
    permissions:
      security-events: write
      contents: read

    steps:
    - uses: actions/checkout@v7

    - name: Setup .NET
      uses: actions/setup-dotnet@v5
      with:
        dotnet-version: 10.0.x

    - name: Restore dependencies
      run: dotnet restore

    - name: Check for vulnerable packages
      run: dotnet list package --vulnerable --include-transitive

    - name: Run CodeQL Analysis
      uses: github/codeql-action/init@v4
      with:
        languages: csharp

    - name: Build
      run: dotnet build --no-restore

    - name: Perform CodeQL Analysis
      uses: github/codeql-action/analyze@v4
```

**File:** `.github/workflows/github-release.yml`

Triggered when a release is published. It derives the .NET version from `SteamProfileManager/SteamProfileManager.csproj`, strips the leading `v` from the release tag, runs the shared release script for every supported runtime identifier, and attaches the resulting `SteamProfileManager/bin/Release/steam-profile-manager_*` artefacts to the release.

```yaml
name: GitHub Release

on:
  release:
    types: [ published ]

permissions:
  contents: write
```

### Extending for Deployment

Add deploy job:

```yaml
  deploy:
    needs: test
    if: github.ref == 'refs/heads/main'
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      - name: Publish
        run: dotnet publish SteamProfileManager/SteamProfileManager.csproj -c Release -r linux-x64 --self-contained true -o ./publish
      - name: Upload Artifact
        uses: actions/upload-artifact@v4
        with:
          name: steam-profile-manager-linux
          path: ./publish/
```

---

## Secrets Management

### GitHub Actions Secrets

Store in repository settings → Secrets → Actions:

| Secret Name | Value |
|-------------|-------|
| `STEAM_USERNAME` | Steam username |
| `STEAM_PASSWORD` | Steam password |
| `STEAM_SHARED_SECRET` | TOTP shared secret (Base32) |
| `STEAM_IDENTITY_SECRET` | Identity secret (Base32) |
| `STEAM_DEVICE_ID` | Device ID (optional) |

### Inject at Runtime

**Option 1: Environment Variables (requires code change)**
```yaml
env:
  STEAM_USERNAME: ${{ secrets.STEAM_USERNAME }}
  # ... etc
```

**Option 2: Generate appsettings.json at Deploy**
```yaml
- name: Generate Config
  run: |
    cat > ./publish/appsettings.json <<EOF
    {
      "botSettings": {
        "pageLoadTimeout": 90,
        "steamAccount": {
          "username": "${{ secrets.STEAM_USERNAME }}",
          "password": "${{ secrets.STEAM_PASSWORD }}",
          "sharedSecret": "${{ secrets.STEAM_SHARED_SECRET }}",
          "identitySecret": "${{ secrets.STEAM_IDENTITY_SECRET }}",
          "deviceId": "${{ secrets.STEAM_DEVICE_ID }}"
        },
        "randomiseProfileName": true,
        "randomiseProfileIdentifier": true,
        "randomiseProfilePicture": true
      },
      "debugSettings": {
        "crashScreenshotFileName": "crash.png",
        "isDebugMode": false
      },
      "nuciLoggerSettings": {
        "minimumLevel": "Info",
        "logFilePath": "logfile.log",
        "isFileOutputEnabled": true
      }
    }
    EOF
```

---

## Scheduling

### Cron (Linux)

```bash
# Run daily at 3 AM
0 3 * * * /opt/steam-profile-manager/SteamProfileManager >> /var/log/steam-profile-manager.log 2>&1
```

### systemd Timer

**Service:** `/etc/systemd/system/steam-profile-manager.service`
```ini
[Unit]
Description=Steam Profile Manager
After=network.target

[Service]
Type=oneshot
WorkingDirectory=/opt/steam-profile-manager
ExecStart=/opt/steam-profile-manager/SteamProfileManager
User=steambot
```

**Timer:** `/etc/systemd/system/steam-profile-manager.timer`
```ini
[Unit]
Description=Run Steam Profile Manager daily

[Timer]
OnCalendar=daily
Persistent=true

[Install]
WantedBy=timers.target
```

**Enable:**
```bash
systemctl daemon-reload
systemctl enable --now steam-profile-manager.timer
```

### GitHub Actions Schedule

```yaml
on:
  schedule:
    - cron: '0 3 * * *'  # Daily at 3 AM UTC
  workflow_dispatch:  # Manual trigger
```

---

## Health Checks

### Pre-Flight Validation (Not Implemented)

Recommended checks before deployment:
1. Browser installed and compatible
2. Network connectivity to Steam
3. Credentials valid (test login)
4. Config file present and valid
5. File lists accessible (if used)
6. Disk space for logs/screenshots

### Post-Deploy Verification

1. Check `logfile.log` for `Operation.StartUp` and `Operation.ShutDown`
2. Verify no `Error` entries
3. Confirm profile changes on Steam (manual)

---

## Rollback

### Binary Rollback

Keep previous publish folder:
```
/opt/steam-profile-manager/
├── current -> v1.2.3
├── v1.2.3/
├── v1.2.2/
└── v1.2.1/
```

Switch symlink:
```bash
ln -sfn v1.2.2 current
```

### Config Rollback

Version control `appsettings.json` (without secrets) or keep backups:
```bash
cp appsettings.json appsettings.json.bak
# ... deploy new ...
cp appsettings.json.bak appsettings.json  # rollback
```

---

## Troubleshooting Deployment

| Issue | Cause | Fix |
|-------|-------|-----|
| "No browser found" | Chromium not installed | Install chromium package |
| "Permission denied" | Executable not executable | `chmod +x SteamProfileManager` |
| "Config not found" | Wrong working directory | `cd` to publish dir or use absolute paths |
| "TOTP invalid" | Clock skew | Sync system time (NTP) |
| "Element not found" | Steam UI changed | Update NuciWeb.Steam package |