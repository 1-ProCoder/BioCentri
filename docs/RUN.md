# How to Run BioCentri

> Quick-start guide for building and launching the BioCentri WPF desktop application.

## Prerequisites

- **Windows 10 (build 17763+) or Windows 11**
- **.NET 8 SDK** (8.0.x) — [download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Visual Studio 2022+** (optional, for debugging) or **VS Code** with C# extension

## Quick start

```powershell
# From the repository root (C:\Users\Princ\BioCentri):

# 1. Build all 3 projects
dotnet build app/BioCentri.sln -c Debug

# 2. Run the app
dotnet run --project app/BioCentri.App/BioCentri.App.csproj
```

## Launch directly (double-click)

After building, the executable is at:

```
app/BioCentri.App/bin/Debug/net8.0-windows10.0.19041.0/BioCentri.App.exe
```

Double-click `BioCentri.App.exe` in File Explorer to launch.

## Run the tests

```powershell
dotnet test app/BioCentri.sln
```

Expected output: **7 passed, 0 failed, 0 skipped**

## Publish (release build)

```powershell
dotnet publish app/BioCentri.App/BioCentri.App.csproj -c Release -o app/publish
```

Produces a single-file executable at `app/publish/BioCentri.App.exe`.

## Troubleshooting

| Issue | Fix |
|---|---|
| `CS0246: type not found` or `NU1603` | Run `dotnet restore app/BioCentri.sln` first |
| App crashes before window appears | Wipe `bin/` and `obj/` folders, then rebuild |
| Windows Hello not available | Enroll in Windows Hello via Settings → Accounts → Sign-in options |
| No apps listed in picker | The app scans the Windows registry for installed programs. Some portable apps won't appear |
