# Security Policy

This document describes the security policy for Steam Profile Manager, including supported versions, vulnerability reporting, and disclosure practices.

## 📑 Table of Contents

- [Supported Versions](#-supported-versions)
- [Reporting a Vulnerability](#-reporting-a-vulnerability)
- [Scope](#-scope)
- [Disclosure Policy](#-disclosure-policy)

## 🛡️ Supported Versions

Use this table to indicate which project versions currently receive security maintenance.

| Version | Distribution Channel | Supported |
|---------|--------------------|-----------|
| Latest version | GitHub Releases | ✅ |
| Preceding versions | Any distribution channel | ❌ |

## 🚨 Reporting a Vulnerability

Please do not disclose suspected vulnerabilities publicly before maintainers have had an opportunity to validate and remediate them.

To report a vulnerability:
- [GitHub Security Advisories](https://github.com/hmlendea/steam-profile-manager/security/advisories)
- Contact the maintainers directly

## 📌 Scope

The subsequent report categories are in scope for this repository:
- Credential handling and storage in `appsettings.json`
- Steam authentication flow and TOTP processing
- Profile data manipulation via Selenium WebDriver
- Local file operations (logs, crash screenshots, temporary files)
- Configuration binding and validation

The subsequent categories are out of scope unless explicitly stated to the contrary:
- Steam platform vulnerabilities (Valve Corporation responsibility)
- Third-party dependency vulnerabilities (NuGet packages: NuciLog, NuciWeb, Selenium, Microsoft.Extensions)
- Operating system or browser vulnerabilities
- Social engineering or phishing attacks targeting users
- Physical access to the host machine

## 📢 Disclosure Policy

This project follows coordinated disclosure:
1. Vulnerabilities are investigated privately.
2. A remediation plan is prepared and validated.
3. Public disclosure is published after a fix, mitigation, or agreed risk decision is available.
4. Credit is attributed in accordance with reporter preference and project policy.