using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BioCentri.App.Windows;

/// <summary>
/// ViewModel for the tray icon context menu. Bound in
/// <c>TrayIcon.xaml</c> as a <c>TaskbarIcon.DataContext</c>.
/// Commands resolved from the app-level <c>App.Host</c> singleton
/// because the tray icon is created before the shell window exists
/// (per the M6 sequence: tray → watcher start → MainWindow).
/// </summary>
public sealed partial class TrayIconViewModel : ObservableObject
{
    private Window? _mainWindow;

    /// <summary>Set by <c>App.OnStartup</c> after <c>MainWindow</c> is created.</summary>
    public Window? MainWindow
    {
        get => _mainWindow;
        set => SetProperty(ref _mainWindow, value);
    }

    [RelayCommand]
    private void Show()
    {
        if (_mainWindow is null) return;
        _mainWindow.Show();
        if (_mainWindow.WindowState == WindowState.Minimized)
            _mainWindow.WindowState = WindowState.Normal;
        _mainWindow.Activate();
    }

    [RelayCommand]
    private void Hide()
    {
        _mainWindow?.Hide();
    }

    [RelayCommand]
    private void Pause()
    {
        // M7 wires a real pause/resume toggle in ShellState.
        MessageBox.Show(
            "Pause protection is coming in the next update.",
            "BioCentri",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    [RelayCommand]
    private void Settings()
    {
        Show();
        // Navigate to Settings page — the shell VM handles this.
        var app = Application.Current as App
            ?? throw new InvalidOperationException("No App instance.");
        app.Dispatcher.InvokeAsync(() =>
        {
            var shell = app.Host.Get<BioCentri.App.Features.Shell.ShellViewModel>();
            shell.NavigateCommand.Execute(BioCentri.App.Routing.Route.Settings);
        });
    }

    [RelayCommand]
    private void Quit()
    {
        Application.Current.Shutdown();
    }
}
