using System.Collections.ObjectModel;
using BioCentri.App.Routing;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BioCentri.App.Features.Activity;

/// <summary>
/// Activity view-model. M2 placeholder per IMPLEMENTATION_PLAN §7.
/// Today the page surfaces empty-state stats + an empty grouped
/// events collection. Milestone 5 (per Decision 3 + §7) replaces
/// the static log with the live audit log fed by the BioCentri
/// process monitor and Windows Hello outcome stream.
/// </summary>
public sealed partial class ActivityViewModel : ObservableObject
{
    public string Title => RouteTable.Get(Route.Activity).Title;
    public string Subtitle => RouteTable.Get(Route.Activity).Subtitle;

    public ObservableCollection<ActivityStatRow> Stats { get; } = new();
    public ObservableCollection<ActivityTimelineRow> Events { get; } = new();

    public ActivityViewModel()
    {
        Stats.Add(new ActivityStatRow("Today",     "0", "Challenges"));
        Stats.Add(new ActivityStatRow("This week", "0", "Successes"));
        Stats.Add(new ActivityStatRow("Failed",    "0", "Required retry"));
        Stats.Add(new ActivityStatRow("Avg. time", "—", "Decision latency"));
    }
}

public sealed record ActivityStatRow(string Label, string Value, string Caption);

public sealed record ActivityTimelineRow(
    string Group,
    DateTime TimestampUtc,
    string Severity,
    string AppName,
    string Description);
