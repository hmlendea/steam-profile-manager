# Operations — Incident Response

## Incident Classification

| Severity | Criteria | Response Time | Escalation |
|----------|----------|---------------|------------|
| **SEV-1** | Complete failure, no runs succeeding | 15 min | Page on-call |
| **SEV-2** | Partial failure (some operations fail) | 1 hour | Notify team |
| **SEV-3** | Degraded performance, warnings in logs | 4 hours | Ticket |
| **SEV-4** | Cosmetic issues, non-critical | Next business day | Backlog |

---

## SEV-1: Complete Failure

### Symptoms
- Exit code non-zero every run
- Crash screenshots generated
- `Error` entries in logs
- No successful `ShutDown` log entry

### Immediate Actions

1. **Check latest logs:**
   ```bash
   tail -50 /opt/steam-profile-manager/logfile.log
   ```

2. **Check crash screenshot:**
   ```bash
   ls -la /opt/steam-profile-manager/crash.png
   # View if X11 available
   ```

3. **Run health check:**
   ```bash
   /opt/steam-profile-manager/health-check.sh
   ```

### Common Root Causes & Fixes

| Cause | Diagnosis | Fix |
|-------|-----------|-----|
| Browser missing | `health-check.sh` fails on chromium | Install chromium |
| Credentials invalid | Log shows login failure, no 2FA prompt | Update credentials |
| TOTP secret wrong | Log shows 2FA failure | Regenerate shared secret |
| Steam UI changed | `NoSuchElementException` in logs | Update NuciWeb.Steam |
| Network blocked | Timeout exceptions, no connectivity | Check firewall/proxy |
| Disk full | `health-check.sh` fails on space | Clean logs/screenshots |

### Recovery Steps

```bash
# 1. Stop scheduled runs
systemctl stop steam-profile-manager.timer

# 2. Diagnose and fix root cause

# 3. Test manually
cd /opt/steam-profile-manager
./SteamProfileManager

# 4. Verify success
grep "Operation=ShutDown" logfile.log

# 5. Resume scheduling
systemctl start steam-profile-manager.timer
```

---

## SEV-2: Partial Failure

### Symptoms
- Some operations succeed, others fail
- E.g., login works but profile name fails
- Mixed `Success` and `Error` in logs

### Common Scenarios

| Scenario | Diagnosis | Fix |
|----------|-----------|-----|
| Name fails, others work | `SetProfileName` error in logs | Check name length, Steam UI |
| Identifier fails | "Already taken" or "30-day cooldown" | Wait or disable flag |
| Picture fails | Download or upload error | Check URL, file size, format |

### Recovery

1. Identify failing operation from logs
2. Disable that flag in `appsettings.json`
3. Re-run to complete other operations
4. Fix root cause, re-enable

---

## SEV-3: Degraded Performance

### Symptoms
- Runs take significantly longer than baseline
- Timeout warnings in logs
- High resource usage

### Diagnosis

```bash
# Check run durations from logs
grep "Operation=StartUp\|Operation=ShutDown" logfile.log | tail -10

# Check system resources
top -p $(pgrep -f SteamProfileManager)
df -h /opt/steam-profile-manager
```

### Common Causes

| Cause | Fix |
|-------|-----|
| Slow network | Increase `pageLoadTimeout` |
| Steam rate limiting | Add delays (not implemented) |
| Resource contention | Adjust systemd limits |
| Browser memory leak | Restart more frequently |

---

## Communication

### Internal Notification

| Severity | Channel | Template |
|----------|---------|----------|
| SEV-1 | PagerDuty / Slack #alerts | "SEV-1: Steam Profile Manager down - {error summary}" |
| SEV-2 | Slack #ops | "SEV-2: Partial failure - {failing operation}" |
| SEV-3 | Ticket (GitHub Issues) | "SEV-3: Performance degradation - {details}" |

### Status Updates

- **Every 15 min** for SEV-1
- **Every hour** for SEV-2
- **Daily** for SEV-3

### Post-Incident

1. **Root Cause Analysis** within 48 hours
2. **Action Items** tracked in GitHub Issues
3. **Documentation Update** if runbook gaps found
4. **Retrospective** for SEV-1/SEV-2

---

## Runbook References

| Incident Type | Runbook |
|---------------|---------|
| Login failure | troubleshooting.md #4 |
| Element not found | troubleshooting.md #5 |
| Timeout | troubleshooting.md #6 |
| Identifier cooldown | troubleshooting.md #7 |
| Picture upload fail | troubleshooting.md #8 |
| No crash screenshot | troubleshooting.md #9 |
| Config issues | troubleshooting.md #3 |
| Browser missing | troubleshooting.md #1 |
| Permission denied | troubleshooting.md #2 |

---

## Escalation Contacts

| Role | Contact | Hours |
|------|---------|-------|
| Primary On-Call | [Name] | 24/7 |
| Secondary On-Call | [Name] | 24/7 |
| Team Lead | [Name] | Business hours |
| Steam API Support | support@steampowered.com | Business hours |

---

## War Room

For SEV-1 lasting > 1 hour:
1. Create Slack channel `#incident-spm-YYYYMMDD`
2. Invite on-call, team lead, relevant engineers
3. Document timeline in channel
4. Assign roles: Commander, Scribe, Communicator