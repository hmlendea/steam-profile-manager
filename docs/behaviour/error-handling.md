# Behaviour — Error Handling

## Exception Hierarchy

```
Exception
├─► WebDriverException (Selenium)
│   ├─► NoSuchElementException
│   ├─► TimeoutException
│   ├─► StaleElementReferenceException
│   └─► ElementClickInterceptedException
├─► WebException (System.Net)
├─► IOException (System.IO)
├─► ArgumentException / ArgumentNullException
└─► InvalidOperationException
```

## Handling Strategy

### No Local Try/Catch

**Pattern:** Exceptions propagate unhandled from service methods to `Program.Main()`

```csharp
// ProfileManager methods — no try/catch
public void LogIn() { ... }
public void SetRandomProfileName() { ... }

// Program.Main — single catch-all
try
{
    LoadConfiguration();
    webDriver = WebDriverInitialiser.InitialiseAvailableWebDriver(...);
    serviceProvider = CreateIOC();
    logger = serviceProvider.GetRequiredService<ILogger>();
    logger.Info(Operation.StartUp, "Application started");
    RunApplication();
}
catch (Exception ex)
{
    logger.Error(ex, "Application crashed");
    if (debugSettings.IsCrashScreenshotEnabled && webDriver is ITakesScreenshot ss)
    {
        ss.GetScreenshot().SaveAsFile(debugSettings.CrashScreenshotFileName);
    }
    throw;
}
finally
{
    webDriver?.Quit();
    logger.Info(Operation.ShutDown, "Application stopped");
}
```

### Rationale

- Console application — crash is acceptable failure mode
- Crash screenshot captures browser state for debugging
- Structured logging provides context
- No retry logic — transient failures not expected to self-resolve

---

## Specific Error Scenarios

### Configuration Errors

| Error | Source | Handling |
|-------|--------|----------|
| Missing appsettings.json | `ConfigurationBuilder.Build()` | Crash — no screenshot (WebDriver not init) |
| Malformed JSON | `ConfigurationBuilder.Build()` | Crash — no screenshot |
| Missing section | `config.Bind()` | Uses defaults (POCO constructor) |
| Invalid type in JSON | `config.Bind()` | Crash — binding exception |

### WebDriver Initialisation Errors

| Error | Cause | Handling |
|-------|-------|----------|
| No browser installed | `WebDriverInitialiser` | Crash — no screenshot |
| Driver version mismatch | Browser updated, driver old | Crash — no screenshot |
| Port conflict | Another WebDriver running | Crash — no screenshot |

**Note:** WebDriver created before DI, so no logger available for these failures.

### Login Errors

| Error | Exception | Screenshot | Log Context |
|-------|-----------|------------|-------------|
| Invalid credentials | `WebDriverException` | Yes | Username |
| Invalid TOTP | `WebDriverException` | Yes | Username |
| Captcha | `WebDriverException` | Yes | Username |
| Element not found | `NoSuchElementException` | Yes | Username |
| Page load timeout | `TimeoutException` | Yes | Username |

### Profile Mutation Errors

| Operation | Error | Exception | Screenshot | Log Context |
|-----------|-------|-----------|------------|-------------|
| Name | Name too long | `WebDriverException` | Yes | ProfileName |
| Name | Element missing | `NoSuchElementException` | Yes | ProfileName |
| Identifier | Taken | `WebDriverException` | Yes | ProfileIdentifier |
| Identifier | Cooldown | `WebDriverException` | Yes | ProfileIdentifier |
| Picture | Download fail | `WebException` | Yes | ProfilePicture |
| Picture | Upload fail | `WebDriverException` | Yes | ProfilePicture |
| Picture | Crop dialog | `NoSuchElementException` | Yes | ProfilePicture |

---

## Crash Screenshot Behaviour

### Trigger Conditions

```csharp
if (debugSettings.IsCrashScreenshotEnabled && webDriver is ITakesScreenshot ss)
```

**Requirements:**
1. `DebugSettings.CrashScreenshotFileName` non-empty
2. `webDriver` not null (initialised)
3. `webDriver` implements `ITakesScreenshot`

### Capture Process

```csharp
Screenshot screenshot = ss.GetScreenshot();
screenshot.SaveAsFile(debugSettings.CrashScreenshotFileName);
```

**Format:** PNG
**Location:** Working directory (or configured path)
**Overwrites:** Existing file with same name

### Limitations

- Only captures visible viewport
- May not capture full page if scrolled
- Fails if browser already closed
- No metadata (timestamp, URL, exception) in image

---

## Logging on Error

### Error Log Entry

```csharp
logger.Error(ex, "Application crashed");
```

**NuciLog Output Includes:**
- Timestamp
- Level: Error
- Message: "Application crashed"
- Exception: Type, message, stack trace
- Operation context (if in operation scope)

### Operation Context

If error occurs within logged operation:
```csharp
logger.Info(MyOperation.LogIn, OperationStatus.Started, ...);
// ... exception thrown ...
// No OperationStatus.Failed logged (exception propagates)
```

**Gap:** No automatic `Failed` status log — only `Started` then crash.

---

## Resource Cleanup on Error

### WebDriver

```csharp
finally
{
    webDriver?.Quit();
}
```

- Always executes (success or failure)
- `Quit()` closes all windows, ends session
- Safe to call on already-quit driver

### Logger

```csharp
finally
{
    logger.Info(Operation.ShutDown, "Application stopped");
}
```

- Logs shutdown even after crash
- NuciLog flushes buffers

### File Handles

- `WebClient` in `FileDownloader` — not disposed (static method)
- `profilePicture.png` — remains on disk
- `crash.png` — remains on disk
- `logfile.log` — NuciLog manages

---

## Missing Error Handling

### Not Implemented

| Scenario | Current Behaviour | Recommended |
|----------|-------------------|-------------|
| Transient network failure | Crash | Retry with backoff |
| Steam rate limiting | Crash | Wait and retry |
| Captcha challenge | Crash | Manual intervention hook |
| Partial success (login ok, name fail) | Crash, no rollback | Continue or compensate |
| Config validation | Runtime crash | Startup validation |
| File download timeout | Infinite block | HttpClient with timeout |
| Concurrent runs | Undefined | Lock file / mutex |

### Known Gaps

1. **No retry logic** — Any transient failure crashes
2. **No partial rollback** — If name changes but identifier fails, name stays changed
3. **No config validation** — Invalid settings discovered at use time
4. **No health checks** — No pre-flight verification
5. **No circuit breaker** — Repeated failures not detected

---

## Testing Error Paths

### Unit Tests (ProfileManagerTests)

```csharp
[Test]
public void LogIn_Throws_WhenSteamProcessorThrows()
{
    mockSteamProcessor.Setup(x => x.LogIn(It.IsAny<SteamAccount>()))
        .Throws<WebDriverException>();

    Assert.Throws<WebDriverException>(() => profileManager.LogIn());
}
```

**Coverage:** Verifies exceptions propagate, logging called

### Integration Tests

None — requires live Steam account, browser, network

### Manual Testing

1. Invalid credentials → Verify crash screenshot, log
2. Valid credentials, wrong TOTP → Verify crash
3. Network disconnect during login → Verify timeout handling
4. Steam UI change → Verify element not found handling

---

## Operational Recommendations

### Monitoring

- Watch `logfile.log` for `Error` entries
- Check for `crash.png` in working directory
- Monitor exit codes (non-zero = failure)

### Debugging

1. Enable `isDebugMode: true` for visible browser
2. Set `crashScreenshotFileName: "crash.png"`
3. Set `minimumLevel: "Debug"` for verbose logs
4. Re-run and observe browser interaction

### Recovery

1. Check `logfile.log` for error details
2. View `crash.png` for browser state
3. Fix root cause (credentials, network, Steam UI)
4. Re-run