# Operations — Monitoring

## Log Output

### NuciLog Output

**Console:** Structured log lines with timestamp, level, operation, status, key-values

**File:** `logfile.log` (configurable via `nuciLoggerSettings.logFilePath`)

**Format Example:**
```
2024-01-15 10:30:45.123 [Info] Operation=StartUp Status=Started
2024-01-15 10:30:45.456 [Info] Operation=LogIn Status=Started Username=myuser@example.com
2024-01-15 10:31:10.789 [Info] Operation=LogIn Status=Success Username=myuser@example.com
2024-01-15 10:31:11.012 [Info] Operation=SetProfileName Status=Started
2024-01-15 10:31:15.345 [Info] Operation=SetProfileName Status=Success ProfileName=CoolGamer123
2024-01-15 10:31:15.346 [Info] Operation=ShutDown Status=Started
```

### Log Levels

| Level | Usage |
|-------|-------|
| `Trace` | Verbose debugging (not used) |
| `Debug` | Detailed flow (not used by default) |
| `Info` | All operations (default) |
| `Warn` | Not used |
| `Error` | Application crashes |
| `Fatal` | Not used |

### Key Log Events

| Event | Operation | Status | Key Fields |
|-------|-----------|--------|------------|
| App start | `StartUp` | `Started` | — |
| Login start | `LogIn` | `Started` | `Username` |
| Login success | `LogIn` | `Success` | `Username` |
| Name change start | `SetProfileName` | `Started` | — |
| Name change success | `SetProfileName` | `Success` | `ProfileName` |
| Identifier change start | `SetProfileIdentifier` | `Started` | — |
| Identifier change success | `SetProfileIdentifier` | `Success` | `ProfileIdentifier` |
| Picture change start | `SetProfilePicture` | `Started` | — |
| Picture change success | `SetProfilePicture` | `Success` | `ProfilePicture` |
| App shutdown | `ShutDown` | `Started` | — |
| Crash | (exception) | — | Exception details |

---

## Metrics (Not Implemented)

### Currently Available

- Exit code: `0` = success, non-zero = failure
- Log file size / line count
- Crash screenshot existence

### Recommended Additions

| Metric | Collection Method |
|--------|-------------------|
| Run duration | Timestamp diff (StartUp → ShutDown) |
| Operation durations | Log timestamps per operation |
| Success/failure rate | Count `Success` vs `Error` in logs |
| Profile changes applied | Count `SetProfile*` Success events |
| Steam API latency | Not directly measurable |

---

## Alerting (Not Implemented)

### Recommended Alerts

| Alert | Condition | Severity |
|-------|-----------|----------|
| Run failed | Exit code != 0 | Critical |
| No run in 24h | Missing log entry | Warning |
| Crash screenshot created | `crash.png` exists | Critical |
| Log file growing rapidly | Size > threshold | Warning |
| Login failure rate > 10% | Recent runs | Warning |

### Implementation Options

1. **Log parsing cron** — Parse `logfile.log`, emit metrics
2. **File watcher** — Watch for `crash.png` creation
3. **systemd** — `OnFailure=` hook for service
4. **GitHub Actions** — Workflow run status

---

## Health Checks

### Pre-Run (Manual)

```bash
# Check config
cat appsettings.json | jq '.botSettings.steamAccount.username'

# Check browser
chromium --version

# Check network
curl -I https://steamcommunity.com

# Check disk space
df -h .
```

### Post-Run (Automated)

```bash
# Check exit code
./SteamProfileManager
echo "Exit code: $?"

# Check log for errors
grep -c "\[Error\]" logfile.log

# Check for crash screenshot
test -f crash.png && echo "CRASH DETECTED"

# Verify operations completed
grep "Operation=SetProfileName Status=Success" logfile.log
```

---

## Log Retention

### Current Behaviour

- `logfile.log` — Appends indefinitely (NuciLog default)
- `crash.png` — Overwrites on each crash
- `profilePicture.png` — Overwrites each run

### Recommended Policy

| File | Retention | Rotation |
|------|-----------|----------|
| `logfile.log` | 30 days | Daily rotate, compress |
| `crash.png` | 7 days | Keep latest |
| `profilePicture.png` | 1 day | Overwrite |

### Implementation (logrotate)

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

---

## Dashboard Queries (Conceptual)

### Grafana/Loki Queries

```logql
# All runs
{job="steam-profile-manager"} |= "Operation=StartUp"

# Failed runs
{job="steam-profile-manager"} |= "Error"

# Login duration
{job="steam-profile-manager"} |= "Operation=LogIn"
| pattern `<_> Operation=LogIn Status=Started Username=<user> <_> Operation=LogIn Status=Success Username=<user>`
| line_format "{{.user}} {{.timestamp}}"

# Success rate
sum(count_over_time({job="steam-profile-manager"} |= "Status=Success" [24h]))
/
sum(count_over_time({job="steam-profile-manager"} |= "Operation=StartUp" [24h]))
```

---

## Debugging Production Issues

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

### Enable Crash Screenshots

```json
{
  "debugSettings": {
    "crashScreenshotFileName": "crash.png",
    "isDebugMode": false
  }
}
```

### Enable Visible Browser (Interactive Debug)

```json
{
  "debugSettings": {
    "isDebugMode": true
  }
}
```

**Note:** Requires display/X11 forwarding for headless servers.

---

## Correlation IDs (Not Implemented)

### Recommended

Add correlation ID to track full run:
```csharp
string runId = Guid.NewGuid().ToString("N")[..8];
logger.Info(Operation.StartUp, OperationStatus.Started, new LogInfo("RunId", runId));
```

Propagate to all operations for traceability.

---

## External Monitoring Integration

### Prometheus Node Exporter (Textfile Collector)

```bash
# Write metrics after run
cat > /var/lib/node_exporter/textfile_collector/steam_profile_manager.prom <<EOF
steam_profile_manager_last_run_timestamp $(date +%s)
steam_profile_manager_last_run_duration_seconds $DURATION
steam_profile_manager_last_run_exit_code $EXIT_CODE
steam_profile_manager_login_success 1
steam_profile_manager_name_changed 1
steam_profile_manager_identifier_changed 1
steam_profile_manager_picture_changed 1
EOF
```

### Datadog / New Relic

Ship logs via agent, parse with pipeline:
- Extract operation, status, key-values
- Create metrics from log events
- Alert on error rate