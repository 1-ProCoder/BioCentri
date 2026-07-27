using System.Collections.ObjectModel;
using System.Globalization;
using BioCentri.App.Routing;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BioCentri.App.Features.Dashboard;

/// <summary>
/// Dashboard view-model. M2: brand hero + 4-up stat grid + recent-activity
/// empty state. No business logic. M3 visual polish
/// (BentoStats + HologramFloat + ReticleRing + BorderTrace) lands in a
/// later milestone per IMPLEMENTATION_PLAN §7.
/// </summary>
public sealed partial class DashboardViewModel : ObservableObject
{
    public string Title => RouteTable.Get(Route.Dashboard).Title;
    public string Subtitle => RouteTable.Get(Route.Dashboard).Subtitle;

    public string GreetingLine { get; }

    public string StatusLine =>
        "BioCentri v1.0.0 is ready. Add an app to protect and Windows Hello will gate it from here.";
    public string ReadinessLine =>
        "BioCentri is local-first. Nothing on this dashboard ever leaves your device.";

    public ObservableCollection<DashboardStatRow> Stats { get; } = new();

    public DashboardViewModel()
    {
        var now = DateTime.Now.ToString("HH:mm", CultureInfo.InvariantCulture);
        var date = DateTime.Now.ToString("ddd, d MMM", CultureInfo.InvariantCulture);
        var h = DateTime.Now.Hour;

        GreetingLine = h < 12 ? "Good morning." : h < 18 ? "Good afternoon." : "Good evening.";

        Stats.Add(new DashboardStatRow("Protected apps",     "0", "Protect your first app to see it here"));
        Stats.Add(new DashboardStatRow("Hello challenges",   "0", "Triggers when a protected app launches"));
        Stats.Add(new DashboardStatRow("Active rules",       "0", "Automation rules — Phase 2 feature"));
        Stats.Add(new DashboardStatRow("Session start",      now, date));
    }
}

public sealed record DashboardStatRow(string Label, string Value, string Caption);
