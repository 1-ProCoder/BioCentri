using System.Windows.Controls;
using BioCentri.App.Features.About;
using BioCentri.App.Features.Activity;
using BioCentri.App.Features.Dashboard;
using BioCentri.App.Features.Diagnostics;
using BioCentri.App.Features.ProtectedApps;
using BioCentri.App.Features.Rules;
using BioCentri.App.Features.Settings;
using BioCentri.App.Services;

namespace BioCentri.App.Routing;

/// <summary>
/// Lazy-instantiating Page registry. Pages are not built until the user
/// navigates to them, so first-time launch stays fast even as we add
/// features. Future M3+ routes plug in here as additional switch arms.
/// </summary>
public interface IPageRegistry
{
    /// <summary>Build a fresh <see cref="Page"/> for the given route.</summary>
    Page Create(Route route);
}

public sealed class PageRegistry : IPageRegistry
{
    private readonly ServiceHost _host;

    public PageRegistry(ServiceHost host)
    {
        ArgumentNullException.ThrowIfNull(host);
        _host = host;
    }

    /// <inheritdoc />
    public Page Create(Route route) => route switch
    {
        Route.Dashboard     => new DashboardPage     { DataContext = _host.Get<DashboardViewModel>() },
        Route.ProtectedApps => new ProtectedAppsPage { DataContext = _host.Get<ProtectedAppsViewModel>() },
        Route.Rules         => new RulesPage         { DataContext = _host.Get<RulesViewModel>() },
        Route.Activity      => new ActivityPage      { DataContext = _host.Get<ActivityViewModel>() },
        Route.Settings      => new SettingsPage      { DataContext = _host.Get<SettingsViewModel>() },
        Route.About         => new AboutPage         { DataContext = _host.Get<AboutViewModel>() },
        Route.Diagnostics   => new DiagnosticsPage   { DataContext = _host.Get<DiagnosticsViewModel>() },
        _ => throw new ArgumentOutOfRangeException(nameof(route), route, "No page registered for that route."),
    };
}
