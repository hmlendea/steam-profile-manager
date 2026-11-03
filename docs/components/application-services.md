# Components — Application Services

## IProfileManager / ProfileManager

**Location:** `SteamProfileManager/Service/ProfileManager.cs`

**Interface:** `SteamProfileManager.Service.IProfileManager`

**Implementation:** `SteamProfileManager.Service.ProfileManager` (sealed)

### Constructor (Primary)

```csharp
public sealed class ProfileManager(
    IInfoGenerator infoGenerator,
    IFileDownloader fileDownloader,
    ISteamProcessor steamProcessor,
    BotSettings botSettings,
    ILogger logger) : IProfileManager
```

### Dependencies

| Dependency | Purpose |
|------------|---------|
| `IInfoGenerator` | Random name/identifier generation |
| `IFileDownloader` | Profile picture download |
| `ISteamProcessor` | Steam authentication and DOM mutations |
| `BotSettings` | Configuration (credentials, flags, file paths) |
| `ILogger` | Structured logging |

### Methods

#### LogIn()

```csharp
public void LogIn()
{
    logger.Info(MyOperation.LogIn, OperationStatus.Started,
        new LogInfo(MyLogInfoKey.Username, botSettings.SteamAccount.Username));

    steamProcessor.LogIn(botSettings.SteamAccount);
    steamProcessor.RejectCookies();

    logger.Info(MyOperation.LogIn, OperationStatus.Success,
        new LogInfo(MyLogInfoKey.Username, botSettings.SteamAccount.Username));
}
```

**Flow:**
1. Log start with username
2. Call `ISteamProcessor.LogIn` with `SteamAccount` credentials
3. Call `ISteamProcessor.RejectCookies` (GDPR cookie banner)
4. Log success with username

**Errors:** Propagates exceptions from `ISteamProcessor`

#### SetRandomProfileName()

```csharp
public void SetRandomProfileName()
{
    logger.Info(MyOperation.SetProfileName, OperationStatus.Started);

    string profileName = string.IsNullOrWhiteSpace(botSettings.ProfileNamesList)
        ? infoGenerator.GetRandomProfileName()
        : GetRandomEntryFromList(botSettings.ProfileNamesList);

    steamProcessor.SetProfileName(profileName);

    logger.Info(MyOperation.SetProfileName, OperationStatus.Success,
        new LogInfo(MyLogInfoKey.ProfileName, profileName));
}
```

**Flow:**
1. Log start
2. If `ProfileNamesList` configured and non-empty: read random line from file
3. Else: generate 12-char alphanumeric via `InfoGenerator.GetRandomProfileName()`
4. Call `ISteamProcessor.SetProfileName`
5. Log success with chosen name

**File Format:** One name per line, UTF-8

#### SetRandomProfileIdentifier()

```csharp
public void SetRandomProfileIdentifier()
{
    logger.Info(MyOperation.SetProfileIdentifier, OperationStatus.Started);

    string profileIdentifier = infoGenerator.GetRandomProfileIdentifier();

    steamProcessor.SetProfileIdentifier(profileIdentifier);

    logger.Info(MyOperation.SetProfileIdentifier, OperationStatus.Success,
        new LogInfo(MyLogInfoKey.ProfileIdentifier, profileIdentifier));
}
```

**Flow:**
1. Log start
2. Generate 8-char alphanumeric via `InfoGenerator.GetRandomProfileIdentifier()`
3. Call `ISteamProcessor.SetProfileIdentifier`
4. Log success with identifier

**Note:** No file-based option — always generated

#### SetRandomProfilePicture()

```csharp
public void SetRandomProfilePicture()
{
    logger.Info(MyOperation.SetProfilePicture, OperationStatus.Started);

    string profilePictureUrl = string.IsNullOrWhiteSpace(botSettings.ProfilePicturesList)
        ? "https://i.imgur.com/rV2RQMO.png"
        : GetRandomEntryFromList(botSettings.ProfilePicturesList);

    string profilePictureFilePath = Path.Join(Directory.GetCurrentDirectory(), "profilePicture.png");

    fileDownloader.Download(profilePictureUrl, profilePictureFilePath);
    steamProcessor.SetProfilePicture(profilePictureFilePath);

    logger.Info(MyOperation.SetProfilePicture, OperationStatus.Success,
        new LogInfo(MyLogInfoKey.ProfilePicture, profilePictureUrl));
}
```

**Flow:**
1. Log start
2. If `ProfilePicturesList` configured and non-empty: read random URL from file
3. Else: use default URL (`https://i.imgur.com/rV2RQMO.png`)
4. Download to `profilePicture.png` in working directory via `IFileDownloader`
5. Call `ISteamProcessor.SetProfilePicture` with local file path
6. Log success with source URL

**File Format:** One URL per line, UTF-8

### Private Helper

```csharp
private static string GetRandomEntryFromList(string listPath)
    => File.ReadAllLines(listPath).GetRandomElement();
```

- Uses `NuciExtensions.GetRandomElement()`
- Throws if file empty or not found (propagates `IOException`)

---

## IInfoGenerator / InfoGenerator

**Location:** `SteamProfileManager/Service/InfoGenerator.cs`

**Interface:** `SteamProfileManager.Service.IInfoGenerator`

**Implementation:** `SteamProfileManager.Service.InfoGenerator` (sealed)

### Constructor (Primary)

```csharp
public sealed class InfoGenerator : IInfoGenerator
```

No dependencies — stateless.

### Character Sets (Static)

```csharp
static string LowerCaseCharacters => "abcdefghijklmnopqrstuvwxyz";
static string UpperCaseCharacters => "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
static string NumericCharacters => "0123456789";
static string AllCharacters => $"{LowerCaseCharacters}{UpperCaseCharacters}{NumericCharacters}";
```

### Methods

#### GetRandomProfileName()

```csharp
public string GetRandomProfileName()
{
    string username = string.Empty;
    for (int i = 0; i < 12; i++)
    {
        username += AllCharacters.GetRandomElement();
    }
    return username;
}
```

**Returns:** 12-character alphanumeric string (a-z, A-Z, 0-9)

**Test:** `Does.Match("\\A[a-zA-Z0-9]{12}\\z")`

#### GetRandomProfileIdentifier()

```csharp
public string GetRandomProfileIdentifier()
{
    string identifier = string.Empty;
    for (int i = 0; i < 8; i++)
    {
        identifier += AllCharacters.GetRandomElement();
    }
    return identifier;
}
```

**Returns:** 8-character alphanumeric string (a-z, A-Z, 0-9)

**Test:** `Does.Match("\\A[a-zA-Z0-9]{8}\\z")`

---

## IFileDownloader / FileDownloader

**Location:** `SteamProfileManager/Service/FileDownloader.cs`

**Interface:** `SteamProfileManager.Service.IFileDownloader`

**Implementation:** `SteamProfileManager.Service.FileDownloader` (sealed)

### Constructor (Primary)

```csharp
public sealed class FileDownloader : IFileDownloader
```

No dependencies.

### Method

#### Download(string url, string outputPath)

```csharp
public void Download(string url, string outputPath)
    => new WebClient().DownloadFile(url, outputPath);
```

**Implementation:** `System.Net.WebClient` (synchronous, blocking)

**Parameters:**
- `url` — Source image URL
- `outputPath` — Local file path (e.g., `profilePicture.png`)

**Known Limitations:**
- `WebClient` is obsolete (use `HttpClient` instead)
- No timeout configuration
- No retry logic
- No progress reporting
- Blocks calling thread

**Errors:** Propagates `WebException` on network failure, `IOException` on file write failure

---

## ISteamProcessor (External)

**Package:** `NuciWeb.Steam` (v1.5.3)

**Interface:** `NuciWeb.Steam.ISteamProcessor`

**Implementation:** `NuciWeb.Steam.SteamProcessor` (internal to package)

### Methods Used

| Method | Parameters | Purpose |
|--------|------------|---------|
| `LogIn` | `SteamAccount` | Steam authentication (username, password, TOTP) |
| `RejectCookies` | — | Dismiss GDPR cookie banner |
| `SetProfileName` | `string` | Update profile display name |
| `SetProfileIdentifier` | `string` | Update custom URL identifier |
| `SetProfilePicture` | `string` (file path) | Upload profile picture from local file |

**All methods:** Synchronous, blocking, propagate exceptions on failure

**Dependencies:** Selenium WebDriver (via `NuciWeb.Automation.Selenium`)