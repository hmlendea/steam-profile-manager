# Design Decisions

## 1. Console Application (Not Service/Daemon)

**Decision:** Build as a .NET console application executed on-demand (e.g., via cron/scheduler).

**Rationale:**
- Simple deployment: single executable, no service management
- Unattended execution via OS scheduler (cron, Task Scheduler)
- No need for background processing, health checks, or REST API
- Aligns with "run once, update profile, exit" use case

**Alternatives considered:**
- Windows Service / systemd service — adds complexity for no benefit
- ASP.NET Core hosted service — overkill for single-shot automation

## 2. Selenium WebDriver for Steam Automation

**Decision:** Use Selenium WebDriver via NuciWeb.Automation.Selenium for browser automation.

**Rationale:**
- Steam requires JavaScript-rendered pages and complex authentication flow
- NuciWeb.Steam provides Steam-specific DOM interaction abstractions
- Selenium supports headless Chrome/Firefox for server execution
- Mature, well-supported, cross-platform

**Alternatives considered:**
- Direct HTTP API — Steam has no public profile update API
- Puppeteer/Playwright — would require separate Node.js dependency
- Steam Web API — limited to public data, no profile mutation

## 3. NuciLog for Structured Logging

**Decision:** Use NuciLog (with NuciLog.Core) for logging instead of Serilog, NLog, or Microsoft.Extensions.Logging.

**Rationale:**
- Project ecosystem (Nuci*) already in use
- Custom `Operation` and `LogInfoKey` types provide domain-structured logging
- Lightweight, no external sink configuration complexity
- Built-in file/console output with level filtering

**Alternatives considered:**
- Serilog — more sinks but heavier
- Microsoft.Extensions.Logging — requires additional providers

## 4. Interface-Based Service Layer

**Decision:** Define interfaces (`IProfileManager`, `IInfoGenerator`, `IFileDownloader`) for all services, inject via DI.

**Rationale:**
- Enables unit testing with mocks (Moq)
- Architecture tests enforce dependency direction
- Clear separation of concerns
- Follows project coding conventions (interfaces for non-static classes with public methods)

## 5. Configuration via appsettings.json + POCO Binding

**Decision:** Use `Microsoft.Extensions.Configuration` with JSON file and `Bind()` to POCO classes.

**Rationale:**
- Standard .NET pattern
- Strongly-typed settings with validation via annotations (if needed)
- Supports environment-specific overrides (appsettings.Development.json)
- No custom configuration parsing

## 6. Random Generation via NuciExtensions.GetRandomElement

**Decision:** Use `NuciExtensions.GetRandomElement()` for random character selection.

**Rationale:**
- Already a project dependency (NuciExtensions)
- Simple, tested implementation
- Avoids `System.Random` thread-safety issues

## 7. FileDownloader Uses WebClient (Legacy)

**Decision:** `FileDownloader` uses `System.Net.WebClient` (deprecated) instead of `HttpClient`.

**Rationale:**
- Simple one-liner for synchronous download
- No async/await needed in current flow
- Low complexity for single-use download

**Known issue:** `WebClient` is obsolete. Should migrate to `HttpClient` with `IHttpClientFactory` for production hardening.

## 8. No Persistent Storage

**Decision:** No database, no local persistence beyond config and logs.

**Rationale:**
- Application is stateless between runs
- Profile data lives on Steam
- Temporary files only (profilePicture.png, crash.png)
- Simplifies deployment and backup

## 9. Architecture Tests as Enforcement

**Decision:** Use ArchUnit.NET in `SteamProfileManager.ArchitectureTests` to enforce dependency rules.

**Rationale:**
- Prevents architectural drift
- Documents rules as executable tests
- Catches violations in CI
- Complements code review

## 10. Primary Constructors for Services

**Decision:** Use C# 12 primary constructors for `ProfileManager`, `InfoGenerator`, `FileDownloader`.

**Rationale:**
- Reduces boilerplate (no private readonly fields + constructor body)
- Clear dependency declaration
- Aligns with project coding conventions

## 11. Expression-Bodied Members

**Decision:** Use expression-bodied syntax for single-statement methods and computed properties.

**Rationale:**
- Project convention
- Concise, readable for simple delegations
- Consistent with `=>` for computed properties

## 12. Sealed Classes by Default

**Decision:** All classes are `sealed` unless designed for inheritance.

**Rationale:**
- Project convention
- Prevents unintended inheritance
- Enables compiler optimisations
- Clear design intent

## 13. Block Namespaces

**Decision:** Use block namespaces (`namespace X { }`) not file-scoped (`namespace X;`).

**Rationale:**
- Project convention
- Consistent with existing codebase
- Explicit scope visibility

## 14. No Ternary Operators

**Decision:** Use `if`/`else` instead of ternary (`? :`) operators.

**Rationale:**
- Project convention
- Better readability for complex conditions
- Easier debugging (breakpoints on each branch)

## 15. Static Fields for Character Sets

**Decision:** `InfoGenerator` uses `static string` fields for character sets.

**Rationale:**
- Constants shared across instances
- Expression-bodied `=>` for computed `AllCharacters`
- No allocation per call