using System.ComponentModel;
using System.Windows;
using BioCentri.App.State;
using BioCentri.App.Types.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BioCentri.App.Components.Auth;

/// <summary>
/// ViewModel for <see cref="AuthenticationOverlay"/>. Mirrors the
/// authentication state exposed by <see cref="ShellState"/>
/// (<c>IsAuthenticationInProgress</c>, <c>PendingAppName</c>) into
/// overlay-friendly properties (<c>Visibility</c>, <c>AppName</c>) and
/// routes user-cancel intent back to <see cref="ShellState.CancelAuthentication"/>.
///
/// The VM never talks directly to <see cref="IBiometricAuthService"/>; the
/// service subscribes to <see cref="ShellState.AuthenticationCancelRequested"/>
/// itself and force-completes the in-flight prompt. Single-channel routing
/// keeps the contract explicit and easy to reason about.
/// </summary>
public sealed partial class AuthenticationOverlayViewModel : ObservableObject
{
    private readonly ShellState _shellState;

    /// <summary>True while an OS Hello prompt is in flight. Drives fade-in/out.</summary>
    private bool _isVisible;
    public bool IsVisible
    {
        get => _isVisible;
        private set => SetProperty(ref _isVisible, value);
    }

    /// <summary><see cref="Visibility"/> mirror — VM owns the converter so XAML stays flat.</summary>
    private Visibility _visibility = Visibility.Collapsed;
    public Visibility Visibility
    {
        get => _visibility;
        private set => SetProperty(ref _visibility, value);
    }

    /// <summary>Friendly name of the protected app being verified.</summary>
    private string _appName = string.Empty;
    public string AppName
    {
        get => _appName;
        private set => SetProperty(ref _appName, value);
    }

    /// <summary>User clicked "Cancel" — closes the overlay. The OS dialog persists until the user dismisses it.</summary>
    public IRelayCommand CancelCommand { get; }

    public AuthenticationOverlayViewModel(ShellState shellState)
    {
        ArgumentNullException.ThrowIfNull(shellState);
        _shellState = shellState;
        _shellState.PropertyChanged += OnShellStatePropertyChanged;
        CancelCommand = new RelayCommand(CancelImpl, () => _isVisible);
        SyncFromShellState();
    }

    private void OnShellStatePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ShellState.IsAuthenticationInProgress)
                          or nameof(ShellState.PendingAppName))
        {
            SyncFromShellState();
        }
    }

    private void SyncFromShellState()
    {
        IsVisible = _shellState.IsAuthenticationInProgress;
        Visibility = IsVisible ? Visibility.Visible : Visibility.Collapsed;
        AppName = _shellState.PendingAppName ?? string.Empty;
        ((RelayCommand)CancelCommand).NotifyCanExecuteChanged();
    }

    private void CancelImpl() => _shellState.CancelAuthentication();
}
