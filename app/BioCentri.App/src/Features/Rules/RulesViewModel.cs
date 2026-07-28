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
/// M7 polish: on first run with no persisted rules, seeds IN-MEMORY
/// sample rules so the polished automation table has content. Samples
/// are NOT written to disk; only the user's real additions persist.
/// </summary>
public sealed partial class RulesViewModel : ObservableObject
{
    private const string StorageFile = "rules.json";

    private readonly ILocalJsonStore _store;

    public string Title => RouteTable.Get(Route.Rules).Title;
    public string Subtitle => RouteTable.Get(Route.Rules).Subtitle;

    public ObservableCollection<Rule> Rules { get; } = new();

    /// <summary>Composer bindings (rule builder row).</summary>
    [ObservableProperty] private string _newRuleName = string.Empty;
    [ObservableProperty] private string _newRuleDescription = string.Empty;
    [ObservableProperty] private string _newRuleTrigger = "Anytime";

    /// <summary>Target app combo — populated from the entry below.</summary>
    [ObservableProperty] private string _newRuleTarget = string.Empty;

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
        NewRuleTarget = string.Empty;

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
            if (saved.Count == 0)
                saved = SampleRules(); // in-memory only; never persisted

            saved = saved.OrderByDescending(r => r.CreatedUtc).ToList();
            Rules.Clear();
            foreach (var r in saved) Rules.Add(r);
        }
        catch
        {
            Rules.Clear();
            foreach (var r in SampleRules()) Rules.Add(r);
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

    /// <summary>Demo rules used only when the on-disk store is empty.
    /// Per the polished UI's table target: Rule Name | Target App |
    /// Condition | Schedule | Status.</summary>
    private static List<Rule> SampleRules() => new()
    {
        new(Guid.NewGuid(), "Secure Browser",  "",
            "All Day",          true,
            new DateTimeOffset(2024, 5, 1, 9, 0, 0, TimeSpan.Zero)),
        new(Guid.NewGuid(), "WorkVPN Auto-Connect", "",
            "Mon–Fri: 9–5",     true,
            new DateTimeOffset(2024, 5, 1, 9, 0, 0, TimeSpan.Zero)),
        new(Guid.NewGuid(), "Late Night Lock",     "",
            "10 PM – 6 AM",     false,
            new DateTimeOffset(2024, 5, 1, 9, 0, 0, TimeSpan.Zero)),
        new(Guid.NewGuid(), "Late Night Lock",     "",
            "10 PM – 6 AM",     false,
            new DateTimeOffset(2024, 5, 1, 9, 0, 0, TimeSpan.Zero)),
        new(Guid.NewGuid(), "Secure Browser",     "Brave",
            "App Launch",       true,
            new DateTimeOffset(2024, 5, 1, 9, 0, 0, TimeSpan.Zero)),
        new(Guid.NewGuid(), "WorkVPN Auto-Connect", "OpenVPN",
            "Network Detection", true,
            new DateTimeOffset(2024, 5, 1, 9, 0, 0, TimeSpan.Zero)),
    };
}
