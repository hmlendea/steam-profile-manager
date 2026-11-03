# Components — Host and Composition

## DI Container Setup

**Location:** `SteamProfileManager/Program.cs` → `CreateIOC()`

**Container:** `Microsoft.Extensions.DependencyInjection.ServiceCollection`

### Registrations

| Service | Lifetime | Implementation | Notes |
|---------|----------|----------------|-------|
| `BotSettings` | Singleton | Configuration instance | Bound from appsettings.json |
| `DebugSettings` | Singleton | Configuration instance | Bound from appsettings.json |
| `NuciLoggerSettings` | Singleton | Configuration instance | Bound from appsettings.json |
| `IWebDriver` | Singleton | Selenium WebDriver | Created before DI, registered as instance |
| `IProfileManager` | Transient | `ProfileManager` | Primary constructor injection |
| `IInfoGenerator` | Transient | `InfoGenerator` | Primary constructor injection |
| `IFileDownloader` | Transient | `FileDownloader` | Primary constructor injection |
| `ISteamProcessor` | Transient | `SteamProcessor` (NuciWeb.Steam) | From NuciWeb.Steam package |
| `ILogger` | Singleton | NuciLog | Via `AddNuciLog` extension |

### Registration Code

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

## WebDriver Lifecycle

### Initialisation

```csharp
webDriver = WebDriverInitialiser.InitialiseAvailableWebDriver(
    debugSettings.IsDebugMode,
    botSettings.PageLoadTimeout);
```

**Location:** `NuciWeb.Automation.Selenium.WebDriverInitialiser` (external package)

**Parameters:**
- `isDebugMode` (bool) — `false` = headless, `true` = visible browser
- `pageLoadTimeout` (int) — Seconds, from `BotSettings.PageLoadTimeout`

**Returns:** `IWebDriver` (Chrome/Chromium/Firefox based on availability)

### Disposal

```csharp
finally
{
    webDriver?.Quit();
    logger.Info(Operation.ShutDown, "Application stopped");
}
```

- Called in `finally` block to ensure cleanup on success or failure
- `Quit()` closes all browser windows and ends WebDriver session
- Logged as `Operation.ShutDown` via NuciLog

## Configuration Loading

### Sequence

1. Create empty settings instances (`new BotSettings()`, etc.)
2. Build `ConfigurationBuilder` with `appsettings.json`
3. Bind each section to its POCO via `config.Bind(nameof(Settings), settings)`

### Code

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

### Binding Details

| Section | Target | Optional | Reload on Change |
|---------|--------|----------|------------------|
| `botSettings` | `BotSettings` | Yes | Yes |
| `debugSettings` | `DebugSettings` | Yes | Yes |
| `nuciLoggerSettings` | `NuciLoggerSettings` | Yes | Yes |

**Note:** `optional: true` means missing file doesn't throw; `reloadOnChange: true` enables hot reload (not used in console app).

## Logger Configuration

### NuciLog Setup

```csharp
services.AddNuciLog(loggerSettings);
```

**Extension:** `NuciLog.DependencyInjection.ServiceCollectionExtensions.AddNuciLog`

**Settings:** `NuciLoggerSettings` with:
- `MinimumLevel` — Log level filter (e.g., "Info")
- `LogFilePath` — File path for file output (e.g., "logfile.log")
- `IsFileOutputEnabled` — Enable file sink

### Custom Logging Keys

Registered via `SteamProfileManager.Logging` namespace:
- `MyOperation` — Domain operations (LogIn, SetProfileName, SetProfileIdentifier, SetProfilePicture)
- `MyLogInfoKey` — Structured fields (Username, ProfileName, ProfileIdentifier, ProfilePicture)

## Composition Root

**Single composition root:** `Program.CreateIOC()` called once in `Main`

**Resolution:**
- `ILogger` resolved immediately after build for StartUp logging
- `IProfileManager` resolved in `RunApplication` for profile operations
- All other services resolved transitively via `ProfileManager` constructor

## Lifetime Management

| Component | Lifetime | Disposal |
|-----------|----------|----------|
| `IWebDriver` | Singleton (app lifetime) | `Quit()` in finally block |
| `ILogger` | Singleton (app lifetime) | NuciLog internal |
| Settings | Singleton (app lifetime) | No disposal needed |
| Services | Transient (per resolve) | No disposal needed |
| `ServiceProvider` | Singleton (app lifetime) | Not explicitly disposed (process exit) |

## Configuration Precedence

1. `appsettings.json` (base)
2. Environment variables (via `AddEnvironmentVariables` — not currently used)
3. Command line (via `AddCommandLine` — not currently used)
4. In-memory overrides (not used)

**Current:** Only `appsettings.json` is loaded. No environment-specific files (e.g., `appsettings.Development.json`).