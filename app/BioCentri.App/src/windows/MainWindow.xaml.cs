using System.Windows;
using BioCentri.App.Components.Auth;
using BioCentri.App.Features.Shell;
using BioCentri.App.Routing;
using BioCentri.App.Services;
using BioCentri.App.Types.Services;

namespace BioCentri.App.Windows;

/// <summary>
/// Shell composition root. <see cref="Initialize"/> is invoked exactly
/// once by <see cref="App.OnStartup"/>, after <see cref="ServiceHost"/>
/// has registered every concrete service. The window then:
///   * sets DataContext to <see cref="ShellViewModel"/> so bindings to
///     <c>ShellState</c> and <c>NavigateCommand</c> resolve through the
///     inheritance chain,
///   * attaches its <see cref="Frame"/> to <see cref="NavigationService"/>,
///   * gives <see cref="ToastLayer"/> its <see cref="IToastService"/> DC
///     and <see cref="DialogOverlay"/> its <see cref="IDialogService"/> DC
///     so the overlay bindings resolve to the right ViewModel-style
///     objects (not the ShellViewModel),
///   * defers the first <c>NavigateTo(Dashboard)</c> to the Loaded event
///     so the Frame has been measured and laid out before the page swap.
/// </summary>
public partial class MainWindow : Window
{
    private ServiceHost? _host;

public MainWindow()
{
    try
    {
        InitializeComponent();
    }
    catch (Exception ex)
    {
        // XAML/baml load failures (StaticResource key not found, composed
        // MarkupExtension, unreachable pack URI, etc.) all surface here
        // as a XamlParseException. WPF wraps the real cause with a
        // generic "Cannot locate resource '...'" wrapper whose source
        // attribution is the baml offset, not the XAML file. So we
        // surface Type + Message + Stack from the InnerException (with
        // a fall-through to ex.*), mirror the same text to a sidecar
        // log so the body is readable from a non-interactive shell, show
        // it in a MessageBox for a human, then rethrow so App.xaml.cs's
        // global DispatcherUnhandledException handler can also log it.
        var text = $"InnerException: {ex.InnerException?.GetType().Name}\n\n" +
                   $"Message: {ex.InnerException?.Message ?? ex.Message}\n\n" +
                   $"Stack:\n{ex.InnerException?.StackTrace ?? ex.StackTrace}";
        try
        {
            var logPath = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "biocentri-xaml-error-" + System.Environment.ProcessId + ".log");
            System.IO.File.WriteAllText(logPath, text);
        }
        catch { /* never let the sidecar write mask the real diagnostic */ }
        System.Windows.MessageBox.Show(text, "Startup Error");
        throw;
    }

    Loaded += OnLoadedOnce;
}

    /// <summary>Called by <see cref="App.OnStartup"/> after the host is built.</summary>
    public void Initialize(ServiceHost host)
    {
        ArgumentNullException.ThrowIfNull(host);
        _host = host;

        DataContext = host.Get<ShellViewModel>();

        var navigation = host.Get<NavigationService>();
        navigation.AttachFrame(PageHost);

        ToastLayer.DataContext    = host.Get<ToastService>();
        DialogOverlay.DataContext = host.Get<DialogService>();
        AuthOverlay.DataContext   = host.Get<AuthenticationOverlayViewModel>();
    }

    private void OnLoadedOnce(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoadedOnce;
        if (_host is null) return;

        var navigation = _host.Get<NavigationService>();
        var shellState = _host.Get<BioCentri.App.State.ShellState>();

        // First navigation synchronously; subsequent navigations arrive
        // from ShellViewModel.NavigateCommand via SidebarItem clicks.
        navigation.NavigateTo(Route.Dashboard);
        shellState.CurrentRoute  = Route.Dashboard;
        shellState.CurrentTitle  = RouteTable.Get(Route.Dashboard).Title;
    }
}
