# Repository Structure

## Source Tree Layout

```
steam-profile-manager/
├── .github/
│   └── workflows/
│       └── dotnet.yml              # CI pipeline
├── SteamProfileManager/            # Main application project
│   ├── Configuration/
│   │   ├── BotSettings.cs          # Bot configuration (credentials, flags, file paths)
│   │   └── DebugSettings.cs        # Debug mode, crash screenshot settings
│   ├── Logging/
│   │   ├── MyLogInfoKey.cs         # Structured log info keys (Username, ProfileName, etc.)
│   │   └── MyOperation.cs          # Custom operations (LogIn, SetProfileName, etc.)
│   ├── Service/
│   │   ├── FileDownloader.cs       # HTTP image download (IFileDownloader impl)
│   │   ├── IFileDownloader.cs      # Download abstraction
│   │   ├── IInfoGenerator.cs       # Random generation abstraction
│   │   ├── InfoGenerator.cs        # Random name/identifier generation
│   │   ├── IProfileManager.cs      # Profile mutation orchestration abstraction
│   │   └── ProfileManager.cs       # Profile mutation orchestration
│   ├── appsettings.json            # Configuration template
│   ├── Program.cs                  # Entry point, DI composition, flow orchestration
│   └── SteamProfileManager.csproj  # Project file
├── SteamProfileManager.UnitTests/  # Unit tests
│   └── Service/
│       ├── InfoGeneratorTests.cs   # InfoGenerator tests
│       └── ProfileManagerTests.cs  # ProfileManager tests (mocked dependencies)
├── SteamProfileManager.ArchitectureTests/  # Architecture/fitness tests
│   ├── DependencyTests.cs          # Dependency direction rules
│   └── ProductionProjectTests.cs   # Project structure rules
├── ARCHITECTURE.md                 # Root architecture document
├── CODE_OF_CONDUCT.md              # Community conduct
├── CONTRIBUTING.md                 # Contribution guidelines
├── LICENSE                         # GNU GPL v3
├── PRIVACY.md                      # Data handling
├── README.md                       # Project overview
├── SECURITY.md                     # Security policy
├── SteamProfileManager.slnx        # Solution file
├── bin/                            # Build output (gitignored)
├── obj/                            # Build intermediates (gitignored)
└── docs/                           # This documentation
```

## Module Organisation

### SteamProfileManager (Main Project)

| Namespace | Folder | Purpose |
|-----------|--------|---------|
| `SteamProfileManager` | Root | Entry point (`Program.cs`) |
| `SteamProfileManager.Configuration` | `Configuration/` | Settings data objects |
| `SteamProfileManager.Logging` | `Logging/` | Domain-specific logging keys |
| `SteamProfileManager.Service` | `Service/` | Business logic behind interfaces |

### SteamProfileManager.UnitTests

| Namespace | Folder | Purpose |
|-----------|--------|---------|
| `SteamProfileManager.UnitTests.Service` | `Service/` | Unit tests for services |

### SteamProfileManager.ArchitectureTests

| Namespace | Folder | Purpose |
|-----------|--------|---------|
| `SteamProfileManager.ArchitectureTests` | Root | Architecture fitness tests |

## Project Dependencies

```
SteamProfileManager.slnx
├── SteamProfileManager (Exe, net10.0)
│   ├── Microsoft.Extensions.Configuration (10.0.13)
│   ├── Microsoft.Extensions.Configuration.Binder (10.0.13)
│   ├── Microsoft.Extensions.Configuration.Json (10.0.13)
│   ├── Microsoft.Extensions.DependencyInjection (10.0.13)
│   ├── NuciExtensions (6.0.2)
│   ├── NuciLog (1.2.1)
│   ├── NuciLog.Core (3.1.0)
│   ├── NuciWeb (4.1.0)
│   ├── NuciWeb.Automation (1.0.1)
│   ├── NuciWeb.Automation.Selenium (1.0.3)
│   └── NuciWeb.Steam (1.5.3)
├── SteamProfileManager.UnitTests (Test, net10.0)
│   └── References SteamProfileManager
└── SteamProfileManager.ArchitectureTests (Test, net10.0)
    ├── ArchUnitNET
    ├── ArchUnitNET.NUnit
    └── References SteamProfileManager
```

## Key Files

| File | Purpose |
|------|---------|
| `Program.cs` | Application entry point, configuration loading, DI setup, WebDriver lifecycle, main flow |
| `appsettings.json` | Configuration template (copied to output) |
| `BotSettings.cs` | All bot behaviour configuration |
| `ProfileManager.cs` | Core orchestration of profile mutations |
| `InfoGenerator.cs` | Random string generation |
| `FileDownloader.cs` | HTTP download adapter |
| `MyOperation.cs` / `MyLogInfoKey.cs` | Structured logging domain keys |
| `DependencyTests.cs` | Enforces dependency direction rules |
| `ProductionProjectTests.cs` | Enforces project structure rules |