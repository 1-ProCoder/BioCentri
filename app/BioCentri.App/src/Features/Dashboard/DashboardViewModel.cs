using System.Collections.ObjectModel;
using System.Globalization;
using BioCentri.App.Features.ProtectedApps;
using BioCentri.App.Routing;
using BioCentri.App.Types;
using BioCentri.App.Types.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BioCentri.App.Features.Dashboard;

/// <summary>
/// Dashboard view-model. Reads the canonical state from
/// <see cref="ILocalJsonStore"/> (protectedApps.json + activity.json)
/// and surfaces it as stat tiles for the Bento grid. Greeting is
/// computed once at construction (M2 chill-promise — the time-of-day
/// branch is stable, the orientation copy is deterministic).
///
/// Recent-activity tile is the 5 newest events from activity.json,
/// pre-grouped by day for the timeline strip.
/// </summary>
public sealed partial class DashboardViewModel : ObservableObject
{
    private const string ProtectedFile = "protectedApps.json";
    private const string ActivityFile   = "activity.json";

    private readonly ILocalJsonStore _store;

    public string Title => RouteTable.Get(Route.Dashboard).Title;
    public string Subtitle => RouteTable.Get(Route.Dashboard).Subtitle;

    public string StatusLine =>
        "BioCentri v1.0.0 is ready. Add an app to protect and Windows Hello will gate it from here.";

    public string ReadinessLine =>
        "BioCentri is local-first. Nothing on this dashboard ever leaves your device.";

    public string GreetingLine { get; }

    public ObservableCollection<DashboardStatRow> Stats { get; } = new();
    public ObservableCollection<DashboardActivityRow> Recent { get; } = new();

    public DashboardViewModel(ILocalJsonStore store)
    {
        _store = store;
        var h = DateTime.Now.Hour;
        GreetingLine = h < 12 ? "Good morning." : h < 18 ? "Good afternoon." : "Good evening.";
        _ = RefreshAsync();
    }

    public async Task RefreshAsync()
    {
        int protectedCount = 0;
        IReadOnlyList<ActivityEvent> events = Array.Empty<ActivityEvent>();

        try
        {
            var file = await _store.LoadAsync<ProtectedAppsFile>(ProtectedFile).ConfigureAwait(false);
            protectedCount = file?.Apps?.Count ?? 0;
        }
        catch { /* first run */ }

        try
        {
            var file = await _store.LoadAsync<ActivityLogFile>(ActivityFile).ConfigureAwait(false);
            events = file?.Events ?? new List<ActivityEvent>();
        }
        catch { /* first run */ }

        var verifiedToday = events.Count(e =>
            e.Outcome.Equals("Verified", StringComparison.OrdinalIgnoreCase) &&
            e.TimestampUtc.Date == DateTimeOffset.UtcNow.Date);
        var blocksToday = events.Count(e =>
            !e.Outcome.Equals("Verified", StringComparison.OrdinalIgnoreCase) &&
            e.TimestampUtc.Date == DateTimeOffset.UtcNow.Date);
        var rulesCount = 0;
        try
        {
            var rulesFile = await _store.LoadAsync<RulesFile>("rules.json").ConfigureAwait(false);
            if (rulesFile?.Rules is not null)
                rulesCount = rulesFile.Rules.Count(r => r.IsEnabled);
        }
        catch { /* first run, no rules yet */ }

        var now = DateTime.Now.ToString("HH:mm", CultureInfo.InvariantCulture);
        var date = DateTime.Now.ToString("ddd, d MMM", CultureInfo.InvariantCulture);

        Stats.Clear();
        Stats.Add(new DashboardStatRow("Protected apps",
            protectedCount.ToString(CultureInfo.InvariantCulture),
            protectedCount == 0 ? "Add an app to start securing it" : "Apps behind Windows Hello"));
        Stats.Add(new DashboardStatRow("Hello challenges",
            verifiedToday.ToString(CultureInfo.InvariantCulture),
            "Verified today"));
        Stats.Add(new DashboardStatRow("Active rules",
            rulesCount.ToString(CultureInfo.InvariantCulture),
            "Phase 2 — automation pipeline"));
        Stats.Add(new DashboardStatRow("Session start", now, date));

        Recent.Clear();
        foreach (var e in events.OrderByDescending(e => e.TimestampUtc).Take(5))
        {
            Recent.Add(new DashboardActivityRow(
                Title: e.AppName,
                Detail: $"{e.Severity} · {e.Description}",
                Timestamp: e.TimestampUtc.ToLocalTime()));
        }
    }
}

public sealed record DashboardStatRow(string Label, string Value, string Caption);
public sealed record DashboardActivityRow(string Title, string Detail, DateTimeOffset Timestamp);
