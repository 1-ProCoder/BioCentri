using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using BioCentri.App.Routing;
using BioCentri.App.Types;
using BioCentri.App.Types.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BioCentri.App.Features.Rules;

/// <summary>
/// Rules view-model. Implements Phase-1 MVP scope for FR-6
/// (CRUD + persist, locally). Deterministic time-window enforcement
/// is Phase 2 (FEATURE_ROADMAP.md) — the rule list today is a stored
/// intent the user can manage, not an enforcement pipeline.
///
/// Rule row schema:
///   - Id (Guid)      — stable identifier.
///   - Name           — short label (e.g. "Lock Discord after 22:00").
///   - Description    — longer copy shown beneath the label.
///   - TriggerText    — human-readable schedule line ("After 22:00", etc).
///   - IsEnabled      — toggle persisted individually.
///   - CreatedUtc     — capture time, used for sort.
/// </summary>
public sealed partial class RulesViewModel : ObservableObject
{
    private const string StorageFile = "rules.json";

    private readonly ILocalJsonStore _store;

    public string Title => RouteTable.Get(Route.Rules).Title;
    public string Subtitle => RouteTable.Get(Route.Rules).Subtitle;

    public ObservableCollection<Rule> Rules { get; } = new();

    [ObservableProperty]
    private string _newRuleName = string.Empty;

    [ObservableProperty]
    private string _newRuleDescription = string.Empty;

    [ObservableProperty]
    private string _newRuleTrigger = "Anytime";

    private bool _initialized;

    public RulesViewModel(ILocalJsonStore store)
    {
        _store = store;
        Rules.CollectionChanged += OnRulesChanged;
        _ = InitializeAsync();
    }

    [RelayCommand]
    private async Task AddAsync()
    {
        var name = (NewRuleName ?? string.Empty).Trim();
        if (name.Length == 0) return;

        var rule = new Rule(
            Id: Guid.NewGuid(),
            Name: name,
            Description: (NewRuleDescription ?? string.Empty).Trim(),
            TriggerText: (NewRuleTrigger ?? string.Empty).Trim(),
            IsEnabled: true,
            CreatedUtc: DateTimeOffset.Now);

        Rules.Insert(0, rule);

        NewRuleName = string.Empty;
        NewRuleDescription = string.Empty;
        NewRuleTrigger = "Anytime";

        await PersistAsync();
    }

    [RelayCommand]
    private async Task ToggleAsync(Rule? rule)
    {
        if (rule is null) return;
        var idx = Rules.IndexOf(rule);
        if (idx < 0) return;
        Rules[idx] = rule with { IsEnabled = !rule.IsEnabled };
        await PersistAsync();
    }

    [RelayCommand]
    private async Task DeleteAsync(Rule? rule)
    {
        if (rule is null) return;
        Rules.Remove(rule);
        await PersistAsync();
    }

    private async void OnRulesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (!_initialized) return;
        await PersistAsync();
    }

    private async Task InitializeAsync()
    {
        if (_initialized) return;
        _initialized = true;
        try
        {
            var file = await _store.LoadAsync<RulesFile>(StorageFile).ConfigureAwait(false);
            var saved = file?.Rules ?? new List<Rule>();
            saved = saved.OrderByDescending(r => r.CreatedUtc).ToList();
            Rules.Clear();
            foreach (var r in saved) Rules.Add(r);
        }
        catch
        {
            /* first run */
        }
    }

    private async Task PersistAsync()
    {
        try
        {
            var snapshot = Rules.ToList();
            await _store.SaveAsync(StorageFile, new RulesFile { Rules = snapshot });
        }
        catch
        {
            /* best-effort */
        }
    }
}
