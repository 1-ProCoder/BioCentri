namespace BioCentri.App.Routing;

/// <summary>
/// Static metadata for each <see cref="Route"/>. Holds the user-visible
/// titles and icon keys so the Sidebar can render without taking a DI
/// dependency. The actual page construction lives in
/// <see cref="PageRegistry"/> which is DI-aware.
/// </summary>
public static class RouteTable
{
    public static readonly IReadOnlyDictionary<Route, RouteMeta> Map = new Dictionary<Route, RouteMeta>
    {
        [Route.Dashboard]     = new("Dashboard",     "Brand Greeting",        "Icons.Route.Dashboard"),
        [Route.ProtectedApps] = new("Protected Apps", "Locked + Hello gates",   "Icons.Route.ProtectedApps"),
        [Route.Rules]         = new("Rules",          "Future automation list","Icons.Route.Rules"),
        [Route.Activity]      = new("Activity",       "Auth log timeline",     "Icons.Route.Activity"),
        [Route.Settings]      = new("Settings",       "Configure BioCentri",   "Icons.Route.Settings"),
        [Route.About]         = new("About",          "Version + credits",     "Icons.Route.About"),
        [Route.Diagnostics]   = new("Diagnostics",    "Environment + logs",    "Icons.Route.Diagnostics"),
    };

    public static RouteMeta Get(Route route) => Map.TryGetValue(route, out var meta)
        ? meta
        : throw new ArgumentOutOfRangeException(nameof(route), route, "Route is not registered in RouteTable.");
}

public sealed record RouteMeta(string Title, string Subtitle, string IconKey);
