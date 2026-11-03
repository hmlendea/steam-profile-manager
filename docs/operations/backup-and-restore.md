# Operations — Backup and Restore

## Backup Scope

### Critical Files

| File | Description | Frequency |
|------|-------------|-----------|
| `appsettings.json` | Configuration (with secrets) | Before each change |
| `names.txt` | Custom profile names | Before each change |
| `pictures.txt` | Custom picture URLs | Before each change |
| `logfile.log` | Operational logs | Daily |
| `crash.png` | Crash screenshots | On occurrence |

### Non-Critical (Regeneratable)

| File | Description | Regeneration |
|------|-------------|--------------|
| `profilePicture.png` | Downloaded profile picture | Re-downloaded each run |
| Executable | Published binary | `dotnet publish` |
| Dependencies | NuGet packages | `dotnet restore` |

---

## Backup Strategy

### Configuration Backup

```bash
# Manual backup
cp appsettings.json appsettings.json.$(date +%Y%m%d_%H%M%S).bak
cp names.txt names.txt.$(date +%Y%m%d_%H%M%S).bak
cp pictures.txt pictures.txt.$(date +%Y%m%d_%H%M%S).bak

# Automated (cron)
0 2 * * * /opt/steam-profile-manager/backup-config.sh
```

**backup-config.sh:**
```bash
#!/bin/bash
BACKUP_DIR="/opt/steam-profile-manager/backups/$(date +%Y%m%d)"
mkdir -p "$BACKUP_DIR"
cd /opt/steam-profile-manager
cp appsettings.json "$BACKUP_DIR/"
cp names.txt "$BACKUP_DIR/" 2>/dev/null || true
cp pictures.txt "$BACKUP_DIR/" 2>/dev/null || true
# Keep 30 days
find /opt/steam-profile-manager/backups -type d -mtime +30 -exec rm -rf {} \;
```

### Log Backup

```bash
# logrotate handles rotation
# See monitoring.md for logrotate config
```

### Full System Backup

```bash
# Include in host-level backup
/opt/steam-profile-manager/
```

---

## Restore Procedures

### Configuration Restore

```bash
# Stop service
systemctl stop steam-profile-manager

# Restore latest backup
cd /opt/steam-profile-manager
LATEST=$(ls -t backups/*/appsettings.json | head -1)
cp "$LATEST" appsettings.json
cp "$(dirname "$LATEST")/names.txt" names.txt 2>/dev/null || true
cp "$(dirname "$LATEST")/pictures.txt" pictures.txt 2>/dev/null || true

# Verify
cat appsettings.json | jq '.botSettings.steamAccount.username'

# Start service
systemctl start steam-profile-manager
```

### Selective Restore

```bash
# Restore only names list
cp backups/20240115/names.txt names.txt

# Restore only pictures list
cp backups/20240115/pictures.txt pictures.txt
```

### Credential Rotation + Restore

```bash
# 1. Generate new credentials on Steam
# 2. Update backup with new credentials
# 3. Restore updated backup
# 4. Verify login works
```

---

## Disaster Recovery

### Scenario: Complete Host Loss

**Recovery Time Objective (RTO):** < 1 hour
**Recovery Point Objective (RPO):** 24 hours (daily config backup)

**Steps:**
1. Provision new host (Linux)
2. Install .NET 10.0 runtime
3. Install Chromium
4. Deploy application (from CI artifact or source)
5. Restore latest config backup
6. Verify execution
7. Update DNS/scheduling

### Scenario: Credential Compromise

**Immediate:**
1. Revoke Steam sessions (Steam website)
2. Change Steam password
3. Regenerate TOTP shared secret
4. Regenerate identity secret

**Recovery:**
1. Update `appsettings.json` with new credentials
2. Test run
3. Verify profile changes apply

### Scenario: Steam UI Breaks Automation

**Detection:** Crash screenshots, `NoSuchElementException` in logs

**Recovery:**
1. Check NuciWeb.Steam for updates
2. Update NuGet package: `dotnet add package NuciWeb.Steam --version <latest>`
3. Rebuild and redeploy
4. Test run

---

## Backup Verification

### Monthly Test

```bash
# 1. Create test restore directory
mkdir -p /tmp/spm-restore-test
cd /tmp/spm-restore-test

# 2. Copy latest backup
cp /opt/steam-profile-manager/backups/$(ls -t /opt/steam-profile-manager/backups | head -1)/* .

# 3. Validate JSON syntax
cat appsettings.json | jq empty && echo "JSON valid"

# 4. Validate required fields
cat appsettings.json | jq -e '.botSettings.steamAccount.username' && echo "Username present"
cat appsettings.json | jq -e '.botSettings.steamAccount.password' && echo "Password present"
cat appsettings.json | jq -e '.botSettings.steamAccount.sharedSecret' && echo "TOTP secret present"

# 5. Test file lists exist (if referenced)
NAMES_LIST=$(cat appsettings.json | jq -r '.botSettings.profileNamesList // empty')
if [ -n "$NAMES_LIST" ]; then
    test -f "$NAMES_LIST" && echo "Names list exists"
fi

PICS_LIST=$(cat appsettings.json | jq -r '.botSettings.profilePicturesList // empty')
if [ -n "$PICS_LIST" ]; then
    test -f "$PICS_LIST" && echo "Pictures list exists"
fi
```

### Automated Verification (CI)

```yaml
# .github/workflows/backup-verification.yml
name: Backup Verification
on:
  schedule:
    - cron: '0 3 1 * *'  # Monthly
jobs:
  verify:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Download backup artifact
        uses: actions/download-artifact@v4
        with:
          name: config-backup
      - name: Verify backup
        run: |
          cat appsettings.json | jq empty
          cat appsettings.json | jq -e '.botSettings.steamAccount.username'
          # ... etc
```

---

## Retention Policy

| Backup Type | Retention | Storage |
|-------------|-----------|---------|
| Daily config | 30 days | Local disk |
| Weekly config | 12 weeks | Offsite |
| Monthly config | 12 months | Offsite |
| Logs (rotated) | 30 days | Local disk |
| Crash screenshots | 7 days | Local disk |

---

## Backup Storage

### Local (Current)

```
/opt/steam-profile-manager/backups/
├── 20240115/
│   ├── appsettings.json
│   ├── names.txt
│   └── pictures.txt
├── 20240116/
│   └── ...
```

### Recommended: Offsite

- **S3/GCS/Azure Blob:** `aws s3 sync /opt/steam-profile-manager/backups s3://bucket/spm-backups/`
- **Git (encrypted):** `git commit -am "config backup" && git push` (use git-crypt or SOPS)
- **Rsync to backup server:** `rsync -avz /opt/steam-profile-manager/backups/ backupserver:/backups/spm/`

---

## Restore Checklist

- [ ] Service stopped
- [ ] Backup files copied to working directory
- [ ] JSON syntax valid
- [ ] Required credentials present
- [ ] File lists present (if configured)
- [ ] File permissions correct (600 for config)
- [ ] Service started
- [ ] Test run successful (check logs)
- [ ] Profile changes visible on Steam