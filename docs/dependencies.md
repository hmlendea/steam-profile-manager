# Dependencies

## External Dependencies (NuGet)

### Production Dependencies

| Package | Version | Purpose | License |
|---------|---------|---------|---------|
| `Microsoft.Extensions.Configuration` | 10.0.13 | Configuration abstraction | MIT |
| `Microsoft.Extensions.Configuration.Binder` | 10.0.13 | POCO binding from configuration | MIT |
| `Microsoft.Extensions.Configuration.Json` | 10.0.13 | JSON configuration provider | MIT |
| `Microsoft.Extensions.DependencyInjection` | 10.0.13 | DI container | MIT |
| `NuciExtensions` | 6.0.2 | Extension methods (GetRandomElement) | MIT |
| `NuciLog` | 1.2.1 | Structured logging facade | MIT |
| `NuciLog.Core` | 3.1.0 | Logging core abstractions | MIT |
| `NuciWeb` | 4.1.0 | Web utilities base | MIT |
| `NuciWeb.Automation` | 1.0.1 | Browser automation abstractions | MIT |
| `NuciWeb.Automation.Selenium` | 1.0.3 | Selenium WebDriver initialisation | MIT |
| `NuciWeb.Steam` | 1.5.3 | Steam-specific DOM interactions | MIT |

### Test Dependencies

| Package | Version | Purpose | Project |
|---------|---------|---------|---------|
| `NUnit` | 4.x | Test framework | UnitTests, ArchitectureTests |
| `Moq` | Latest | Mocking framework | UnitTests |
| `ArchUnitNET` | Latest | Architecture rules | ArchitectureTests |
| `ArchUnitNET.NUnit` | Latest | NUnit integration | ArchitectureTests |

## Internal Dependencies

### Project References

```
SteamProfileManager.slnx
├── SteamProfileManager (Production)
├── SteamProfileManager.UnitTests
│   └── → SteamProfileManager
└── SteamProfileManager.ArchitectureTests
    └── → SteamProfileManager
```

### Namespace Dependencies (Enforced by Architecture Tests)

**Allowed:**
- `SteamProfileManager` (Program) → `SteamProfileManager.Configuration`
- `SteamProfileManager` (Program) → `SteamProfileManager.Logging`
- `SteamProfileManager` (Program) → `SteamProfileManager.Service`
- `SteamProfileManager.Service` → `SteamProfileManager.Configuration` (BotSettings only)
- `SteamProfileManager.Service` → `SteamProfileManager.Logging` (ILogger, MyOperation, MyLogInfoKey)
- `SteamProfileManager.Logging` → (no internal deps)
- `SteamProfileManager.Configuration` → (no internal deps)

**Forbidden (enforced by tests):**
- `SteamProfileManager.Configuration` → `SteamProfileManager.Service`
- `SteamProfileManager.Configuration` → `SteamProfileManager.Logging`
- `SteamProfileManager.Logging` → `SteamProfileManager.Configuration`
- `SteamProfileManager.Logging` → `SteamProfileManager.Service`
- Any service (except FileDownloader) → `System.Net`, `OpenQA.Selenium`, `Microsoft.Extensions.*`
- Production → Test assemblies

## External System Integrations

| System | Integration Point | Protocol | Authentication |
|--------|-------------------|----------|----------------|
| Steam Website | `ISteamProcessor` (NuciWeb.Steam) | HTTPS via Selenium | Username/Password/TOTP |
| Selenium WebDriver | `IWebDriver` | WebDriver Protocol | N/A (local browser) |
| File System | `appsettings.json`, logs, temp files | Local FS | OS permissions |

## Dependency Graph

```
SteamProfileManager (Exe)
├── Microsoft.Extensions.Configuration*
├── Microsoft.Extensions.DependencyInjection
├── NuciExtensions
├── NuciLog
├── NuciLog.Core
├── NuciWeb
├── NuciWeb.Automation
├── NuciWeb.Automation.Selenium
│   └── Selenium.WebDriver (transitive)
├── NuciWeb.Steam
│   └── Selenium.WebDriver (transitive)
│
├── SteamProfileManager.UnitTests
│   ├── NUnit
│   ├── Moq
│   └── → SteamProfileManager
│
└── SteamProfileManager.ArchitectureTests
    ├── ArchUnitNET
    ├── ArchUnitNET.NUnit
    ├── NUnit
    └── → SteamProfileManager
```

## Version Policy

- Target Framework: `net10.0` (all projects)
- Package references: Alphabetically ordered in csproj
- `LangVersion: latest`
- `Nullable: enable`
- `ImplicitUsings: disable`
- `IsPackable: false` (all projects)

## Transitive Dependency Notes

- `Selenium.WebDriver` comes transitively via `NuciWeb.Automation.Selenium` and `NuciWeb.Steam`
- `NuciLog.Core` is a dependency of `NuciLog`
- `NuciWeb` is a dependency of `NuciWeb.Automation`, `NuciWeb.Automation.Selenium`, `NuciWeb.Steam`