# BioCentri Desktop App — Implementation Plan

> **Status:** Pre-Milestone-1. Awaiting founder approval.
> **Audience:** Future AI coding sessions and human contributors.
> **Supersedes:** nothing. **Conflicts:** none. Locked decisions stay in
> `docs/DECISIONS.md`. This plan operationalises them.

---

## 0. Audit summary (what was inspected)

Inspected before writing this plan, per the founder's standing rule:

- `docs/PROJECT_BIBLE.md` — vision, principles, non-goals (source of truth on
  intent; in case of conflict, wins over everything in this plan).
- `docs/PRODUCT_REQUIREMENTS.md` — FR-1..7, NFRs, success criteria.
- `docs/FEATURE_ROADMAP.md` — phase split; MVP is Phase 1 only.
- `docs/DECISIONS.md` — locked technical decisions (1-5). Already chose
  **WPF on .NET 8 + Wpf.Ui**, **C#**, **WinRT Hello via
  `UserConsentVerifier`**, **user-mode process monitor + foreground
  challenge modal**, **MVVM**, **Windows Credential Manager / DPAPI for
  secrets**, **zero outbound network in v1**.
- `docs/TASKS.md` — TASK-004 ("plan MVP architecture") is satisfied by
  this document; TASK-005 ("first prototype — app discovery + protection
  toggle") is the next item of work after plan approval.
- `website/` — production-ready and **frozen**. Treated as visual
  reference only: typography, palette, spacing, motion, atmosphere
  primitives all derived from `website/tailwind.config.js`,
  `website/src/motion.js`, `website/src/index.css`.
- `app/` — **empty** apart from a `.gitkeep`. Nothing to preserve. No
  obsolete code to remove. Starting clean.

---

## 1. Architecture at a glance

- **One WPF host project** for v1: `BioCentri.App`. Multi-project splits
  (Core / Contracts / Tests) are added at Milestone 5 when the auth
  surface needs testability guarantees, not before. Premature splitting
  is friction.
- **MVVM** end-to-end via **CommunityToolkit.Mvvm** (`[ObservableProperty]`,
  `[RelayCommand]`, `INavigationService`). Code-behind is reserved for
  shell lifecycle (`App.xaml.cs`, splash, dispatcher plumbing) **only**.
- **No service install, no driver, no admin elevation** for v1. BioCentri
  is a regular user-mode Windows app that publishes **a tray icon** and
  reacts to `Win32_ProcessStartTrace` events.
- **Local store**: protected app list → a single JSON file in
  `%LOCALAPPDATA%\BioCentri\store.json`. Auth-related secrets (none in v1,
  reserve path) → Windows Credential Manager via `CredentialManager` NuGet.
  Atomic write-through with file lock.
- **Zero network calls in v1.** No telemetry, no analytics, no auto-update
  ping. A `NetworkGuard` static guard refuses any code path that touches
  `HttpClient`/`Socket` — enforced in code review checklist.
- **Visual parity with website** through XAML resource dictionaries that
  mirror `tailwind.config.js`. Every motion primitive gets a WPF
  counterpart defined once in `app/src/styles/Motion.xaml` / `.cs`.

---

## 2. Folder layout (v1, MVP)

The founder's asked-for root is `app/`. Inside, **a single WPF solution**.
Every folder has a documented purpose; nothing is speculative.

```
app/
├── BioCentri.sln
├── README.md                         ← stack, how to build, how to run
├── global.json                       ← pins .NET SDK 8.x
├── .editorconfig                     ← style
├── Directory.Build.props             ← NuGet versions, analyzers, warnings-as-errors
│
├── BioCentri.App/
│   ├── BioCentri.App.csproj         (net8.0-windows10.0.19041.0, WPF, WinRT)
│   ├── App.xaml / App.xaml.cs       ← composition root, theme, tray host
│   ├── Program.cs                   ← entry, single-instance, unhandled-exception handlers
│   ├── AssemblyInfo.cs
│   │
│   ├── src/
│   │   ├── assets/                  ← binary resources ONLY (icons, splash, font binaries)
│   │   │   ├── icons/               app icons (Splash.png, Tray.ico, App.ico)
│   │   │   ├── fonts/               Inter, Plus Jakarta Sans binaries
│   │   │   └── audio/               (reserved; not used in v1)
│   │   │
│   │   ├── components/              ← reusable cross-cutting WPF UI primitives.
│   │   │   │                         NO business logic. NO feature state.
│   │   │   ├── surface/             FocalCard, GlassSurface, MicaWindow chrome, Divider.
│   │   │   ├── inputs/              TextField, Toggle, SegmentedControl, Slider, SearchBox primitives.
│   │   │   ├── feedback/            Toast, Snackbar, EmptyState, LoadingState, ErrorBanner, ProgressRing.
│   │   │   ├── motion/              TransitionHost, FadeInUp, BorderTrace, HologramFloat, ReticleRing.
│   │   │   ├── nav/                 SidebarItem, TopBarButton, Breadcrumb, NavIndicator.
│   │   │   └── iconography/         Icon (single control, vector path source of truth).
│   │   │
│   │   ├── features/                ← vertical slices. Each owns its pages, VMs, and feature-only helpers.
│   │   │   ├── onboarding/          WelcomeWindow, OnboardingShell, OnboardingViewModel, steps.
│   │   │   ├── dashboard/           DashboardPage, DashboardViewModel, MetricCard, RecentActivityList.
│   │   │   ├── protected-apps/      ProtectedAppsPage, AddApplicationDialog, ProtectedAppDetail,
│   │   │   │                         ProtectedAppsViewModel, AppRow, ProtectionToggle.
│   │   │   ├── hello/               HelloChallengeWindow (topmost modal), HelloService adapter,
│   │   │   │                         HelloOutcomeViewModel, FailedAttemptsViewModel.
│   │   │   ├── settings/            SettingsPage, SettingsViewModel, DefaultAuthMethodControl,
│   │   │   │                         ProtectedAppsManagementLink.
│   │   │   └── notifications/       NotificationCenterPage, NotificationItem, ToastDispatcher.
│   │   │
│   │   ├── windows/                 ← Window subclasses that are not pages and not generic.
│   │   │   ├── SplashWindow.xaml
│   │   │   ├── MainWindow.xaml      ← primary shell (sidebar + topbar + routed page)
│   │   │   ├── HelloChallengeWindow.xaml
│   │   │   └── OnboardingWindow.xaml
│   │   │
│   │   ├── hooks/                   ← small dispatcher/lifecycle helpers for VMs and views.
│   │   │   │                         Named "hooks" to mirror the founder's mental model;
│   │   │   │                         here it means `UseXxx` style helpers consumed by a VM.
│   │   │   ├── UseReducedMotion.cs  (mirrors `useReducedMotion` on the website)
│   │   │   ├── UseDispatcherTimer.cs
│   │   │   ├── UseCloseOnEscape.cs
│   │   │   └── UseTopMostFocus.cs
│   │   │
│   │   ├── services/                ← UI-side service facades (interfaces in /types or /lib).
│   │   │   ├── NavigationService.cs (INavigationService impl)
│   │   │   ├── DialogService.cs     (IDialogService impl → ShowMessage, Confirm, OpenAddApp)
│   │   │   ├── ToastService.cs
│   │   │   ├── SearchService.cs
│   │   │   ├── NotificationService.cs
│   │   │   ├── AppLifecycleService.cs (splash → onboarding → main → tray)
│   │   │   └── ProcessMonitor.cs    (Win32_ProcessStartTrace subscriber + WMI fallback)
│   │   │
│   │   ├── lib/                     ← thin wrappers / adapters around 3rd-party libs.
│   │   │   ├── Hello/               HelloInterop (UserConsentVerifier awaiter, marshalled to WPF Dispatcher).
│   │   │   ├── WpfUi/               Theme adapter (Wpf.Ui's Fluent theme bridged into our resource dictionary).
│   │   │   └── NotifyIcon/          Hardcodet.NotifyIcon.Wpf builder (toy icon, flyout menu).
│   │   │
│   │   ├── styles/                  ← visual ONLY. The single source of truth for tokens.
│   │   │   ├── Themes/
│   │   │   │   ├── Dark.xaml        dark theme (only theme for v1)
│   │   │   │   └── HighContrast.xaml maps to Windows High Contrast settings
│   │   │   ├── Brushes.xaml         ink palette, accent, semantic (success/warn/danger),
│   │   │   │                         text gradients, reticle conics, glare gradient.
│   │   │   ├── Typography.xaml      Inter, Plus Jakarta Sans, font sizes, tracking, numeric.
│   │   │   ├── Spacing.xaml         4-px scale (xxs..xxxl) + named gutters.
│   │   │   ├── Shadows.xaml         drop / inset / mica-stand-in (we use System DropShadow).
│   │   │   ├── Motion.xaml          keyframes, easings, durations. Reduced-motion guard hook.
│   │   │   └── Tokens.xaml          merged: the file App.xaml actually MergedDictionaries.
│   │   │
│   │   ├── routing/                 ← page navigation controller; nothing else lives here.
│   │   │   ├── RouteTable.cs        static, compile-time-checked routes
│   │   │   ├── NavigationStore.cs   current VM (DataContext of MainWindow content)
│   │   │   └── NavigationService.cs ← concrete impl
│   │   │
│   │   ├── state/                   ← app-wide ViewModels + stores shared across features.
│   │   │   ├── AppState.cs          single ObservableObject holding current user, settings, etc.
│   │   │   ├── ProtectedAppStore.cs observable list backed by store.json
│   │   │   └── DispatcherHolder.cs  UI thread dispatcher
│   │   │
│   │   ├── types/                   ← POCO / DTO / interface definitions, no logic.
│   │   │   ├── InstalledApp.cs
│   │   │   ├── ProtectedApp.cs
│   │   │   ├── HelloOutcome.cs      enum + reasons
│   │   │   ├── NotificationItem.cs
│   │   │   └── Services/            INavigationService, IDialogService, IHelloService, IAppDiscoveryService,
│   │   │                             IAppLifecycleService, IProcessMonitor
│   │   │
│   │   └── utils/                   ← pure static helpers; no state, no IO unless trivial.
│   │       ├── Paths.cs             %LOCALAPPDATA%\BioCentri\…
│   │       ├── SafeJson.cs          atomic write with file lock
│   │       ├── AtomicFile.cs        tiny wrapper around SafeJson
│   │       ├── IconExtractor.cs     read app icon from .exe (Shell32, byte-buffer to BitmapSource)
│   │       ├── WindowHelpers.cs     centre, topmost, flash taskbar
│   │       └── AssemblyInfo.cs      (internal) version provider — actually lives in /Properties
│   │
│   └── tests/                       ← leave empty at MVP; xUnit project lands in Milestone 5
│
├── BioCentri.Core/                  ← DEFERRED to Milestone 5
│   └── (placeholder README only)
│
└── BioCentri.Tests/                 ← DEFERRED to Milestone 5
    └── (placeholder README only)
```

**Two placeholder projects** are scaffolded as empty `.csproj` stubs at
Milestone 1 to lock the multi-project migration off the critical path.
This is the cheapest way to keep the option open without forcing a split
today. Both contain a `README.md` reading *"Activated at Milestone 5 to
gain HeadlessHelloService testing."*

**Why a single host project for v1?**

- WPF project → Core class-library split forces every WinRT interop call
  to live behind an interface. For MVP-sized code, that's ceremony for
  ceremony's sake.
- We get `Microsoft.Windows.SDK.NET`, `Wpf.Ui`, and `CommunityToolkit.Mvvm`
  in one place where the XAML can see them.
- Milestone 5 is the *first* moment we actually need headless testability
  of `HelloService`. That's when Core spins up for real — not before.

---

## 3. What every folder exists for (terse rationale)

| Folder | Why it exists |
|---|---|
| `app/` | Root for the desktop application. Frozen boundary; website/ untouched. |
| `BioCentri.App/` | Single WPF host. Hosts UI, XAML, WinRT interop. |
| `App.xaml` / `Program.cs` | Composition root, theme injection, single-instance, unhandled exception. |
| `src/assets/` | **Binary** assets only (icons, fonts). CSS/SVG design source stays in `assets/` at repo root for marketing pipeline. |
| `src/components/` | Cross-feature primitives. Anything reusable across ≥2 features lives here. Anything used once lives inside its feature. |
| `src/components/surface/` | Cards, glass, dividers. The visual foundation. |
| `src/components/inputs/` | Form controls that match website feel (text, toggle, segmented). |
| `src/components/feedback/` | Toast, snackbar, empty/loading states. Standardised so each feature is consistent. |
| `src/components/motion/` | WPF Storyboards / VisualAnimations decoded once and reused. |
| `src/components/nav/` | Sidebar items, top-bar buttons, breadcrumbs. |
| `src/components/iconography/` | `Icon` user control — single source of vector icon truth. |
| `src/features/` | Vertical slices. Each folder owns everything only that feature needs. |
| `src/features/onboarding/` | First-run flow. Lives separately because it's the only path where the MainWindow is not the host. |
| `src/features/dashboard/` | FR-Dashboard surface (no dedicated FR in PRD; UX requirement). |
| `src/features/protected-apps/` | FR-1, FR-2, FR-5 work — list, toggle, add, remove, detail. |
| `src/features/hello/` | FR-3, FR-4 — HelloChallengeWindow + adapters. |
| `src/features/settings/` | FR-6 — SettingsPage + sub-controls. |
| `src/features/notifications/` | In-app notification centre + toast surface. |
| `src/windows/` | Window subclasses that are not pages (Splash, Main, Hello, Onboarding). |
| `src/hooks/` | Small `UseXxx` helpers (analogous to React hooks); dispatcher/lifecycle utilities. |
| `src/services/` | UI-thread service facades. Implementations of interfaces declared in `src/types/Services/`. |
| `src/lib/` | Wrappers / adapters around 3rd-party libs (`Wpf.Ui`, `Hardcodet.NotifyIcon.Wpf`, WinRT). |
| `src/styles/` | Resource dictionaries. The single source of truth for tokens. |
| `src/routing/` | Page navigation. Nothing else. |
| `src/state/` | App-wide view-models and observable stores. |
| `src/types/` | POCOs, DTOs, interfaces. No logic. |
| `src/utils/` | Pure static helpers. No state, no IO unless trivial. |
| `BioCentri.Core/`, `BioCentri.Tests/` | Stub projects activated at Milestone 5. |

---

## 4. Visual language → WPF resource mapping

The website's tokens become WPF resources 1:1, with names that make the
mapping obvious in code.

| Website (`tailwind.config.js` / `index.css`) | WPF (XAML resource) |
|---|---|
| `ink.950..400` | `Brushes.Ink.950`..`400` (`SolidColorBrush`) |
| `#c7d2fe → #818cf8 → #c4b5fd` (accent gradient) | `Brushes.Accent.Violet` (`LinearGradientBrush`) |
| `#34d399` (emerald) | `Brushes.Accent.Emerald` |
| `text-gradient` (white→62%) | `Brushes.Text.Primary` (`LinearGradientBrush`) |
| `.glass` (`rgba 255,255,255,0.02` + 12px blur) | `Styles.Glass` (control template on `Border`) |
| `.glass-strong` (same, 20px blur) | `Styles.GlassStrong` |
| `.focal` (radial + linear gradient) | `Styles.Focal` (background on `FocalCard`) |
| `.noisé` (SVG turbulence) | `Brushes.Noise` (`DrawingBrush`, frozen) |
| `.grid-faint` | `Brushes.GridFaint` (`DrawingBrush`, 56×56 tiling) |
| `.grid-iso` | `Brushes.GridIso` |
| `.topography` | `Brushes.Topography` |
| `.border-trace` (animated gradient border) | `Styles.BorderTrace` (`Storyboard` of `Background` offset) |
| `.reticle-ring` (conic gradient) | `Styles.ReticleRing` (`DrawingBrush` rotated via `RotateTransform` + `Storyboard`) |
| `.glare` (diagonal sweep) | `Styles.Glare` (masked gradient, mouse-follow via attached behaviour) |
| `font-sans: Inter`, `font-display: Plus Jakarta Sans` | `Typography.Sans`, `Typography.Display` (`FontFamily`) |
| `letter-spacing -0.025em / -0.045em` | `Typography.Tracking.Display` (`DependencyProperty` on labels) |
| `transition-timing-function out-expo` (`[0.16, 1, 0.3, 1]`) | `Motion.Easing.OutExpo` (`KeySpline`, reusable) |
| `stiffness:300, damping:20` (spring hover) | `Motion.Hover.Spring` (`QuadraticEase` + small `ElasticEase` blend — approximation) |
| `@media (prefers-reduced-motion)` | `Motion.RespectReducedMotion` + `UseReducedMotion` hook |
| `keyframes / animations` (`fade-in-up`, `pulse-glow`, `border-trace`, `hologram-float`, `reticle-spin`, `data-node`, `laser-sweep`, `pipeline-descend`, `caret`, `shimmer`) | `Motion.Animations.*` (`Storyboards` keyed by name) |
| `font-feature-settings: 'ss01', 'cv01', 'cv11'` | `Typography.TextOpts` (`TypographyGroup`) |
| `aria-hidden: true` atmosphere layers | `Styles.AtmosphereLayer` (base class for stacked backgrounds) |

**Themes** (`Themes/Dark.xaml`, `Themes/HighContrast.xaml`) merge only
**brush** / **typography** resources; **motion** and **spacing** are
theme-independent.

**High contrast map**: subscribe to
`SystemParameters.HighContrast`; when `true`, `App.xaml` swaps
`Dark.xaml` for `HighContrast.xaml`. We do not invent our own HC palette;
we honour Windows'.

---

## 5. Project conventions (every file must respect these)

- **C#** with `Nullable enable`, `LangVersion latest`, `ImplicitUsings enable`,
  `TreatWarningsAsErrors true`.
- **File-scoped namespaces**, **one** type per file, file name = type name.
- **MVVM** via `CommunityToolkit.Mvvm` (no Caliburn, no Prism).
- **Code-behind is for shell lifecycle only.** Every Page has zero
  logic in code-behind except animation hooks that cannot live elsewhere.
- **Public API surface** of `BioCentri.App` is annotated with
  `InternalsVisibleTo("BioCentri.Tests")` from Milestone 5 forward.
- **Single instance** enforced via named `Mutex` in `Program.cs`. The
  second launch surfaces a `ShowWindow` instead of starting a new app.
- **No HttpClient.** `src/lib` contains a `NoHttpClientGuard` analyzer-style
  rule posted to `Directory.Build.props`: `CA1054` is escalated; new code
  cannot reference `System.Net.Http` without a justification comment.
- **Naming**: types PascalCase, private fields `_camelCase`, XAML
  resources `PascalCase.Thematic.SubName` (e.g. `Brushes.Ink.700`).
- **XML doc** is mandatory on public types in `src/types/Services/` and on
  any public method the founder might revisit later.
- **XAML** uses self-closing tags where allowed, explicit width/height in
  design surface only, `x:Name` reserved for code-behind use only.

---

## 6. Companion NuGet packages (locked at Milestone 1)

| Package | Why |
|---|---|
| `Wpf.Ui` (≥ 3.x) | Fluent Design / Mica / Acrylic over classic WPF (Decision 1). |
| `CommunityToolkit.Mvvm` | `[ObservableProperty]`, `[RelayCommand]`, DI-friendly base classes. |
| `Microsoft.Windows.SDK.NET` | WinRT projection for `Windows.Security.Credentials.UI.UserConsentVerifier`. |
| `Microsoft.Windows.CsWinRT` | WinRT type projection generator. |
| `Hardcodet.NotifyIcon.Wpf` | Tray icon with XAML-context menu. |
| `CommunityToolkit.Mvvm.DependencyInjection` | `Ioc.Default` for VM→VM wiring (small). |
| `System.Text.Json` (in-box) | Local store persistence. |
| `Nito.AsyncEx` | `AsyncContextMenu` / context for WMI event threading. |
| `xUnit`, `Microsoft.NET.Test.Sdk`, `FluentAssertions` | Milestone 5 — added then, not now. |
| `CredentialManagement` (or direct `CredWrite`/`CredRead` P/Invoke) | Windows Credential Manager wrapper. Decision deferred to Milestone 5; we won't add anything that breaches the reserve. |

No Microsoft.Extensions.Hosting, no Serilog, no MediatR for v1. **Keep
dependencies small.** Each new one requires a Decision-6+ entry in
`docs/DECISIONS.md`.

---

## 7. Milestone file plan

Reuses the founder's 7-milestone split. Per milestone: *what is created*,
*what is modified*, *why*. This is what the founder reviews at the end of
each milestone before approving the next.

### Milestone 1 — Architecture & tech stack

**Goal:** Open the solution, build, and produce an empty main window.

**Created**

- `app/BioCentri.sln`
- `app/global.json` (`.NET 8.0.x`)
- `app/.editorconfig`
- `app/Directory.Build.props`
- `app/README.md` (build/run instructions)
- `app/BioCentri.App/BioCentri.App.csproj`
- `app/BioCentri.App/App.xaml` + `App.xaml.cs` (composition root; merges `Tokens.xaml`)
- `app/BioCentri.App/Program.cs` (single-instance, unhandled exception handlers)
- `app/BioCentri.App/AssemblyInfo.cs`
- `app/BioCentri.App/src/styles/Themes/Dark.xaml`
- `app/BioCentri.App/src/styles/Themes/HighContrast.xaml`
- `app/BioCentri.App/src/styles/Brushes.xaml`
- `app/BioCentri.App/src/styles/Typography.xaml`
- `app/BioCentri.App/src/styles/Spacing.xaml`
- `app/BioCentri.App/src/styles/Shadows.xaml`
- `app/BioCentri.App/src/styles/Motion.xaml`
- `app/BioCentri.App/src/styles/Tokens.xaml` (merged-dictionary root)
- `app/BioCentri.App/src/assets/icons/.gitkeep`
- `app/BioCentri.App/src/assets/fonts/.gitkeep`
- `app/BioCentri.App/src/components/.gitkeep`
- `app/BioCentri.App/src/features/.gitkeep`
- `app/BioCentri.App/src/windows/.gitkeep`
- `app/BioCentri.App/src/hooks/.gitkeep`
- `app/BioCentri.App/src/services/.gitkeep`
- `app/BioCentri.App/src/lib/.gitkeep`
- `app/BioCentri.App/src/routing/.gitkeep`
- `app/BioCentri.App/src/state/.gitkeep`
- `app/BioCentri.App/src/types/.gitkeep`
- `app/BioCentri.App/src/utils/.gitkeep`
- `app/BioCentri.Core/README.md` (deferred)
- `app/BioCentri.Tests/README.md` (deferred)
- `app/BioCentri.App/src/windows/MainWindow.xaml` + `.cs` (empty shell)

**Modified**

- `docs/CHANGELOG.md` — entry for scaffold.
- `docs/DECISIONS.md` — append "Decision 6: Single-project MVP with
  deferred Core/Tests split" referencing this plan.

**Why:** WPF compiles, every folder exists with a `.gitkeep` and a
documented purpose, and styling primitives are named but empty. Future
work has unambiguous homes.

### Milestone 2 — Shell (window, sidebar, top bar, routing, theme)

**Goal:** MainWindow feels like the website. Sidebar, top bar, page host,
routing, theme injection all wired.

**Created**

- `src/components/surface/MicaWindow.cs` (a `Window` base that applies
  Wpf.Ui's `WindowBackdropType.Mica` and respects `SystemParameters.HighContrast`)
- `src/components/nav/SidebarItem.cs`, `Sidebar.cs`
- `src/components/nav/TopBar.cs`, `TopBarButton.cs`
- `src/components/nav/NavIndicator.cs` (the animated active-route indicator)
- `src/components/motion/FadeInUp.cs` (behaviour)
- `src/components/motion/TransitionHost.cs` (page transition wrapper)
- `src/routing/RouteTable.cs`
- `src/routing/NavigationStore.cs`
- `src/types/Services/INavigationService.cs`
- `src/services/NavigationService.cs`
- `src/types/IAppLifecycleService.cs`
- `src/services/AppLifecycleService.cs`
- `src/state/AppState.cs`
- `src/windows/MainWindow.xaml` / `.cs` (sidebar + topbar + `Frame` for `Page`s)
- `src/components/feedback/LoadingState.cs` (default for content area)
- `src/components/feedback/EmptyState.cs` (default for empty feature pages)
- `src/components/iconography/Icon.cs` (single XAML `ContentControl` driven by `Geometry` paths)
- `src/hooks/UseReducedMotion.cs`

**Modified**

- `src/windows/MainWindow.xaml`: replaces skeleton with shell chrome.
- `App.xaml`: injects theme selection + DI container.
- `src/types/Services/`: namespace introduction.

**Why:** the shell is the longest-lived surface. Build it once and
correctly. Every later feature just routes to it.

### Milestone 3 — Dashboard

**Goal:** DashboardPage is the home route. Calm, metric-led, premium —
visual parity with the website's BentoFeatures / Metrics sections.

**Created**

- `src/features/dashboard/DashboardPage.xaml(.cs)`
- `src/features/dashboard/DashboardViewModel.cs`
- `src/features/dashboard/MetricCard.cs` (control)
- `src/features/dashboard/RecentActivityList.cs` (placeholder list, will
  be wired later)
- `src/services/ToastService.cs` + `IToastService`
- `src/types/NotificationItem.cs`
- `src/components/surface/FocalCard.cs`
- `src/components/surface/GlassSurface.cs`
- `src/components/motion/HologramFloat.cs`
- `src/components/motion/ReticleRing.cs`
- `src/components/motion/BorderTrace.cs`
- `src/lib/WpfUi/ThemeAdapter.cs` (boot strap to Wpf.Ui's Fluent theme merged with our tokens)

**Why:** proves the visual system end-to-end with the most expressive
surface (reticle, hologram, bento metrics). Once this looks right, every
other page inherits the language.

### Milestone 4 — Protected Apps (UI)

**Goal:** List, add, remove, toggle — without auth interception. See,
decide, manage. **`TASK-005` lands here per `docs/TASKS.md`.**

**Created**

- `src/types/InstalledApp.cs`
- `src/types/ProtectedApp.cs`
- `src/types/Services/IAppDiscoveryService.cs`
- `src/types/Services/IProtectedAppStore.cs`
- `src/services/AppDiscoveryService.cs` (uses `PackageManager` for
  packaged apps, `MsiEnumProductsEx` for MSI, scans
  `HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall` and
  `HKCU` equivalents for traditional apps, plus Start-menu shortcuts via
  `IShellItem`)
- `src/services/ProtectedAppStore.cs`
- `src/state/ProtectedAppStore.cs` (observable wrapper)
- `src/features/protected-apps/ProtectedAppsPage.xaml(.cs)`
- `src/features/protected-apps/ProtectedAppsViewModel.cs`
- `src/features/protected-apps/AppRow.cs` (ListBoxItem template)
- `src/features/protected-apps/AddApplicationDialog.xaml(.cs)`
- `src/features/protected-apps/AddApplicationViewModel.cs`
- `src/features/protected-apps/ProtectionToggle.cs` (a ThemedToggle
  built on components/inputs)
- `src/services/DialogService.cs` + `IDialogService`
- `src/services/SearchService.cs` + `ISearchService` (used inside AddApplicationDialog)
- `src/components/inputs/TextField.cs`
- `src/components/inputs/Toggle.cs`
- `src/components/inputs/SearchBox.cs`
- `src/utils/IconExtractor.cs` (exposes BitmapSource for thumbnails)
- `src/utils/Paths.cs`
- `src/utils/SafeJson.cs`

**Modified**

- `src/components/feedback/EmptyState.cs` is referenced by ProtectedAppsPage
  when no apps are protected.
- `src/routing/RouteTable.cs` registers `protected-apps` and
  `protected-apps/add` routes.

**Why:** Most of FR-1, FR-2, FR-5 are handled here, before Hello
interception. This is the test-the-leak-lightly moment: store + UI without
security surface yet.

### Milestone 5 — Windows Hello (interception infrastructure)

**Goal:** Hello gate works in isolation. Modal challenge is wired,
adapter handles `UserConsentVerifier`, outcome is observed. **The
`BioCentri.Core` and `BioCentri.Tests` projects activate now** to gain
headless test coverage of the hello path.

**Created**

- `BioCentri.Core/BioCentri.Core.csproj`
- `BioCentri.Core/src/Services/HelloService.cs` + `IHelloService`
- `BioCentri.Core/src/Model/HelloOutcome.cs`
- `BioCentri.Core/src/Interop/UserConsentVerifierAdapter.cs` (calls the
  WinRT API via `Microsoft.Windows.SDK.NET`/`CsWinRT`)
- `BioCentri.Tests/BioCentri.Tests.csproj`
- `BioCentri.Tests/src/Unit/HelloServiceTests.cs` (uses a fake outcome
  source)
- `BioCentri.App/src/lib/Hello/HelloInterop.cs` (Dispatcher-aware wrapper
  around `HelloService`)
- `BioCentri.App/src/features/hello/HelloChallengeWindow.xaml(.cs)`
- `BioCentri.App/src/features/hello/HelloChallengeViewModel.cs`
- `BioCentri.App/src/features/hello/FailedAttemptsViewModel.cs`
- `BioCentri.App/src/features/hello/AuditLogSink.cs` (writes
  `%LOCALAPPDATA%\BioCentri\hello.log` — local-only, opt-out later)
- `BioCentri.App/src/state/HoldOnHelloGuard.cs` (UI helper that suspends
  owner-window input during challenge)

**Modified**

- `BioCentri.App.csproj` references `BioCentri.Core`.
- `src/types/Services/` exposes the hello interfaces to the UI.
- `src/services/NavigationService` does not change; the modal lives in `features/hello/`.

**Why:** hello is the most security-sensitive surface. We isolate it in
Core so we can test it without spinning up WPF. Rate-limiting and the
local audit log live here too.

### Milestone 6 — App-locking logic

**Goal:** A protected-app launch triggers the challenge modal. FR-4
becomes a real, testable behaviour.

**Created**

- `src/types/Services/IProcessMonitor.cs`
- `src/services/ProcessMonitor.cs`
- `src/services/AppLockController.cs`
- `src/lib/NotifyIcon/TrayIconBuilder.cs` (uses `Hardcodet.NotifyIcon.Wpf`)
- `src/components/feedback/Toast.cs` (Wpf.Ui's `Snackbar` adapter)
- `src/utils/WindowHelpers.cs`
- `src/state/PendingAuthRegistry.cs` (tracks which protected PID is awaiting)
- `src/state/DispatcherHolder.cs`

**Modified**

- `App.xaml.cs` registers the tray on startup; closes the splash on
  completion; opens the main window.
- `src/components/feedback/EmptyState.cs` extended with auth-fail
  messages.
- `src/components/feedback/ErrorBanner.cs` (new sub-control) for
  Hello-failed UX.
- `src/features/protected-apps/AddApplicationDialog` shows live protection
  status once locked.

**Why:** this is the moment that needs human end-to-end testing on a
clean Windows 11 profile (face, fingerprint, PIN, failure path) per
`docs/DECISIONS.md` Decision 5 rule #2. Stop here for a manual QA pass
before Milestone 7 starts.

### Milestone 7 — Polish, optimise, test

**Goal:** meet every MVP success criterion in `PRODUCT_REQUIREMENTS.md`.
Ship SmartScreen-clean.

**Created / modified (selected)**

- Add accessibility pass: tab order, high-contrast, screen reader,
  reduced-motion. Honour `SystemParameters.HighContrast`.
- Add installer project (`Microsoft Visual Studio Installer Projects` or
  WiX 3 → `BioCentriSetup.msi`) → `FR-7`.
- Add code-signing hook for SmartScreen → `FR-7`/`MVP-6`.
- Reduce-motion audit on every animation surface.
- p95 launch overhead measurement, idle CPU testing → `NFR-Performance`.
- Manual test script for hello paths (face / fingerprint / PIN / fail /
  timeout) per `docs/DECISIONS.md` rule #2.
- Manual test script for "under 2-minute install" per MVP-1.
- Manual test script for "zero outbound network calls during a fresh
  protected-app launch" per MVP-4 (NetworkGuard already enforces, we add
  a CI-side assertion).

---

## 8. FR mapping (one row, one truth)

| FR | Owner files |
|---|---|
| **FR-1 — App discovery** | `src/services/AppDiscoveryService.cs`, `src/features/protected-apps/AddApplicationDialog.xaml.cs`, `src/features/protected-apps/AddApplicationViewModel.cs`, `src/utils/IconExtractor.cs`. Acceptance test: every installed user app is shown with a name+icon. |
| **FR-2 — Protection toggle** | `src/features/protected-apps/ProtectionToggle.cs`, `src/features/protected-apps/ProtectedAppsViewModel.cs`, `src/state/ProtectedAppStore.cs`. Acceptance test: toggle off removes enforcement within the same session. |
| **FR-3 — Windows Hello authentication** | `BioCentri.Core/src/Services/HelloService.cs`, `src/lib/Hello/HelloInterop.cs`, `src/features/hello/HelloChallengeWindow.xaml.cs`. Acceptance test: face / fingerprint / PIN each unlock. |
| **FR-4 — Authentication enforcement** | `src/services/ProcessMonitor.cs`, `src/services/AppLockController.cs`, `src/state/PendingAuthRegistry.cs`, `src/features/hello/HelloChallengeWindow.xaml.cs`. Acceptance test: no Hello → no launch. |
| **FR-5 — Protected apps management UI** | `src/features/protected-apps/ProtectedAppsPage.xaml.cs`, `AddApplicationDialog`, `src/state/ProtectedAppStore.cs`. Acceptance test: every entry has a remove control; removal takes effect without reinstall. |
| **FR-6 — Settings screen** | `src/features/settings/SettingsPage.xaml.cs`, `SettingsViewModel.cs`, `src/state/AppState.cs`. Acceptance test: every setting reachable in ≤ 2 clicks from home. |
| **FR-7 — Native Windows packaging** | `app/BioCentriSetup/` (WiX or VS Installer Projects), code-signing script. Acceptance test: clean W10/W11 install completes in ≤ 2 min. |

PRs must reference FR / NFR ids per `docs/DECISIONS.md` Decision 5 rule #8.

---

## 9. UI surface inventory (all 14 required)

| Surface | Home | Notes |
|---|---|---|
| Splash | `src/windows/SplashWindow.xaml` | Renders during early boot, hides on composition root ready. |
| Onboarding | `src/windows/OnboardingWindow.xaml`, `src/features/onboarding/` | First-run only. Skipped forever after. Persisted flag in `AppState`. |
| Dashboard | `src/features/dashboard/` | Home route `/`. |
| Protected apps | `src/features/protected-apps/` | Route `/protected-apps`. |
| Add application | `src/features/protected-apps/AddApplicationDialog.xaml` | Modal. |
| Windows Hello flow | `src/features/hello/HelloChallengeWindow.xaml` | Topmost, focused, with `state/PendingAuthRegistry`. |
| Settings | `src/features/settings/` | Route `/settings`. |
| Notifications | `src/features/notifications/` + `components/feedback/Toast.cs` | Centre route + ghost toasts. |
| Tray menu | `src/lib/NotifyIcon/TrayIconBuilder.cs` + `NotifyIconMenu.xaml` | Open / Pause / Settings / Quit. |
| Dialogs | `src/services/DialogService.cs` | Confirm / Info / AddApp. |
| Search | `src/components/inputs/SearchBox.cs` + `src/services/SearchService.cs` | Used in AddApplicationDialog + ProtectedAppsPage filter. |
| Animations | `src/styles/Motion.xaml` + `src/components/motion/*` | Every visual transition goes through one of these. |
| Empty states | `src/components/feedback/EmptyState.cs` | Reused across every list page. |
| Loading states | `src/components/feedback/LoadingState.cs` | Shimmer + reticle variants. |

---

## 10. Anti-pattern traps (early warnings)

This layout **prevents** the things that turn desktop projects into
generic Electron-shaped mush:

| Risk | How we prevent it |
|---|---|
| Duplicate components | `src/components/` is the only place cross-feature UI lives. Code review rule: if a thing is used by ≥2 features, it must move up from `features/`. |
| Logic in code-behind | MVVM via CommunityToolkit.Mvvm. Code-behind reserved for shell + animation hooks. Reviewers reject PRs with non-trivial code-behind. |
| Generic template vibes | Theme resources overwrite Wpf.Ui's defaults. Mica/Acrylic material lifted from website's atmosphere primitives. No "Acrylic over a flat gray." |
| Style duplication | All brushes/typography/spacing/motion in `src/styles/`. Components reference resources by key. No `#xxxxxx` literals outside `Brushes.xaml`. |
| Business logic in services leaking | WinRT interop lives in `src/lib/Hello/`. Business semantics live in `src/services/` (Pure C#). |
| Hidden network calls | `NoHttpClientGuard` rule in `Directory.Build.props` + PR checklist + a CI grep. |
| Mixing Pages and Windows | `windows/` for non-page Window subclasses; `features/` for Pages; navigation only navigates to Pages. |
| Big silent rewrites | Milestone gates. Each milestone produces a written summary of files created/modified. PRs reference FR ids. |
| Hard-coded strings | `strings.resx` lives at `src/state/Strings.resx`. Fluent columns always. |
| Per-page styles | Every page uses `StaticResource` only. No new XAML resource dictionaries per page. |

---

## 11. Open decisions before Milestone 1 can start

These are the questions the founder should answer before any code lands.
Default answers are listed for speed of review; **explicit "other" is
appreciated**.

1. **Single-project approval.** Confirm one WPF project for v1 with
   `Core`/`Tests` spinning up at Milestone 5. (Default: yes.)
2. **Package manager / installer.** WiX 3.14 (mature, XAML-light) vs
   `Microsoft Visual Studio Installer Projects` (Visual Studio native,
   less scriptable). (Default: WiX 3, scripted.)
3. **Code-signing path.** Microsoft EV cert (paid) vs Azure Trusted
   Signing (cloud, pay-as-you-go) for SmartScreen-1-time. (Default:
   Azure Trusted Signing.)
4. **Telemetry guard level.** Static analyzers-only (current) vs full
   runtime `NetworkGuard` proxy that bans `HttpClient`/`Socket` instances.
   (Default: analyzers + CI grep in v1; runtime proxy deferred to Phase 2.)

Document the answers inline in `docs/DECISIONS.md` as "Decision N+1",
"Decision N+2", … per the appendix-only rule.

---

## 12. Order of next moves

1. Founder reviews this plan, answers the four open decisions.
2. Milestone 1 executes (scaffold + tokens). Ends with a buildable empty
   shell and a written milestone summary.
3. Milestone 2 (shell + routing) executes. Ends with the visual language
   verifiable in a single window.
4. …and so on through Milestone 7, with the founder reviewing each.

A milestone is **done** when (a) its written summary is in
`docs/CHANGELOG.md` and (b) every FR referenced by that milestone has at
least one automated test in `BioCentri.Tests` (added in Milestone 5+).

---

_Last drafted before Milestone 1. Treat as a working document; supersede
in-place when reality disagrees with the plan._
