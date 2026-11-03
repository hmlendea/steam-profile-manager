# API Reference — Configuration Schema

## appsettings.json Structure

```json
{
  "botSettings": {
    "pageLoadTimeout": 90,
    "steamAccount": {
      "username": "string",
      "password": "string",
      "sharedSecret": "string",
      "identitySecret": "string",
      "deviceId": "string"
    },
    "randomiseProfileName": false,
    "randomiseProfileIdentifier": false,
    "randomiseProfilePicture": false,
    "profileNamesList": "string|null",
    "profilePicturesList": "string|null"
  },
  "debugSettings": {
    "crashScreenshotFileName": "string",
    "isDebugMode": false
  },
  "nuciLoggerSettings": {
    "minimumLevel": "string",
    "logFilePath": "string",
    "isFileOutputEnabled": true
  }
}
```

## botSettings

| Property | Type | Required | Default | Description |
|----------|------|----------|---------|-------------|
| `pageLoadTimeout` | `integer` | Yes | 90 | Selenium page load timeout in seconds. Must be > 0. |
| `steamAccount` | `object` | Yes | — | Steam credentials object (see below). |
| `randomiseProfileName` | `boolean` | No | `false` | Enable random profile display name update. |
| `randomiseProfileIdentifier` | `boolean` | No | `false` | Enable random profile custom URL identifier update. |
| `randomiseProfilePicture` | `boolean` | No | `false` | Enable random profile picture upload. |
| `profileNamesList` | `string|null` | No | `null` | Path to text file with names (one per line, UTF-8). Used when `randomiseProfileName=true`. |
| `profilePicturesList` | `string|null` | No | `null` | Path to text file with image URLs (one per line, UTF-8). Used when `randomiseProfilePicture=true`. |

### steamAccount

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `username` | `string` | Yes | Steam login username or email. |
| `password` | `string` | Yes | Steam login password. |
| `sharedSecret` | `string` | Yes | TOTP shared secret (Base32) for Steam Guard 2FA. |
| `identitySecret` | `string` | Yes | Identity secret (Base32) for trade confirmations. |
| `deviceId` | `string` | No | Steam Guard device ID (optional). |

## debugSettings

| Property | Type | Required | Default | Description |
|----------|------|----------|---------|-------------|
| `crashScreenshotFileName` | `string` | No | `""` | Filename for crash screenshot (PNG). Empty string disables crash screenshots. |
| `isDebugMode` | `boolean` | No | `false` | `true` = visible browser, `false` = headless mode. |

### Computed Properties (Not in JSON)

| Property | Expression | Description |
|----------|------------|-------------|
| `IsHeadless` | `!IsDebugMode` | Passed to WebDriver initialiser. |
| `IsCrashScreenshotEnabled` | `!string.IsNullOrWhiteSpace(CrashScreenshotFileName)` | Checked in catch block. |

## nuciLoggerSettings

| Property | Type | Required | Default | Description |
|----------|------|----------|---------|-------------|
| `minimumLevel` | `string` | Yes | `"Info"` | Minimum log level: `Trace`, `Debug`, `Info`, `Warn`, `Error`, `Fatal`. |
| `logFilePath` | `string` | Yes | `"logfile.log"` | File path for file log output. |
| `isFileOutputEnabled` | `boolean` | Yes | `true` | Enable file sink. |

## Example (Complete)

```json
{
  "botSettings": {
    "pageLoadTimeout": 90,
    "steamAccount": {
      "username": "myuser@example.com",
      "password": "securepassword123",
      "sharedSecret": "ABCDEF123456GHIJKL7890MNOPQRST",
      "identitySecret": "UVWXYZ123456ABCDEF7890GHIJKLMNOP",
      "deviceId": "android:1234567890abcdef"
    },
    "randomiseProfileName": true,
    "randomiseProfileIdentifier": true,
    "randomiseProfilePicture": true,
    "profileNamesList": "names.txt",
    "profilePicturesList": "pictures.txt"
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
```

## File Formats

### names.txt (profileNamesList)

```
CoolGamer123
ProPlayer
SteamUser
AnotherName
```

- UTF-8 encoding
- One name per line
- Empty lines ignored
- No validation on name content (Steam validates on submit)

### pictures.txt (profilePicturesList)

```
https://example.com/image1.png
https://imgur.com/abc123.jpg
https://cdn.example.com/avatar.gif
```

- UTF-8 encoding
- One URL per line
- Must be direct image URLs (not HTML pages)
- Supported formats: JPG, PNG, GIF
- Max size ~5MB (Steam limit)

## Environment Variable Override

Not currently supported. Only `appsettings.json` is loaded.

To add: `ConfigurationBuilder().AddEnvironmentVariables()` in `LoadConfiguration()`.

## Command Line Override

Not currently supported.

To add: `ConfigurationBuilder().AddCommandLine(args)` in `LoadConfiguration()`.