# Components — Integration Models

## SteamAccount

**Package:** `NuciWeb.Steam` (v1.5.3)

**Type:** `NuciWeb.Steam.SteamAccount` (record/class)

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Username` | `string` | Steam account username/email |
| `Password` | `string` | Steam account password |
| `SharedSecret` | `string` | TOTP shared secret for 2FA (Base32) |
| `IdentitySecret` | `string` | Identity secret for trade confirmations (Base32) |
| `DeviceId` | `string` | Device ID for Steam Guard (optional) |

### Usage in Configuration

**appsettings.json:**
```json
"botSettings": {
  "steamAccount": {
    "username": "your_username",
    "password": "your_password",
    "sharedSecret": "BASE32_TOTP_SECRET",
    "identitySecret": "BASE32_IDENTITY_SECRET",
    "deviceId": "optional_device_id"
  }
}
```

**Binding:** `BotSettings.SteamAccount` → `SteamAccount` via `config.Bind()`

### TOTP Generation

- `SharedSecret` used to generate time-based one-time passwords (RFC 6238)
- 30-second windows, 6-digit codes
- Handled internally by `NuciWeb.Steam.SteamProcessor.LogIn()`

---

## BotSettings

**Location:** `SteamProfileManager/Configuration/BotSettings.cs`

**Type:** `SteamProfileManager.Configuration.BotSettings` (sealed class)

### Properties

| Property | Type | Required | Default | Description |
|----------|------|----------|---------|-------------|
| `PageLoadTimeout` | `int` | Yes | 90 | Selenium page load timeout (seconds) |
| `SteamAccount` | `SteamAccount` | Yes | — | Steam credentials and 2FA secrets |
| `RandomiseProfileName` | `bool` | No | `false` | Enable random profile name update |
| `RandomiseProfileIdentifier` | `bool` | No | `false` | Enable random profile identifier update |
| `RandomiseProfilePicture` | `bool` | No | `false` | Enable random profile picture update |
| `ProfileNamesList` | `string?` | No | `null` | Path to file with names (one per line) |
| `ProfilePicturesList` | `string?` | No | `null` | Path to file with image URLs (one per line) |

### Constructor

```csharp
public BotSettings()
{
    PageLoadTimeout = 90;
    SteamAccount = new SteamAccount();
}
```

### Validation

- No built-in validation — missing credentials cause runtime failure in `SteamProcessor.LogIn()`
- File paths not validated until `ProfileManager` attempts to read them

---

## DebugSettings

**Location:** `SteamProfileManager/Configuration/DebugSettings.cs`

**Type:** `SteamProfileManager.Configuration.DebugSettings` (sealed class)

### Properties

| Property | Type | Required | Default | Description |
|----------|------|----------|---------|-------------|
| `CrashScreenshotFileName` | `string` | No | `""` | Filename for crash screenshot (empty = disabled) |
| `IsDebugMode` | `bool` | No | `false` | `true` = visible browser, `false` = headless |

### Computed Properties

```csharp
public bool IsHeadless => !IsDebugMode;
public bool IsCrashScreenshotEnabled => !string.IsNullOrWhiteSpace(CrashScreenshotFileName);
```

### Usage

| Computed Property | Consumer |
|-------------------|----------|
| `IsHeadless` | `WebDriverInitialiser.InitialiseAvailableWebDriver()` |
| `IsCrashScreenshotEnabled` | `Program.Main()` catch block |

---

## NuciLoggerSettings

**Package:** `NuciLog` (v1.2.1)

**Type:** `NuciLog.NuciLoggerSettings`

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `MinimumLevel` | `string` | Log level: "Trace", "Debug", "Info", "Warn", "Error", "Fatal" |
| `LogFilePath` | `string` | File path for file output |
| `IsFileOutputEnabled` | `bool` | Enable file sink |

### Usage

```csharp
services.AddNuciLog(loggerSettings);
```

---

## ISteamProcessor

**Package:** `NuciWeb.Steam` (v1.5.3)

**Interface:** `NuciWeb.Steam.ISteamProcessor`

**Implementation:** `NuciWeb.Steam.SteamProcessor` (internal)

### Methods

| Method | Signature | Description |
|--------|-----------|-------------|
| `LogIn` | `void LogIn(SteamAccount account)` | Authenticate with Steam |
| `RejectCookies` | `void RejectCookies()` | Dismiss GDPR cookie banner |
| `SetProfileName` | `void SetProfileName(string name)` | Update display name |
| `SetProfileIdentifier` | `void SetProfileIdentifier(string identifier)` | Update custom URL |
| `SetProfilePicture` | `void SetProfilePicture(string filePath)` | Upload profile picture |

### Dependencies

- Requires `IWebDriver` (injected via DI in NuciWeb.Steam)
- Uses Selenium for all DOM interactions

### Error Behaviour

- All methods synchronous, blocking
- Exceptions propagate: `WebDriverException`, `NoSuchElementException`, `TimeoutException`
- No retry logic

---

## MyOperation (Custom Logging Operations)

**Location:** `SteamProfileManager/Logging/MyOperation.cs`

**Type:** `SteamProfileManager.Logging.MyOperation` (static class)

### Values

| Operation | Description |
|-----------|-------------|
| `LogIn` | Steam authentication |
| `SetProfileName` | Profile display name update |
| `SetProfileIdentifier` | Profile custom URL update |
| `SetProfilePicture` | Profile picture upload |

### Usage

```csharp
logger.Info(MyOperation.LogIn, OperationStatus.Started, ...);
logger.Info(MyOperation.SetProfileName, OperationStatus.Success, ...);
```

---

## MyLogInfoKey (Custom Logging Keys)

**Location:** `SteamProfileManager/Logging/MyLogInfoKey.cs`

**Type:** `SteamProfileManager.Logging.MyLogInfoKey` (static class)

### Keys

| Key | Type | Description |
|-----|------|-------------|
| `Username` | `string` | Steam username |
| `ProfileName` | `string` | Display name |
| `ProfileIdentifier` | `string` | Custom URL identifier |
| `ProfilePicture` | `string` | Source URL or file path |

### Usage

```csharp
new LogInfo(MyLogInfoKey.Username, botSettings.SteamAccount.Username)
new LogInfo(MyLogInfoKey.ProfileName, profileName)
```

---

## Data Flow Summary

```
appsettings.json
    │
    ├─► BotSettings (SteamAccount, flags, file paths)
    ├─► DebugSettings (headless, crash screenshot)
    └─► NuciLoggerSettings (log level, file output)
           │
           ▼
    DI Container (ServiceCollection)
           │
           ├─► IWebDriver (WebDriverInitialiser)
           ├─► IProfileManager → ProfileManager
           │       ├─► IInfoGenerator → InfoGenerator
           │       ├─► IFileDownloader → FileDownloader
           │       └─► ISteamProcessor → SteamProcessor (NuciWeb.Steam)
           └─► ILogger → NuciLog
```