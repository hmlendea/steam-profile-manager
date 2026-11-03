# Flows — Calendar Search (Steam Login and Cookie Handling)

## Overview

This flow covers the Steam authentication process and GDPR cookie banner handling, executed via `ProfileManager.LogIn()` → `ISteamProcessor.LogIn()` + `RejectCookies()`.

## Sequence Diagram

```
ProfileManager.LogIn()
  │
  ├─► logger.Info(MyOperation.LogIn, Started, Username)
  │
  ├─► ISteamProcessor.LogIn(SteamAccount)
  │     │
  │     ├─► Navigate to https://steamcommunity.com/login
  │     │
  │     ├─► Wait for login form
  │     │
  │     ├─► Enter username
  │     │
  │     ├─► Enter password
  │     │
  │     ├─► Submit form
  │     │
  │     ├─► Detect 2FA challenge (Steam Guard)
  │     │     ├─► Generate TOTP from SharedSecret
  │     │     ├─► Enter TOTP code
  │     │     └─► Submit 2FA form
  │     │
  │     ├─► Wait for redirect to profile/home
  │     │
  │     └─► Return (success) or throw
  │
  ├─► ISteamProcessor.RejectCookies()
  │     │
  │     ├─► Locate GDPR cookie banner
  │     ├─► Find "Reject All" / "Reject" button
  │     ├─► Click reject
  │     └─► Wait for banner dismissal
  │
  └─► logger.Info(MyOperation.LogIn, Success, Username)
```

## ISteamProcessor.LogIn(SteamAccount) Detail

**Package:** `NuciWeb.Steam` (v1.5.3)

### Input: SteamAccount

| Field | Source | Used For |
|-------|--------|----------|
| `Username` | `botSettings.SteamAccount.Username` | Login form username field |
| `Password` | `botSettings.SteamAccount.Password` | Login form password field |
| `SharedSecret` | `botSettings.SteamAccount.SharedSecret` | TOTP generation for 2FA |
| `IdentitySecret` | `botSettings.SteamAccount.IdentitySecret` | Trade confirmations (not used in login) |
| `DeviceId` | `botSettings.SteamAccount.DeviceId` | Device identification (optional) |

### Steps

1. **Navigate to Login Page**
   - URL: `https://steamcommunity.com/login`
   - Wait for page load (PageLoadTimeout)

2. **Enter Credentials**
   - Locate username input (`#input_username` or similar)
   - SendKeys(username)
   - Locate password input (`#input_password` or similar)
   - SendKeys(password)

3. **Submit Login Form**
   - Click submit button (`#login_btn_signin` or similar)
   - Wait for navigation

4. **Handle Steam Guard (2FA)**
   - Detect 2FA page (URL contains `/twofactor/` or element present)
   - Generate TOTP: `TotpGenerator.Generate(SharedSecret)` (RFC 6238, 30s window, 6 digits)
   - Locate 2FA input (`#twofactorcode_entry` or similar)
   - SendKeys(totpCode)
   - Click submit (`#login_twofactorauth_buttonset_entercode` or similar)
   - Wait for redirect

5. **Verify Success**
   - Check for logged-in state (profile menu, username visible)
   - Return or throw on failure

### Failure Modes

| Failure | Exception | Cause |
|---------|-----------|-------|
| Invalid credentials | `WebDriverException` / custom | Wrong username/password |
| 2FA code invalid | `WebDriverException` | Clock skew, wrong secret |
| Element not found | `NoSuchElementException` | Steam UI changed |
| Timeout | `TimeoutException` | Slow network, page load |
| Captcha | `WebDriverException` | Steam bot detection |

## ISteamProcessor.RejectCookies() Detail

### Purpose

Dismiss GDPR cookie consent banner that appears on first visit.

### Steps

1. **Locate Banner**
   - Wait for cookie banner element (typically `#cookie_banner` or `.cookie-consent`)
   - Timeout: few seconds

2. **Find Reject Button**
   - Locate "Reject All" or "Reject" button
   - Selector: button with text "Reject" / "Reject All" / "Decline"

3. **Click Reject**
   - Click button
   - Wait for banner to disappear

4. **Verify Dismissal**
   - Confirm banner no longer visible

### Failure Modes

| Failure | Handling |
|---------|----------|
| Banner not present | Silent success (already dismissed or not shown) |
| Button not found | Exception → propagates to caller |
| Click intercepted | Exception → propagates |

**Note:** No retry logic. If banner handling fails, login still succeeded.

## Logging

### Start
```csharp
logger.Info(MyOperation.LogIn, OperationStatus.Started,
    new LogInfo(MyLogInfoKey.Username, botSettings.SteamAccount.Username));
```

### Success
```csharp
logger.Info(MyOperation.LogIn, OperationStatus.Success,
    new LogInfo(MyLogInfoKey.Username, botSettings.SteamAccount.Username));
```

### Structured Fields
- `Operation`: `MyOperation.LogIn`
- `Status`: `Started` → `Success` (or exception)
- `Username`: Steam username

## Timing

| Step | Typical Duration |
|------|-----------------|
| Navigate to login | 1-3s |
| Enter credentials | < 1s |
| Form submit + redirect | 2-5s |
| 2FA challenge | 3-10s (includes TOTP generation) |
| Post-login redirect | 1-3s |
| Cookie banner | 1-3s |
| **Total** | **8-25s** |

## Configuration Dependencies

| Setting | Used In |
|---------|---------|
| `BotSettings.PageLoadTimeout` | All page loads, waits |
| `BotSettings.SteamAccount.Username` | Login form |
| `BotSettings.SteamAccount.Password` | Login form |
| `BotSettings.SteamAccount.SharedSecret` | TOTP generation |
| `DebugSettings.IsDebugMode` | Headless vs visible (affects bot detection) |

## Error Recovery

**None implemented.** Exceptions propagate to `Program.Main()` catch block:

```csharp
catch (Exception ex)
{
    logger.Error(ex, "Application crashed");
    if (debugSettings.IsCrashScreenshotEnabled && webDriver is ITakesScreenshot ss)
    {
        ss.GetScreenshot().SaveAsFile(debugSettings.CrashScreenshotFileName);
    }
    throw;
}
```

## Security Notes

- Credentials in memory only (no persistence)
- TOTP generated client-side from `SharedSecret`
- `SharedSecret` must be kept secure (Base32 encoded)
- No credential logging (only username in logs)
- WebDriver session cleaned up on exit