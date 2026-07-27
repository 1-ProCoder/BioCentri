using System.Windows.Controls;
using BioCentri.App.Routing;
using BioCentri.App.Types.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BioCentri.App.Services;

/// <summary>
/// Concrete navigation service. The <see cref="Frame"/> reference is
/// assigned by <see cref="MainWindow"/> at composition time so that
/// service registration stays UI-light. The journal is owned by the
/// service (no shared shell journal) to keep memory bounded.
/// </summary>
public sealed class NavigationService : ObservableObject, INavigationService
{
    private readonly IPageRegistry _registry;
    private Frame? _frame;

    private Route _currentRoute = Route.Dashboard;
    public Route CurrentRoute
    {
        get => _currentRoute;
        private set => SetProperty(ref _currentRoute, value);
    }

    public NavigationService(IPageRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);
        _registry = registry;
    }

    /// <summary>Bind the Frame that will host the routed pages. Called once by MainWindow.</summary>
    public void AttachFrame(Frame frame)
    {
        ArgumentNullException.ThrowIfNull(frame);
        _frame = frame;
    }

    /// <inheritdoc />
    public void NavigateTo(Route route)
    {
        if (_frame is null)
            throw new InvalidOperationException(
                "NavigationService.AttachFrame must be called before NavigateTo.");

        // ClearJournal-style behavior without ReservationSetManager:
        // remove the current entry first, then push the new page; this
        // bounds memory to a single tracked entry between navigations.
        while (_frame.CanGoBack) _frame.RemoveBackEntry();

        var page = _registry.Create(route);
        _frame.Navigate(page);
        CurrentRoute = route;
    }

    /// <inheritdoc />
    public void RequestNavigate(Route route) => NavigateTo(route);
}
