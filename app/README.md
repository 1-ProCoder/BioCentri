# BioCentri — Desktop Application

> **Status:** Milestone 1 — foundation scaffold only. No application features yet.

This folder hosts the BioCentri Windows desktop application: a WPF app on
.NET 8, layered with the **Wpf.Ui** Fluent Design surface, using
**CommunityToolkit.Mvvm** and **Microsoft.Extensions.DependencyInjection**.

---

## Requirements

| Tool | Version | Notes |
|---|---|---|
| Windows | 10 19041+ or Windows 11 | WPF + WinRT require this baseline. |
| .NET SDK | 8.0.x | Pinned in `global.json` (`8.0.420`). |
| IDE | Visual Studio 2022 17.10+ _or_ JetBrains Rider 2024.3+ | Either works. |

No Visual Studio installed-workload is required — `Microsoft.WindowsDesktop.App`
is already part of the .NET SDK install on Windows.

---

## Build & run

From this folder (`app/`):

```powershell
dotnet restore
dotnet build -c Debug
dotnet run --project BioCentri.App
```

Or open `BioCentri.sln` in Visual Studio / Rider and F5.

The first run shows a single window titled **"BioCentri"** with the dark
Mica backdrop and the foundation-ready placeholder text. Nothing else is
wired up yet — that is by design ([`docs/FEATURE_ROADMAP.md`](../docs/FEATURE_ROADMAP.md)).
Milestone 2 onward fills the shell, dashboard, protected apps, Hello,
locking, and signing.

---

## Projects

| Project | Status | Purpose |
|---|---|---|
| `BioCentri.App` | active | The WPF host. UI, XAML, WinRT interop. |
| `BioCentri.Core` | **deferred** (placeholder README only) | Headless no-UI services for testability. Activated at Milestone 5. |
| `BioCentri.Tests` | **deferred** (placeholder README only) | xUnit test project. Activated at Milestone 5. |

The deferred splits are documented in `IMPLEMENTATION_PLAN.md` §11.

---

## Architecture in one minute

- **MVVM** via CommunityToolkit.Mvvm (`[ObservableProperty]`, `[RelayCommand]`).
- **DI** is bootstrapped in `App.OnStartup`. The container lives in
  `App.Services` for lightweight lookup; constructor injection is preferred.
- **Theming** is one merged dictionary chain rooted at
  `src/styles/Tokens.xaml`. Theme switches live in
  `src/styles/Themes/Dark.xaml` and `HighContrast.xaml`. The website's
  Tailwind palette maps 1:1 onto WPF resource keys — see
  `IMPLEMENTATION_PLAN.md` §4.
- **Single-instance / signed installer** are intentionally Milestone 6 and 7.

---

## Coding rules (enforced by `Directory.Build.props`)

- Nullable + warnings-as-errors.
- File-scoped namespaces.
- One type per file, file name = type name.
- Public API gets `///` doc comments.
- **No** `HttpClient` / `Socket` references in `BioCentri.App` —
  priv/sep guard, see `docs/DECISIONS.md` Decision 6.

---

## Where things live

See [`IMPLEMENTATION_PLAN.md`](IMPLEMENTATION_PLAN.md) for the canonical
folder rationale and milestone-file map.
