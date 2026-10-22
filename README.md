[![Donate](https://img.shields.io/badge/-%E2%99%A5%20Donate-%23ff69b4)](https://hmlendea.go.ro/funding)
[![Build Status](https://github.com/hmlendea/steam-profile-manager/actions/workflows/dotnet.yml/badge.svg)](https://github.com/hmlendea/steam-profile-manager/actions/workflows/dotnet.yml)
[![Latest GitHub release](https://img.shields.io/github/v/release/hmlendea/steam-profile-manager)](https://github.com/hmlendea/steam-profile-manager/releases/latest)
[![License](https://img.shields.io/github/license/hmlendea/steam-profile-manager)](https://github.com/hmlendea/steam-profile-manager/blob/master/LICENSE)

# Steam Profile Manager

Steam Profile Manager is a small .NET console application that automates selected Steam profile updates.

## ✨ Capabilities

- Log into a Steam account
- Randomise the profile display name
- Randomise the profile identifier (custom URL part)
- Randomise the profile picture
- Configuration-driven through `appsettings.json`

## 📑 Table of Contents

- [Capabilities](#-capabilities)
- [System Requirements](#-system-requirements)
- [Installation](#-installation)
  - [Installation from Source](#installation-from-source)
- [Configuration](#-configuration)
  - [Configuration Files](#configuration-files)
  - [Settings](#settings)
- [Known Limitations](#-known-limitations)
- [Architecture](#-architecture)
- [Privacy and Data](#-privacy-and-data)
  - [Data Locations](#data-locations)
- [Project Structure](#-project-structure)
  - [Directories](#directories)
- [GitHub Actions](#-github-actions)
  - [Dependencies](#dependencies)
- [Contributing](#-contributing)
- [Code of Conduct](#-code-of-conduct)
- [Security](#-security)
- [License](#-license)

## 🖥️ System Requirements

| Component | Minimum | Recommended |
|-----------|---------|-------------|
| .NET SDK | 10.0 | 10.0 |
| Browser | Any Selenium-supported browser | Chrome/Chromium or Firefox |
| Steam Account | Required | Required |
| Steam Guard TOTP Key | Required | Required |

## 📦 Installation

[![Obtain it from GitHub](https://raw.githubusercontent.com/hmlendea/readme-assets/master/badges/stores/github.png)](https://github.com/hmlendea/steam-profile-manager/releases)

### Installation from Source

```bash
git clone https://github.com/hmlendea/steam-profile-manager.git
cd steam-profile-manager
dotnet restore
dotnet build --configuration Release
```

## ⚙️ Configuration

The application reads settings from `appsettings.json`.

### Configuration Files

| File | Scope | Purpose |
|------|-------|---------|
| `appsettings.json` | Application | Steam credentials, bot settings, debug settings, logger settings |

### Settings

The subsequent settings are recognised:

| Section | Key | Type | Default | Required | Description |
|---------|-----|------|---------|----------|-------------|
| `botSettings` | `pageLoadTimeout` | `int` | — | Yes | Page load timeout in seconds |
| `botSettings` | `steamAccount.username` | `string` | — | Yes | Steam account username |
| `botSettings` | `steamAccount.password` | `string` | — | Yes | Steam account password |
| `botSettings` | `steamAccount.totpKey` | `string` | — | Yes | Steam Guard shared secret used for TOTP |
| `botSettings` | `randomiseProfileName` | `bool` | — | Yes | Enable/disable random profile name update |
| `botSettings` | `randomiseProfileIdentifier` | `bool` | — | Yes | Enable/disable random profile identifier update |
| `botSettings` | `randomiseProfilePicture` | `bool` | — | Yes | Enable/disable random profile picture update |
| `botSettings` | `profileNamesList` | `string` | — | No | Path to text file with candidate profile names (one per line) |
| `botSettings` | `profilePicturesList` | `string` | — | No | Path to text file with candidate image URLs (one per line) |
| `debugSettings` | `isDebugMode` | `bool` | `false` | No | Run browser in debug (non-headless) mode |
| `debugSettings` | `crashScreenshotFileName` | `string` | — | No | Filename for crash screenshots (empty disables) |
| `nuciLoggerSettings` | `minimumLevel` | `string` | — | No | Minimum log level |
| `nuciLoggerSettings` | `logFilePath` | `string` | — | No | Log file path |
| `nuciLoggerSettings` | `isFileOutputEnabled` | `bool` | — | No | Enable file log output |

If `profileNamesList` is missing or empty, a random 12-character alphanumeric name is generated.

If `profilePicturesList` is missing or empty, the app uses a default image URL.

### File Formats

`profileNamesList` file example:

```text
CoolAlias123
NightDriver
ArcadeWizard
```

`profilePicturesList` file example:

```text
https://example.com/avatar-1.png
https://example.com/avatar-2.jpg
https://example.com/avatar-3.webp
```

## ⚠️ Known Limitations

- The application performs automated web interactions through Selenium, which may be affected by Steam website changes.
- A temporary file named `profilePicture.png` is created in the working directory when updating profile pictures.
- Use this tool responsibly and in accordance with Steam's terms and policies.

## 🏗️ Architecture

Steam Profile Manager follows a layered architecture with dependency injection and interface-based contracts. See the [architecture documentation](./ARCHITECTURE.md) for the system context, principal components, runtime flows, ownership boundaries, dependencies, constraints, and extension points.

## 🛡️ Privacy and Data

Steam Profile Manager is a self-hosted application that processes Steam account credentials and profile data locally. No data is transmitted to the project maintainers.

For the detailed description of how the application handles privacy and personal data, see [PRIVACY.md](./PRIVACY.md).

| Data | Purpose | Storage | Retention | Optional |
|------|---------|---------|-----------|----------|
| Steam username, password, TOTP key | Steam authentication | `appsettings.json` (local file) | Until manually deleted | No |
| Profile name candidates | Profile name randomisation | `profileNamesList` file (local) | Until manually deleted | Yes |
| Profile picture URLs | Profile picture randomisation | `profilePicturesList` file (local) | Until manually deleted | Yes |
| Application logs | Operational visibility | `logfile.log` (local file) | Until manually deleted | Yes (configurable) |
| Crash screenshots | Debugging | `crash.png` (local file) | Overwritten on each crash | Yes (configurable via `crashScreenshotFileName`) |
| Temporary profile picture | Profile picture update | `profilePicture.png` (working directory) | Overwritten on each run | No |

### Data Locations

| Platform or Scope | Location | Contents |
|-------------------|----------|----------|
| All platforms | `appsettings.json` | Steam credentials, bot settings, debug settings, logger settings |
| All platforms | `logfile.log` | Application logs (when file output enabled) |
| All platforms | `crash.png` | Crash screenshot (when `crashScreenshotFileName` configured) |
| All platforms | `profilePicture.png` | Temporary profile picture during update |
| All platforms | `profileNamesList` path | Candidate profile names (when configured) |
| All platforms | `profilePicturesList` path | Candidate profile picture URLs (when configured) |

## 🤝 Contributing

You are welcome to submit any suggestion, feedback, or modification to this project.

When doing so, please:
- Maintain cross-platform compatibility
- Submit focused pull requests that conform to the existing code style
- Maintain your branch synchronised with `master`
- Revise the documentation when functionality changes
- Properly test all modifications, including edge cases and error conditions
- Include tests for additional or modified functionality
- Raise a new [issue](https://github.com/hmlendea/steam-profile-manager/issues) for problems or suggestions

See the [contributing guidelines](./CONTRIBUTING.md) for details on how to report issues, suggest enhancements, and submit changes.

## 📜 Code of Conduct

See the [Code of Conduct](./CODE_OF_CONDUCT.md) for the standards of conduct expected in all project spaces.

## 🔒 Security

Steam Profile Manager follows a coordinated disclosure policy. Only the latest version distributed via GitHub Releases receives security maintenance.

For information on reporting security vulnerabilities, see [SECURITY.md](./SECURITY.md).

## 🗂️ Project Structure

This is a single-project .NET console application.

### Directories

| Directory | Purpose |
|-----------|---------|
| `Configuration/` | Strongly-typed settings classes (`BotSettings`, `DebugSettings`) |
| `Logging/` | Custom NuciLog operation and info key definitions |
| `Service/` | Business logic implementations and interfaces (`IProfileManager`, `IInfoGenerator`, `IFileDownloader`) |

## ⚙️ GitHub Actions

| Workflow | Purpose | What it does |
|----------|---------|--------------|
| [ dotnet.yml ](https://github.com/hmlendea/steam-profile-manager/blob/master/.github/workflows/dotnet.yml) | Continuous Integration | Restores dependencies, builds the project, and runs tests on push and pull requests to `master` |

### Dependencies

| Package | Version | Scope | Purpose |
|---------|---------|-------|---------|
| `Microsoft.Extensions.Configuration` | 10.0.9 | Runtime | Configuration abstraction |
| `Microsoft.Extensions.Configuration.Binder` | 10.0.9 | Runtime | Configuration binding |
| `Microsoft.Extensions.Configuration.Json` | 10.0.9 | Runtime | JSON configuration provider |
| `Microsoft.Extensions.DependencyInjection` | 10.0.9 | Runtime | Dependency injection container |
| `NuciExtensions` | 5.3.1 | Runtime | Utility extensions |
| `NuciLog` | 1.2.0 | Runtime | Structured logging |
| `NuciLog.Core` | 2.7.0 | Runtime | Logging core |
| `NuciWeb` | 4.0.0 | Runtime | Web utilities |
| `NuciWeb.Automation` | 1.0.0 | Runtime | Browser automation abstractions |
| `NuciWeb.Automation.Selenium` | 1.0.2 | Runtime | Selenium WebDriver integration |
| `NuciWeb.Steam` | 1.5.3 | Runtime | Steam-specific web interactions |

## 💝 Project Engagement

Discovered a problem or have a suggestion? [Open an issue](https://github.com/hmlendea/steam-profile-manager/issues)!

If you find this project useful, consider [funding it](https://hmlendea.go.ro/funding) or starring ⭐️ it on GitHub!

[![Donate](https://raw.githubusercontent.com/hmlendea/readme-assets/master/donate_generic.png)](https://hmlendea.go.ro/funding)

## 📄 License

This project is being distributed under the `GNU General Public License v3.0` or later.
See [LICENSE](./LICENSE) for additional information.
