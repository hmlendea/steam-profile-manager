# API Reference — Service Interfaces

## IProfileManager

**Namespace:** `SteamProfileManager.Service`

**Assembly:** `SteamProfileManager`

### Definition

```csharp
public interface IProfileManager
{
    void LogIn();
    void SetRandomProfileName();
    void SetRandomProfileIdentifier();
    void SetRandomProfilePicture();
}
```

### Methods

#### LogIn()

```csharp
void LogIn();
```

**Description:** Authenticates with Steam using configured credentials and handles 2FA.

**Preconditions:**
- `BotSettings.SteamAccount` populated with valid credentials
- WebDriver initialised and available

**Postconditions:**
- Steam session established
- Cookies rejected (GDPR banner dismissed)

**Exceptions:**
- `WebDriverException` — Browser/navigation failure
- `NoSuchElementException` — Steam UI changed
- `TimeoutException` — Page load timeout
- Any exception from `ISteamProcessor.LogIn()` or `RejectCookies()`

**Logging:**
- `MyOperation.LogIn`, `Started` — Username
- `MyOperation.LogIn`, `Success` — Username

---

#### SetRandomProfileName()

```csharp
void SetRandomProfileName();
```

**Description:** Updates Steam profile display name to random value.

**Preconditions:**
- `LogIn()` completed successfully
- `BotSettings.RandomiseProfileName == true` (checked by caller)

**Behaviour:**
- If `ProfileNamesList` configured: reads random line from file
- Else: generates 12-char alphanumeric via `IInfoGenerator`

**Exceptions:**
- `IOException` — File read failure
- `WebDriverException` — Browser/navigation failure
- `NoSuchElementException` — Steam UI changed
- `TimeoutException` — Page load timeout

**Logging:**
- `MyOperation.SetProfileName`, `Started`
- `MyOperation.SetProfileName`, `Success` — ProfileName

---

#### SetRandomProfileIdentifier()

```csharp
void SetRandomProfileIdentifier();
```

**Description:** Updates Steam profile custom URL identifier to random value.

**Preconditions:**
- `LogIn()` completed successfully
- `BotSettings.RandomiseProfileIdentifier == true` (checked by caller)
- 30 days since last identifier change (Steam policy)

**Behaviour:**
- Always generates 8-char alphanumeric via `IInfoGenerator`

**Exceptions:**
- `WebDriverException` — Browser/navigation failure
- `NoSuchElementException` — Steam UI changed
- `TimeoutException` — Page load timeout
- Steam-side: identifier taken, rate limited

**Logging:**
- `MyOperation.SetProfileIdentifier`, `Started`
- `MyOperation.SetProfileIdentifier`, `Success` — ProfileIdentifier

---

#### SetRandomProfilePicture()

```csharp
void SetRandomProfilePicture();
```

**Description:** Downloads and uploads a random profile picture.

**Preconditions:**
- `LogIn()` completed successfully
- `BotSettings.RandomiseProfilePicture == true` (checked by caller)

**Behaviour:**
- If `ProfilePicturesList` configured: reads random URL from file
- Else: uses default URL (`https://i.imgur.com/rV2RQMO.png`)
- Downloads to `profilePicture.png` in working directory
- Uploads via `ISteamProcessor.SetProfilePicture()`

**Exceptions:**
- `IOException` — File read failure
- `WebException` — Download failure
- `WebDriverException` — Browser/navigation failure
- `NoSuchElementException` — Steam UI changed
- `TimeoutException` — Page load/upload timeout

**Logging:**
- `MyOperation.SetProfilePicture`, `Started`
- `MyOperation.SetProfilePicture`, `Success` — ProfilePicture (source URL)

---

## IInfoGenerator

**Namespace:** `SteamProfileManager.Service`

**Assembly:** `SteamProfileManager`

### Definition

```csharp
public interface IInfoGenerator
{
    string GetRandomProfileName();
    string GetRandomProfileIdentifier();
}
```

### Methods

#### GetRandomProfileName()

```csharp
string GetRandomProfileName();
```

**Returns:** 12-character string matching regex `^[a-zA-Z0-9]{12}$`

**Character Set:** `a-z`, `A-Z`, `0-9` (62 characters)

**Algorithm:** 12 iterations of `AllCharacters.GetRandomElement()` (NuciExtensions)

**Thread Safety:** Stateless, thread-safe

**Determinism:** Non-deterministic (uses random)

---

#### GetRandomProfileIdentifier()

```csharp
string GetRandomProfileIdentifier();
```

**Returns:** 8-character string matching regex `^[a-zA-Z0-9]{8}$`

**Character Set:** `a-z`, `A-Z`, `0-9` (62 characters)

**Algorithm:** 8 iterations of `AllCharacters.GetRandomElement()` (NuciExtensions)

**Thread Safety:** Stateless, thread-safe

**Determinism:** Non-deterministic (uses random)

---

## IFileDownloader

**Namespace:** `SteamProfileManager.Service`

**Assembly:** `SteamProfileManager`

### Definition

```csharp
public interface IFileDownloader
{
    void Download(string url, string outputPath);
}
```

### Methods

#### Download(string url, string outputPath)

```csharp
void Download(string url, string outputPath);
```

**Parameters:**
- `url` — Source image URL (HTTP/HTTPS)
- `outputPath` — Local file path for saved image

**Implementation:** `System.Net.WebClient.DownloadFile()` (synchronous, blocking)

**Behaviour:**
- Creates/overwrites file at `outputPath`
- No timeout
- No retry
- No progress reporting

**Exceptions:**
- `WebException` — Network error, HTTP error (4xx, 5xx)
- `IOException` — File system error (permissions, disk full)
- `ArgumentNullException` — Null url or outputPath
- `ArgumentException` — Invalid URL format

**Thread Safety:** Not thread-safe (WebClient instance per call)

**Obsolescence:** `WebClient` is obsolete; consider `HttpClient` for new code.

---

## ISteamProcessor (External)

**Package:** `NuciWeb.Steam` (v1.5.3)

**Namespace:** `NuciWeb.Steam`

### Definition

```csharp
public interface ISteamProcessor
{
    void LogIn(SteamAccount account);
    void RejectCookies();
    void SetProfileName(string name);
    void SetProfileIdentifier(string identifier);
    void SetProfilePicture(string filePath);
}
```

### Methods

#### LogIn(SteamAccount account)

```csharp
void LogIn(SteamAccount account);
```

**Parameters:** `account` — Steam credentials including TOTP secret

**Behaviour:** Full Steam authentication flow with 2FA

**Exceptions:** `WebDriverException`, `NoSuchElementException`, `TimeoutException`

---

#### RejectCookies()

```csharp
void RejectCookies();
```

**Behaviour:** Dismisses GDPR cookie consent banner

**Exceptions:** `NoSuchElementException` (if banner not found), `WebDriverException`

---

#### SetProfileName(string name)

```csharp
void SetProfileName(string name);
```

**Parameters:** `name` — New display name (max ~64 chars)

**Behaviour:** Navigates to profile edit, updates name, saves

**Exceptions:** `WebDriverException`, `NoSuchElementException`, `TimeoutException`

---

#### SetProfileIdentifier(string identifier)

```csharp
void SetProfileIdentifier(string identifier);
```

**Parameters:** `identifier` — New custom URL identifier (alphanumeric, 3-32 chars)

**Behaviour:** Navigates to profile edit, updates identifier, saves

**Exceptions:** `WebDriverException`, `NoSuchElementException`, `TimeoutException`
Steam-side: identifier taken, 30-day cooldown

---

#### SetProfilePicture(string filePath)

```csharp
void SetProfilePicture(string filePath);
```

**Parameters:** `filePath` — Absolute path to local image file

**Behaviour:** Navigates to profile edit, uploads file via `<input type="file">`, handles crop, saves

**Exceptions:** `WebDriverException`, `NoSuchElementException`, `TimeoutException`
Steam-side: file too large, invalid format

---

## NuciLog ILogger (External)

**Package:** `NuciLog.Core` (v3.1.0)

**Interface:** `NuciLog.ILogger`

### Key Methods Used

```csharp
void Info(Operation operation, OperationStatus status, params LogInfo[] infos);
void Error(Exception exception, string message);
```

### Custom Types Used

| Type | Source | Values |
|------|--------|--------|
| `Operation` | `NuciLog` | `StartUp`, `ShutDown`, `MyOperation.*` |
| `OperationStatus` | `NuciLog` | `Started`, `Success`, `Failed` |
| `LogInfo` | `NuciLog` | Key-value pairs via `MyLogInfoKey.*` |

### MyOperation (Local)

```csharp
static class MyOperation
{
    static Operation LogIn { get; }
    static Operation SetProfileName { get; }
    static Operation SetProfileIdentifier { get; }
    static Operation SetProfilePicture { get; }
}
```

### MyLogInfoKey (Local)

```csharp
static class MyLogInfoKey
{
    static LogInfoKey Username { get; }
    static LogInfoKey ProfileName { get; }
    static LogInfoKey ProfileIdentifier { get; }
    static LogInfoKey ProfilePicture { get; }
}
```

---

## Dependency Injection Registration

**Location:** `Program.CreateIOC()`

```csharp
services.AddTransient<IProfileManager, ProfileManager>();
services.AddTransient<IInfoGenerator, InfoGenerator>();
services.AddTransient<IFileDownloader, FileDownloader>();
services.AddTransient<ISteamProcessor, SteamProcessor>();
```

**Lifetimes:** All transient (new instance per resolve)

**Resolution:** `IProfileManager` resolved in `RunApplication()`, others injected via constructor.