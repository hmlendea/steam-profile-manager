# Repository Overview

## Purpose

Steam Profile Manager automates Steam profile updates (display name, custom URL identifier, profile picture) via browser automation. It is a self-hosted .NET 10.0 console application for unattended execution.

## Scope

**In scope:**
- Steam authentication (username, password, TOTP)
- Profile display name randomisation (from file or generated)
- Profile identifier (custom URL) randomisation (generated)
- Profile picture randomisation (from file or default, downloaded and uploaded)
- Configuration via `appsettings.json`
- Structured logging via NuciLog
- Crash screenshot capture (optional)

**Out of scope:**
- Steam platform features beyond profile updates
- GUI or web interface
- Background/daemon operation
- Multi-account management
- Profile data persistence beyond temporary files

## Entry Points

| Entry Point | Location | Description |
|-------------|----------|-------------|
| `Main` | `SteamProfileManager/Program.cs` | Application bootstrap, configuration, DI, WebDriver, execution flow |

## Principal Capabilities

1. **Steam Login** — Authenticates via `ISteamProcessor.LogIn` using `SteamAccount` credentials
2. **Cookie Rejection** — Calls `ISteamProcessor.RejectCookies` after login
3. **Profile Name Update** — Random name from file or 12-char alphanumeric generation via `InfoGenerator`
4. **Profile Identifier Update** — 8-char alphanumeric generation via `InfoGenerator`
5. **Profile Picture Update** — Downloads image from URL (file list or default) via `FileDownloader`, uploads via `ISteamProcessor.SetProfilePicture`

## System Context

```
User/Scheduler → Steam Profile Manager → Selenium WebDriver → Steam Website
                      ↓
                 appsettings.json
                      ↓
                 Logs (console/file)
                      ↓
                 profilePicture.png (temp)
```

## High-Level Components

| Component | Location | Responsibility |
|-----------|----------|----------------|
| Program | `Program.cs` | Lifecycle, composition, error handling |
| Configuration | `Configuration/*.cs` | Settings data objects |
| ProfileManager | `Service/ProfileManager.cs` | Orchestrates profile mutations |
| InfoGenerator | `Service/InfoGenerator.cs` | Random name/identifier generation |
| FileDownloader | `Service/FileDownloader.cs` | HTTP image download |
| Logging | `Logging/*.cs` | Domain-specific structured logging keys |
| Steam Integration | `NuciWeb.Steam.ISteamProcessor` | Steam DOM interactions |
| Browser Control | `NuciWeb.Automation.Selenium` | WebDriver initialisation |