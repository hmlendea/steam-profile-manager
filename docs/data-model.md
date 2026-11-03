# Data Model

## Domain Entities

### SteamAccount (NuciWeb.Steam.Models)

```csharp
public sealed class SteamAccount
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string TotpKey { get; set; }
}
```

**Source:** `NuciWeb.Steam.Models.SteamAccount` (external package)

**Usage:** Provided via `BotSettings.SteamAccount`, passed to `ISteamProcessor.LogIn`

**Fields:**
| Field | Type | Description |
|-------|------|-------------|
| Username | string | Steam account username |
| Password | string | Steam account password |
| TotpKey | string | Steam Guard shared secret for TOTP generation |

### BotSettings (Configuration)

```csharp
public sealed class BotSettings
{
    public int PageLoadTimeout { get; set; }
    public SteamAccount SteamAccount { get; set; }
    public bool RandomiseProfileName { get; set; }
    public bool RandomiseProfileIdentifier { get; set; }
    public bool RandomiseProfilePicture { get; set; }
    public string ProfileNamesList { get; set; }
    public string ProfilePicturesList { get; set; }
}
```

**Source:** `SteamProfileManager.Configuration.BotSettings`

**Usage:** Bound from `appsettings.json` → `BotSettings` section, injected into `ProfileManager`

**Fields:**
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| PageLoadTimeout | int | Yes | Selenium page load timeout in seconds |
| SteamAccount | SteamAccount | Yes | Credentials for Steam login |
| RandomiseProfileName | bool | Yes | Enable profile name randomisation |
| RandomiseProfileIdentifier | bool | Yes | Enable profile identifier randomisation |
| RandomiseProfilePicture | bool | Yes | Enable profile picture randomisation |
| ProfileNamesList | string | No | Path to file with candidate names (one per line) |
| ProfilePicturesList | string | No | Path to file with candidate image URLs (one per line) |

### DebugSettings (Configuration)

```csharp
public sealed class DebugSettings
{
    public string CrashScreenshotFileName { get; set; }
    public bool IsDebugMode { get; set; }
    public bool IsHeadless => !IsDebugMode;
    public bool IsCrashScreenshotEnabled => !string.IsNullOrWhiteSpace(CrashScreenshotFileName);
}
```

**Source:** `SteamProfileManager.Configuration.DebugSettings`

**Usage:** Bound from `appsettings.json` → `DebugSettings` section, used in `Program.cs` for WebDriver initialisation

**Fields:**
| Field | Type | Description |
|-------|------|-------------|
| CrashScreenshotFileName | string | Filename for crash screenshots (empty disables) |
| IsDebugMode | bool | Run browser in non-headless mode |
| IsHeadless | bool (computed) | Inverse of IsDebugMode |
| IsCrashScreenshotEnabled | bool (computed) | True when CrashScreenshotFileName is set |

### NuciLoggerSettings (NuciLog.Configuration)

```csharp
public sealed class NuciLoggerSettings
{
    public string MinimumLevel { get; set; }
    public string LogFilePath { get; set; }
    public bool IsFileOutputEnabled { get; set; }
}
```

**Source:** `NuciLog.Configuration.NuciLoggerSettings` (external package)

**Usage:** Bound from `appsettings.json` → `nuciLoggerSettings` section, used in `Program.CreateIOC` for logger configuration

## Transient Runtime State

### ProfileManager Dependencies (injected)

| Dependency | Interface | Implementation | Lifetime |
|------------|-----------|----------------|----------|
| InfoGenerator | `IInfoGenerator` | `InfoGenerator` | Transient |
| FileDownloader | `IFileDownloader` | `FileDownloader` | Transient |
| SteamProcessor | `ISteamProcessor` | NuciWeb.Steam | Transient |
| BotSettings | `BotSettings` | Configuration | Singleton (static in Program) |
| Logger | `ILogger` | NuciLog | Singleton |

### Temporary Files

| File | Location | Lifecycle | Purpose |
|------|----------|-----------|---------|
| `profilePicture.png` | Working directory | Created per run, overwritten | Downloaded profile picture for upload |
| `crash.png` | Working directory | Created on crash when enabled | Debug screenshot |
| `logfile.log` | Working directory | Append-only, operator-managed | Application logs |

## Relationships

```
BotSettings
    ├── SteamAccount (1:1)
    ├── ProfileNamesList (optional file path)
    └── ProfilePicturesList (optional file path)

ProfileManager
    ├── IInfoGenerator → InfoGenerator
    ├── IFileDownloader → FileDownloader
    ├── ISteamProcessor → NuciWeb.Steam
    ├── BotSettings (data)
    └── ILogger → NuciLog

InfoGenerator
    └── NuciExtensions.GetRandomElement (static)

FileDownloader
    └── System.Net.WebClient (BCL)

ISteamProcessor
    └── Selenium WebDriver (via NuciWeb.Steam)
```

## Persistence

**No persistent data store.** All state is:
- Configuration: `appsettings.json` (file, operator-managed)
- Credentials: In-memory only during execution
- Logs: Append-only file (operator-managed rotation)
- Temporary files: Overwritten each run

## Serialisation

- Configuration: `Microsoft.Extensions.Configuration.Json` → POCO binding
- Logs: NuciLog structured format (JSON-like)
- No domain entity serialisation required