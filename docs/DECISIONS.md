# Technical Decisions — BioCentri (MVP)

> Locks in the *why* behind the stack choices for BioCentri MVP so future AI
> coding sessions (and people) don't relitigate them. **Append** new decisions
> as `Decision N+1` — never retroactively edit prior decisions; supersede them
> explicitly.

This document is **practical, not academic**. Every section states the choice,
the *short* reason it wins, and the alternatives we considered and rejected.

---

## Decision 1: Desktop Application Framework

**Decision:** **WPF on .NET 8 (LTS) or later**, layered with the **Wpf.Ui**
library to bring Fluent Design visuals (Mica, Acrylic, modern controls) on top
of classic WPF.

**Reason (Windows-only + Windows Hello + performance + AI support + maintainability):**

- **Windows-only target.** Removes the strongest argument for cross-platform
  frameworks. MAUI's Windows backend is the weakest of its targets and we get
  no macOS/Linux payoff to compensate.
- **Windows Hello interop is mature in WPF.** The WinRT path
  (`Windows.Security.Credentials`, `Windows.Security.Credentials.UI`) is
  reachable from C# / .NET 8 via `Microsoft.Windows.SDK.NET` and works
  identically from WPF and WinUI 3 — so we lose nothing by choosing WPF.
- **Performance is more than enough.** WPF on .NET 8 handles a list view,
  tray icon, and an auth modal without breaking a sweat. Idle CPU stays near
  zero with a `Dispatcher`-driven event subscriber.
- **Modern UI capability.** Wpf.Ui gives us a Fluent Design surface on
  classic WPF — Mica window backgrounds, modern navigation patterns,
  accessibility-aware controls — *without* committing to the Windows App SDK
  runtime story.
- **AI coding support is the largest of any WPF-class option.** Twenty years
  of community examples, StackOverflow answers, blog posts, and Microsoft
  docs means better autocomplete suggestions and fewer hallucinated APIs.
- **Maintainability.** WPF has been Microsoft's most-stable desktop UI
  surface for nearly two decades. Tooling (Visual Studio + XAML Hot Reload,
  Rider, WPF trace sources) is well-known.

### Alternatives considered

- **WinUI 3 / Windows App SDK.** Best long-term *native* UX and the closest
  to Fluent Design's north star. **Rejected for MVP** because the Windows
  App SDK has shipped enough breaking-version churn over its lifetime that
  small teams absorb real cost; the AI-coding corpus is thinner; and there
  is no clear UX feature BioCentri MVP needs that *only* WinUI 3 can
  deliver. Re-evaluate in Phase 3.
- **Avalonia.** Cross-platform XAML. **Rejected** because cross-platform is
  feature-creep for v1 and would dilute focus. Re-evaluate if BioCentri ever
  ships to macOS.
- **.NET MAUI.** Microsoft's cross-platform flagship. **Rejected** because
  Microsoft itself recommends WPF or WinUI 3 for Windows-first desktop, and
  BioCentri is explicitly Windows-only for MVP.

---

## Decision 2: Programming Language

**Decision:** **C#**, on .NET 8 (LTS) or later.

**Reason:**

- **Native fit with Decision 1.** WPF, WinRT interop, and Windows Hello APIs
  are all first-class in C#.
- **Largest corpus in the .NET ecosystem** — the single highest-ROI input
  for AI-assisted coding sessions on a small team.
- **Tooling.** Visual Studio (with XAML Hot Reload) and JetBrains Rider both
  cover WPF/.NET 8 depth. Community packages (for instance
  `Microsoft.Windows.SDK.NET`, `Wpf.Ui`, `WindowsAPICodePack`) ship C#-first.
- **Runtime stability.** .NET 8 LTS gives a three-year support window — long
  enough to span MVP through Phase 3.

### Alternatives considered (briefly)

- **F#.** Strong on .NET but the WPF/WinRT story is weaker; AI-coding corpus
  is thinner. **Rejected.**
- **VB.NET.** Legacy, shrinking ecosystem. **Rejected.**
- **Rust.** No WPF-equivalent; would force a full stack re-decision. **Deferred**
  to a future "BioCentri Engine" only if a hot-path native component ever
  becomes a real bottleneck (none today).
- **C++/Win32.** Radically higher engineering cost for marginal performance
  benefit we do not need. **Rejected.**

---

## Decision 3: Authentication Approach (Windows Hello)

**Decision:** Use **Windows Hello via WinRT** from the .NET host. Two WinRT
APIs split the work cleanly:

- **`Windows.Security.Credentials.UI.UserConsentVerifier`** — the MVP gate.
  Returns *yes/no* (with reason) when we only need "is the user present and
  biometrically / PIN verified?". This is what every MVP-triggered challenge
  will use.
- **`Windows.Security.Credentials.KeyCredentialManager`** — only when we
  need a *cryptographic* operation tied to the user's TPM-protected key
  (sign a nonce, derive per-app identity, etc.). Not used by v1's plain
  "challenge before launch" path; reserved for Phase 2 if we ever bind keys
  per protected app.

**Security considerations:**

- **Biometric material never leaves the device.** The Hello APIs only return
  *verified* / *not verified* (and a reason); the underlying fingerprint or
  face data is sealed inside the TPM / secure enclave by Windows.
- **PIN is mandatory fallback.** Windows Hello enrolment requires a PIN; a
  healthy machine always has PIN as the "I lost my thumb" path.
- **Secrets stay in OS stores.** Any non-biometric auth-related secret goes
  through **Windows Credential Manager** or **DPAPI**. **No plaintext on
  disk. No custom password store.** BioCentri does not invent its own auth
  data model.
- **Rate limiting is a client-side responsibility.** Failed challenges are
  counted per session; repeated failures trigger a cool-down. We do *not*
  silently let an attacker brute-force on a stolen machine.
- **No network calls during auth.** Hello challenges never touch the
  network. v1 makes **zero** outbound calls, full stop.

**Fallback authentication considerations:**

- **Hardware not available.** If the device has no biometric + no TPM, fall
  back to the existing Windows sign-in credential via `KeyCredentialManager`
  broad availability paths. UX degrades gracefully; security posture holds.
- **Hello not enrolled.** BioCentri detects "user hasn't set up Hello" and
  offers a deep-link into Windows Settings. We do **not** ask users to
  enrol *inside* BioCentri — that is Windows' job.
- **Untrusted environment.** If the host is a kiosk or remote-desktop
  session where Hello is unsupported, BioCentri surfaces an explicit error
  rather than silently degrading.

> **MVP simplification:** use `UserConsentVerifier` exclusively for v1.
> Re-introduce `KeyCredentialManager` only when a real Phase 2 requirement
> actually needs it.

---

## Decision 4: Application Protection Approach

**Goal.** When the user marks application X as protected, BioCentri prevents
X from *effectively* starting without a successful Hello challenge.

### Possible methods

| # | Method | Pros | Risks |
|---|---|---|---|
| 1 | **User-mode event-driven monitor + foreground challenge modal (RECOMMENDED for MVP)** | No service/admin install. No anti-malware flag. Easy to debug. Defensible scope. | Not a hard guarantee on elevated apps. Race window of ~hundreds of ms before BioCentri can react. |
| 2 | **User-mode process suspension (`NtSuspendProcess`)** | "Harder" block — process is frozen until auth completes. | Requires `SYNCHRONIZE` access. Fails silently on elevated / system processes. Wrong impression of security. |
| 3 | **Windows Service with process-create callback** | Stronger interception; runs even when user UI isn't foreground. | Admin install, more SmartScreen friction, more QA surface, more failure modes. |
| 4 | **Kernel-mode driver / minifilter** | Strongest guarantee. | Requires Microsoft-signed driver, attestation, enterprise trust posture. Massive overkill for v1. |
| 5 | **Launcher wrapper ("always open via BioCentri")** | Simple, no detection logic. | Breaks user expectation: users don't expect apps to need a "launcher". Confusing. |

### MVP recommendation — Method 1, narrowly scoped

- **Run BioCentri as a user-mode app.** No service install for v1.
- **Detect protected-app launches.** Subscribe to `ManagementEventWatcher`
  on `Win32_ProcessStartTrace` (preferred) for low-overhead event-driven
  detection, with a short-interval `Process.GetProcesses()` poll as a
  fallback for environments where WMI events are unreliable.
- **On protected-app detection:**
  1. Mark the detected PID as `pending-auth`.
  2. Raise a **topmost, focused** BioCentri window requesting
     `UserConsentVerifier.RequestVerificationAsync`.
  3. While the modal is up, set the protected-app's main window hidden /
     flashed so it doesn't get unintended input.
  4. **Success** → bring the protected app forward, clear `pending-auth`.
  5. **Failure / timeout (≈15s)** → terminate the process if the user owns
     it; otherwise force the BioCentri modal to the foreground and require
     an explicit user action.

### Explicit limits — communicated in-app

- **Best-effort block of non-elevated desktop apps.** Elevated apps and
  system services are out of scope for v1.
- **No silent enforcement.** The user always sees what BioCentri is doing
  (modal challenge, status in management view).
- **Race window acknowledged.** The detection-to-modal latency is the
  weakest link in v1's defence — we close it in later phases, not in MVP.

These limits are inserted as acceptance notes under FR-4 / NFR-Reliability
in `PRODUCT_REQUIREMENTS.md` and surfaced in the in-product copy.

---

## Decision 5: Project Development Rules

These rules apply to **every** future AI coding session and every human
contributor working on BioCentri MVP. They are not aspirational — they are
the bar.

1. **Keep features small.** Each change = one user-visible behaviour + its
   tests. Reject drive-by "while we're here" additions.
2. **Test after every major change.** Every `FR-*` in
   `PRODUCT_REQUIREMENTS.md` gets at least one automated test. Every
   auth-related change gets a manual end-to-end test on a clean Windows
   profile (face + fingerprint + PIN + failure path).
3. **No features outside MVP scope.** Phase 2 and Phase 3 items in
   `FEATURE_ROADMAP.md` are *not* MVP. Bundling them is rejected by default.
4. **Security and privacy are priorities.** A feature that weakens the
   security posture (even slightly) is rejected by default. Privacy
   defaults: no network, no telemetry, no background pings. Opt-in only,
   with UI surfacing and a "delete my data" path.
5. **Local-first by default.** Anything that wants to send data off-device
   must be opt-in, surfaced in the UI, and easy to reverse.
6. **Premium quality, no rough edges.** No half-built features, no hidden
   options, no surprise behaviour. Incomplete work stays in the local build
   or behind a feature flag — not in a shipped binary.
7. **No silent regressions.** A change to one FR must not weaken another.
   Cross-reference `PRODUCT_REQUIREMENTS.md` in every PR description.
8. **One PR per requirement.** A pull request maps to one or more numbered
   `FR-*` / `NFR-*` IDs in `PRODUCT_REQUIREMENTS.md`. Decisions in this
   document are not up for re-debate in PR review — open a **new** decision
   section instead.

---

_Last reviewed: pre-coding foundation._

---

## Decision 6: Solution Shape for v1 (single project + deferred split)

**Decision:** For Milestones 1–4, BioCentri ships **one WPF host project**
(`BioCentri.App`). The deferred `BioCentri.Core` and `BioCentri.Tests`
projects are present as `README.md` placeholders only and spin up
permanently at **Milestone 5**, when the Hello and store surfaces arrive
and need headless testability.

**Reason:**

- **Tiny surface lives effectively in one project today.** Every
  boundary we would draw (Core class library, Contracts interfaces,
  Tests project) needs an `interface` + DI registration. For a milestone
  whose only code is the technology demonstrator, that's pure ceremony.
- **Visual parity lives in `BioCentri.App`'s XAML.** Splitting XAML into
  a separate assembly requires either `pack://`-prefixed Source URIs
  across assemblies (which triggers `ReflectionPermission` friction)
  or duplicate dictionary copies — both worse than inline-at-M1.
- **Hello is the moment the split pays off.** Decision 3 puts the Hello
  gate behind `IHelloService`. Once that exists, having those bytes
  inside a non-WPF class library makes xUnit headless tests viable —
  and that is the catalyst for the real split. Not before.
- **Tests don't exist for v1.** We can ship `BioCentri.Tests` from M5
  with the first test, not from M1 with an empty assembly.

### Alternatives considered

- **Full three-project split at M1.** Rejected for the rationale above.
  Would also force every service interface into a `BioCentri.Contracts`
  assembly ahead of need.
- **Code sharing via `Linked Files`** (M1 csproj includes M5 target's
  source via `<Compile Include="..\..\BioCentri.Core\**\*.cs">`).
  Considered as a transitional pattern; rejected because it would
  silently couple boundary layouts before we know what the boundaries
  should be.

### Known followups

- Promote `IAppLifecycleService` to an `ObservableObject` in M2 — at that
  point it acquires change-notification consumers and the interface
  itself stops needing internal setters.
- Add `InternalsVisibleTo("BioCentri.Tests")` at M5.
- Add a `FakeHelloService` test fixture at M5 alongside `BioCentri.Core`.

---

## Decision 7: DI Container Choice

**Decision:** Plain **`Microsoft.Extensions.DependencyInjection` 8.0.x**,
**without** `Microsoft.Extensions.Hosting`, **without**
`Microsoft.Extensions.Logging`, **without**
`Microsoft.Extensions.Configuration`.

**Reason:**

- **Minimal surface.** We only need `IServiceCollection`,
  `IServiceProvider`, and life-times. The full hosting stack pulls in
  `IHostedService`, `IConfiguration`, the logging pipeline, and the
  options system. None of those are useful in v1.
- **No telemetry, no logging abstraction yet.** `ILogger<T>` predates
  any consumer we have. Decision 3 explicitly forbids telemetry in v1;
  adding the abstraction would invite accidental use.
- **Zero outbound network.** `IConfiguration` providers like
  `EnvironmentVariablesConfigurationProvider` are local-only, but a
  future contributor could add `AzureKeyVaultConfigurationProvider`,
  which would silently violate Decision 3. We do not import
  `Microsoft.Extensions.Configuration` until needed.
- **Cost-free migration path.** `ServiceHost.Build()` is the single call
  site — when M5 (or M7) needs hosting, we replace the body with
  `Host.CreateDefaultBuilder().ConfigureServices(...)`.

---

## Decision 8: Network Privacy Guard

**Decision:** `BioCentri.App` (and any future `BioCentri.Core`) must not
reference `System.Net.Http`, `System.Net.Sockets`, `System.Net.WebClient`,
`System.Net.WebRequest`, or `Windows.Web.Http` in any non-internal
production code path. The rule is enforced by:

1. `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` +
   `<EnableNETAnalyzers>true</EnableNETAnalyzers>`
   in `app/Directory.Build.props`.
2. Code-review checklist line in every PR template:
   *"Has this change introduced any outbound network path? If yes, link
   to a `docs/DECISIONS.md` entry that authorises it."*
3. CI grep (introduced at M2):
   `git grep -nE 'System\.Net\.(Http|Sockets|WebClient|WebRequest)' app/src`
   must return zero non-test hits.

**Reason:** Decision 3 mandates **zero outbound network calls in v1**.
Without a mechanical guard, the rule is one copy-paste away from being
silently broken. Privacy posture is most fragile at the diff level, so
the guard lives at the diff level.

### Activation

This decision was activated at **Milestone 1** as policy. The mechanical
CI grep and analyzer escalations become enforceable from M2 onward;
until then this document is the only enforcement.

---

_Last reviewed after Milestone 1 — architecture & tech stack._

---

## Decision 9: M1 NuGet Strategy (dep-light in-house bootstrap)

**Decision:** Milestone 1 ships BioCentri.App with **only
`CommunityToolkit.Mvvm 8.1.0`** as a package reference. `Wpf.Ui` and
`Microsoft.Extensions.DependencyInjection` are deferred to Milestone 2,
same as the package-mapped functionality they support.

A **20-line in-house `ServiceHost`** replaces `M.E.DI`'s
`ServiceCollection` / `IServiceProvider` while the offline NuGet cache
on this build machine is dep-light. The host is replaced by
`Microsoft.Extensions.DependencyInjection` at Milestone 2 when the
cached/restored package list is verified. The in-house host returns
typed singletons and refuses unknown types.

**Reason:**

- **Build has to succeed today.** The offline cache and the network
  feed disagree on existing versions for `Wpf.Ui 3.0.5` and
  `M.E.DI 8.0.2`, returning false NU1603 / NU1701 errors when restoring
  the locked M1 list. Wpf.Ui 3.1.0 was resolved as `.NETFramework-only`
  — wrong target. M5 features (Wpf.Ui controls + nested DI scopes for
  per-feature VMs) are more legitimately M2+ concerns anyway.
- **M1 doesn't need them.** Plain WPF + CommunityToolkit.Mvvm are
  enough for the foundation surface (a window that opens and resolves
  through a service). No `UiWindow`, no scoped lifetimes, no
  transient-vs-singleton nuance.
- **Migration is one class.** `ServiceHost.cs` is the only consumer.
  At M2, `dotnet add package Microsoft.Extensions.DependencyInjection`,
  delete `ServiceHost.cs`, change `OnStartup` from `new ServiceHost()…`
  to `new ServiceCollection()…BuildServiceProvider()`, and the rest of
  the app is untouched.

### Migration script (M2)

When the network is available again:

```powershell
cd app
dotnet add BioCentri.App package Wpf.Ui --version 3.0.5
dotnet add BioCentri.App package Microsoft.Extensions.DependencyInjection --version 8.0.2
# Replace BioCentri.App/src/services/ServiceHost.cs with the
# AddSingleton<…, TImpl>() factory-style template. Replace
# `Services.Get<T>()` with `Services.GetRequiredService<T>()` everywhere.
```

### Risk added

- A 20-line DI host is not a `Microsoft.Extensions.DependencyInjection`
  work-alike. Features beyond singleton-instance lookup (transient
  lifetimes, scoped lifetimes, keyed services, async resolution) need
  M2 migration. We will not add those features on the in-house host —
  we'll move to M.E.DI instead.

### Known followups

- **Easings live in C#, not XAML.** WPF baml on this SDK refuses to
  register `System.Windows.Media.Animation.SplineEase` in resource
  dictionaries — both `assembly=PresentationFramework` and
  `assembly=PresentationCore` for `clr-namespace:System.Windows.Media.Animation`
  fail with MC3074. The easings therefore live in a C# companion at
  `app/BioCentri.App/src/styles/Motion.cs` (created at M2 alongside the
  first animation). Consumers reference them via `{x:Static styles:Motion.OutExpo}`
  rather than `StaticResource`. **Do not** re-introduce `<anim:SplineEase …/>`
  in `Motion.xaml`; the next contributor who tries will hit the same
  MC3074.
- **Set `<EnableDefaultPageItems>false</EnableDefaultPageItems>`** in
  `BioCentri.App.csproj` whenever the explicit `<Page Include="…">`
  list is in use, otherwise the SDK's recursive `**\*.xaml` glob plus
  the explicit list collides with NETSDK1022.

### Worked example: M1 MainWindow build fix

The first build of MainWindow failed with `CS0103: 'InitializeComponent'
does not exist in the current context` even with the XAML/C# code-behind
_pair_ in place. The trap: `MainWindow.xaml`'s `x:Class="BioCentri.App.MainWindow"`
produces a BAML partial class at `BioCentri.App.MainWindow`, while
`MainWindow.xaml.cs` declares `namespace BioCentri.App.Windows` +
`partial class MainWindow`. Those resolve to two distinct types — the
partial-class bridge never forms, so `InitializeComponent()` is unreachable.

**Rule:** every new `Window` / `Page` / `UserControl` at M2 onwards
**must** set `x:Class` in the XAML to the fully-qualified
`namespace + class` of its code-behind. The standard pairing is
`x:Class="BioCentri.App.Windows.<Type>"` matching
`namespace BioCentri.App.Windows; public partial class <Type>`. The
folder structure under `src/windows/`, `src/features/<feature>/`, etc.
already encodes the namespace — copy it into the XAML's `x:Class`
verbatim.

---

_Last reviewed after Milestone 1 — architecture & tech stack._
## Decision 10: Page host pattern (Frame + Page)

**Decision:** BioCentri routes are realised as `System.Windows.Controls.Page`
subclasses hosted inside a single `System.Windows.Controls.Frame`
(`PageHost`) in the Shell. Navigation is driven by a `NavigationService`
that owns a journal-cleaning contract (`RemoveBackEntry` loop on every
`NavigateTo` so the Frame never accumulates stale pages between
route swaps). The shell exposes seven routes: `Dashboard`,
`ProtectedApps`, `Rules`, `Activity`, `Settings`, `About`,
`Diagnostics`.

**Reason:**

- **MVVM purity.** `Page` lets the XAML author declare its own
  title, background, navigation-chrome defaults which the Frame
  honours uniformly. `ContentControl` + `DataTemplate` works but
  pushes every feature to namespace its own template; with seven
  features plus M5+ additions, that's busywork. `Page` keeps the
  per-feature scaffold explicit.
- **Swap-by-Feature.** Each feature is a *Page + ViewModel* pair
  in its own folder (`src/features/<feature>/`). The Frame treats
  them as black boxes, so M6 (Hello), M7 (rules + settings) and the
  deferred `BioCentri.Core` split don't touch any shell code.
- **Diary-clean by design.** `JournalOwnership="OwnsJournal"` +
  `NavigationUIVisability="Hidden"` plus the explicit
  `RemoveBackEntry` loop in `NavigationService.NavigateTo` keeps
  memory bounded to *one* outstanding page between navigations.
  Without the loop, the default Frame behaviour leaks each previous
  Page instance to the WPF navigation journal (`JournalEntry` retains
  a strong reference plus saves BAML state). The leak is silent
  until a long-running session swaps hundreds of routes, at which
  point it becomes obvious.
- **URL-less navigation.** We deliberately do *not* use URI-based
  navigation (`Navigate(uri, extraData)`). The seven routes have
  no per-instance state worth URL-encoding; URI routing would
  invite a parallel API surface (deep-link parameters, query
  strings) and Decision 8 forbids any outbound HTTP path.

### Page registry

`PageRegistry` is the lazy factory. Each `Create(Route)` call
resolves the route's `*ViewModel` from the in-house `ServiceHost`
and constructs a fresh `Page` with that VM as `DataContext`. The
host is wired once at `App.OnStartup` so construction is order-strict:
feature VMs are registered *before* `NavigationService` is registered,
because `NavigationService` calls into `PageRegistry` immediately on
the first `NavigateTo(route)`.

### Compositional seam (`MainWindow.Initialize`)

```csharp
DataContext                  = host.Get<ShellViewModel>();
var nav                      = host.Get<NavigationService>();
nav.AttachFrame(PageHost);   // idempotent; throws on re-attach
ToastLayer.DataContext       = host.Get<ToastService>();     // exposes Toasts
DialogOverlay.DataContext    = host.Get<DialogService>();    // exposes ActiveDialog
```

Then `Loaded += (_, _) => nav.NavigateTo(Route.Dashboard);` so the
Frame has finished layout before the first page swap. Calling
`NavigateTo` synchronously inside `Initialize` is unsafe: if the
shell constructor (or any future mirrored start-up path) runs
before the Frame is connected to the visual tree, the page would
be queued in the journal but never rendered.

### Alternatives considered

- **`ContentControl` + `DataTemplate`.** Cleaner MVVM but erodes
  the per-feature scaffold (RulesPage becomes a `<DataTemplate
  DataType="{x:Type RulesViewModel}">` inside MainWindow). The
  seven routes still need their own XAML file either way, so a
  Page submodule is cleaner than a centralised template dump.
- **WebView2-based navigation.** Out of question for v1 — Decision 3
  forbids any web runtime. Re-evaluate only if Phase 4 introduces
  a "BioCentri in-browser" surface.
- **`Hyperlink` + `RequestNavigate` per element.** Useful for in-page
  links; not a shell-shape. Out of scope.

---

## Decision 11: Component discipline (declarative first, code-behind surgical)

**Decision:** Every reusable component in BioCentri follows:

1. **Resources over literals.** No raw hex, pixel, or font-size
   values in component XAML — every value is bound to a
   `StaticResource` from `Brushes.xaml`, `Typography.xaml`,
   `Spacing.xaml`, `Corners.xaml`, `Elevation.xaml`, `Motion.xaml`,
   `FieldStyles.xaml`, or `Icons.xaml`. The same rule applies to
   the shell.
2. **`DependencyProperty` over code-behind coupling.** When a
   component takes parent-supplied values (`Title`, `Subtitle`,
   `ShellState`, `NavigateCommand`), it exposes each as a DP with
   a `static readonly` `*Property` field rather than constructor
   arguments. This keeps XAML declarative — consumers bind rather
   than pass arguments at construction time.
3. **No event hooks beyond Windows/Loaded.** Components subscribe
   to their own `Loaded` for first-paint adjustments only.
   Subscribe to other components' events only via an established
   service (`ShellState` / `INavigationService` / etc.).
4. **Animations are Storyboards, defined in code-behind when they
   are component-private; defined in `Transitions.xaml` when
   shared.** Component-private Storyboards live in the
   code-behind's `Build…Storyboard()` method with explicit
   `KeySpline`s (Decision 9 followup). Shared transitions —
   dimmer fade-in, dialog pop-in — live in `Transitions.xaml`.
5. **Each component's XAML namespace mirrors its C# namespace.**
   Rationale: WPF's partial-class bridge requires `x:Class` to
   match the code-behind's `namespace + class`. (See Decision 9
   worked example.) Folder-derived namespaces
   (`BioCentri.App.Components.Nav.SidebarItem`,
   `BioCentri.App.Features.Dashboard.DashboardPage`, etc.)
   are stable and copy-pasteable.
6. **No business logic in components.** SidebarItem knows how to
   render a selected state, but does *not* know what a
   `Route.Dashboard` *is*. `SettingsRow` knows how to render a
   title/subtitle/chevron, but does not know whether it sits in
   `Settings > Privacy` or in `Activity`. The boundary is the
   `Core/Features/Shells design` from IMPLEMENTATION_PLAN.md.

**Reason:**

- **Long-term maintainability over cleverness.** The components
  shipped at M2 (`SidebarItem`, `PageHeader`, `StatisticCard`,
  `ListTile`, `EmptyState`, `Toast`, etc.) will be cloned-and-tweaked
  under M5–M7 (stat cards for Hello outcomes, list tiles for
  protected app rows, empty states for "no challenges yet").
  A component whose values are bound to resources keeps the new
  feature on-style for free.
- **No more 'default WPF look'.** Raw `Border`, `TextBlock`,
  `Button` are *never* used at M2+; consumers pick the matched
  reusable control (`FocalCard`, `PageHeader`, `Icon` button, etc.).
  This is the single rule that prevents the app from drifting toward
  the gray-gradient WPF default look that the prompt explicitly
  rejects.
- **Reduced motion lives at the resource level.** A component's
  Storyboards are gated on `Motion.RespectReducedMotion` at the
  resource level. Future hooks (with the actual media query
  detection) flip a global flag and ALL components fall silent.
- **Single test surface.** Components that don't reach into DI or
  pull business data are trivially renderable in design preview
  tools, which makes visual parity review between the Figma/website
  and the WPF surface dramatically cheaper.

### Known exceptions and where they live

- **Storyboard `BeginAnimation`** calls in component code-behind
  have to know the target element. That's fine — but the
  `KeySpline`/keyframe values themselves MUST still come from a
  resource (`Motion.Duration.*` for duration; explicit KeySpline
  for the easing).
- **`Loaded += (_, _) => …`** is permitted for layout-aware
  adjustments (e.g. `ApplySelection()` in `SidebarItem`). It is
  not permitted for any data-load.
- **Service-specific `DataContext`** — `ToastLayer.DataContext` is
  a `ToastService`, not a `ShellViewModel`. This is the one place
  where the shell intentionally *breaks* the inherited DataContext
  chain, and the rule is documented inline in `MainWindow.xaml`.

### Re-review checklist for new components

When reviewing a new component added under M2+, verify:

- [ ] No raw `#RRGGBB`, `12`, `0,0,0,0` literals.
- [ ] No `<Border>`, `<TextBlock>`, `<Button>` if a reusable
      equivalent exists.
- [ ] Public settable properties are `DependencyProperty`-backed.
- [ ] `x:Class` matches the code-behind's `namespace + class`
      (Decision 9 followup rule).
- [ ] Any new XAML primitive used is added to the corresponding
      primitive dictionary (`Brushes.xaml`, `Corners.xaml`,
      `Spacing.xaml`, etc.) and registered in `Tokens.xaml`'s
      `MergedDictionaries`.

---

_Last reviewed after Milestone 2 — shell, navigation, components._
