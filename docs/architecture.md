# Architecture

This document elaborates on the root [ARCHITECTURE.md](../ARCHITECTURE.md) with implementation-level detail.

## Layered Architecture

```
Program.cs (Entry Point)
    ├── Configuration Layer (BotSettings, DebugSettings, NuciLoggerSettings)
    ├── DI Composition (Microsoft.Extensions.DependencyInjection)
    ├── WebDriver Lifecycle (NuciWeb.Automation.Selenium.WebDriverInitialiser)
    └── Service Layer (IProfileManager, IInfoGenerator, IFileDownloader)
            ├── ProfileManager (orchestrates profile mutations)
            ├── InfoGenerator (random name/identifier generation)
            ├── FileDownloader (HTTP image download)
            └── ISteamProcessor (NuciWeb.Steam — Steam DOM interactions)
```

## Dependency Direction

```
Program.cs
    → Configuration (data only)
    → WebDriverInitialiser (infrastructure)
    → DI Container
        → ProfileManager
            → IInfoGenerator (abstraction)
            → IFileDownloader (abstraction)
            → ISteamProcessor (abstraction)
            → BotSettings (data)
            → ILogger (abstraction)
        → InfoGenerator (implements IInfoGenerator)
        → FileDownloader (implements IFileDownloader)
```

**Rules enforced by architecture tests:**
- Configuration and Logging namespaces do not depend on Service
- Service namespace does not depend on Program (entry point)
- Service namespace (except FileDownloader) does not depend on `System.Net`, `OpenQA.Selenium`, or `Microsoft.Extensions.*`
- Production assembly has no test dependencies
- No `InternalsVisibleTo` attributes

## Runtime Flow

```
Main
  → LoadConfiguration (appsettings.json → BotSettings, DebugSettings, NuciLoggerSettings)
  → InitialiseAvailableWebDriver (headless/debug mode, page load timeout)
  → CreateIOC (register services, resolve ILogger)
  → Log StartUp
  → RunApplication
        → ProfileManager.LogIn
              → ISteamProcessor.LogIn (SteamAccount credentials)
              → ISteamProcessor.RejectCookies
        → if RandomiseProfileName: ProfileManager.SetRandomProfileName
              → GetRandomEntryFromList (file) OR InfoGenerator.GetRandomProfileName
              → ISteamProcessor.SetProfileName
        → if RandomiseProfilePicture: ProfileManager.SetRandomProfilePicture
              → GetRandomEntryFromList (file) OR default URL
              → IFileDownloader.Download (to profilePicture.png)
              → ISteamProcessor.SetProfilePicture (local file path)
        → if RandomiseProfileIdentifier: ProfileManager.SetRandomProfileIdentifier
              → InfoGenerator.GetRandomProfileIdentifier
              → ISteamProcessor.SetProfileIdentifier
  → Log ShutDown
  → WebDriver.Quit
```

## Cross-Cutting Concerns

| Concern | Implementation |
|---------|----------------|
| Logging | NuciLog with `MyOperation` and `MyLogInfoKey` for structured entries |
| Configuration | `Microsoft.Extensions.Configuration` JSON binding |
| DI | `Microsoft.Extensions.DependencyInjection` |
| Browser Automation | Selenium WebDriver via NuciWeb.Automation.Selenium |
| Steam Integration | NuciWeb.Steam `ISteamProcessor` |
| HTTP Download | `System.Net.WebClient` (FileDownloader) |
| Randomness | `NuciExtensions.GetRandomElement` |

## Architectural Areas

| Area | Namespaces | Responsibility |
|------|------------|----------------|
| Entry Point | `SteamProfileManager` | Lifecycle, composition, error handling |
| Configuration | `SteamProfileManager.Configuration` | Settings data objects |
| Services | `SteamProfileManager.Service` | Business logic behind interfaces |
| Logging | `SteamProfileManager.Logging` | Domain-specific operation/info keys |
| External | `NuciWeb.Steam`, `OpenQA.Selenium` | Steam DOM, browser control |

## Data Architecture

No persistent data store. Transient state only:
- `BotSettings` — configuration (immutable after load)
- `SteamAccount` — credentials (in memory only)
- `profilePicture.png` — temporary file (overwritten each run)
- Logs — append-only file (operator-managed retention)

## Interfaces and Integrations

| Interface | Implementation | Purpose |
|-----------|----------------|---------|
| `IProfileManager` | `ProfileManager` | Orchestrates profile mutations |
| `IInfoGenerator` | `InfoGenerator` | Generates random names/identifiers |
| `IFileDownloader` | `FileDownloader` | Downloads profile pictures |
| `ISteamProcessor` | NuciWeb.Steam | Steam authentication and DOM mutations |
| `ILogger` | NuciLog | Structured logging |
| `IWebDriver` | Selenium | Browser automation |

## Deployment and Operations

- Self-contained .NET 10.0 console application
- Requires Selenium-supported browser (Chrome/Chromium/Firefox)
- Configuration via `appsettings.json` (copied to output directory)
- Logs to console and/or `logfile.log`
- Crash screenshots to `crash.png` when enabled
- No background services, no daemon mode

## Testing and Verification

| Test Project | Framework | Scope |
|--------------|-----------|-------|
| `SteamProfileManager.UnitTests` | NUnit 4.x + Moq | Service logic, InfoGenerator |
| `SteamProfileManager.ArchitectureTests` | NUnit + ArchUnit.NET | Dependency rules, project structure |

## Design Constraints

- .NET 10.0 minimum
- No `var` — explicit types required
- No ternary operators — use `if`/`else`
- No `ref`/`out` parameters
- No `#region` directives
- Block namespaces (`namespace X { }`)
- Primary constructors for services
- Expression-bodied single-statement methods
- `sealed` classes unless designed for inheritance
- Interfaces for all non-static classes with public methods