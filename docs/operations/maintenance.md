# Operations — Maintenance

## Routine Maintenance

### Log Rotation

**Automated via logrotate:**
```conf
/opt/steam-profile-manager/logfile.log {
    daily
    rotate 30
    compress
    delaycompress
    missingok
    notifempty
    create 644 steambot steambot
}
```

**Manual:**
```bash
# Rotate now
logrotate -f /etc/logrotate.d/steam-profile-manager

# Check status
logrotate -d /etc/logrotate.d/steam-profile-manager
```

### Crash Screenshot Cleanup

```bash
# Remove old crash screenshots (>7 days)
find /opt/steam-profile-manager -name "crash.png" -mtime +7 -delete

# Or keep only latest
cd /opt/steam-profile-manager
ls -t crash.png 2>/dev/null | tail -n +2 | xargs rm -f
```

### Profile Picture Cleanup

```bash
# Remove old profile pictures (>1 day)
find /opt/steam-profile-manager -name "profilePicture.png" -mtime +1 -delete
```

### Backup Verification

**Monthly:** Run backup verification script (see backup-and-restore.md)

---

## Dependency Updates

### NuGet Package Updates

```bash
# Check outdated packages
dotnet list SteamProfileManager/SteamProfileManager.csproj package --outdated

# Update specific package
dotnet add SteamProfileManager/SteamProfileManager.csproj package NuciWeb.Steam --version <latest>

# Update all (interactive)
dotnet add SteamProfileManager/SteamProfileManager.csproj package --interactive
```

### Critical Packages to Monitor

| Package | Reason |
|---------|--------|
| `NuciWeb.Steam` | Steam UI changes break selectors |
| `NuciWeb.Automation.Selenium` | Browser compatibility |
| `Selenium.WebDriver` | Browser version support |
| `NuciLog` | Logging fixes |

### .NET Runtime Updates

```bash
# Check installed runtimes
dotnet --list-runtimes

# Install latest 10.0.x
# Via package manager or download from dotnet.microsoft.com
```

---

## Browser Updates

### Chromium/Chrome

```bash
# Ubuntu/Debian
apt-get update && apt-get upgrade -y chromium

# Check version
chromium --version
```

### Firefox

```bash
# Ubuntu/Debian
apt-get update && apt-get upgrade -y firefox

# Check version
firefox --version
```

### WebDriver Compatibility

After browser update:
1. Run test execution
2. If `SessionNotCreatedException` → Update `NuciWeb.Automation.Selenium` or Selenium.WebDriver
3. Rebuild and redeploy

---

## Credential Rotation

### Schedule

**Recommended:** Every 90 days

### Procedure

1. **Generate new credentials on Steam:**
   - Change password
   - Regenerate TOTP shared secret (remove/add authenticator)
   - Regenerate identity secret (revoke/create API key)

2. **Update configuration:**
   ```bash
   # Stop service
   systemctl stop steam-profile-manager

   # Update appsettings.json with new credentials
   # Or update environment variables / secret store

   # Start service
   systemctl start steam-profile-manager
   ```

3. **Verify:**
   - Check logs for successful login
   - Verify profile changes apply

---

## Configuration Drift Detection

### Compare Running Config to Backup

```bash
# Diff current vs latest backup
LATEST=$(ls -t /opt/steam-profile-manager/backups/*/appsettings.json | head -1)
diff -u "$LATEST" /opt/steam-profile-manager/appsettings.json
```

### Automated Check (Cron)

```bash
# Daily config drift check
0 4 * * * /opt/steam-profile-manager/check-config-drift.sh
```

**check-config-drift.sh:**
```bash
#!/bin/bash
CURRENT="/opt/steam-profile-manager/appsettings.json"
LATEST=$(ls -t /opt/steam-profile-manager/backups/*/appsettings.json 2>/dev/null | head -1)

if [ -n "$LATEST" ]; then
    if ! diff -q "$CURRENT" "$LATEST" >/dev/null; then
        echo "CONFIG DRIFT DETECTED"
        diff -u "$LATEST" "$CURRENT" | mail -s "SPM Config Drift" admin@example.com
    fi
fi
```

---

## Health Check Script

**File:** `/opt/steam-profile-manager/health-check.sh`

```bash
#!/bin/bash
set -e

cd /opt/steam-profile-manager

# 1. Check executable exists
test -f SteamProfileManager || { echo "FAIL: Executable missing"; exit 1; }

# 2. Check config exists and valid
test -f appsettings.json || { echo "FAIL: Config missing"; exit 1; }
cat appsettings.json | jq empty || { echo "FAIL: Invalid JSON"; exit 1; }

# 3. Check required fields
cat appsettings.json | jq -e '.botSettings.steamAccount.username' >/dev/null || { echo "FAIL: No username"; exit 1; }
cat appsettings.json | jq -e '.botSettings.steamAccount.password' >/dev/null || { echo "FAIL: No password"; exit 1; }
cat appsettings.json | jq -e '.botSettings.steamAccount.sharedSecret' >/dev/null || { echo "FAIL: No TOTP secret"; exit 1; }

# 4. Check browser
command -v chromium >/dev/null || { echo "FAIL: No chromium"; exit 1; }

# 5. Check disk space (>100MB free)
AVAIL=$(df --output=avail . | tail -1)
[ "$AVAIL" -gt 102400 ] || { echo "FAIL: Low disk space"; exit 1; }

# 6. Test run (dry-run not supported, so check last run)
if [ -f logfile.log ]; then
    LAST_RUN=$(grep "Operation=StartUp" logfile.log | tail -1 | cut -d' ' -f1-2)
    echo "Last run: $LAST_RUN"
fi

echo "HEALTH OK"
exit 0
```

**Schedule:**
```bash
# Every 6 hours
0 */6 * * * /opt/steam-profile-manager/health-check.sh >> /var/log/spm-health.log 2>&1
```

---

## Performance Tuning

### Page Load Timeout

**Current:** 90 seconds (configurable)

**Adjust based on network:**
```json
{
  "botSettings": {
    "pageLoadTimeout": 180
  }
}
```

### Headless vs Debug

- **Production:** Headless (faster, less resources)
- **Debugging:** Visible (slower, but observable)

### Resource Limits (systemd)

```ini
[Service]
MemoryMax=512M
CPUQuota=50%
```

---

## Version Upgrade Procedure

### Minor Version (NuGet packages)

1. Update packages
2. Run tests: `dotnet test`
3. Build: `dotnet build -c Release`
4. Deploy to staging
5. Run integration test
6. Deploy to production

### Major Version (.NET Runtime)

1. Install new .NET runtime
2. Update `TargetFramework` in `.csproj` if needed
3. Update `global.json` if used
4. Full test suite
5. Deploy with rollback plan

---

## Maintenance Checklist

### Daily
- [ ] Health check passes
- [ ] No crash screenshots
- [ ] Logs show successful runs

### Weekly
- [ ] Log rotation working
- [ ] Disk space adequate
- [ ] Backup verification passes

### Monthly
- [ ] Dependency updates checked
- [ ] Browser updated
- [ ] Credential rotation (quarterly)
- [ ] Config drift check
- [ ] Backup restore test

### Quarterly
- [ ] Full disaster recovery test
- [ ] Security audit (permissions, secrets)
- [ ] Performance review
- [ ] Documentation update