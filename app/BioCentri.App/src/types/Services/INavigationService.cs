using BioCentri.App.Routing;

namespace BioCentri.App.Types.Services;

/// <summary>
/// Routes are addressable by name. The View binds to <see cref="CurrentRoute"/>
/// for highlight / breadcrumb logic; the App-side calls
/// <see cref="NavigateTo"/> whenever the user picks a sidebar item or
/// triggers a deep link.
/// </summary>
public interface INavigationService
{
    /// <summary>Currently displayed route. Reactive.</summary>
    Route CurrentRoute { get; }

    /// <summary>Swap the active page; replace journal entries to avoid leaks.</summary>
    void NavigateTo(Route route);

    /// <summary>Request navigation. Useful for menu commands that arrive via ICommand.</summary>
    void RequestNavigate(Route route);
}
