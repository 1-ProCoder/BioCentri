using BioCentri.App.Routing;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BioCentri.App.State;

/// <summary>
/// App-wide shell state: sidebar collapsed/expanded, current route,
/// and current page title (mirrored from <see cref="RouteTable"/>).
/// Observable so the Sidebar, TopBar, and MainWindow's status bar
/// reactively reflect the active view.
/// </summary>
public sealed class ShellState : ObservableObject
{
    private bool _isSidebarExpanded = true;
    public bool IsSidebarExpanded
    {
        get => _isSidebarExpanded;
        set => SetProperty(ref _isSidebarExpanded, value);
    }

    private Route _currentRoute = Route.Dashboard;
    public Route CurrentRoute
    {
        get => _currentRoute;
        set
        {
            if (SetProperty(ref _currentRoute, value))
                CurrentTitle = RouteTable.Get(value).Title;
        }
    }

    private string _currentTitle = "Dashboard";
    public string CurrentTitle
    {
        get => _currentTitle;
        set => SetProperty(ref _currentTitle, value);
    }

    /// <summary>
    /// Milestone 4: true while a biometric authentication prompt is
    /// in flight. Bound by <c>AuthenticationOverlay</c> to drive the
    /// full-shell overlay visibility.
    /// </summary>
    private bool _isAuthenticationInProgress;
    public bool IsAuthenticationInProgress
    {
        get => _isAuthenticationInProgress;
        set
        {
            if (SetProperty(ref _isAuthenticationInProgress, value)
                && !value) PendingAppName = string.Empty;
        }
    }

    /// <summary>
    /// Milestone 4: friendly name of the process currently being
    /// authenticated. Drives the overlay's "<c>Verifying… for X</c>" copy.
    /// </summary>
    private string _pendingAppName = string.Empty;
    public string PendingAppName
    {
        get => _pendingAppName;
        set => SetProperty(ref _pendingAppName, value);
    }

    /// <summary>
    /// Milestone 4: raised when the user dismisses the overlay via the
    /// Cancel button. Subscribed by <c>BiometricAuthService</c> to
    /// force-complete the in-flight <see cref="System.Threading.Tasks.TaskCompletionSource{TResult}"/>
    /// with <c>AuthOutcome.UserCancelled</c> so the watcher unblocks
    /// immediately. The OS-level prompt itself persists until the user
    /// dismisses it (WinRT cannot be cancelled externally).
    /// </summary>
    public event EventHandler? AuthenticationCancelRequested;

    /// <summary>
    /// Milestone 4: cancel handler invoked by the overlay's
    /// <c>CancelCommand</c>. Clears shell state immediately so the
    /// overlay fades out (220ms) and notifies the auth service so its
    /// in-flight task resolves with <c>UserCancelled</c>.
    /// </summary>
    public void CancelAuthentication()
    {
        AuthenticationCancelRequested?.Invoke(this, EventArgs.Empty);
        IsAuthenticationInProgress = false;
        PendingAppName = string.Empty;
    }
}
