# Flows — Detail Mutation (Profile Updates)

## Overview

Covers three profile mutation flows executed conditionally after login:
1. `SetRandomProfileName()` — Display name update
2. `SetRandomProfileIdentifier()` — Custom URL identifier update
3. `SetRandomProfilePicture()` — Profile picture upload

All invoked from `RunApplication()` based on `BotSettings` flags.

---

## SetRandomProfileName() Flow

### Sequence

```
ProfileManager.SetRandomProfileName()
  │
  ├─► logger.Info(MyOperation.SetProfileName, Started)
  │
  ├─► Determine name source
  │     ├─► If ProfileNamesList configured & non-empty
  │     │     └─► Read random line from file (GetRandomEntryFromList)
  │     └─► Else
  │           └─► infoGenerator.GetRandomProfileName() → 12-char alphanumeric
  │
  ├─► ISteamProcessor.SetProfileName(profileName)
  │     │
  │     ├─► Navigate to profile edit page
  │     ├─► Locate name input field
  │     ├─► Clear existing value
  │     ├─► SendKeys(profileName)
  │     ├─► Click save button
  │     └─► Wait for confirmation
  │
  └─► logger.Info(MyOperation.SetProfileName, Success, ProfileName)
```

### Name Generation

**File-based:**
```csharp
string profileName = GetRandomEntryFromList(botSettings.ProfileNamesList);
```
- File: UTF-8, one name per line
- Selection: `NuciExtensions.GetRandomElement()`

**Generated:**
```csharp
string profileName = infoGenerator.GetRandomProfileName();
// 12 chars from [a-zA-Z0-9]
```

### ISteamProcessor.SetProfileName(string)

**Package:** `NuciWeb.Steam`

**Steps (inferred):**
1. Navigate to `https://steamcommunity.com/id/<identifier>/edit` or profile edit URL
2. Wait for form load
3. Locate display name input (`#profile_name` or similar)
4. Clear and send new name
5. Click save (`#save_btn` or similar)
6. Wait for success indicator

**Failure Modes:**
- Name too long (> 64 chars) — Steam validation
- Name taken — Not applicable (display name not unique)
- Element not found — UI change
- Timeout — Network/slow page

---

## SetRandomProfileIdentifier() Flow

### Sequence

```
ProfileManager.SetRandomProfileIdentifier()
  │
  ├─► logger.Info(MyOperation.SetProfileIdentifier, Started)
  │
  ├─► infoGenerator.GetRandomProfileIdentifier() → 8-char alphanumeric
  │
  ├─► ISteamProcessor.SetProfileIdentifier(profileIdentifier)
  │     │
  │     ├─► Navigate to profile edit page
  │     ├─► Locate custom URL input field
  │     ├─► Clear existing value
  │     ├─► SendKeys(profileIdentifier)
  │     ├─► Click save button
  │     └─► Wait for confirmation / availability check
  │
  └─► logger.Info(MyOperation.SetProfileIdentifier, Success, ProfileIdentifier)
```

### Identifier Generation

```csharp
string profileIdentifier = infoGenerator.GetRandomProfileIdentifier();
// 8 chars from [a-zA-Z0-9]
```

**Always generated** — no file-based option.

### ISteamProcessor.SetProfileIdentifier(string)

**Steps (inferred):**
1. Navigate to profile edit page (same as name)
2. Locate custom URL field (`#customURL` or similar)
3. Clear and send new identifier
4. Client-side availability check (AJAX) or form submit
5. Click save
6. Wait for success

**Constraints:**
- Must be unique across Steam
- Alphanumeric only (enforced by generator)
- Minimum 3 chars, maximum 32 chars (generator produces 8)
- Cannot be changed again for 30 days (Steam policy)

**Failure Modes:**
- Identifier taken — Steam returns error
- Too short/long — Validation error
- Rate limited — 30-day cooldown
- Element not found — UI change

---

## SetRandomProfilePicture() Flow

### Sequence

```
ProfileManager.SetRandomProfilePicture()
  │
  ├─► logger.Info(MyOperation.SetProfilePicture, Started)
  │
  ├─► Determine picture URL
  │     ├─► If ProfilePicturesList configured & non-empty
  │     │     └─► Read random line from file (GetRandomEntryFromList)
  │     └─► Else
  │           └─► Default: "https://i.imgur.com/rV2RQMO.png"
  │
  ├─► fileDownloader.Download(url, "profilePicture.png")
  │     └─► WebClient.DownloadFile(url, path) — synchronous
  │
  ├─► ISteamProcessor.SetProfilePicture("profilePicture.png")
  │     │
  │     ├─► Navigate to profile edit page
  │     ├─► Locate file input (<input type="file">)
  │     ├─► SendKeys(absolutePath)
  │     ├─► Click upload/save
  │     └─► Wait for upload + crop/confirm
  │
  └─► logger.Info(MyOperation.SetProfilePicture, Success, ProfilePicture)
```

### Picture Source

**File-based:**
- File: UTF-8, one URL per line
- Selection: `NuciExtensions.GetRandomElement()`

**Default:**
- `https://i.imgur.com/rV2RQMO.png` (placeholder image)

### Download

```csharp
string profilePictureFilePath = Path.Join(Directory.GetCurrentDirectory(), "profilePicture.png");
fileDownloader.Download(profilePictureUrl, profilePictureFilePath);
```

**Implementation:** `System.Net.WebClient.DownloadFile()` — blocking, no timeout, no retry.

**Output:** `profilePicture.png` in working directory (overwritten each run).

### ISteamProcessor.SetProfilePicture(string filePath)

**Steps (inferred):**
1. Navigate to profile edit page
2. Locate file input (`<input type="file" accept="image/*">`)
3. SendKeys(absolute file path) — triggers OS file dialog bypass
4. Wait for image preview/upload
5. Handle crop dialog if present (Steam shows cropper)
6. Click save/confirm
7. Wait for success

**Requirements:**
- Image format: JPG, PNG, GIF
- Max file size: ~5MB (Steam limit)
- Dimensions: Square recommended (184x184 minimum)

**Failure Modes:**
- Download failure (network, 404) — `WebException`
- File too large — Steam rejects
- Invalid format — Steam rejects
- Upload timeout — Network
- Crop dialog handling — UI change

---

## Logging Summary

| Operation | Start Log | Success Log | Key Fields |
|-----------|-----------|-------------|------------|
| SetProfileName | `Started` | `Success` | `ProfileName` |
| SetProfileIdentifier | `Started` | `Success` | `ProfileIdentifier` |
| SetProfilePicture | `Started` | `Success` | `ProfilePicture` (source URL) |

All use `MyOperation.*` and `MyLogInfoKey.*`.

---

## Configuration Flags

| Flag | Default | Controls |
|------|---------|----------|
| `RandomiseProfileName` | `false` | Execute name flow |
| `RandomiseProfileIdentifier` | `false` | Execute identifier flow |
| `RandomiseProfilePicture` | `false` | Execute picture flow |
| `ProfileNamesList` | `null` | File path for names (optional) |
| `ProfilePicturesList` | `null` | File path for picture URLs (optional) |

---

## Execution Order

```csharp
profileManager.LogIn();  // Always first

if (botSettings.RandomiseProfileName)
    profileManager.SetRandomProfileName();

if (botSettings.RandomiseProfileIdentifier)
    profileManager.SetRandomProfileIdentifier();

if (botSettings.RandomiseProfilePicture)
    profileManager.SetRandomProfilePicture();
```

**Sequential** — each waits for previous to complete.

---

## Error Handling

**No per-operation try/catch.** Exceptions propagate to `Program.Main()`:

```csharp
catch (Exception ex)
{
    logger.Error(ex, "Application crashed");
    if (debugSettings.IsCrashScreenshotEnabled && webDriver is ITakesScreenshot ss)
    {
        ss.GetScreenshot().SaveAsFile(debugSettings.CrashScreenshotFileName);
    }
    throw;
}
```

**Crash screenshot** captures browser state at failure point.

---

## Timing Estimates

| Operation | Typical Duration |
|-----------|-----------------|
| Name generation | < 1ms |
| File read (if used) | < 10ms |
| Picture download | 1-10s (network) |
| Steam page navigation | 1-3s each |
| Form fill + submit | 1-3s each |
| Picture upload + crop | 3-10s |
| **Total (all three)** | **10-40s** |

---

## Idempotency

- **Name:** Can be changed anytime, no restrictions
- **Identifier:** 30-day cooldown after change (Steam policy)
- **Picture:** Can be changed anytime

**Re-running:** If identifier changed recently, `SetProfileIdentifier` will fail.