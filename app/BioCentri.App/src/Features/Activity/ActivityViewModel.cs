using System.Collections.ObjectModel;
using System.Globalization;
using BioCentri.App.Routing;
using BioCentri.App.Types;
using BioCentri.App.Types.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BioCentri.App.Features.Activity;

/// <summary>
/// Activity view-model. Reads <c>activity.json</c> on init, groups
/// events by day, surfaces top-line stat tiles, and exposes the
/// recent-event list bound by <c>ActivityPage.xaml</c>.
///
/// Counts are recomputed every time the underlying
/// <see cref="ObservableCollection{T}"/> changes — keeps stat tiles
/// and timeline in lock-step without a manual refresh.
/// </summary>
public sealed partial class ActivityViewModel : ObservableObject
{
    private const string StorageFile = "activity.json";

    private readonly ILocalJsonStore _store;

    public string Title => RouteTable.Get(Route.Activity).Title;
    public string Subtitle => RouteTable.Get(Route.Activity).Subtitle;

    public ObservableCollection<ActivityStatRow> Stats { get; } = new();
    public ObservableCollection<ActivityTimelineRow> Events { get; } = new();

    private bool _initialized;

    public ActivityViewModel(ILocalJsonStore store)
    {
        _store = store;
        _ = InitializeAsync();
    }

    [RelayCommand]
    public async Task RefreshAsync()
    {
        await LoadAsync();
    }

    [RelayCommand]
    public async Task ClearAsync()
    {
        try
        {
            await _store.DeleteAsync(StorageFile);
            Events.Clear();
            RecomputeStats();
        }
        catch
        {
            /* swallow — clearing is best-effort */
        }
    }

    /// <summary>Seed an event programmatically (used by other services
    /// via <see cref="AppendAsync"/>) and persist immediately.</summary>
    public async Task AppendAsync(ActivityEvent ev)
    {
        var snapshot = new List<ActivityEvent> { ev };
        if (Events.Count > 0)
        {
            // Replay the existing on-screen events so we don't double-append
            // when Append is called from a non-AO thread between renders.
            snapshot.AddRange(Events.Select(RowToEvent));
        }
        snapshot = snapshot.OrderByDescending(e => e.TimestampUtc).ToList();

        Events.Clear();
        foreach (var e in snapshot)
        {
            Events.Add(EventToRow(e));
        }
        RecomputeStats();

        await PersistAsync(snapshot);
    }

    private async Task InitializeAsync()
    {
        if (_initialized) return;
        _initialized = true;
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            var file = await _store.LoadAsync<ActivityLogFile>(StorageFile).ConfigureAwait(false);
            var events = file?.Events ?? new List<ActivityEvent>();
            events = events.OrderByDescending(e => e.TimestampUtc).ToList();
            Events.Clear();
            foreach (var e in events) Events.Add(EventToRow(e));
            RecomputeStats();
        }
        catch
        {
            /* file missing is normal on first run */
            Events.Clear();
            RecomputeStats();
        }
    }

    private void RecomputeStats()
    {
        var verified = 0;
        var blocked = 0;
        var cancelled = 0;

        foreach (var row in Events)
        {
            switch (row.Outcome?.ToLowerInvariant())
            {
                case "verified":
                    verified++; break;
                case "usercancelled":
                case "user_canceled":
                    cancelled++; break;
                default:
                    blocked++; break;
            }
        }

        var last7 = Events.Count(e =>
            (DateTimeOffset.UtcNow - e.Timestamp) <= TimeSpan.FromDays(7));
        var today = Events.Count(e => e.Timestamp.Date == DateTimeOffset.UtcNow.Date);

        Stats.Clear();
        Stats.Add(new ActivityStatRow("Today",     today.ToString(CultureInfo.InvariantCulture), "challenges"));
        Stats.Add(new ActivityStatRow("This week", last7.ToString(CultureInfo.InvariantCulture), "events"));
        Stats.Add(new ActivityStatRow("Verified",  verified.ToString(CultureInfo.InvariantCulture), "passed Hello"));
        Stats.Add(new ActivityStatRow("Blocked",   blocked.ToString(CultureInfo.InvariantCulture), "by BioCentri"));
    }

    private async Task PersistAsync(IReadOnlyList<ActivityEvent> snapshot)
    {
        try
        {
            await _store.SaveAsync(StorageFile,
                new ActivityLogFile { Events = snapshot.ToList() });
        }
        catch
        {
            /* persistence is best-effort */
        }
    }

    private static ActivityTimelineRow EventToRow(ActivityEvent e) => new(
        Group:     e.TimestampUtc.ToLocalTime().Date.ToString("ddd, d MMM", CultureInfo.InvariantCulture),
        Timestamp: e.TimestampUtc,
        Severity:  e.Severity,
        AppName:   e.AppName,
        Description: e.Description);

    private static ActivityEvent RowToEvent(ActivityTimelineRow r) => new(
        TimestampUtc: r.Timestamp,
        Severity:     r.Severity,
        AppName:      r.AppName,
        Outcome:      r.Outcome,
        Description:  r.Description);
}

public sealed record ActivityStatRow(string Label, string Value, string Caption);

public sealed record ActivityTimelineRow(
    string Group,
    DateTimeOffset Timestamp,
    string Severity,
    string AppName,
    string Description)
{
    public string Outcome { get; init; } = string.Empty;
}
