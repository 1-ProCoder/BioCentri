using System;
using System.Windows;
using System.Windows.Threading;
using BioCentri.App.Features.About;
using BioCentri.App.Features.Activity;
using BioCentri.App.Features.Dashboard;
using BioCentri.App.Features.Diagnostics;
using BioCentri.App.Features.ProtectedApps;
using BioCentri.App.Features.Rules;
using BioCentri.App.Features.Settings;
using BioCentri.App.Features.Shell;
using BioCentri.App.Components.Auth;
using BioCentri.App.Routing;
using BioCentri.App.Services;
using BioCentri.App.State;
using BioCentri.App.Types.Services;
using ActivityEvent = BioCentri.App.Types.ActivityEvent;
using BioCentri.App.Windows;
using BioCentri.Core.Interop;
using BioCentri.Core.Services;
// M7.1 pending: using Hardcodet.Wpf.TaskbarNotification;

namespace BioCentri.App;

/// <summary>
/// Composition root. Builds the (in-house, dep-light M1) DI host in the
/// correct dependency order at startup, installs global exception
/// handlers, then composes <see cref=\"MainWindow\"/>. See
/// <c>docs/DECISIONS.md</c> Decision 9 for the rationale on the bespoke
/// DI host, Decision 12 for the JSON-file persistence layer.
/// </summary>
public partial class App : Application
{
    /// <summary>DI host. New VM/control code MUST use constructor
    /// injection; this is the escape hatch only.</summary>
    public ServiceHost Host { get; private set; } = null!;

    private IAppLifecycleService? _lifecycle;

    public App()
    {
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        // Milestone 7: watch for Windows High Contrast toggle. When the
        // user flips it in Windows Settings, we swap the theme dictionary
        // at MergedDictionaries[0] between Dark.xaml and HighContrast.xaml
        // so the entire UI palette changes without a restart.
        SystemParameters.StaticPropertyChanged += OnSystemParameterChanged;
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var dispatcher = Dispatcher;
        var host = new ServiceHost();

        // ---- Order matters: each `AddSingleton` lays down a service
        //      the ones below depend on. The chain is linear and the
        //      lambda/closure passes for NavigationService below resolve
        //      against this `host` reference.

        host.AddSingleton<IDispatcher>(new DispatcherHolder(dispatcher))
            .AddSingleton<IAppLifecycleService>(new AppLifecycleService())
            .AddSingleton<AppState>(new AppState())
            .AddSingleton<ShellState>(new ShellState())
            .AddSingleton<ILocalJsonStore>(new LocalJsonStore())
            // Milestone 4: registry-based installed-app discovery. Stateless +
            // thread-pool-safe (own deadline) — registered next to the JSON store
            // because both feed ProtectedAppsViewModel.
            .AddSingleton<InstalledAppsDiscovery>(new InstalledAppsDiscovery())
            .AddSingleton<IInstalledAppsDiscovery>(host.Get<InstalledAppsDiscovery>())
            // ToastService had to land BEFORE the page-VM chain because
            // ProtectedAppsViewModel binds IToastService in its ctor
            // for the visible \"Add application\" toast (M2 placeholder
            // for the M4 AddApplicationDialog pipeline).
            .AddSingleton<ToastService>(new ToastService())
            .AddSingleton<IToastService>(host.Get<ToastService>())
            // DialogService+IDialogService MUST be registered before
            // ProtectedAppsViewModel — its ctor injects IDialogService.
            .AddSingleton<DialogService>(new DialogService())
            .AddSingleton<IDialogService>(host.Get<DialogService>());

        // ---- Page ViewModels — Milestone 5: each loads + persists its
        //      own JSON file via ILocalJsonStore. Activity / Dashboard /
        //      Diagnostics depend on services registered further down, so
        //      they are added later in this method, in dependency order.
        // ---- Page ViewModels (M2 scope). The Milestone-5-strict
        //      variants (each loading + persisting its own JSON file via
        //      ILocalJsonStore, Activity/Dashboard subscribing to events)
        //      are intentionally deferred — the navigation pipeline must
        //      verify end-to-end against the M2 placeholders first. The
        //      M2-faithful parameterless registration here matches the
        //      VMs in BioCentri.App/src/Features/<feature>/.
        host.AddSingleton<ProtectedAppsViewModel>(new ProtectedAppsViewModel(
            host.Get<IToastService>(),
            host.Get<ILocalJsonStore>(),
            host.Get<IDialogService>(),
            host.Get<IDispatcher>(),
            host.Get<IInstalledAppsDiscovery>()))
            .AddSingleton<RulesViewModel>(new RulesViewModel(host.Get<ILocalJsonStore>()))
            .AddSingleton<AboutViewModel>(new AboutViewModel());

        // ---- Navigation / overlay services
        host.AddSingleton<IPageRegistry>(new PageRegistry(host));
        var navigation = new NavigationService(host.Get<IPageRegistry>());
        host.AddSingleton<NavigationService>(navigation)
            .AddSingleton<INavigationService>(navigation);

        // ---- Shell VM (depends on INavigationService + ShellState)
        host.AddSingleton<ShellViewModel>(
            new ShellViewModel(host.Get<INavigationService>(), host.Get<ShellState>()));

        // ---- Milestone 5+6: auth + process monitoring pipeline.
        //      ProcessMonitor (WMI Win32_ProcessStartTrace + 5s polling
        //      fallback) replaces StubProcessMonitor.
        //      FileBackedAuthAppRules reads the same protectedApps.json
        //      the UI writes, via last-write-time TTL cache.
        //      AppLockController kills blocked processes; injected into
        //      ProcessWatcher so the kill fires after an auth failure.
        host.AddSingleton<IProcessMonitor>(new ProcessMonitor())
            .AddSingleton<IAuthAppRules>(new FileBackedAuthAppRules(
                host.Get<ILocalJsonStore>()))
            .AddSingleton<AppLockController>(new AppLockController(
                host.Get<IToastService>()))
            .AddSingleton<IActivityLogger>(new ActivityLogger(host.Get<ILocalJsonStore>()))
            .AddSingleton<IHelloService>(new UserConsentVerifierAdapter())
            .AddSingleton<IBiometricAuthService>(new BiometricAuthService(
                host.Get<IDispatcher>(),
                host.Get<IToastService>(),
                host.Get<ShellState>(),
                host.Get<IHelloService>(),
                host.Get<IActivityLogger>()))
            .AddSingleton<ProcessWatcher>(new ProcessWatcher(
                host.Get<IProcessMonitor>(),
                host.Get<IAuthAppRules>(),
                host.Get<IBiometricAuthService>(),
                host.Get<ShellState>(),
                host.Get<IAppLifecycleService>(),
                host.Get<IDispatcher>(),
                host.Get<AppLockController>(),
                host.Get<IActivityLogger>()))
            .AddSingleton<AuthenticationOverlayViewModel>(new AuthenticationOverlayViewModel(
                host.Get<ShellState>()))
            // M7.5: SettingsViewModel now injects IBiometricAuthService +
            // IToastService + IDispatcher so the page exposes a live
            // "Test Windows Hello" affordance. The registration moves
            // here (after the auth pipeline) to satisfy the dep-light
            // bespoke DI host's order-sensitive AddSingleton chain.
            .AddSingleton<SettingsViewModel>(new SettingsViewModel(
                host.Get<IBiometricAuthService>(),
                host.Get<IToastService>(),
                host.Get<IDispatcher>()));

        // ---- DiagnosticsViewModel → ActivityViewModel → DashboardViewModel
        //      are registered LAST among the page VMs so their dependencies
        //      are guaranteed live. M2 placeholder scope — the constructors
        //      are parameterless and match BioCentri.App/src/Features/...
        //      The M5-strict event-subscription wiring is documented in
        //      DECISIONS.md and re-introduced when those milestones launch.
        host.AddSingleton<DiagnosticsViewModel>(new DiagnosticsViewModel())
            .AddSingleton<ActivityViewModel>(new ActivityViewModel(host.Get<ILocalJsonStore>()))
            .AddSingleton<DashboardViewModel>(new DashboardViewModel(host.Get<ILocalJsonStore>()));

        // ---- TrayIcon VM (M7.1 pending) — pre-registered so the
        //      DI slot is reserved. Tray icon activation is deferred:
        //      H.NotifyIcon.Wpf 2.x namespace resolution fails on this
        //      machine's offline NuGet cache (Decision 9). See docs/
        //      INSTALLER.md § Tray Icon for the re-activation checklist.
        host.AddSingleton<TrayIconViewModel>(new TrayIconViewModel());

        // ---- MainWindow last — constructor pulls no DI; Initialize()
        //      happens immediately after this block to wire DataContext.
        //      The AuthenticationOverlay cell inside MainWindow reads its
        //      VM through host.Get<AuthenticationOverlayViewModel>().
        host.AddSingleton<MainWindow>(new MainWindow());

        Host = host;
        _lifecycle = Host.Get<IAppLifecycleService>();

        // Per Milestone-4 spec: ProcessWatcher MUST be running before
        // MainWindow.Show() so any process launches that race with the
        // initial render are still seen by the watcher.
        Host.Get<ProcessWatcher>().Start();

        MainWindow shell = Host.Get<MainWindow>();
        shell.Initialize(Host);
        shell.Show();

        _lifecycle.MainWindowShown = true;

        // ---------------- Milestone 7+ defence-in-depth ----------------
        // Smoke-test the documented design-system keys at startup. WPF's
        // MarkupCompilePass1 does NOT validate StaticResource keys — a
        // missing reference (e.g. Typography.Size.Title) silently compiles,
        // and only surfaces as a XamlParseException when the offending
        // template is loaded by the runtime. That 'late' failure is what
        // produced the long chain of 'Cannot find resource named
        // Elevation.2.Resting / Timeline2.Resting / TimelineSeverity /
        // Brushes.Text.Muted' popups during M4 debugging. Unconditional:
        // a Release build with a missing key silently fails the same way
        // as DEBUG, so the smoke test is always on. Cost <1ms at startup.
        //
        // FindResource throws ResourceReferenceKeyNotFoundException here,
        // failing fast on F5 (and in Release) if anyone deletes or renames
        // a token without updating every consumer. The list is curated for
        // high-impact keys; designers adding a new token SHOULD add it here
        // so future typos land at startup instead of mid-click.
        try
        {
            AssertCriticalResourceKeys();
        }
        catch (Exception ex)
        {
            // Re-raise as InvalidOperationException with a single actionable
            // line — much easier for a developer to grep than wading through
            // the native ResourceReferenceKeyNotFoundException text.
            throw new InvalidOperationException(
                "BioCentri startup smoke-test FAILED \u2014 a design-system " +
                "token is referenced by some *.xaml but never defined. " +
                "Add the missing key to the matching style/<Name>.xaml and " +
                "rebuild.\n\n" + ex.Message, ex);
        }
    }

    /// <summary>
    /// Fail-loud startup smoke test. Each <see cref="Application.FindResource(object)"/>
    /// throws immediately if the key is undefined, so the app exits before
    /// the user ever sees a runtime popup storm.
    /// </summary>
    private static void AssertCriticalResourceKeys()
    {
        var required = new[]
        {
            // Typography (M6+ additions)
            "Typography.Sans", "Typography.Display", "Typography.Monospace",
            "Typography.Size.Caption", "Typography.Size.Small", "Typography.Size.Body",
            "Typography.Size.Large", "Typography.Size.H4", "Typography.Size.Title",
            "Typography.Size.H3", "Typography.Size.H2", "Typography.Size.H1",
            "Typography.Size.Display",
            "Typography.Weight.Regular", "Typography.Weight.Medium",
            "Typography.Weight.SemiBold", "Typography.Weight.Bold",

            // Brush surfaces (heavily used by every page)
            "Brushes.Surface.Base", "Brushes.Surface.Card", "Brushes.Surface.Sunken",
            "Brushes.Surface.Raised",
            "Brushes.Text.Primary", "Brushes.Text.Muted", "Brushes.Text.Violet",
            "Brushes.Accent.Indigo", "Brushes.Accent.IndigoLight",
            "Brushes.Accent.VioletLight", "Brushes.Accent.Emerald",
            "Brushes.Accent.IndigoGlow", "Brushes.Accent.EmeraldGlow",
            "Brushes.Accent.Gradient",
            "Brushes.Border.Hairline", "Brushes.Border.HairlineStrong",
            "Brushes.Border.HairlineSoft", "Brushes.Border.HairlineInner",
            "Brushes.Glass.Tint", "Brushes.GlassStrong.Tint",
            "Brushes.Scrim", "Brushes.Subtle.Surface", "Brushes.Selection",
            "Brushes.Status.Success", "Brushes.Status.Warn", "Brushes.Status.Danger",

            // Elevation ladder (each tier referenced somewhere)
            "Elevation.0.Flat", "Elevation.1.Resting", "Elevation.2.Resting",
            "Elevation.2.Hover", "Elevation.3.Lifted", "Elevation.4.Modal",
            "Elevation.Accent.Glow",

            // Shadows
            "Shadows.Card.Default", "Shadows.Card.Focal", "Shadows.Hover.Lift",
            "Shadows.Accent.Glow", "Shadows.InnerHighlight.Default",

            // Spacing recipes
            "Spacing.Pad.Xxs", "Spacing.Pad.Xs", "Spacing.Pad.Sm", "Spacing.Pad.Md",
            "Spacing.Pad.Lg", "Spacing.Pad.Xl", "Spacing.Pad.Xxl",

            // Corner radii
            "Corners.None", "Corners.Xs", "Corners.Sm", "Corners.Md",
            "Corners.Lg", "Corners.Xl", "Corners.Pill",

            // Borders / strokes
            "Border.Thin", "Stroke.Thin", "Stroke.Thick",

            // Colors (GradientStops need Color, not Brush)
            "Colors.Ink.950", "Colors.Ink.900",
            "Colors.Accent.IndigoGlow", "Colors.Accent.EmeraldGlow",
        };

        var app = Application.Current;
        foreach (var key in required)
        {
            // FindResource throws ResourceReferenceKeyNotFoundException
            // immediately if 'key' isn't in the merged dictionary chain.
            _ = app.FindResource(key);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (_lifecycle is not null) _lifecycle.IsShuttingDown = true;
        // Stop the process watcher before the dispatcher tears down so
        // any in-flight background callback (auth flow) doesn't race the
        // AppDomain unload.
        if (Host is not null && Host.IsRegistered<ProcessWatcher>())
        {
            try { Host.Get<ProcessWatcher>().Stop(); } catch { /* shutdown; swallow */ }
        }
        base.OnExit(e);
    }

    // ------------------------------------------------------------
    // Loop-breaker for DispatcherUnhandledException.
    //
    // The pre-fix handler would mark every exception as Handled and
    // immediately re-show the popup. WPF exceptions fired from a render
    // tick (animations, frozen-resource mutations, path-resolution
    // failures, etc.) re-evaluate on every ~16ms composition pass. The
    // result was an infinite MessageBox cascade that could crash the
    // app via modal exhaustion.
    //
    // Behaviour now:
    //   * First time a given exception fingerprint surfaces        → show
    //     the popup so the user can still copy the inner-exception text.
    //   * Same fingerprint re-fires within 1500ms                 → silently
    //     swallow (e.Handled = true). This is the loop-breaker; the user
    //     keeps using the app instead of being modal-bombed.
    //   * Different fingerprint, or more than 1500ms later         → next
    //     popup is shown again (so genuinely new errors aren't hidden).
    //
    // Static fields are intentional: App is process-singleton so the
    // previous-handler-cache survives the lifetime of the application.
    private static string? _lastShownFingerprint;
    private static DateTime _lastShownAtUtc = DateTime.MinValue;
    private static readonly TimeSpan LoopBreakWindow = TimeSpan.FromMilliseconds(1500);

    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        // CRITICAL: NEVER swallow markup / parsing exceptions.
        //
        // When a XamlParseException is marked Handled, WPF continues
        // to drive the render loop on a broken visual tree. The BAML
        // string-pool reader state has been corrupted by the first
        // failure; subsequent render passes then read adjacent / wrong
        // entries from the string pool and emit phantom 'Cannot find
        // resource named X' exceptions where X is a random internal
        // WPF class name (TimelineSeverity, Timeline2.Resting,
        // Elevation.2.Resting, Typography.Size.Title, Corners.Base,
        // Brushes.Text.Muted, ...). Those keys do not actually exist
        // anywhere in the source — they were never referenced, just
        // emitted by BAML after the corruption. The real underlying
        // error is upstream and masked by the loop.
        //
        // Letting XamlParseException propagate (e.Handled = false)
        // preserves the original inner-exception type + message +
        // stack so the user (or the sidecar log) reports the actual
        // root cause ONCE — no cascading popup storm, no BAML
        // desynchronisation.
        if (e.Exception is System.Windows.Markup.XamlParseException)
        {
            TryWriteSidecarLog(e.Exception);
            e.Handled = false;
            return;
        }

        // For non-markup exceptions (e.g. NullReferenceException from
        // a code bug, IO error, dispatcher-level problem) mark handled
        // and debounce identical re-fires within 1500ms so a per-tick
        // bug doesn't modal-exhaust the App via cascading popups.
        e.Handled = true;

        var fingerprint = ComputeFingerprint(e.Exception);
        var now = DateTime.UtcNow;

        if (_lastShownFingerprint is not null
            && string.Equals(_lastShownFingerprint, fingerprint, StringComparison.Ordinal)
            && (now - _lastShownAtUtc) < LoopBreakWindow)
        {
            return;
        }

        _lastShownFingerprint = fingerprint;
        _lastShownAtUtc = now;

        TryWriteSidecarLog(e.Exception);

        // e.Exception.ToString() unwraps the full chain (message + all inner
        // exceptions + stack traces), unlike .Message which shows only the
        // top-level string (often a generic wrapper like "Cannot locate
        // resource"). WPF exceptions routinely nest 3+ levels deep.
        MessageBox.Show(
            $"BioCentri hit an unexpected error:\n\n{e.Exception}",
            "BioCentri",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }

    /// <summary>
    /// Persist the current unhandled exception to %TEMP%/biocentri-xaml-error-*.log
    /// so the diagnostic is recoverable across crashes. Tagged with type +
    /// message + full ToString() (which unwraps the InnerException chain).
    /// The companion scripts/diag_clean_build_probe.ps1 grep-reads this
    /// on startup to surface stale exceptions from a prior crash.
    /// </summary>
    private static void TryWriteSidecarLog(Exception ex)
    {
        try
        {
            var logPath = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"biocentri-xaml-error-{DateTime.UtcNow:yyyyMMddHHmmssfff}.log");
            System.IO.File.WriteAllText(
                logPath,
                $"Timestamp: {DateTime.UtcNow:O}\n" +
                $"Exception type: {ex.GetType().FullName}\n" +
                $"Message: {ex.Message}\n" +
                $"\n--- ToString() (includes InnerException chain + stack) ---\n{ex}\n");
        }
        catch
        {
            // logging must never throw — it runs from inside the
            // exception handler itself.
        }
    }

    /// <summary>
    /// Stable per-exception fingerprint: exception type + the first
    /// ~256 chars of stack trace. We deliberately include the stack so
    /// two unrelated exceptions of the same type don't get collapsed.
    /// </summary>
    private static string ComputeFingerprint(Exception ex)
    {
        var typeName = ex.GetType().FullName ?? ex.GetType().Name;
        var stackHead = ex.StackTrace ?? string.Empty;
        const int head = 256;
        var stackSample = stackHead.Length <= head ? stackHead : stackHead.Substring(0, head);
        return typeName + "::" + stackSample;
    }

    private static void OnDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            MessageBox.Show(
                $"BioCentri hit a fatal error:\n\n{ex.Message}",
                "BioCentri",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        // Swallow so micro-thread leaks don't crash the host.
        // Debug breadcrumb surfaces in attached-debugger runs; M5 wires
        // a real local audit log alongside the protected-apps probe.
        System.Diagnostics.Debug.WriteLine($"[unobserved] {e.Exception}");
        e.SetObserved();
    }

    /// <summary>
    /// Milestone 7: swap the active theme at MergedDictionaries[0] when
    /// the user toggles Windows High Contrast on or off. The rest of the
    /// dictionary chain (Corners, Elevation, Icons, etc.) stays unchanged
    /// — only the theme surface flips.
    /// </summary>
    // Subscriptions are intentional process-lifetime bindings — the
    // App singleton never goes away, so no unsubscribe is needed.
    private static void OnSystemParameterChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(SystemParameters.HighContrast)) return;

        var resources = Application.Current.Resources.MergedDictionaries;
        if (resources.Count == 0) return;

        var oldTheme = resources[0];
        var newTheme = new ResourceDictionary
        {
            Source = SystemParameters.HighContrast
                ? new Uri("pack://application:,,,/src/styles/Themes/HighContrast.xaml", UriKind.Absolute)
                : new Uri("pack://application:,,,/src/styles/Themes/Dark.xaml", UriKind.Absolute),
        };

        resources.RemoveAt(0);
        resources.Insert(0, newTheme);
    }
}
