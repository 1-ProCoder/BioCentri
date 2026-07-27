# BioCentri — Installer & Deployment Checklist

> Status: **pre-release.** This document tracks the remaining production-readiness
> steps for shipping BioCentri as a signed, SmartScreen-clean .msi installer.

---

## WiX 3.14 Installer (.msi)

### Prerequisites
- [ ] Install WiX Toolset 3.14 (`wix311-binaries.zip` or `wix311.exe`)
- [ ] Add `%WIX%\bin` to `PATH` so `candle.exe` and `light.exe` are available
- [ ] Create `app/BioCentri.Setup/BioCentri.Setup.wixproj` (WiX MSBuild project)

### .wxs authoring checklist
- [ ] `<Product>`: `Id="*"`, `Name="BioCentri"`, `Manufacturer="BioCentri"`, `UpgradeCode` (stable GUID)
- [ ] `<Package>`: `InstallerVersion="500"`, `Compressed="yes"`, `InstallScope="perMachine"`
- [ ] `<MajorUpgrade>`: `DowngradeErrorMessage` for blocked downgrades
- [ ] `<Directory>`: `ProgramFilesFolder` → `BioCentri` → `{app, assets, styles}`
- [ ] `<ComponentGroup>`: every `.dll`, `.exe`, `.xaml`, `.json`, `.ico` from publish output
- [ ] `<Feature>`: `Title="BioCentri"`, `Level="1"`, `ConfigurableDirectory="INSTALLFOLDER"`
- [ ] `<Icon>`: `Id="BioCentri.ico"` — embed a 256×256 .ico
- [ ] `<Shortcut>`: Start Menu shortcut → `BioCentri.App.exe`
- [ ] `<Property>`: `ARPCONTACT="support@biocentri.app"`

### Build
- [ ] `dotnet publish app/BioCentri.App -c Release -o app/publish`
- [ ] `candle.exe BioCentri.Setup.wxs -o obj\`
- [ ] `light.exe obj\BioCentri.Setup.wixobj -o bin\BioCentri.msi`

---

## Code Signing

### EV Certificate (Option A)
- [ ] Obtain Extended Validation (EV) code-signing certificate (DigiCert, Sectigo, etc.)
- [ ] Store EV certificate on a hardware token (USB key)

### Azure Trusted Signing (Option B)
- [ ] Create Azure Trusted Signing account + certificate profile
- [ ] Install `Microsoft.Trusted.Signing.Client` NuGet or `signtool.exe` plug-in
- [ ] Sign: `signtool sign /fd SHA256 /tr http://timestamp.acs.microsoft.com /td SHA256 /v BioCentri.msi`

### Verification
- [ ] `signtool verify /pa BioCentri.msi` → "Successfully verified"
- [ ] Right-click `.msi` → Properties → Digital Signatures → certificate chain is intact

---

## SmartScreen Readiness

> SmartScreen trust builds over time — it is not a one-time configuration.

- [ ] Submit `BioCentri.msi` to [Microsoft SmartScreen](https://www.microsoft.com/en-us/wdsi/filesubmission) for initial reputation seeding
- [ ] Distribute `BioCentri.msi` through a known HTTPS domain (`https://biocentri.app/download`)
- [ ] Accumulate downloads + scans over several weeks before v1 launch
- [ ] Monitor reputation via [Microsoft Defender Portal](https://security.microsoft.com)

### First-user experience
- [ ] On first download, SmartScreen shows "Windows protected your PC" warning
- [ ] After reputation builds (~30 days, ~1k downloads), SmartScreen shows no warning

---

## Tray Icon (.ico asset)

- [ ] Create a 256×256 indigo BioCentri `.ico` (multi-resolution: 16/32/48/256)
- [ ] Place at `app/BioCentri.App/src/assets/icons/Tray.ico`
- [ ] Add `H.NotifyIcon.Wpf` NuGet (2.0.124+ for net8.0-windows)
- [ ] Wire `TrayIconViewModel.cs` in `App.xaml.cs` (task deferred to M7.1 — see `src/windows/TrayIconViewModel.cs`)

---

## Publish Profile

- [ ] `.pubxml` in `app/BioCentri.App/Properties/PublishProfiles/`
- [ ] `PublishSingleFile=true`, `SelfContained=false`, `RuntimeIdentifier=win-x64`
- [ ] Publish output → `app/publish/` → consumed by WiX

---

## Versioning

- [ ] `BioCentri.App.csproj`: `<Version>1.0.0</Version>`, `<FileVersion>1.0.0.0</FileVersion>`
- [ ] `BioCentri.Core.csproj`: same version
- [ ] `BioCentri.Tests.csproj`: same version
- [ ] `app/CHANGELOG.md`: release entry for v1.0.0
