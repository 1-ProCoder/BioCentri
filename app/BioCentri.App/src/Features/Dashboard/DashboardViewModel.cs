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

    public string GreetingLine => "Good to see you.";
    public string StatusLine =>
        "BioCentri is ready. Add an application to protect and Windows Hello will gate it from here.";
    public string ReadinessLine =>
        "BioCentri is local-first. Nothing on this dashboard ever leaves your device.";

    public ObservableCollection<DashboardStatRow> Stats { get; } = new();

    public DashboardViewModel()
    {
        Stats.Add(new DashboardStatRow("Protected apps",     "0", "Add one in Protected apps"));
        Stats.Add(new DashboardStatRow("Hello challenges",   "0", "Waiting on first protected launch"));
        Stats.Add(new DashboardStatRow("Active rules",       "0", "Automation comes in Milestone 4"));
        Stats.Add(new DashboardStatRow("Session start",      DateTime.Now.ToString("HH:mm", CultureInfo.InvariantCulture), "Today"));
    }
}

public sealed record DashboardStatRow(string Label, string Value, string Caption);
