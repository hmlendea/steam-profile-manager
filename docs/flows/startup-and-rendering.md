# Flows — Startup and Rendering

## Application Bootstrap Sequence

```
Main()
  │
  ├─► LoadConfiguration()
  │     ├─► new BotSettings()
  │     ├─► new DebugSettings()
  │     ├─► new NuciLoggerSettings()
  │     └─► ConfigurationBuilder → appsettings.json → Bind()
  │
  ├─► WebDriverInitialiser.InitialiseAvailableWebDriver()
  │     ├─► Detect available browser (Chrome/Chromium/Firefox)
  │     ├─► Configure options (headless vs debug)
  │     └─► Set page load timeout
  │
  ├─► CreateIOC()
  │     ├─► ServiceCollection
  │     ├─► Register singletons (settings, WebDriver)
  │     ├─► Register transients (services)
  │     ├─► AddNuciLog(loggerSettings)
  │     └─► BuildServiceProvider()
  │
  ├─► Resolve ILogger
  │     └─► logger.Info(Operation.StartUp, "Application started")
  │
  └─► RunApplication()
        └─► Resolve IProfileManager → Execute profile operations
```

## LoadConfiguration() Detail

**Location:** `Program.cs`

```csharp
static void LoadConfiguration()
{
    botSettings = new BotSettings();
    debugSettings = new DebugSettings();
    loggerSettings = new NuciLoggerSettings();

    IConfiguration config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", true, true)
        .Build();

    config.Bind(nameof(BotSettings), botSettings);
    config.Bind(nameof(DebugSettings), debugSettings);
    config.Bind(nameof(NuciLoggerSettings), loggerSettings);
}
```

**Steps:**
1. Instantiate empty settings objects (provides defaults)
2. Build configuration from `appsettings.json` (optional, reloadable)
3. Bind each section to corresponding POCO

**Error Handling:** None — missing file or malformed JSON throws at `Build()` or `Bind()`

## WebDriver Initialisation Detail

**Location:** `Program.cs` → `Main()` after `LoadConfiguration()`

```csharp
webDriver = WebDriverInitialiser.InitialiseAvailableWebDriver(
    debugSettings.IsDebugMode,
    botSettings.PageLoadTimeout);
```

**External:** `NuciWeb.Automation.Selenium.WebDriverInitialiser`

**Internal Flow (inferred):**
1. Detect installed browsers (Chrome, Chromium, Firefox)
2. Create appropriate `DriverOptions`
3. If `isDebugMode == false`: Add headless arguments
4. Set `PageLoadTimeout`
5. Instantiate `IWebDriver`
6. Return driver instance

**Failure Modes:**
- No supported browser installed → Exception
- Driver version mismatch → Exception
- Timeout during startup → Exception

## DI Container Composition Detail

**Location:** `Program.cs` → `CreateIOC()`

```csharp
static IServiceProvider CreateIOC()
{
    IServiceCollection services = new ServiceCollection();

    services.AddSingleton(botSettings);
    services.AddSingleton(debugSettings);
    services.AddSingleton(loggerSettings);
    services.AddSingleton(webDriver);
    services.AddTransient<IProfileManager, ProfileManager>();
    services.AddTransient<IInfoGenerator, InfoGenerator>();
    services.AddTransient<IFileDownloader, FileDownloader>();
    services.AddTransient<ISteamProcessor, SteamProcessor>();
    services.AddNuciLog(loggerSettings);

    return services.BuildServiceProvider();
}
```

**Registration Order:** Settings → WebDriver → Services → Logging

**Resolution Graph:**
```
ServiceProvider
  ├─► BotSettings (singleton)
  ├─► DebugSettings (singleton)
  ├─► NuciLoggerSettings (singleton)
  ├─► IWebDriver (singleton instance)
  ├─► ILogger (singleton from NuciLog)
  ├─► IProfileManager (transient)
  │     ├─► IInfoGenerator (transient)
  │     ├─► IFileDownloader (transient)
  │     ├─► ISteamProcessor (transient)
  │     ├─► BotSettings (singleton)
  │     └─► ILogger (singleton)
  ├─► IInfoGenerator (transient)
  ├─► IFileDownloader (transient)
  └─► ISteamProcessor (transient)
       └─► IWebDriver (singleton)
```

## Logger Initialisation

**NuciLog Registration:**
```csharp
services.AddNuciLog(loggerSettings);
```

**Logger Resolution:**
```csharp
logger = serviceProvider.GetRequiredService<ILogger>();
logger.Info(Operation.StartUp, "Application started");
```

**Log Output:** Console + optional file (`logfile.log`)

## RunApplication() Flow

```csharp
static void RunApplication()
{
    IProfileManager profileManager = serviceProvider.GetRequiredService<IProfileManager>();

    profileManager.LogIn();

    if (botSettings.RandomiseProfileName)
        profileManager.SetRandomProfileName();

    if (botSettings.RandomiseProfileIdentifier)
        profileManager.SetRandomProfileIdentifier();

    if (botSettings.RandomiseProfilePicture)
        profileManager.SetRandomProfilePicture();
}
```

**Sequence:**
1. Resolve `IProfileManager` (creates transient instance with all dependencies)
2. `LogIn()` — mandatory, always executed
3. Conditional profile updates based on flags

**Error Handling:** None in `RunApplication()` — exceptions propagate to `Main()` try/catch

## Startup Timing

| Phase | Approximate Duration |
|-------|---------------------|
| Configuration loading | < 100ms |
| WebDriver startup | 2-10s (browser dependent) |
| DI container build | < 50ms |
| Logger init | < 50ms |
| Login (Steam) | 5-30s (network, 2FA) |
| Profile operations | 2-10s each |

## Failure Points

| Phase | Failure Mode | Handling |
|-------|--------------|----------|
| Config | Missing/malformed appsettings.json | Crash, no screenshot |
| WebDriver | No browser, version mismatch | Crash, no screenshot |
| DI | Missing registration | Crash (GetRequiredService throws) |
| Login | Invalid credentials, 2FA failure | Crash → screenshot if enabled |
| Profile ops | Element not found, timeout | Crash → screenshot if enabled |

## Cleanup

```csharp
finally
{
    webDriver?.Quit();
    logger.Info(Operation.ShutDown, "Application stopped");
}
```

- Always executes (success or failure)
- `webDriver.Quit()` closes browser
- Logs shutdown via NuciLog