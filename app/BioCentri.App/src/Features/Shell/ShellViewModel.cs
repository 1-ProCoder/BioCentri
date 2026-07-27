using BioCentri.App.Routing;
using BioCentri.App.State;
using BioCentri.App.Types.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BioCentri.App.Features.Shell;

/// <summary>
/// Composition owner of the main shell. Exposes the current
/// <see cref="ShellState"/> to <c>MainWindow.xaml</c> bindings (set as
/// <c>MainWindow.DataContext</c>) and a navigation-tunnelling command
/// that Sidebar / drill-in buttons call into.
///
/// M2 scope: thin glue between the navigation service and the chrome
/// state. Logic lives in <c>INavigationService</c> and
/// <c>ShellState</c>; this class never decides anything, it only
/// forwards.
/// </summary>
public sealed partial class ShellViewModel : ObservableObject
{
    private readonly INavigationService _nav;

    /// <summary>The single per-app observable state the shell reads.</summary>
    public ShellState State { get; }

    public ShellViewModel(INavigationService nav, ShellState state)
    {
        _nav = nav;
        State = state;
    }

    /// <summary>
    /// Forward a route navigation into the navigation service AND keep
    /// the shell-state selection synchronised. Sidebar items bind this
    /// with <c>Command="{Binding DataContext.NavigateCommand,
    /// RelativeSource={RelativeSource AncestorType=Window}}"
    /// CommandParameter="{x:Static routing:Route.Xxx}</c>.
    /// </summary>
    [RelayCommand]
    private void Navigate(Route route)
    {
        _nav.NavigateTo(route);
        State.CurrentRoute = route;
    }

    /// <summary>
    /// Toggle the sidebar collapse. Bound from the topbar hamburger.
    /// </summary>
    [RelayCommand]
    private void ToggleSidebar() => State.IsSidebarExpanded = !State.IsSidebarExpanded;
}
