# Changelog

All notable changes to BioCentri will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project will adhere to [Semantic Versioning](https://semver.org/spec/v2.0.0.html)
once v1 ships.

---

## Unreleased

### Added
- Initial BioCentri project foundation:
  - Repository scaffold: `app/`, `website/`, `api/`, `extension/`, `docs/`,
    `assets/`.
  - Documentation foundation: `PROJECT_BIBLE.md`, `PRODUCT_REQUIREMENTS.md`,
    `FEATURE_ROADMAP.md`, `TASKS.md`, and this `CHANGELOG.md`.
- **Milestone 1 — architecture & tech stack** (`app/`):
  - Solution scaffold: `BioCentri.sln`, `global.json`,
    `.editorconfig`, `Directory.Build.props`, `NuGet.config`,
    `.gitignore`, `app/README.md`.
  - Single WPF host project: `BioCentri.App` on `net8.0-windows10.0.19041.0`
    with WPF + WinRT projection targets.
  - Theming system: `Brushes.xaml`, `Typography.xaml`, `Spacing.xaml`,
    `Shadows.xaml`, `Motion.xaml`, plus `Tokens.xaml` and
    `Themes/Dark.xaml`, `Themes/HighContrast.xaml`. The website's
    Tailwind/Framer-Motion tokens map 1:1 onto WPF resource keys.
  - DI infrastructure (`Microsoft.Extensions.DependencyInjection`):
    `ServiceHost.Build(Dispatcher)` is the single point of registration;
    `IDispatcher`, `IAppLifecycleService`, `DispatcherHolder`,
    `AppLifecycleService` are wired.
  - MVVM infrastructure: CommunityToolkit.Mvvm 8.4 referenced;
    `ObservableViewModelBase` placeholder for navigation-aware VMs in M2.
  - Empty `MainWindow.xaml` shell that opens on startup, demonstrating
    the theme + typography + DI bootstrap end-to-end.
  - `UseReducedMotion` hook.
  - Placeholder READMEs for `BioCentri.Core` and `BioCentri.Tests`
    (deferred to Milestone 5).
  - Documented architecture in `app/IMPLEMENTATION_PLAN.md`.

### Notes
- No application features yet. Splash, onboarding, dashboard, protected
  apps, Hello, locking, signing — all in subsequent milestones per
  `app/IMPLEMENTATION_PLAN.md` §7.

---

## Release template

When cutting a release, copy the block below, rename `X.Y.Z` to the new
version, set the date, and fill in only the categories that have meaningful
content.

```markdown
## [X.Y.Z] — YYYY-MM-DD

### Added
- ...

### Changed
- ...

### Fixed
- ...

### Removed
- ...

### Security
- ...
```

---

---



## Milestone 6 — App-locking (process monitoring + enforcement) (2026-08)

The stubs are replaced: `StubProcessMonitor` → real WMI `Win32_ProcessStartTrace`
subscriber with 5 s polling fallback; `StubAuthAppRules` → `FileBackedAuthAppRules`
reading the same `protectedApps.json` the Protected Apps UI writes;
a new `AppLockController` kills blocked processes after a failed biometric
challenge. ProcessWatcher now enforces, not just logs. Tray icon deferred
to M7.

### Added
- **`services/ProcessMonitor.cs`** — real `IProcessMonitor` over WMI
  `ManagementEventWatcher` (`SELECT * FROM Win32_ProcessStartTrace`) +
  5 s polling fallback via `System.Timers.Timer` + `Process.GetProcesses()`.
  Dedupes by PID (`HashSet<int>` + lock). `IDisposable` — `Stop()` unsubscribes
  WMI and disposes the poller timer. Implements the full `IProcessMonitor`
  contract (`Start`, `Stop`, `ProcessLaunchDetected` event).
- **`services/FileBackedAuthAppRules.cs`** — real `IAuthAppRules` that
  reads the canonical `protectedApps.json` via `ILocalJsonStore.StorageRoot`
  (path) and direct `File.ReadAllText` (sync I/O — because `IsProtected`
  runs on a background thread and must be O(1)). Last-write-time TTL
  cache: reloads only when the file changed. Suffix-matches candidate
  process names against normalized cache entries (case-insensitive).
  Inner `ProtectedAppsFile` POCO matches the ViewModel's schema; a
  `static readonly JsonSerializerOptions` instance avoids CA1869.
- **`services/AppLockController.cs`** — kills blocked processes.
  `Kill(int pid, string displayName, string? outcome)`: graceful close
  (`CloseMainWindow()` + 2 s wait) → force (`Process.Kill()`). Catches
  `ArgumentException` / `InvalidOperationException` / `Win32Exception`
  for already-exited processes. Emits a warning toast on block.
- **`windows/TrayIconViewModel.cs`** — Show / Hide / Pause / Settings /
  Quit commands for the tray icon context menu. Not wired to a live
  `H.NotifyIcon.TaskbarIcon` yet (M7 installs the NuGet + .ico asset +
  binds this VM).

### Modified
- **`services/ProcessWatcher.cs`** — ctor now takes 7 args (adds
  `AppLockController`). `HandleProtectedLaunchAsync` calls `_lock.Kill()`
  in the block path (after auth failure). Allow path unchanged.
- **`App.xaml.cs`** — swapped `StubProcessMonitor` → `ProcessMonitor`,
  `StubAuthAppRules.Defaults()` → `FileBackedAuthAppRules(host.Get<ILocalJsonStore>())`,
  registered `AppLockController` before `ProcessWatcher` (DI ordering),
  updated `ProcessWatcher` factory to 7-arg ctor. Removed stubs entirely.
  Tray icon creation commented out (deferred to M7 pending `.ico` asset
  + `H.NotifyIcon.Wpf` NuGet resolution).
- **`BioCentri.App.csproj`** — added `System.Management` 8.0.0
  (NuGet) for `ManagementEventWatcher` / WMI. `H.NotifyIcon.Wpf` removed
  (will re-land in M7).

### Notes
- Build: `dotnet build app/BioCentri.sln -c Debug` reports
  **0 errors, 0 warnings** after full bin/obj purge + clean restore +
  no-incremental rebuild (3 projects).
- `NoHttpClientGuard` still clean — no network primitives touched.
- `CA1305` silenced in `ProcessMonitor.OnWmiProcessStarted` via
  `Convert.ToInt32(pidRaw, CultureInfo.InvariantCulture)`.
- `CA1869` silenced in `FileBackedAuthAppRules` via `static readonly
  JsonSerializerOptions`.
- `FileBackedAuthAppRules` reads the JSON file directly (sync) rather
  than via `ILocalJsonStore.LoadAsync<T>` because `IsProtected` is
  synchronous and must run fast on a background WMI/poller thread.
  A code comment explains the trade-off. The store's atomic rename
  guarantees the file is never partially written.
- `TrayIconViewModel.cs` is dormant — its commands are wired but no
  `TaskbarIcon` host exists. M7 adds `H.NotifyIcon.Wpf` + a real
  `.ico` asset and binds this VM.
- `ProcessWatcher.Kill` fires after every non-`Verified` outcome.
  Rapid-relaunch deduped calls (which return `Deduped`) also trigger
  the kill — the dedupe path never starts a new process, so `Kill`
  on a non-existent PID is caught and silently swallowed.

---

## Milestone 5 — Windows Hello Core/Tests split (2026-08)

The single-project architecture splits into three (App + Core + Tests)
per IMPLEMENTATION_PLAN §1. The WinRT `UserConsentVerifier` call moves
behind `IHelloService` in `BioCentri.Core` so it can be tested headlessly;
`BiometricAuthService` stays in App as the UI-thread orchestrator
(coalescing, dedupe, `ShellState` mutation, toast feedback). 7 xUnit
tests (coalescing, dedupe, cancel-overlay, capability) pass against a
synchronous `FakeDispatcher` + `FakeHelloService`.

### Added
- **`BioCentri.Core/BioCentri.Core.csproj`** — class library on
  `net8.0-windows10.0.19041.0` (`UseWPF=false`). WinRT projection
  is available via the TFM itself — no separate NuGet.
- **`Core/src/Services/IHelloService.cs`** — interface:
  `RequestVerificationAsync(string, CancellationToken)` &arr; `HelloOutcome`;
  `CheckAvailabilityAsync(CancellationToken)` &arr; `HelloCapability`.
  No WPF dependency.
- **`Core/src/Model/HelloOutcome.cs`** — `HelloOutcome` +
  `HelloCapability` enums mirroring the WinRT results.
- **`Core/src/Interop/UserConsentVerifierAdapter.cs`** — concrete
  adapter wrapping `UserConsentVerifier.RequestVerificationAsync` /
  `CheckAvailabilityAsync`. Maps the 5 known 19041-SDK outcome values
  + the 3 known capability values; unknown values fall through to
  `Error` / `Unknown`. Catches `OperationCanceledException` and
  `Exception` so a broken COM marshal doesn't tear down the caller.
- **`BioCentri.Tests/BioCentri.Tests.csproj`** — xUnit 2.6.0 +
  `FluentAssertions` 6.12.0. References Core (for `IHelloService`) and
  App (for `BiometricAuthService`, via `InternalsVisibleTo`).
- **`Tests/src/Unit/FakeHelloService.cs`** — settable `NextOutcome` /
  `NextCapability` / `Delay` + recorded `Messages` + `Outcomes` for
  post-hoc assertion.
- **`Tests/src/Unit/FakeDispatcher.cs`** — synchronous `IDispatcher`
  double (runs every action immediately; no WPF thread required).
- **`Tests/src/Unit/BiometricAuthServiceTests.cs`** — 7 `[Fact]`s:
  - Verified sets/clears `ShellState` correctly.
  - `UserCancelled` returns `UserCancelled`.
  - Same appName coalesces to exactly 1 Hello prompt.
  - Different appNames do NOT coalesce.
  - Within-500ms rapid relaunch returns `Deduped`.
  - `GetCapabilityAsync` returns `Available`.
  - Overlay cancel resolves to `UserCancelled` (proves the TCS
    force-complete path works end-to-end with the fixed
    `return tcs.Task.Result` pattern).

### Modified
- **`BiometricAuthService.cs`** — ctor now injects
  `(IDispatcher, IToastService, ShellState, IHelloService)`; delegates
  the actual WinRT calls to `_hello.RequestVerificationAsync` /
  `_hello.CheckAvailabilityAsync`. The old direct `UserConsentVerifier`
  references + `using Windows.Security.Credentials.UI;` are removed.
  Translate / `TranslateCapability` now map from `HelloOutcome` /
  `HelloCapability` (Core enums) to `AuthOutcome` / `AuthCapability`
  (App enums). **Bug fix** in the call path: both the success and the
  `catch(Exception)` return sites now use `return tcs.Task.Result`
  instead of the local outcome variable, so the cancel handler's
  force-completed `UserCancelled` is always propagated correctly.
- **`App.xaml.cs`** — registered `IHelloService` (`UserConsentVerifierAdapter`)
  immediately before the `BiometricAuthService` registration (4-arg
  ctor now). DI ordering comment present.
- **`BioCentri.App.csproj`** — added `<ProjectReference Include="..\BioCentri.Core\BioCentri.Core.csproj" />`.
- **`BioCentri.sln`** — now contains 3 projects (`BioCentri.App`,
  `BioCentri.Core`, `BioCentri.Tests`) with full
  `ConfigurationPlatforms` blocks; BOM preserved.
- **`BioCentri.App/AssemblyInfo.cs`** — new file with
  `[assembly: InternalsVisibleTo("BioCentri.Tests")]` per
  `IMPLEMENTATION_PLAN` §5.

### Notes
- Build: `dotnet build app/BioCentri.sln -c Debug` reports
  **0 errors, 0 warnings** (all 3 projects).
- Tests: `dotnet test` reports **7 passed, 0 failed, 0 skipped**.
- `NoHttpClientGuard` still clean — no network primitives touched.
- The `UserConsentVerifierAdapter` uses only the 19041-SDK-safe
  enum values: `DeviceNotPresent` / `DeviceNotAvailable` were
  excluded per the original `BiometricAuthService`'s comment
  (CS0117 proved it). A comment explains the gap.
- Per-test isolation is enforced: the test class uses `out`
  parameters on `CreateSut` to give each test its own fresh
  `FakeHelloService`, `FakeToastService`, and `ShellState` — no
  stale subscriptions or message-bag carryover.

---

## Milestone 4 — Protected Apps UI (2026-08)

Protected Apps now reads + persists a real user-managed list via
`ILocalJsonStore`, opens a registry-based discovery picker rendered
through the existing `IDialogService` shell, and updates the page list
immediately on Add / Unprotect. No code-behind for business logic.
Architecture untouched: MVVM is intact, the DI chain is extended
(`IInstalledAppsDiscovery` added in dependency order), and
`NoHttpClientGuard` is still clean.

### Added
- **`BioCentri.App/src/types/InstalledApp.cs`** &mdash; record POCO
  (`DisplayName`, `Path`, `Publisher?`, `IconKey`). `[JsonPropertyName(...)]`
  on each init-only parameter for explicit camelCase serialization.
- **`BioCentri.App/src/types/ProtectedApp.cs`** &mdash; record POCO
  (`DisplayName`, `Path`, `IconKey`, `AddedUtc`). Record equality
  means path-based dedupe has one source of truth.
- **`BioCentri.App/src/types/Services/IInstalledAppsDiscovery.cs`** &mdash;
  contract. Single method:
  `Task<IReadOnlyList<InstalledApp>> DiscoverAsync(CancellationToken)`.
- **`BioCentri.App/src/services/InstalledAppsDiscovery.cs`** &mdash;
  registry implementation. Walks HKLM, HKLM Wow6432Node, HKCU
  `...\Uninstall` hives. Runs the walk on `Task.Run` with a 1500 ms
  internal `CancelAfter` linked to the caller's token. Path
  resolution tries `DisplayIcon` &rarr; `InstallLocation/<DisplayName>.exe`
  enumeration &rarr; `UninstallString` regex. Skips entries without a
  resolvable `.exe`. Silently catches `SecurityException`,
  `IOException`, `UnauthorizedAccessException` per-key so a
  restricted hive never takes down the whole walk.
- **`BioCentri.App/src/Features/ProtectedApps/AppPickerViewModel.cs`** &mdash;
  `IDialogHostViewModel<InstalledApp?>`. Internal `SearchText`
  observable + `ObservableCollection<InstalledApp> Filtered`.
  `LoadAsync` runs on a thread-pool task, then marshals the
  collection update onto the UI thread via
  `IDispatcher.InvokeAsync(Action)`. `Confirm` / `Cancel` resolve
  one `TaskCompletionSource<InstalledApp?>` so the dialog host
  closes cleanly on either path.
- **`BioCentri.App/src/Features/ProtectedApps/AppPickerView.xaml(.cs)`** &mdash;
  UserControl rendered by the shell's `DialogHost` via
  `DataTemplate` (by-`x:Type`) registration in `App.xaml`.
  Search box, ListBox of discovered apps with row-level "Protect"
  buttons, `LoadingState` visibility binding, error `TextBlock`,
  `EmptyState` for the filter result. Already-protected paths
  (passed in by the parent VM) are filtered out so re-adding is
  impossible through the picker.
- **`BioCentri.App/src/styles/Converters.cs`** &mdash; 4 new
  `IValueConverter` implementations used by every page that needs
  binding-to-Visibility logic:
  - `BoolToVisibilityConverter` &mdash; `BoolToVisibility` key.
  - `InverseBoolToVisibilityConverter` &mdash; `InverseBoolToVisibility` key.
  - `StringToVisibilityConverter` &mdash; `StringToVisibility` key.
  - `CountToVisibilityConverter` &mdash; `CountToVisibility` key, with
    optional `ConverterParameter` `"zero"` (Visible when count == 0)
    / `"nonzero"` (Visible when count &gt; 0) / unset (default
    visible-when-items-present).

### Modified
- **`Features/ProtectedApps/ProtectedAppsPage.xaml`** &mdash; full
  rewrite. Header row kept; action row now has a search `TextBox`
  + the indigo "Add application" button bound to `AddCommand`;
  the body toggles between `<feedback:EmptyState>` (visible when
  `Protected.Count == 0`) and a `ListBox` (visible when &gt;0) using
  `CountToVisibility` with the `"zero"` / `"nonzero"` parameters.
  Each protected row has an "Unprotect" button that fires
  `UnprotectCommand` with the row's `ProtectedApp` as
  `CommandParameter`.
- **`Features/ProtectedApps/ProtectedAppsViewModel.cs`** &mdash; full
  rewrite. Constructor now injects
  `(IToastService, ILocalJsonStore, IDialogService, IDispatcher, IInstalledAppsDiscovery)`.
  Idempotent `InitializeAsync()` loads
  `%LOCALAPPDATA%\BioCentri\ProtectedApps.json` on the UI thread.
  `[RelayCommand] AddAsync` constructs an `AppPickerViewModel`
  (with already-protected paths filtered out) and calls
  `_dialog.ShowAsync<InstalledApp?>(...)`. `[RelayCommand] UnprotectAsync`
  removes the row by path (Ordinal-ignore-case) and rewrites the
  JSON file. The snapshot inside `PersistAsync` uses
  `_dispatcher.InvokeAsync(Func<Task<T>>)` for type-safe definite
  assignment. Errors during load / save surface via
  `ToastService.Show(ToastSeverity.Danger, ...)`.
- **`App.xaml`** &mdash; added the namespace aliases
  `xmlns:items="clr-namespace:BioCentri.App.Features.ProtectedApps"`
  and `xmlns:conv="clr-namespace:BioCentri.App.Styles"`. A
  `<DataTemplate DataType="{x:Type items:AppPickerViewModel}"><items:AppPickerView /></DataTemplate>`
  in `<Application.Resources>` lets the shell's DialogHost
  auto-render the picker. The 4 `StaticResource` converter entries
  live inside the existing `ResourceDictionary` block, after
  `MergedDictionaries`.
- **`App.xaml.cs`** &mdash; registered `InstalledAppsDiscovery` +
  `IInstalledAppsDiscovery` immediately after `ILocalJsonStore`
  (DI ordering rule: any consumer must resolve before its own
  factory runs). The `ProtectedAppsViewModel` factory now injects
  the 5 new constructor parameters via `host.Get<T>()`.
- **`BioCentri.App.csproj`** &mdash; added a `<Page Include="src/Features/ProtectedApps/AppPickerView.xaml">`
  entry so the XAML compiles into the assembly.

### Removed
- **`src/services/ServiceLocator.cs`** &mdash; written then deleted the
  same turn. Injecting `IInstalledAppsDiscovery` directly into
  `ProtectedAppsViewModel` made the global accessor unnecessary.

### Notes
- Build: `dotnet build app/BioCentri.sln -c Debug` reports
  **0 errors, 0 warnings** after a clean rebuild (kill stale
  MSBuild + build-server shutdown + bin/obj wipe + no-incremental).
- `NoHttpClientGuard` still clean &mdash; discovery is registry-only,
  no network primitives touched.
- All four round-by-round fixes documented as called out by the
  build pipeline:
  - `ToastSeverity.Danger` (the actual enum value) replaces the
    non-existent `ToastSeverity.Error` (resolves CS0117 &times;3).
  - `_dispatcher.InvokeAsync(Func<Task<T>>)` typed-result snapshot
    pattern replaces a captured `List<T> snapshot := null; await`
    pattern (resolves CS0165 at `PersistAsync`).
  - `LoadingState` exposes no custom `Label` DP &mdash; the hard-coded
    "Loading..." text is the intended look (resolves MC3072 on
    `AppPickerView.xaml` line 56). The bogus attribute is removed.
  - The 4 `IValueConverter` implementations + `App.xaml`
    registrations were created because the M4 XAML uses them and
    they did not exist in the prior codebase &mdash; the original
    `dotnet build` succeeded at compile-time because `StaticResource`
    resolution is runtime, but navigation would have thrown a
    `XamlParseException` had M4 shipped without them.
- Pattern note for subsequent milestones: every VM-side
  `ObservableCollection` mutation is marshalled to the UI thread
  via `IDispatcher.InvokeAsync(Action)` (or its `Func<Task<T>>`
  overload for snapshot reads). M5&rsquo;s `HelloService` adapter
  reuses the same envelope.

---

## Milestone 3 — visual polish (2026-08)

The dashboard's home route is now a calm, premium surface with
asymmetric bento layout and three motion ornaments on the hero card.
Architecture untouched; no business logic added.

### Added
- **`components/motion/HologramFloat.xaml(.cs)`** — UserControl
  wrapping a single content child in a gentle vertical float (auto-
  reversing 0 → −6 px Y translation, 6 s loop, easing C# inlined per
  Decision 9 followup). Honours `Motion.RespectReducedMotion`.
- **`components/motion/ReticleRing.xaml(.cs)`** — decorative target-
  reticle ornament (two concentric ellipses + four crosshair lines +
  centre dot) with a continuous 0 → 360° rotation over 12 s
  (`Motion.Duration.Reticle`). Renders static when reduced-motion is
  requested.
- **`components/motion/BorderTrace.xaml(.cs)`** — hover-lift border.
  At rest: hairline. On `MouseEnter`: border brightens to the indigo
  accent and gains +1 px thickness over 220 ms. Static when reduced-
  motion is requested.
- **`components/surface/BentoStat.xaml(.cs)`** — single tile
  (Label / Value / Caption) used as the building block of the
  dashboard's asymmetric bento grid. Wraps content in `BorderTrace`
  so every tile lift-responds to hover. Three `DependencyProperty`-backed
  string properties.

### Modified
- **`Features/Dashboard/DashboardPage.xaml`** — full rewrite. Hero
  card now pairs a `ReticleRing` ornament with a `HologramFloat`-wrapped
  BioCentri logo. The 4-up `UniformGrid` of stat tiles is replaced by
  a 3-col × 2-row asymmetric bento: a WIDE tile (col 0-1) + a SMALL
  tile (col 2) on row 0; a SMALL tile (col 0) + a WIDE tile (col 1-2)
  on row 1. Recent-activity empty state unchanged.
- **`BioCentri.App.csproj`** — added 4 `<Page Include="…">` entries
  for the new components (`HologramFloat.xaml`, `ReticleRing.xaml`,
  `BorderTrace.xaml`, `BentoStat.xaml`).

### Notes
- Build: `dotnet build app/BioCentri.sln -c Debug` reports
  **0 errors, 0 warnings**.
- NoHttpClientGuard still clean — no network primitives introduced.
- All animation timings reference `Motion.Duration.*` tokens (or
  the equivalent ms literal in component-private Storyboards).
- `Motion.RespectReducedMotion` is read at component `Loaded` time
  via `Application.Current.TryFindResource(...)` and gates every
  animation. M7 will wire that resource to `SystemParameters`.

---

## Milestone 2 — recovery (2026-08)

The M2 release summary below had a truthful content layer but a
dishonest metadata layer: it claimed "Build: 0 errors, 0 warnings"
when in fact the working tree was missing files the csproj listed.
This entry documents the actual fix-up work that re-aligned the tree
with the spec.

### Fixed
- **Build integrity now actually matches the M2 claim.** Restored
  five Page XAMLs (`DashboardPage`, `ProtectedAppsPage`, `RulesPage`,
  `SettingsPage`, `DiagnosticsPage`), six Page ViewModels
  (`DashboardViewModel`, `ProtectedAppsViewModel`, `RulesViewModel`,
  `SettingsViewModel`, `DiagnosticsViewModel`, `ActivityViewModel`),
  the shell composition owner (`Features/Shell/ShellViewModel.cs`),
  and the missing `Features/Rules/RuleStatus` enum.
- **CA1305 culture-suppressed warning.** `DashboardViewModel` now
  passes `CultureInfo.InvariantCulture` to `DateTime.ToString(string)`.
- **DI ordering bug.** `AddSingleton<ToastService>(...)` /
  `AddSingleton<IToastService>(host.Get<ToastService>())` had been
  registered *after* page-VMs that consume `IToastService`; the order
  is now correct (registered alongside `ILocalJsonStore`, before any
  consumer).
- **DI constructor lie.** Dropped the parameterless `null!` fallback
  on `ProtectedAppsViewModel` so any future registration mistake
  fails loudly instead of silently via null propagation.
- **DataModels type resolution.** `TimelineSeverity` resolves via
  `BioCentri.App.Components.Feedback` (its true home in
  `TimelineEntry.xaml.cs`); the unused `Features.Activity` import is
  removed.
- **MC3072 missing property on `<feedback:EmptyState>`.** Three
  pages used `Description=` where EmptyState's actual property is
  `Subtitle`; converted to `Subtitle=` (the visible-error lines
  reported by the XAML compiler).
- **Component contract mismatch on `<input:FilterChip>`.** Four
  `IsActive=` bindings fixed to `IsChecked=` (FilterChip's actual
  DP).
- **Component contract mismatch on `<nav:SettingsRow>`.** Six
  bindings fixed (`Subtitle=` → `Description=`, `Glyph=` →
  `IconKey=`).
- **csproj casing.** Five newly-restored Page Include paths now use
  PascalCase to match the on-disk directory layout.

### Added
- **`Features/Shell/ShellViewModel.cs`** — applies
  `INavigationService` updates to `ShellState.CurrentRoute` and
  exposes a `ToggleSidebarCommand` for the topbar hamburger. Per
  Decision 11: glue only.
- **`Features/Rules/RuleStatus.cs`** — enum
  (`Draft` / `Active` / `Paused`) co-located with `RulesViewModel`
  per the project file-name-equals-type-name convention. Round-trips
  through `System.Text.Json`.
- **`EmptyState.IconKeyProperty`** — shared design-system component
  now honours an optional leading icon key (parity with
  `SettingsRow`).
- **ToastService wiring on "Add application".** The placeholder
  button on `ProtectedAppsPage` now emits a real
  `ToastService.Show(...)` info toast on click — the smallest
  visible UX delta that doesn't smuggle Milestone 4 logic in.

### Changed
- `App.xaml.cs` page-VM DI registrations rewritten to use M2-faithful
  parameterless constructors (matches the M2 VM bodies). The
  Milestone-5-strict DI chain (`ProcessWatcher`, `IBiometricAuthService`,
  `AuthenticationOverlayViewModel`) is left intact because those
  services are real and usable today.

### Deferred
- `RuleStatus.Paused` vs `RuleEntry.IsEnabled` source-of-truth
  resolution lands in Milestone 4.
- Single-writer enforcement for `ShellState.CurrentRoute` lands in
  Milestone 5.
- 5 missing glyphs in `Icons.xaml`
  (`Palette` / `Eye` / `Power` / `Lock` / `Plus`) land with the M3
  visual polish.

---

## Milestone 2 — Shell, Navigation, Components (2026-07)

### Added — application shell
- **Route + navigation infrastructure**
  (`BioCentri.App/src/routing/`)
  - `Route` enum: `Dashboard`, `ProtectedApps`, `Rules`,
    `Activity`, `Settings`, `About`, `Diagnostics`.
  - `RouteTable`: static metadata (`Title`, `Subtitle`, `IconKey`)
    per route. ViewModels surface their title via `RouteTable.Get(route)`.
  - `PageRegistry` (DI-aware, `IPageRegistry`): lazy `Page Create(route)`
    that uses the `ServiceHost` to resolve each `*ViewModel` independently.
- **State** (`BioCentri.App/src/state/`)
  - `ShellState` (observable): `IsSidebarExpanded`, `CurrentRoute`,
    `CurrentTitle` (mirrors `RouteTable.Get(value).Title` so Sidebar,
    TopBar, and StatusBar all bind to the same source of truth).
  - `AppState`: process-wide `BuildLabel`, `AppReadyAtUtc`,
    `ThemeStyle` — promoted from a stub in M2 because reactive consumers
    (debug overlay, About page) read it.
- **Services** (`BioCentri.App/src/services/`)
  - `NavigationService` (concrete + `INavigationService` interface).
    `AttachFrame(Frame)` is the composition seam; `NavigateTo(route)`
    clears the frame journal (`RemoveBackEntry` loop) so we don't leak
    pages between navigations.
  - `ToastService` + `IToastService`: `ObservableCollection<ToastViewModel>`
    with auto-dismiss via `DispatcherTimer`. Toasts expose
    `Severity`, `Title`, `Description`, `ExpiresAt`.
  - `DialogService` + `IDialogService`: `ActiveDialog` is the bound
    property; `ConfirmAsync`/`NotifyAsync`/`ShowAsync<TResult>` are
    the three flavours. Modal lifecycle is bound to the
    `TaskCompletionSource` pattern in `ConfirmDialogViewModel` /
    `NotifyDialogViewModel`.
- **Shell composition root** (`BioCentri.App/src/windows/MainWindow.xaml`)
  - Sidebar column (driven by `BoolToGridLengthConverter` mapped from
    `ShellState.IsSidebarExpanded`, 240 ↔ 64).
  - TopBar row (`60 px`) hosting the active route title and a
    status pill placeholder.
  - Frame `PageHost` (`JournalOwnership="OwnsJournal"`,
    `NavigationUIVisibility="Hidden"`).
  - StatusBar row (`32 px`) with build tag and process-light footer text.
  - ToastHost overlay (`bottom-right`, `40 px` inset above StatusBar).
  - DialogHost overlay (`Grid.ColumnSpan="2"`, dimmer + presenter).
- **MainWindow.Initialize(ServiceHost)** is the composition seam:
  DataContext = `ShellViewModel`; `ToastLayer.DataContext = ToastService`;
  `DialogOverlay.DataContext = DialogService`; Frame is attached to
  `NavigationService`. First navigation is deferred to the Loaded
  event so the Frame has been measured + laid out before the page swap.

### Added — reusable components (`BioCentri.App/src/components/`)
- `Iconography/Icon` (`UserControl`) — `GeometryKey` resolution against
  the Icons dictionary; `Stroke` / `IconSize` settable.
- `Nav/Sidebar`, `Nav/SidebarItem`, `Nav/TopBar`, `Nav/PageHeader`,
  `Nav/StatisticCard`, `Nav/ListTile`, `Nav/SettingsRow`,
  `Nav/StatusPill`, `Nav/NavIndicator`.
- `Feedback/EmptyState`, `Feedback/LoadingState`, `Feedback/Toast`,
  `Feedback/ToastHost`, `Feedback/DialogHost`.
- `Surface/FocalCard`.

### Added — feature page scaffolds (`BioCentri.App/src/features/`)
- `Dashboard/` — `DashboardViewModel`, `DashboardPage`. Bento stat grid,
  focal greeting, recent-activity placeholder. Bound to
  `DashboardStat` (`Label`, `Value`, `Caption`) and a future
  `ActivityRow` placeholder.
- `ProtectedApps/` — `ProtectedAppsViewModel`, `ProtectedAppsPage`.
  Top-bar Add button (M6 ownership), search box + filter chips,
  `ListTile`-based list fed from `ProtectedAppRow`.
- `Rules/` — `RulesViewModel`, `RulesPage`. Card list of placeholder
  automation rules fed from `RuleCardRow`.
- `Activity/` — `ActivityViewModel`, `ActivityPage`. Stat strip +
  empty-state await message; future `ActivityLogRow` records.
- `Settings/` — `SettingsViewModel`, `SettingsPage`. Six category rows
  (`Appearance`, `Accessibility`, `Startup`, `Notifications`,
  `Privacy`, `About`) anchored to a `SettingsCategory` record set;
  M7 wires the detail panel.
- `About/` — `AboutViewModel`, `AboutPage`. Three-up stat grid:
  Version / Build / License + GitHub / Website / Architecture.
- `Diagnostics/` — `DiagnosticsViewModel`, `DiagnosticsPage`. Environment
  readouts (`Application`, `Hello availability`, `OS`, `Runtime`) +
  recent-signals list fed from a placeholder `LogEntry` record.

### Added — primitive resource dictionaries
- `Brushes.xaml` (was M1) — extended with `Brushes.Theme.Window.*` and
  `Brushes.Theme.StatusBar.*` aliases so the shell binds to one set of
  themed tokens (no hex leakage).
- `Corners.xaml` — `Corners.Sm/Md/Lg/Xl/Pill` named radii.
- `Elevation.xaml` — `Elevation.0..4.Lifted` `Thickness`-based drop
  shadow wrappers for cards + dialogs.
- `FieldStyles.xaml` — `Brushes.Subtle.Surface`, `Brushes.Border.Hairline`
  tightened aliases plus `Stroke.Thin` constant used across chrome.
- `Icons.xaml` — 24-×-24 `Geometry` stream glyphs grouped by namespace:
  `Icons.Shell.Logo`, `Icons.Route.<Route>` (7 entries),
  `Icons.Status.<Status>` (4 entries), `Icons.Action.<Action>`
  (7 entries), `Icons.Brand.Placeholder`.
- `Transitions.xaml` — `Transitions.Dimmer.Show`, `Transitions.Dialog.PopIn`
  Storyboards used by `DialogHost`; duration tokens bound to
  `Motion.Duration.*`.
- `Themes/Dark.xaml` extended — theme-resolved brushes now include the
  StatusBar pair and the Window-pair aliases.

### Changed
- `MainWindow.xaml` — M1 placeholder replaced by the real shell
  (sidebar + topbar + frame + statusbar + overlays).
- `MainWindow.xaml.cs` — added `Initialize(ServiceHost)` seam; first
  navigation deferred to Loaded event.
- `App.xaml.cs` — full chain of `AddSingleton<…>(…)` in dependency
  order: dispatcher → lifecycle → AppState → ShellState →
  7 feature VMs → `IPageRegistry` → `NavigationService` →
  `ToastService` → `DialogService` → `ShellViewModel` → `MainWindow`.
  Then `shell.Initialize(Host); shell.Show();`.
- `SidebarItem.xaml.cs` — selection-stripe `KeySpline` is now inlined
  (`new KeySpline(0.16, 1.0, 0.3, 1.0)`) per Decision 9 followup,
  instead of `TryFindResource("Motion.Easing.OutExpo")` which would
  have returned `null` (baml refuses to register `SplineEase` in this
  project's SDK).

### Notes
- No business logic. No Hello. No app-protection. No process monitoring.
  No registry edits. No Win32 hooks. All routes render placeholder
  content — the M3+ milestones fill them in.
- Build: `dotnet build app/BioCentri.sln -c Debug` reports
  **0 errors, 0 warnings**.
- Touched: nothing in `website/`. Nothing in `app/IMPLEMENTATION_PLAN.md`.

---

## Milestone 7 — Polish & Installer readiness (2026-08)

Accessibility pass, high-contrast theme hook, reduced-motion user toggle,
WiX installer checklist, code-signing roadmap, manual QA script. Tray icon
deferred to M7.1 pending `H.NotifyIcon.Wpf` 2.x NuGet resolution.

### Added
- **`docs/MANUAL_QA.md`** — 8-section manual QA script covering install,
  shell navigation, protected apps CRUD, Windows Hello challenge,
  process enforcement, settings (reduced motion + high contrast), visual
  polish (motion components), and xUnit test verification. Includes a
  "Deferred / Known Limitations" section.
- **`docs/INSTALLER.md`** — production deployment checklist: WiX 3.14
  `.wxs` authoring notes, EV code-signing vs Azure Trusted Signing
  options, SmartScreen reputation-building timeline, .ico tray asset
  spec, publish profile, and versioning alignment across the 3 projects.

### Added — accessibility
- **`SidebarItem.xaml`** — `AutomationProperties.Name="{Binding Title}"`
  on the root `Border` so screen readers announce each nav item by its
  route label.
- **`AppPickerView.xaml`** — `Focusable="True"` +
  `KeyDown="OnEscapeKey"` on the UserControl root. Esc closes the
  picker instantly, matching standard dialog UX.
- **`AppPickerView.xaml.cs`** — `OnEscapeKey` handler: casts
  `DataContext` to `AppPickerViewModel` and invokes
  `CancelCommand.Execute(null)` on Escape. Guarded: only Escape triggers
  the command; other keystrokes (typing in the search box) are
  unaffected.

### Added — high-contrast theme hook
- **`App.xaml.cs`** — `SystemParameters.StaticPropertyChanged`
  subscription in the App constructor. `OnSystemParameterChanged` watches
  for `"HighContrast"`; when the OS toggle flips, swaps
  `MergedDictionaries[0]` between `Dark.xaml` and `HighContrast.xaml`
  at runtime. All brushes are consumed via `DynamicResource` so the
  entire UI palette changes without a restart.

### Added — reduced-motion user toggle
- **`SettingsViewModel.cs`** — `IsReducedMotionEnabled` property with
  manual `SetProperty` gate. On change: calls
  `UseReducedMotion.Enable()` / `Disable()` and sets
  `Application.Current.Resources["Motion.RespectReducedMotion"]` so
  all motion components (`HologramFloat`, `ReticleRing`, `BorderTrace`,
  `AuthenticationOverlay`) freeze immediately.
- **`SettingsPage.xaml`** — new `Border` row at the bottom of the
  settings list with a `ToggleSwitch` bound to `IsReducedMotionEnabled`.
  Uses `xmlns:input` for the `ToggleSwitch` namespace.

### Notes
- Build: `dotnet build app/BioCentri.sln -c Debug` reports
  **0 errors, 0 warnings** (3 projects).
- `NoHttpClientGuard` still clean — no network primitives touched.
- The `[ContentProperty]` attributes on `SettingsRow` and `ListTile`
  were removed in this milestone because they caused CS0759
  (`x:Name` field not generated in time for the source generator)
  when `TrailingAction` was used as an `x:Name`-based content slot.
  Both components retain their `DependencyProperty`-backed properties
  and work correctly without the attribute.
- `SettingsViewModel.IsReducedMotionEnabled` uses a manual
  `SetProperty` call rather than `[ObservableProperty]` because the
  source generator's partial field resolution conflicted with the
  fully-qualified `System.Windows.Application.Current.Resources`
  reference in the change handler.
- Tray icon activation (`H.NotifyIcon.Wpf` 2.x + .ico asset) is
  deferred to M7.1. The `TrayIconViewModel.cs` commands are fully
  wired but await the NuGet and asset. See `docs/INSTALLER.md` for
  the .ico specification.

---

_Last reviewed: project foundation + M1 + M2 (incl. recovery) + M3 + M4 + M5 + M6 + M7 + v1.0.0._

---

## v1.0.0 — Phase 1 MVP (2026-08)

Phase 1 MVP is code-complete. Seven milestones delivered end-to-end:
app discovery → protection toggle → Windows Hello biometric challenge →
process monitoring → enforcement. Built on WPF / .NET 8, local-first,
zero outbound network.

### Distribution
- WiX 3.14 installer source authored (`app/BioCentri.Setup/`)
- Publish profile: single-file, framework-dependent, win-x64
- All 3 projects bumped to v1.0.0
- Tray icon deferred to M7.1 (H.NotifyIcon.Wpf blocked by offline NuGet cache)

