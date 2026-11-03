# Operations — Troubleshooting

## Common Issues

### 1. "No browser found" / WebDriver Initialisation Failed

**Symptoms:**
- Exception in `WebDriverInitialiser.InitialiseAvailableWebDriver()`
- Application crashes before login

**Causes:**
- No supported browser installed (Chrome, Chromium, Firefox)
- Browser installed but not in PATH
- Selenium driver version mismatch

**Fixes:**
```bash
# Ubuntu/Debian
apt-get update && apt-get install -y chromium

# Check version
chromium --version

# Or install Chrome
wget -q -O - https://dl.google.com/linux/linux_signing_key.pub | apt-key add -
echo "deb [arch=amd64] http://dl.google.com/linux/chrome/deb/ stable main" > /etc/apt/sources.list.d/google-chrome.list
apt-get update && apt-get install -y google-chrome-stable
```

### 2. "Permission denied" on Executable

**Symptoms:**
- `bash: ./SteamProfileManager: Permission denied`

**Fix:**
```bash
chmod +x SteamProfileManager
```

### 3. "Config not found" / Settings Not Loaded

**Symptoms:**
- Default values used (PageLoadTimeout=90, flags=false)
- SteamAccount empty → login fails

**Causes:**
- Running from wrong directory
- `appsettings.json` not in working directory
- JSON syntax error

**Fixes:**
```bash
# Run from publish directory
cd /opt/steam-profile-manager
./SteamProfileManager

# Verify config
cat appsettings.json | jq empty  # Validates JSON
cat appsettings.json | jq '.botSettings.steamAccount.username'
```

### 4. "TOTP invalid" / 2FA Failure

**Symptoms:**
- Login starts but fails at 2FA step
- "Invalid code" error from Steam

**Causes:**
- System clock skew (TOTP time-based)
- Wrong shared secret in config
- Shared secret format (must be Base32)

**Fixes:**
```bash
# Sync time
timedatectl set-ntp true
# Or
ntpdate -s time.nist.gov

# Verify shared secret format (Base32, no spaces)
echo "ABCDEF123456GHIJKL7890MNOPQRST" | base32 -d  # Should decode without error
```

### 5. "Element not found" / NoSuchElementException

**Symptoms:**
- Crash during login or profile operations
- `OpenQA.Selenium.NoSuchElementException` in logs
- Crash screenshot shows Steam page

**Causes:**
- Steam UI changed (selectors outdated)
- Page not fully loaded
- Running in headless mode (different DOM)

**Fixes:**
```bash
# 1. Update NuciWeb.Steam package
dotnet add SteamProfileManager/SteamProfileManager.csproj package NuciWeb.Steam --version <latest>

# 2. Rebuild
dotnet build SteamProfileManager.slnx -c Release

# 3. Test in debug mode (visible browser)
# Set in appsettings.json:
# "debugSettings": { "isDebugMode": true }
```

### 6. "Page load timeout" / TimeoutException

**Symptoms:**
- `OpenQA.Selenium.WebDriverTimeoutException`
- PageLoadTimeout exceeded

**Causes:**
- Slow network
- Steam rate limiting
- Heavy page (many scripts)

**Fixes:**
```json
// Increase timeout in appsettings.json
{
  "botSettings": {
    "pageLoadTimeout": 180
  }
}
```

### 7. Profile Identifier "Already Taken" / 30-Day Cooldown

**Symptoms:**
- `SetRandomProfileIdentifier` fails
- Steam shows "This custom URL is already in use" or "You can only change this once every 30 days"

**Causes:**
- Random collision (unlikely with 8 chars)
- Previous change within 30 days

**Fixes:**
```bash
# Wait 30 days, or
# Manually set unique identifier on Steam website
# Or disable identifier randomisation:
# "randomiseProfileIdentifier": false
```

### 8. Profile Picture Upload Fails

**Symptoms:**
- `SetRandomProfilePicture` fails
- Download or upload exception

**Causes:**
- Image URL inaccessible (404, auth required)
- Image too large (>5MB)
- Invalid format
- Crop dialog handling failed

**Fixes:**
```bash
# Test URL manually
curl -I "https://example.com/image.png"
# Check Content-Type: image/*
# Check Content-Length < 5MB

# Use direct image URLs only (not HTML pages)
# Verify in pictures.txt
```

### 9. Crash Screenshot Not Created

**Symptoms:**
- Application crashes
- No `crash.png` file

**Causes:**
- `crashScreenshotFileName` empty in config
- WebDriver not initialised (crash before init)
- WebDriver doesn't implement `ITakesScreenshot`
- No write permission

**Fixes:**
```json
// Enable in appsettings.json
{
  "debugSettings": {
    "crashScreenshotFileName": "crash.png",
    "isDebugMode": false
  }
}
```

```bash
# Check permissions
ls -la crash.png 2>/dev/null || echo "Not created"
```

---

## Debugging Techniques

### Enable Debug Logging

```json
{
  "nuciLoggerSettings": {
    "minimumLevel": "Debug",
    "logFilePath": "logfile.log",
    "isFileOutputEnabled": true
  }
}
```

### Enable Visible Browser

```json
{
  "debugSettings": {
    "isDebugMode": true
  }
}
```

**Note:** Requires display. For headless servers:
```bash
# Xvfb (virtual framebuffer)
Xvfb :99 -screen 0 1920x1080x24 &
export DISPLAY=:99
./SteamProfileManager
```

### Inspect Crash Screenshot

```bash
# View on server with X11 forwarding
ssh -X user@server
eog crash.png

# Or copy locally
scp user@server:/path/crash.png .
```

### Manual Steam Login Test

```bash
# Test credentials manually
# 1. Open browser
# 2. Go to steamcommunity.com/login
# 3. Enter credentials
# 4. Verify 2FA works
```

### Network Connectivity Test

```bash
# Test Steam endpoints
curl -I https://steamcommunity.com
curl -I https://steampowered.com
curl -I https://api.steampowered.com

# Test image URLs
curl -I "https://i.imgur.com/abc123.png"
```

---

## Log Analysis

### Find Errors

```bash
grep "\[Error\]" logfile.log
grep "Exception" logfile.log
```

### Trace Operation Flow

```bash
grep "Operation=LogIn" logfile.log
grep "Operation=SetProfileName" logfile.log
```

### Check Timing

```bash
# First and last log entries
head -1 logfile.log
tail -1 logfile.log
```

---

## Escalation

### When to Update NuciWeb.Steam

- `NoSuchElementException` on Steam pages
- Steam UI visually changed
- Package has updates on NuGet

### When to Report Issue

- Persistent crashes after troubleshooting
- New Steam security measures (captcha, device auth)
- Suspected bug in NuciWeb.Steam / NuciWeb.Automation.Selenium

### Information to Collect

1. `logfile.log` (full)
2. `crash.png` (if exists)
3. `appsettings.json` (redact secrets)
4. .NET version: `dotnet --version`
5. Browser version: `chromium --version`
6. OS: `cat /etc/os-release`
7. NuGet packages: `dotnet list package`