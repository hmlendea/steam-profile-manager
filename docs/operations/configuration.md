# Operations — Configuration

## Configuration Sources

### Primary: appsettings.json

**Location:** Working directory (same as executable)

**Structure:** See [Configuration Schema](../api-reference/configuration-schema.md)

**Loading:** `ConfigurationBuilder().AddJsonFile("appsettings.json", true, true)`

**Options:**
- `optional: true` — Missing file doesn't throw
- `reloadOnChange: true` — Hot reload enabled (not effective in console app)

### Not Currently Supported

| Source | Status | Implementation |
|--------|--------|----------------|
| Environment variables | ❌ | Add `AddEnvironmentVariables()` |
| Command line args | ❌ | Add `AddCommandLine(args)` |
| User secrets | ❌ | Add `AddUserSecrets<Program>()` |
| Azure Key Vault | ❌ | Add `AddAzureKeyVault()` |
| Consul/etcd | ❌ | Custom provider |

---

## Configuration Sections

### botSettings

**Class:** `SteamProfileManager.Configuration.BotSettings`

| Property | Type | Default | Validation |
|----------|------|---------|------------|
| `PageLoadTimeout` | `int` | 90 | > 0 |
| `SteamAccount` | `SteamAccount` | new() | Required at runtime |
| `RandomiseProfileName` | `bool` | false | — |
| `RandomiseProfileIdentifier` | `bool` | false | — |
| `RandomiseProfilePicture` | `bool` | false | — |
| `ProfileNamesList` | `string?` | null | File exists if set |
| `ProfilePicturesList` | `string?` | null | File exists if set |

**SteamAccount Properties:**
| Property | Type | Required |
|----------|------|----------|
| `Username` | `string` | Yes |
| `Password` | `string` | Yes |
| `SharedSecret` | `string` | Yes |
| `IdentitySecret` | `string` | Yes |
| `DeviceId` | `string` | No |

### debugSettings

**Class:** `SteamProfileManager.Configuration.DebugSettings`

| Property | Type | Default | Computed |
|----------|------|---------|----------|
| `CrashScreenshotFileName` | `string` | `""` | `IsCrashScreenshotEnabled` |
| `IsDebugMode` | `bool` | false | `IsHeadless = !IsDebugMode` |

### nuciLoggerSettings

**Class:** `NuciLog.NuciLoggerSettings`

| Property | Type | Default | Values |
|----------|------|---------|--------|
| `MinimumLevel` | `string` | "Info" | Trace, Debug, Info, Warn, Error, Fatal |
| `LogFilePath` | `string` | "logfile.log" | Path |
| `IsFileOutputEnabled` | `bool` | true | — |

---

## Configuration Validation

### Current: None at Startup

Settings validated at **use time**:
- `SteamAccount` → `ISteamProcessor.LogIn()` throws if invalid
- `ProfileNamesList` → `File.ReadAllLines()` throws if missing
- `PageLoadTimeout` → WebDriver throws if ≤ 0

### Recommended: Startup Validation

```csharp
static void ValidateConfiguration()
{
    if (string.IsNullOrWhiteSpace(botSettings.SteamAccount.Username))
        throw new InvalidOperationException("Steam username required");

    if (string.IsNullOrWhiteSpace(botSettings.SteamAccount.Password))
        throw new InvalidOperationException("Steam password required");

    if (string.IsNullOrWhiteSpace(botSettings.SteamAccount.SharedSecret))
        throw new InvalidOperationException("TOTP shared secret required");

    if (botSettings.PageLoadTimeout <= 0)
        throw new InvalidOperationException("PageLoadTimeout must be > 0");

    if (botSettings.RandomiseProfileName && !string.IsNullOrWhiteSpace(botSettings.ProfileNamesList))
    {
        if (!File.Exists(botSettings.ProfileNamesList))
            throw new FileNotFoundException("Profile names list not found", botSettings.ProfileNamesList);
    }

    if (botSettings.RandomiseProfilePicture && !string.IsNullOrWhiteSpace(botSettings.ProfilePicturesList))
    {
        if (!File.Exists(botSettings.ProfilePicturesList))
            throw new FileNotFoundException("Profile pictures list not found", botSettings.ProfilePicturesList);
    }
}
```

---

## Secrets Management

### Current: Plain Text in appsettings.json

**Risk:** Credentials visible in config file

### Options for Production

#### 1. Environment Variables (Minimal Code Change)

```csharp
// In LoadConfiguration()
IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", true, true)
    .AddEnvironmentVariables(prefix: "SPM_")  // SPM_STEAM_USERNAME, etc.
    .Build();
```

**appsettings.json** (template, no secrets):
```json
{
  "botSettings": {
    "steamAccount": {
      "username": "",
      "password": "",
      "sharedSecret": "",
      "identitySecret": "",
      "deviceId": ""
    }
  }
}
```

**Runtime:** Set `SPM_BOTSETTINGS__STEAMACCOUNT__USERNAME=myuser` etc.

#### 2. User Secrets (Development)

```bash
dotnet user-secrets set "botSettings:steamAccount:username" "myuser"
dotnet user-secrets set "botSettings:steamAccount:password" "pass"
# ...
```

```csharp
// In LoadConfiguration() - only in Development
.AddUserSecrets<Program>()
```

#### 3. External Secret Store (Production)

- Azure Key Vault
- HashiCorp Vault
- AWS Secrets Manager
- Kubernetes Secrets

Requires custom configuration provider.

---

## File-Based Lists

### Profile Names List

**Config:** `botSettings.profileNamesList`

**Format:** UTF-8 text file, one name per line

**Example `names.txt`:**
```
CoolGamer123
ProPlayer
SteamUser
AnotherName
GamerTag
```

**Validation:** None — empty lines produce empty names (Steam rejects)

**Encoding:** Must be UTF-8 (no BOM)

### Profile Pictures List

**Config:** `botSettings.profilePicturesList`

**Format:** UTF-8 text file, one direct image URL per line

**Example `pictures.txt`:**
```
https://i.imgur.com/abc123.png
https://example.com/avatar.jpg
https://cdn.site.com/image.gif
```

**Requirements:**
- Direct image URLs (not HTML pages)
- Accessible without auth
- Supported formats: JPG, PNG, GIF
- Max ~5MB (Steam limit)

---

## Configuration Precedence

**Current (single source):**
1. `appsettings.json` only

**With Extensions:**
1. `appsettings.json` (base)
2. `appsettings.{Environment}.json` (environment-specific)
3. User secrets (Development)
4. Environment variables
5. Command line arguments

**Environment Detection:** Not implemented — would need `ASPNETCORE_ENVIRONMENT` or `DOTNET_ENVIRONMENT`

---

## Hot Reload

**Enabled in config:** `reloadOnChange: true`

**Effective:** No — console app exits after single run

**For Long-Running Services:** Would work with `IOptionsMonitor<TOptions>`

---

## Schema Validation (Not Implemented)

### JSON Schema

Could add `appsettings.schema.json` for IDE validation:

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "properties": {
    "botSettings": { "$ref": "#/definitions/botSettings" },
    "debugSettings": { "$ref": "#/definitions/debugSettings" },
    "nuciLoggerSettings": { "$ref": "#/definitions/nuciLoggerSettings" }
  },
  "definitions": {
    "botSettings": { ... }
  }
}
```

### C# Validation Attributes

Could add to POCOs:
```csharp
public class BotSettings
{
    [Range(1, 300)]
    public int PageLoadTimeout { get; set; }

    [Required]
    public SteamAccount SteamAccount { get; set; }

    [FileExists(ErrorMessage = "Profile names list not found")]
    public string? ProfileNamesList { get; set; }
}
```

Requires `System.ComponentModel.Annotations` and custom validators.

---

## Migration Guide

### Adding Environment Variable Support

1. Modify `LoadConfiguration()`:
```csharp
IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", true, true)
    .AddEnvironmentVariables("SPM_")
    .Build();
```

2. Update deployment to set env vars

3. Keep `appsettings.json` as template (no secrets)

### Adding User Secrets (Dev)

1. `dotnet user-secrets init` in project dir
2. Set secrets via CLI
3. Add `.AddUserSecrets<Program>()` in `LoadConfiguration()` (conditional on `Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == "Development"`)

### Adding Config Validation

1. Create `ValidateConfiguration()` method
2. Call after `LoadConfiguration()` in `Main()`
3. Throw descriptive exceptions for missing/invalid config