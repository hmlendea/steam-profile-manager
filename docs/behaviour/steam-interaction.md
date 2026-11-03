# Behaviour — Steam Interaction

## Authentication Behaviour

### Login Flow

**Trigger:** `ProfileManager.LogIn()`

**Steps:**
1. Navigate to `https://steamcommunity.com/login`
2. Enter username
3. Enter password
4. Submit form
5. Detect 2FA challenge
6. Generate TOTP from `SharedSecret` (RFC 6238, 30s window, 6 digits)
7. Enter TOTP code
8. Submit 2FA form
9. Wait for redirect to logged-in state

**Success Criteria:**
- Redirect to profile/home page
- Session cookies established
- No error messages displayed

**Failure Modes:**
| Failure | Detection | Recovery |
|---------|-----------|----------|
| Wrong password | Error message on page | None — exception propagates |
| Wrong TOTP | Error message on 2FA page | None — exception propagates |
| Captcha | Captcha element present | None — manual intervention required |
| Rate limit | Steam error page | None — wait and retry manually |
| Network timeout | `TimeoutException` | None — crash |

### Cookie Rejection

**Trigger:** `ProfileManager.LogIn()` after successful login

**Steps:**
1. Detect GDPR cookie banner
2. Locate "Reject All" button
3. Click reject
4. Wait for banner dismissal

**Behaviour:**
- Silent success if banner not present
- Exception if button not found (propagates)

**Timing:** Must occur before profile mutations (banner may block interactions)

---

## Profile Mutation Behaviour

### Set Profile Name

**Trigger:** `ProfileManager.SetRandomProfileName()` when `RandomiseProfileName=true`

**Steps:**
1. Navigate to profile edit page
2. Locate display name input
3. Clear existing value
4. Enter new name
5. Click save
6. Wait for confirmation

**Constraints:**
- Max length: ~64 characters (Steam limit)
- No uniqueness requirement
- Can be changed anytime

**Name Source:**
- File-based: random line from `ProfileNamesList` file
- Generated: 12-char alphanumeric via `InfoGenerator`

**Failure Modes:**
| Failure | Detection | Recovery |
|---------|-----------|----------|
| Name too long | Steam validation error | None — crash |
| Element not found | `NoSuchElementException` | None — crash |
| Save timeout | `TimeoutException` | None — crash |

---

### Set Profile Identifier

**Trigger:** `ProfileManager.SetRandomProfileIdentifier()` when `RandomiseProfileIdentifier=true`

**Steps:**
1. Navigate to profile edit page
2. Locate custom URL input
3. Clear existing value
4. Enter new identifier
5. Client-side availability check (AJAX)
6. Click save
7. Wait for confirmation

**Constraints:**
- Length: 3-32 characters (generator produces 8)
- Alphanumeric only (generator enforces)
- Must be unique across Steam
- 30-day cooldown after change (Steam policy)

**Failure Modes:**
| Failure | Detection | Recovery |
|---------|-----------|----------|
| Identifier taken | Steam validation error | None — crash |
| 30-day cooldown | Steam error message | None — crash |
| Rate limited | Steam error | None — crash |
| Element not found | `NoSuchElementException` | None — crash |

**Critical Note:** 30-day cooldown means re-running within 30 days of last change will fail.

---

### Set Profile Picture

**Trigger:** `ProfileManager.SetRandomProfilePicture()` when `RandomiseProfilePicture=true`

**Steps:**
1. Determine picture URL (file or default)
2. Download image to `profilePicture.png`
3. Navigate to profile edit page
4. Locate file input (`<input type="file">`)
5. SendKeys(absolute file path)
6. Wait for upload + preview
7. Handle crop dialog (if present)
8. Click save/confirm
9. Wait for success

**Constraints:**
- Format: JPG, PNG, GIF
- Max size: ~5MB (Steam limit)
- Recommended: square, 184x184 minimum

**Failure Modes:**
| Failure | Detection | Recovery |
|---------|-----------|----------|
| Download failure | `WebException` | None — crash |
| File too large | Steam validation | None — crash |
| Invalid format | Steam validation | None — crash |
| Upload timeout | `TimeoutException` | None — crash |
| Crop dialog change | `NoSuchElementException` | None — crash |

---

## Timing Behaviour

### Page Load Timeout

**Setting:** `BotSettings.PageLoadTimeout` (default 90 seconds)

**Applied to:** All `driver.Navigate().GoToUrl()` calls

**Behaviour:** `TimeoutException` if page not loaded within timeout

### Implicit Waits

**Managed by:** `NuciWeb.Steam.SteamProcessor` (internal)

**Typical:** 5-10 seconds for element visibility

### Operation Durations

| Operation | Typical | Worst Case |
|-----------|---------|------------|
| Login | 8-15s | 30s+ (2FA, slow network) |
| Cookie rejection | 1-3s | 5s |
| Name update | 3-5s | 10s |
| Identifier update | 3-5s | 10s |
| Picture download | 1-5s | 30s+ (large file) |
| Picture upload | 5-10s | 30s+ (crop dialog) |

---

## Error Handling Behaviour

### Exception Propagation

All exceptions propagate from `ISteamProcessor` → `ProfileManager` → `Program.Main()` catch block.

**No retry logic** at any layer.

### Crash Screenshot

**Trigger:** Any exception in `Main()` try block

**Condition:** `DebugSettings.IsCrashScreenshotEnabled == true`

**Action:**
```csharp
if (webDriver is ITakesScreenshot screenshotDriver)
{
    Screenshot screenshot = screenshotDriver.GetScreenshot();
    screenshot.SaveAsFile(debugSettings.CrashScreenshotFileName);
}
```

**Output:** PNG file at configured path (default `crash.png`)

### Logging on Crash

```csharp
logger.Error(ex, "Application crashed");
```

**Includes:** Exception message, stack trace, operation context

---

## Concurrency Behaviour

**Single-threaded:** No parallelism

**Sequential execution:** Login → Name → Identifier → Picture

**No locking:** Not needed (single thread)

**WebDriver thread safety:** Not thread-safe, but single-threaded usage is safe

---

## State Management

### Session State

- **Authentication:** In-memory cookies (WebDriver session)
- **Persistence:** None — new session each run
- **Cleanup:** `webDriver.Quit()` in finally block

### File State

- **profilePicture.png:** Created/overwritten per run, not cleaned up
- **crash.png:** Created on crash, not cleaned up
- **logfile.log:** Appended per run (NuciLog)

### Configuration State

- **Immutable:** Settings loaded once at startup
- **No hot reload:** Changes require restart

---

## Bot Detection Considerations

### Risk Factors

| Factor | Headless | Debug |
|--------|----------|-------|
| Navigator.webdriver | `true` | `true` |
| Window size | Default | User-visible |
| Interaction speed | Fast (no delays) | Fast |
| IP reputation | Same | Same |

### Mitigations (Not Implemented)

- Random delays between actions
- Human-like mouse movements
- User-agent spoofing
- Proxy rotation

### Current Risk

Steam may detect automation and:
- Require captcha
- Temporarily lock account
- Trigger Steam Guard alerts

**Recommendation:** Use debug mode for initial testing, monitor logs for anomalies.