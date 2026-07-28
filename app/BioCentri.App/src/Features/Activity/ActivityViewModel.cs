using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text;
using BioCentri.App.Routing;
using BioCentri.App.Types;
using BioCentri.App.Types.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;

namespace BioCentri.App.Features.Activity;

/// <summary>
/// Activity view-model. Reads <c>activity.json</c> on init, groups
/// events by day, surfaces top-line stat tiles, and exposes the
/// recent-event list bound by <c>ActivityPage.xaml</c>.
///
/// M7 polish:
///   * Filters (TimeRange / EventType / Target / Outcome) bound by
///     the polished filter row; toggle visibility via
///     <see cref="ApplyFilter"/>.
///   * ExportCsv command writes a UTF-8 CSV via a SaveFileDialog.
///   * On first run with no persisted activity, seeds IN-MEMORY sample
///     events across the last 7 days so the audit timeline reads as
///     a real product. Samples NEVER persist.
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

    // ----- Filter row (M7+) ---------------------------------------------
    [ObservableProperty] private string _timeRange = "All Time";
    [ObservableProperty] private string _eventType = "Event Type";
    [ObservableProperty] private string _target = "Target";
    [ObservableProperty] private string _outcome = "Outcome";

    public IReadOnlyList<string> TimeRangeOptions { get; } =
        new[] { "All Time", "Last 24 hours", "Today", "Last 7 days" };
    public IReadOnlyList<string> EventTypeOptions { get; } =
        new[] { "Event Type", "App Launch", "Biometric Challenge",
                "Settings Change", "System Startup", "App Launch Block" };
    public IReadOnlyList<string> TargetOptions { get; } =
        new[] { "Target", "Brave Browser", "Spotify", "BioCentri.App",
                "User Prefs", "System Info" };
    public IReadOnlyList<string> OutcomeOptions { get; } =
        new[] { "Outcome", "Success", "Block", "Info" };

    public ActivityViewModel(ILocalJsonStore store)
    {
        _store = store;
        Events.CollectionChanged += (_, _) => RecomputeStats();
        _ = InitializeAsync();
    }

    partial void OnTimeRangeChanged(string value) => ApplyFilter();
    partial void OnEventTypeChanged(string value) => ApplyFilter();
    partial void OnTargetChanged(string value) => ApplyFilter();
    partial void OnOutcomeChanged(string value) => ApplyFilter();

    [RelayCommand]
    public async Task RefreshAsync() => await LoadAsync();

    [RelayCommand]
    public async Task ClearAsync()
    {
        try
        {
            await _store.DeleteAsync(StorageFile);
        }
        catch
        {
            /* best-effort */
        }
        Events.Clear();
        RecomputeStats();
    }

    [RelayCommand]
    public void ExportCsv()
    {
        var dlg = new SaveFileDialog
        {
            FileName = $"biocentri-audit-{DateTime.UtcNow:yyyy-MM-dd}.csv",
            DefaultExt = ".csv",
            Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
            Title = "Export BioCentri activity log",
        };

        var owner = System.Windows.Application.Current.MainWindow;
        var accepted = owner is null ? dlg.ShowDialog() == true : dlg.ShowDialog(owner) == true;
        if (!accepted) return;

        var sb = new StringBuilder();
        sb.AppendLine("Time,EventType,Target,Details,Outcome");
        foreach (var r in Events)
        {
            sb.Append(EscapeCsv(r.Timestamp.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture))); sb.Append(',');
            sb.Append(EscapeCsv(r.Severity ?? string.Empty)); sb.Append(',');
            sb.Append(EscapeCsv(r.AppName ?? string.Empty)); sb.Append(',');
            sb.Append(EscapeCsv(r.Description ?? string.Empty)); sb.Append(',');
            sb.AppendLine(EscapeCsv(r.Outcome ?? string.Empty));
        }
        File.WriteAllText(dlg.FileName, sb.ToString(), Encoding.UTF8);
    }

    public async Task AppendAsync(ActivityEvent ev)
    {
        var snapshot = new List<ActivityEvent> { ev };
        if (Events.Count > 0)
            snapshot.AddRange(Events.Select(RowToEvent));
        snapshot = snapshot.OrderByDescending(e => e.TimestampUtc).ToList();

        Events.Clear();
        foreach (var e in snapshot)
            Events.Add(EventToRow(e));
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
            if (events.Count == 0)
                events = SampleEvents(); // in-memory only

            ApplyFilterOnto(events.OrderByDescending(e => e.TimestampUtc).ToList());
        }
        catch
        {
            ApplyFilterOnto(SampleEvents());
        }
    }

    private void ApplyFilterOnto(IReadOnlyList<ActivityEvent> events)
    {
        // We snapshot to a single ordered list then filter live.
        _allEvents.Clear();
        _allEvents.AddRange(events);
        ApplyFilter();
    }

    private readonly List<ActivityEvent> _allEvents = new();

    private void ApplyFilter()
    {
        if (_allEvents.Count == 0)
        {
            Events.Clear();
            RecomputeStats();
            return;
        }

        var rangeFilter = TimeRange ?? "All Time";
        var typeFilter = EventType ?? "Event Type";
        var targetFilter = Target ?? "Target";
        var outcomeFilter = Outcome ?? "Outcome";

        Events.Clear();
        foreach (var e in _allEvents)
        {
            if (rangeFilter != "All Time" && !InRange(e.TimestampUtc, rangeFilter)) continue;
            if (typeFilter != "Event Type" &&
                !string.Equals(e.Severity, typeFilter, StringComparison.OrdinalIgnoreCase)) continue;
            if (targetFilter != "Target" &&
                !string.Equals(e.AppName, targetFilter, StringComparison.OrdinalIgnoreCase)) continue;
            if (outcomeFilter != "Outcome" &&
                !string.Equals(ClassifyOutcome(e.Outcome), outcomeFilter, StringComparison.OrdinalIgnoreCase)) continue;

            Events.Add(EventToRow(e));
        }
        RecomputeStats();
    }

    private static bool InRange(DateTimeOffset ts, string range) => range switch
    {
        "Last 24 hours" => (DateTimeOffset.UtcNow - ts) <= TimeSpan.FromHours(24),
        "Today"         => ts.Date == DateTimeOffset.UtcNow.Date,
        "Last 7 days"   => (DateTimeOffset.UtcNow - ts) <= TimeSpan.FromDays(7),
        _ => true,
    };

    private static string ClassifyOutcome(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "Info";
        if (raw.Equals("Verified", StringComparison.OrdinalIgnoreCase)) return "Success";
        if (raw.Contains("Cancel", StringComparison.OrdinalIgnoreCase)) return "Block";
        if (raw.Equals("Block", StringComparison.OrdinalIgnoreCase) ||
            raw.Equals("Blocked", StringComparison.OrdinalIgnoreCase)) return "Block";
        return "Info";
    }

    private void RecomputeStats()
    {
        var verified = 0;
        var blocked = 0;

        foreach (var row in Events)
        {
            switch (row.Outcome?.ToLowerInvariant())
            {
                case "verified": verified++; break;
                default: blocked++; break;
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
        catch { /* best-effort */ }
    }

    private static readonly char[] CsvEscapeChars = { ',', '"', '\n', '\r' };

    private static string EscapeCsv(string s)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;
        if (s.IndexOfAny(CsvEscapeChars) < 0) return s;
        return "\"" + s.Replace("\"", "\"\"") + "\"";
    }

    private static ActivityTimelineRow EventToRow(ActivityEvent e) => new(
        Group:       e.TimestampUtc.ToLocalTime().Date.ToString("ddd, d MMM", CultureInfo.InvariantCulture),
        Timestamp:   e.TimestampUtc,
        Severity:    e.Severity,
        AppName:     e.AppName,
        Description: e.Description)
    {
        Outcome = e.Outcome,
    };

    private static ActivityEvent RowToEvent(ActivityTimelineRow r) => new(
        TimestampUtc: r.Timestamp,
        Severity:     r.Severity,
        AppName:      r.AppName,
        Outcome:      r.Outcome,
        Description:  r.Description);

    /// <summary>Demo timeline used only when the on-disk store is empty.
    /// Seeds a realistic 7-day spread of Mixed Outcomes / Targets / Event
    /// Types so the polished audit table reads like a real product.
    /// In-memory only — never written to disk.</summary>
    private static List<ActivityEvent> SampleEvents()
    {
        var today = DateTimeOffset.UtcNow.Date;
        return new()
        {
            new(today.AddHours(12).AddMinutes(23), "Biometric Challenge", "Brave Browser",   "Verified", "Windows Hello passed for user Admin"),
            new(today.AddHours(12).AddMinutes(22), "App Launch Block",    "Spotify",         "Block",    "Process blocked due to rule"),
            new(today.AddHours(12).AddMinutes(20), "System Startup",      "BioCentri.App",   "Info",     "Application started successfully"),
            new(today.AddHours(12).AddMinutes(15), "Settings Change",     "User Prefs",      "Verified", "Dark mode settings updated"),
            new(today.AddHours(12).AddMinutes(12), "Settings Change",     "User Prefs",      "Verified", "Dark mode settings updated"),
            new(today.AddHours(11).AddMinutes(45), "Settings Change",     "User Prefs",      "Verified", "Dark mode settings updated"),
            new(today.AddHours(11).AddMinutes(35), "System Startup",      "BioCentri.App",   "Info",     "Application started successfully"),
            new(today.AddHours(11).AddMinutes(10), "App Launch",          "User Prefs",      "Verified", "Application started successfully"),
            new(today.AddHours(10).AddMinutes(2),  "Biometric Challenge", "BioCentri.App",   "Verified", "Application started successfully"),
            new(today.AddHours(9).AddMinutes(50),  "App Launch Block",    "User Prefs",      "Verified", "Application started successfully"),
            new(today.AddHours(9).AddMinutes(33),  "Biometric Challenge", "BioCentri.Auth",  "Verified", "Application started successfully"),
            new(today.AddHours(9).AddMinutes(2),   "App Launch Block",    "BioCentri.App",   "Verified", "Application stopped successfully"),
            new(today.AddHours(8).AddMinutes(50),  "App Launch",          "System Info",     "Verified", "Application stopped successfully"),
            new(today.AddHours(8).AddMinutes(33),  "Settings Change",     "BioCentri.Core",  "Verified", "Application stopped successfully"),
        };
    }
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
