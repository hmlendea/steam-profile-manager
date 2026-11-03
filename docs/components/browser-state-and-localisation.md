# Components — Browser State and Localisation

## Selenium WebDriver Integration

**Package:** `NuciWeb.Automation.Selenium` (v1.0.3)

**Initialisation:** `WebDriverInitialiser.InitialiseAvailableWebDriver(bool isDebugMode, int pageLoadTimeout)`

### WebDriver Initialisation

**Location:** `Program.cs` → `Main()` → `LoadConfiguration()` → `WebDriverInitialiser.InitialiseAvailableWebDriver()`

```csharp
webDriver = WebDriverInitialiser.InitialiseAvailableWebDriver(
    debugSettings.IsDebugMode,
    botSettings.PageLoadTimeout);
```

**Parameters:**
| Parameter | Source | Description |
|-----------|--------|-------------|
| `isDebugMode` | `DebugSettings.IsDebugMode` | `false` = headless, `true` = visible browser |
| `pageLoadTimeout` | `BotSettings.PageLoadTimeout` | Page load timeout in seconds |

**Returns:** `OpenQA.Selenium.IWebDriver` (Chrome, Chromium, or Firefox based on system availability)

### Browser Modes

| Mode | Condition | Behaviour |
|------|-----------|-----------|
| Headless | `IsDebugMode == false` | No visible UI, suitable for servers/CI |
| Debug/Visible | `IsDebugMode == true` | Full browser UI for debugging |

**Implementation:** `NuciWeb.Automation.Selenium.WebDriverInitialiser` handles browser detection and option configuration.

### Page Load Timeout

```csharp
webDriver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(pageLoadTimeout);
```

Applied during WebDriver initialisation. Default from config: 90 seconds.

### WebDriver Lifecycle

| Phase | Code | Location |
|-------|------|----------|
| Create | `WebDriverInitialiser.InitialiseAvailableWebDriver()` | `Program.Main` |
| Use | Injected into `SteamProcessor` (NuciWeb.Steam) | `ProfileManager` → `ISteamProcessor` |
| Dispose | `webDriver?.Quit()` | `Program.Main` finally block |

**Note:** `Quit()` closes all windows and ends session. `Dispose()` not called explicitly.

## Steam Website Interaction (NuciWeb.Steam)

**Package:** `NuciWeb.Steam` (v1.5.3)

**Interface:** `ISteamProcessor`

**Implementation:** Internal to package, uses injected `IWebDriver`

### Key Interactions

| Operation | Method | Description |
|-----------|--------|-------------|
| Login | `LogIn(SteamAccount)` | Navigates to Steam login, enters credentials, handles 2FA via TOTP |
| Cookie Rejection | `RejectCookies()` | Clicks "Reject" on GDPR cookie banner |
| Set Profile Name | `SetProfileName(string)` | Navigates to profile edit, updates display name |
| Set Profile Identifier | `SetProfileIdentifier(string)` | Navigates to profile edit, updates custom URL |
| Set Profile Picture | `SetProfilePicture(string filePath)` | Uploads local image file as profile picture |

### DOM Interaction Patterns

All interactions use Selenium WebDriver commands:
- Element location: CSS selectors, XPath
- Waits: Implicit/explicit waits for element visibility
- Actions: Click, SendKeys, File upload via `<input type="file">`
- Navigation: `driver.Navigate().GoToUrl()`

### Error Handling

- Exceptions propagate to caller (`ProfileManager` → `Program.Main`)
- No retry logic in NuciWeb.Steam
- Caller handles via try/catch in `Program.Main`

## Localisation

**Current:** No localisation support. All Steam interactions assume English UI.

**Steam UI Language:** Determined by Steam account settings or browser Accept-Language header.

**Known Limitation:** If Steam serves non-English UI, element selectors may fail.

**Mitigation:** Could force English via URL parameter (`?l=english`) or browser language preference.

## Browser State Management

### Cookies

- **Authentication cookies:** Managed by Selenium WebDriver session
- **Session persistence:** None — new WebDriver session each run
- **Cookie rejection:** Explicit `RejectCookies()` call after login

### Cache

- **Browser cache:** Not persisted between runs (new profile each session)
- **Image cache:** `profilePicture.png` downloaded fresh each run

### Local Storage / Session Storage

- Not explicitly managed
- Cleared with WebDriver session

## Window/Tab Management

- Single browser window
- No tab switching logic
- All navigation in same tab

## Screenshots

### Crash Screenshots

**Enabled when:** `DebugSettings.CrashScreenshotFileName` is non-empty

**Capture:**
```csharp
if (webDriver is ITakesScreenshot screenshotDriver)
{
    Screenshot screenshot = screenshotDriver.GetScreenshot();
    screenshot.SaveAsFile(debugSettings.CrashScreenshotFileName);
}
```

**Format:** PNG
**Location:** Working directory (configurable filename, default `crash.png`)

### Profile Picture (Temporary)

**File:** `profilePicture.png` in working directory
**Lifecycle:** Created per run, overwritten, not cleaned up
**Upload:** Passed to `ISteamProcessor.SetProfilePicture(filePath)`

## Headless Mode Considerations

| Aspect | Headless | Debug |
|--------|----------|-------|
| Visibility | No UI | Full UI |
| Performance | Faster | Slower |
| Debugging | Difficult | Easy (visual) |
| CI/CD | Suitable | Not suitable |
| Steam Detection | May trigger bot detection | Less likely |

**Recommendation:** Use headless for production, debug mode for troubleshooting.

## WebDriver Version Compatibility

- Managed by `NuciWeb.Automation.Selenium` package
- Compatible with installed browser versions
- No explicit version pinning in project