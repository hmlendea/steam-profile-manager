[![Donate](https://img.shields.io/badge/-%E2%99%A5%20Donate-%23ff69b4)](https://hmlendea.go.ro/fund.html) [![Build Status](https://github.com/hmlendea/steam-profile-manager/actions/workflows/dotnet.yml/badge.svg)](https://github.com/hmlendea/steam-profile-manager/actions/workflows/dotnet.yml) [![Latest GitHub release](https://img.shields.io/github/v/release/hmlendea/steam-profile-manager)](https://github.com/hmlendea/steam-profile-manager/releases/latest)

# Steam Profile Manager

## About

Steam Profile Manager is a small .NET console application that automates selected Steam profile updates.

It can:

- log into a Steam account;
- randomize the profile display name;
- randomize the profile identifier *(custom URL part)*;
- randomize the profile picture.

The app is configuration-driven through `appsettings.json`.

## Requirements

- .NET SDK 10.0 (the project targets `net10.0`)
- A working browser supported by Selenium WebDriver
- A Steam account
- Steam Guard TOTP key *(for 2FA login automation)*

## Quick Start

1. Restore dependencies:

	```bash
	dotnet restore
	```

2. Update `appsettings.json` with your Steam credentials and desired options.

3. Run the application:

	```bash
	dotnet run --project SteamProfileManager.csproj
	```

## Configuration

The application reads settings from `appsettings.json`.

### `botSettings`

- `pageLoadTimeout`: page load timeout in seconds.
- `steamAccount.username`: Steam account username.
- `steamAccount.password`: Steam account password.
- `steamAccount.totpKey`: Steam Guard shared secret used for TOTP.
- `randomiseProfileName`: enable/disable random profile name update.
- `randomiseProfileIdentifier`: enable/disable random profile identifier update.
- `randomiseProfilePicture`: enable/disable random profile picture update.
- `profileNamesList`: optional path to a text file with candidate profile names *(one entry per line)*.
- `profilePicturesList`: optional path to a text file with candidate image URLs *(one URL per line)*.

If `profileNamesList` is missing or empty, a random 12-character alphanumeric name is generated.

If `profilePicturesList` is missing or empty, the app uses a default image URL.

### `debugSettings`

- `isDebugMode`: if `true`, runs browser in debug *(non-headless)* mode.
- `crashScreenshotFileName`: filename used for crash screenshots.

If `crashScreenshotFileName` is empty or whitespace, crash screenshot generation is disabled.

### `nuciLoggerSettings`

Logger options are provided by `NuciLog`, including:

- `minimumLevel`
- `logFilePath`
- `isFileOutputEnabled`

## File Formats

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

## Notes

- The application performs automated web interactions through Selenium.
- A temporary file named `profilePicture.png` is created in the working directory when updating profile pictures.
- Use this tool responsibly and in accordance with Steam's terms and policies.

## License

This project is licensed under the `GNU General Public License v3.0` or later. See [LICENSE](./LICENSE) for details.
