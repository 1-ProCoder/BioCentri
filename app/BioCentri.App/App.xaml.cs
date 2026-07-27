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
            .AddSingleton<SettingsViewModel>(new SettingsViewModel())
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
                host.Get<ShellState>()));

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

        // ---- M7.1 pending: tray icon activation. The TrayIconViewModel
        //      (Show / Hide / Pause / Settings / Quit) is registered in
        //      DI and ready to bind. Once the offline NuGet cache
        //      resolves H.NotifyIcon.Wpf 2.x (see Decision 9), uncomment
        //      the block below to create the TaskbarIcon.
        //
        // var trayVm = host.Get<TrayIconViewModel>();
        // trayVm.MainWindow = shell;
        // _ = new Hardcodet.Wpf.TaskbarNotification.TaskbarIcon
        // {
        //     Icon = System.Drawing.Icon.ExtractAssociatedIcon(...),
        //     ToolTipText = "BioCentri",
        //     ContextMenu = /* WPF ContextMenu bound to trayVm.*Command */,
        //     Visibility = System.Windows.Visibility.Visible,
        // };
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

    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        // e.Exception.ToString() unwraps the full chain (message + all inner
        // exceptions + stack traces), unlike .Message which shows only the
        // top-level string (often a generic wrapper like "Cannot locate
        // resource"). WPF exceptions routinely nest 3+ levels deep.
        MessageBox.Show(
            $"BioCentri hit an unexpected error:\n\n{e.Exception}",
            "BioCentri",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        e.Handled = true;
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
