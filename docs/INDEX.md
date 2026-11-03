# Steam Profile Manager Documentation

## Repository Summary

Steam Profile Manager is a .NET 10.0 console application that automates Steam profile updates including display name, custom URL identifier, and profile picture. It uses Selenium WebDriver for browser automation and NuciLog for structured logging. The application is configuration-driven through `appsettings.json` and designed for unattended execution.

## Root Document Map

- [ARCHITECTURE.md](../ARCHITECTURE.md) — High-level architecture
- [SECURITY.md](../SECURITY.md) — Security policy and vulnerability reporting
- [PRIVACY.md](../PRIVACY.md) — Data handling and privacy
- [LICENSE](../LICENSE) — GNU GPL v3 license
- [CONTRIBUTING.md](../CONTRIBUTING.md) — Contribution guidelines
- [CODE_OF_CONDUCT.md](../CODE_OF_CONDUCT.md) — Community conduct standards

## Documentation Catalogue

### Architecture & Design
- [architecture.md](./architecture.md) — Detailed architecture complementing root ARCHITECTURE.md
- [design-decisions.md](./design-decisions.md) — Key architectural choices and rationale
- [repository-overview.md](./repository-overview.md) — Repository purpose, scope, entry points
- [repository-structure.md](./repository-structure.md) — Source tree layout, module organisation
- [data-model.md](./data-model.md) — Domain entities, relationships, persistence
- [dependencies.md](./dependencies.md) — External and internal dependencies

### Components
- [components/presentation.md](./components/presentation.md) — Console entry point and CLI
- [components/host-and-composition.md](./components/host-and-composition.md) — DI container, configuration, WebDriver lifecycle
- [components/application-services.md](./components/application-services.md) — ProfileManager, InfoGenerator, FileDownloader
- [components/browser-state-and-localisation.md](./components/browser-state-and-localisation.md) — Selenium WebDriver integration
- [components/integration-models.md](./components/integration-models.md) — SteamAccount, NuciWeb.Steam models

### Flows
- [flows/startup-and-rendering.md](./flows/startup-and-rendering.md) — Application bootstrap, configuration, WebDriver init
- [flows/calendar-search.md](./flows/calendar-search.md) — Steam login and cookie handling
- [flows/detail-mutation.md](./flows/detail-mutation.md) — Profile name, identifier, picture updates

### API Reference
- [api-reference/configuration-schema.md](./api-reference/configuration-schema.md) — appsettings.json schema and options
- [api-reference/service-interfaces.md](./api-reference/service-interfaces.md) — IProfileManager, IInfoGenerator, IFileDownloader

### Operations
- [operations/configuration.md](./operations/configuration.md) — Configuration management, validation, secrets
- [operations/deployment.md](./operations/deployment.md) — Build pipeline, deployment, environments
- [operations/monitoring.md](./operations/monitoring.md) — Logging, health checks, alerting
- [operations/security.md](./operations/security.md) — Security model, threats, mitigations
- [operations/backup-and-restore.md](./operations/backup-and-restore.md) — Backup strategy, restore procedures
- [operations/troubleshooting.md](./operations/troubleshooting.md) — Common issues and solutions
- [operations/maintenance.md](./operations/maintenance.md) — Routine maintenance, updates, health checks
- [operations/incident-response.md](./operations/incident-response.md) — Incident classification, runbooks, escalation

### Behaviour
- [behaviour/error-handling.md](./behaviour/error-handling.md) — Error taxonomy, handling patterns, recovery
- [behaviour/steam-interaction.md](./behaviour/steam-interaction.md) — Steam navigation, element location, DOM interaction

## Navigation Aids

- **Start here** (newcomers): [repository-overview.md](./repository-overview.md) → [operations/configuration.md](./operations/configuration.md) → [flows/startup-and-rendering.md](./flows/startup-and-rendering.md)
- **Deep dive** (component owners): [components/](./components/) → [architecture.md](./architecture.md) → [design-decisions.md](./design-decisions.md)
- **Flows** (debuggers): [flows/](./flows/) → [behaviour/error-handling.md](./behaviour/error-handling.md) → [operations/monitoring.md](./operations/monitoring.md)
- **Operations** (operators): [operations/deployment.md](./operations/deployment.md) → [operations/monitoring.md](./operations/monitoring.md) → [operations/incident-response.md](./operations/incident-response.md)

## Maintenance Metadata

- Last reviewed: 2026-11-03
- Owner: hmlendea
- Coverage status: Complete — 27 files generated covering all specified areas