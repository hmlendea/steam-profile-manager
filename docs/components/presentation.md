# Components — Presentation

## Overview

This is a **console application** with no graphical user interface, no web UI, and no API endpoints. The "presentation" is the command-line execution and structured log output.

## Execution Interface

### Command Line

```bash
./SteamProfileManager
```

**Arguments:** None supported

**Environment Variables:** None supported (currently)

**Exit Codes:**
| Code | Meaning |
|------|---------|
| 0 | Success — all operations completed |
| Non-zero | Failure — exception thrown (see logs) |

### Working Directory

- Reads `appsettings.json` from current directory
- Writes `logfile.log` to current directory
- Writes `crash.png` to current directory (on crash)
- Writes `profilePicture.png` to current directory (temporary)

---

## Log Output (Primary Presentation)

### Console Output

Structured log lines via NuciLog:

```
2024-01-15 10:30:45.123 [Info] Operation=StartUp Status=Started
2024-01-15 10:30:45.456 [Info] Operation=LogIn Status=Started Username=myuser@example.com
2024-01-15 10:31:10.789 [Info] Operation=LogIn Status=Success Username=myuser@example.com
2024-01-15 10:31:11.012 [Info] Operation=SetProfileName Status=Started
2024-01-15 10:31:15.345 [Info] Operation=SetProfileName Status=Success ProfileName=CoolGamer123
2024-01-15 10:31:15.346 [Info] Operation=ShutDown Status=Started
```

### Log File

Same content written to `logfile.log` (configurable path).

### Log Structure

| Field | Description |
|-------|-------------|
| Timestamp | ISO 8601 with milliseconds |
| Level | Info, Error, etc. |
| Operation | `StartUp`, `LogIn`, `SetProfileName`, `SetProfileIdentifier`, `SetProfilePicture`, `ShutDown` |
| Status | `Started`, `Success` (or exception for Error) |
| Key-Values | Domain-specific: `Username`, `ProfileName`, `ProfileIdentifier`, `ProfilePicture` |

---

## Visual Feedback (Debug Mode)

### When `debugSettings.isDebugMode = true`

- Visible browser window opens
- User can observe:
  1. Steam login page
  2. 2FA entry
  3. Profile edit pages
  4. Form interactions
  5. Picture upload + crop dialog

### When `debugSettings.isDebugMode = false` (Default)

- Headless browser (no visible UI)
- No visual feedback
- Only log output

---

## Crash Screenshot

### When Enabled

```json
{
  "debugSettings": {
    "crashScreenshotFileName": "crash.png"
  }
}
```

**Output:** PNG file capturing browser viewport at crash moment

**Use Case:** Debugging `NoSuchElementException`, Steam UI changes, captcha challenges

---

## No Interactive Prompts

- No stdin reading
- No confirmation prompts
- No progress bars
- Fully automated execution

---

## Integration Interfaces

### For Orchestration Systems

| Interface | Mechanism |
|-----------|-----------|
| Scheduling | Cron, systemd timer, GitHub Actions schedule |
| Config injection | File replacement, environment variables (planned) |
| Secrets | Environment variables (planned), external secret store |
| Monitoring | Exit code, log parsing, crash screenshot detection |
| Artifacts | `logfile.log`, `crash.png`, `profilePicture.png` |

### For CI/CD

```yaml
# GitHub Actions example
- name: Run Steam Profile Manager
  run: ./SteamProfileManager
  env:
    # Future: inject secrets via env vars
```

---

## Accessibility

- **Screen readers:** Log output is text-based, readable
- **Colour:** No colour codes in logs (plain text)
- **Language:** English only (log messages, config keys)
- **Localisation:** Not supported

---

## Summary

| Aspect | Implementation |
|--------|----------------|
| UI Type | Console / Headless browser |
| User Interaction | None (fully automated) |
| Primary Output | Structured logs (console + file) |
| Debug Output | Visible browser + crash screenshots |
| Config Interface | `appsettings.json` file |
| Secrets Interface | Plain text in config (planned: env vars) |
| Scheduling | External (cron, systemd, CI/CD) |
| Monitoring | Exit code + log parsing |