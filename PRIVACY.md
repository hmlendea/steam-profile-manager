# Privacy and Personal Data

This document describes how Steam Profile Manager handles personal data. It covers the application behaviour and verified integrations described below. Where the software is self-hosted, the instance operator may have separate responsibilities described below.

## 📑 Table of Contents

- [What This Document Covers](#what-this-document-covers)
- [Self-Hosted Deployments](#self-hosted-deployments)
- [Data We Handle](#data-we-handle)
- [Processing and Use](#processing-and-use)
- [Storage, Retention, and Deletion](#storage-retention-and-deletion)
- [External Processing and Integrations](#external-processing-and-integrations)
- [International Transfers](#international-transfers)
- [Data Protection and Security](#data-protection-and-security)
- [Document Changes](#document-changes)
- [Contact](#contact)

## 🔎 What This Document Covers

This document describes how Steam Profile Manager at https://github.com/hmlendea/steam-profile-manager handles personal data. It covers the application behaviour and verified integrations described below. Where the software is self-hosted, the instance operator may have separate responsibilities described below.

## 🏠 Self-Hosted Deployments

Steam Profile Manager is a self-hosted .NET console application. The instance operator (the person running the application) controls the application's configuration, local storage, logs, backups, access controls, retention, and request handling. The project maintainers do not operate any central service and do not receive data from self-hosted instances.

No data is sent from a self-hosted instance to project maintainers. The application communicates only with Steam's servers (store.steampowered.com, steamcommunity.com) for authentication and profile updates. There is no telemetry, update checks, crash reporting, or monitoring integrations built into the application. Crash screenshots are saved locally only when `crashScreenshotFileName` is configured in `appsettings.json`.

## 📥 Data We Handle

### Data Provided to the Application

- Steam account credentials: username, password, and Steam Guard TOTP shared secret (provided in `appsettings.json`)
- Optional profile name candidates (from `profileNamesList` file, one entry per line)
- Optional profile picture URLs (from `profilePicturesList` file, one URL per line)

### Data Generated or Collected by the Application

- Application logs (written to `logfile.log` when file output is enabled)
- Crash screenshots (saved as `crash.png` when `crashScreenshotFileName` is configured and a crash occurs)
- Temporary profile picture file (`profilePicture.png` created in working directory during profile picture updates)

### Data Received from Integrations

- No personal data is received from integrations or third parties. The application only sends data to Steam's servers for authentication and profile modification.

## 🧭 Processing and Use

The application processes the data described above for these verified functions:
- Steam authentication — Steam account credentials (username, password, TOTP key)
- Profile name randomisation — Profile name candidates or generated random name
- Profile identifier randomisation — Generated random identifier
- Profile picture randomisation — Profile picture URLs or default image URL
- Application logging — Log entries (info, warnings, errors) for operational visibility
- Crash diagnostics — Screenshot capture for debugging when enabled

## 🗄️ Storage, Retention, and Deletion

- **Configuration (`appsettings.json`)**: Stored locally on the operator's filesystem. Retained until manually deleted or overwritten by the operator.
- **Logs (`logfile.log`)**: Stored locally on the operator's filesystem. Retained until manually deleted or rotated by the operator. No automatic rotation is configured.
- **Crash screenshots (`crash.png`)**: Stored locally in the working directory. Overwritten on each crash when enabled. Retained until manually deleted.
- **Temporary profile picture (`profilePicture.png`)**: Created in the working directory during profile picture updates. Overwritten on each run. Retained until manually deleted.
- **Profile name/picture list files**: Stored locally at operator-specified paths. Retained until manually deleted.

For self-hosted deployments, the instance operator controls all storage, deletion, and backups. The project does not operate any central storage.

## 🔗 External Processing and Integrations

| Service or integration | Purpose | Data involved | Configuration or documentation |
|-----------------------|---------|---------------|--------------------------------|
| Steam (store.steampowered.com, steamcommunity.com) | Authentication and profile updates | Steam username, password, TOTP key, profile name, profile identifier, profile picture | https://steamcommunity.com/dev |

The application has no other built-in external data transfers.

## 🌍 International Transfers

The application sends data to Steam's servers, which are primarily located in the United States. The instance operator's deployment location determines the origin of the transfer. The project maintainers do not control or operate any data processing infrastructure.

## 🛡️ Data Protection and Security

- Steam credentials are stored in plain text in `appsettings.json` on the operator's filesystem. The operator is responsible for file permissions and access controls.
- Communication with Steam uses HTTPS via Selenium WebDriver.
- No encryption at rest is provided by the application for local files.
- The operator is responsible for: keeping the application and dependencies updated, securing the filesystem where credentials and logs reside, controlling network exposure, and protecting log files from unauthorised access.
- The project does not promise absolute security. This is a technical description of application behaviour, not a compliance certification.

## 🔄 Document Changes

Update this document when application data flows, storage, integrations, or deployment responsibilities change. The current version is published at https://github.com/hmlendea/steam-profile-manager/blob/master/PRIVACY.md.

## 📬 Contact

For questions about application data handling, contact the project maintainers at https://github.com/hmlendea/steam-profile-manager/issues. For a self-hosted instance, contact the instance operator, unless the project explicitly handles the request. Include the deployment environment and configuration details if relevant; do not send passwords, access tokens, or other secrets.