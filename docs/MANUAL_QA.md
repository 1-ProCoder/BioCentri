# BioCentri — Manual QA Script

> To be run before every release. Each section covers one feature area.
> Mark ✅ pass or ❌ fail. Attach screenshots for failures.

---

## 1. Install & Launch

- [ ] Open `app/BioCentri.sln` in Visual Studio 2022+ or `dotnet build`
- [ ] Build: `dotnet build app/BioCentri.sln -c Debug` → **0 errors, 0 warnings**
- [ ] Run BioCentri.App.exe directly from `app/BioCentri.App/bin/Debug/net8.0-windows10.0.19041.0/`
- [ ] Main window opens with sidebar (Dashboard selected), TopBar, and StatusBar
- [ ] No startup dialogs or crash popups

## 2. Shell Navigation

- [ ] Click each sidebar item (Dashboard, Protected Apps, Rules, Activity, Settings, About, Diagnostics)
- [ ] Each page renders with a PageHeader matching the route title
- [ ] Sidebar selection stripe follows the active route
- [ ] Sidebar collapse (topbar hamburger) → sidebar shrinks to icon-only; same routes still selectable
- [ ] Window can be resized; layout stays responsive

## 3. Protected Apps

- [ ] Navigate to Protected Apps page
- [ ] **Empty state:** "No protected apps yet" card visible with icon
- [ ] Click **"Add application"** → picker overlay opens
- [ ] Picker shows discovered installed apps (registry Uninstall hive)
- [ ] Type in picker search box → list filters by display name / publisher
- [ ] Click "Protect" on a row → picker closes, app appears in protected list with "Added {date}"
- [ ] **Escape key** closes the picker without adding
- [ ] Click "Unprotect" on a row → app removed from list
- [ ] Quit and relaunch the app → protected list persists

## 4. Windows Hello

- [ ] Navigate to **Diagnostics** page → "Hello availability" shows Available or NotConfigured
- [ ] (Optional, requires Hello hardware) Launch a protected app (e.g. Chrome) externally
- [ ] Authentication overlay appears with "Verifying identity…" and the app name
- [ ] Complete Windows Hello (face/fingerprint/PIN)
- [ ] On success: overlay fades out, app launches normally
- [ ] On cancel (click "Cancel" on overlay): overlay fades out, auth is cancelled
- [ ] Rapid double-click same app → only one Hello prompt (dedupe)

## 5. Process Enforcement (M6)

- [ ] ProcessWatcher starts before MainWindow (no startup race)
- [ ] Protected app launch triggers auth challenge (see §4)
- [ ] Auth failure → toast "Blocked: {app} was blocked by BioCentri"
- [ ] Auth success → process allowed (no toast)

## 6. Settings

- [ ] Navigate to Settings page → 6 category rows visible
- [ ] Reduced motion toggle → flips on/off
- [ ] Dashbaord motion components (HologramFloat, ReticleRing) freeze when toggle is on
- [ ] High Contrast: enable Windows High Contrast → app theme swaps to SystemColors (M7)

## 7. Visual Polish

- [ ] Dashboard hero card shows ReticleRing ornament rotating (unless reduced motion)
- [ ] BentoStats (4 tiles) render with hover lift (BorderTrace)
- [ ] BioCentri logo floats gently (HologramFloat)
- [ ] Theme is dark (#060606 background, indigo accent)

## 8. Tests

- [ ] `dotnet test app/BioCentri.Tests/BioCentri.Tests.csproj` → **7 passed, 0 failed**

---

## Deferred / Known Limitations

- **Tray icon:** not yet activated (requires H.NotifyIcon.Wpf NuGet + .ico asset — tracked for M7.1)
- **WiX installer:** not yet built (see `docs/INSTALLER.md` for the MSI checklist)
- **Code signing:** not yet applied (requires EV certificate or Azure Trusted Signing)
- **Startup tray behaviour:** app currently shows the main window on launch; minimise-to-tray deferred
- **Pause protection:** "Pause protection" button on tray menu shows placeholder message
