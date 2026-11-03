# Operations — Security

## Threat Model

### Assets

| Asset | Sensitivity | Location |
|-------|-------------|----------|
| Steam credentials | Critical | `appsettings.json` (plain text) |
| TOTP shared secret | Critical | `appsettings.json` (plain text) |
| Identity secret | High | `appsettings.json` (plain text) |
| Session cookies | High | In-memory (WebDriver) |
| Profile pictures | Low | Downloaded to working directory |
| Logs | Medium | `logfile.log` |
| Crash screenshots | Medium | `crash.png` |

### Threat Actors

| Actor | Capability | Motivation |
|-------|------------|------------|
| Local user | File read | Credential theft |
| Malicious process | Memory read | Session hijack |
| Network attacker | MITM | Credential interception |
| Steam | Platform control | Account lock/ban |

---

## Current Security Posture

### Credentials Storage

**Status:** ❌ **Plain text in appsettings.json**

```json
{
  "botSettings": {
    "steamAccount": {
      "username": "myuser@example.com",
      "password": "plaintextpassword",
      "sharedSecret": "BASE32SECRET",
      "identitySecret": "BASE32SECRET2"
    }
  }
}
```

**Risks:**
- Git commit exposure
- File system access
- Backup leakage
- Log injection (if logged)

### Network Security

| Connection | Protocol | Verification |
|------------|----------|--------------|
| Steam login | HTTPS | TLS 1.2+ (browser) |
| Steam pages | HTTPS | TLS 1.2+ (browser) |
| Image download | HTTPS | TLS 1.2+ (WebClient) |
| WebDriver | Local | Loopback only |

**WebClient:** Uses system CA store, validates certificates

### Browser Security

- **Headless mode:** Reduces attack surface (no UI)
- **User profile:** Fresh each run (no persistent cookies)
- **Extensions:** None loaded
- **JavaScript:** Enabled (required for Steam)

### Logging Security

**Current:** Username logged (not password/secrets)

```csharp
logger.Info(MyOperation.LogIn, OperationStatus.Started,
    new LogInfo(MyLogInfoKey.Username, botSettings.SteamAccount.Username));
```

**Risk:** Username in logs — low sensitivity

**Not Logged:** Password, SharedSecret, IdentitySecret, DeviceId

---

## Recommended Improvements

### 1. Secrets Management (Critical)

**Option A: Environment Variables**
```csharp
// Program.cs LoadConfiguration()
.AddEnvironmentVariables("SPM_")
```

**Deployment:** Set via orchestrator (systemd, Docker, CI/CD)

**Environment Variables:**
```
SPM_BOTSETTINGS__STEAMACCOUNT__USERNAME=myuser
SPM_BOTSETTINGS__STEAMACCOUNT__PASSWORD=pass
SPM_BOTSETTINGS__STEAMACCOUNT__SHAREDSECRET=SECRET
SPM_BOTSETTINGS__STEAMACCOUNT__IDENTITYSECRET=SECRET2
```

**Option B: File with Restricted Permissions**
```bash
chmod 600 appsettings.json
chown steambot:steambot appsettings.json
```

**Option C: External Secret Store**
- Azure Key Vault
- HashiCorp Vault
- AWS Secrets Manager

### 2. Credential Validation at Startup

```csharp
static void ValidateCredentials()
{
    if (string.IsNullOrWhiteSpace(botSettings.SteamAccount.Username))
        throw new InvalidOperationException("Username not configured");

    if (string.IsNullOrWhiteSpace(botSettings.SteamAccount.Password))
        throw new InvalidOperationException("Password not configured");

    if (string.IsNullOrWhiteSpace(botSettings.SteamAccount.SharedSecret))
        throw new InvalidOperationException("TOTP secret not configured");
}
```

### 3. Secure File Permissions

```bash
# Config
chmod 600 appsettings.json

# Logs
chmod 640 logfile.log
chown steambot:steambot logfile.log

# Screenshots
chmod 640 crash.png
```

### 4. Log Sanitization

**Current:** Structured logging with explicit fields — safe

**Risk:** Exception messages may contain sensitive data

**Mitigation:** NuciLog doesn't log exception details by default in `Error()` call

### 5. Network Hardening

- Use proxy for egress control
- Restrict outbound to Steam domains only
- Certificate pinning (not supported by WebClient)

### 6. Browser Hardening

```csharp
// In WebDriverInitialiser (NuciWeb.Automation.Selenium)
options.AddArgument("--disable-extensions");
options.AddArgument("--disable-plugins");
options.AddArgument("--disable-java");
options.AddArgument("--no-sandbox");  // Required for Docker
options.AddArgument("--disable-dev-shm-usage");
```

---

## Compliance Considerations

### GDPR

- **Personal data:** Steam username (logged), profile data (processed)
- **Legal basis:** Legitimate interest (account management)
- **Retention:** Logs kept indefinitely (configure rotation)
- **Right to erasure:** Delete `logfile.log`, `crash.png`, `profilePicture.png`

### Steam ToS

- **Automation:** May violate Steam Subscriber Agreement
- **Rate limits:** Respect 30-day identifier cooldown
- **Captcha:** Indicates bot detection — stop automation

---

## Incident Response

### Credential Compromise

1. **Revoke Steam session:** Steam → Account → Manage Steam Guard → Deauthorize all devices
2. **Change password:** Steam → Account → Change password
3. **Regenerate TOTP:** Steam → Account → Manage Steam Guard → Remove authenticator → Add new
4. **Rotate identity secret:** Trade settings → Revoke API key
5. **Update config:** Deploy new credentials

### Log Exposure

1. Assess log content (username only)
2. Rotate logs
3. Review access controls

### Crash Screenshot Exposure

1. Check `crash.png` for sensitive UI content
2. Delete if contains credentials (unlikely)
3. Disable screenshots if risk unacceptable

---

## Security Checklist

### Pre-Deployment

- [ ] Credentials not in source control
- [ ] `appsettings.json` in `.gitignore`
- [ ] File permissions 600 for config
- [ ] Secrets injected via environment/CI
- [ ] Log directory permissions 750
- [ ] Network egress restricted

### Runtime

- [ ] Run as dedicated user (not root)
- [ ] Monitor for crash screenshots
- [ ] Alert on failed runs
- [ ] Regular credential rotation (90 days)

### Post-Incident

- [ ] Credential rotation procedure documented
- [ ] Log retention policy enforced
- [ ] Incident response tested