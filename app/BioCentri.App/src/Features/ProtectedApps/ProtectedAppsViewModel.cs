using System.Collections.ObjectModel;
using BioCentri.App.Routing;
using BioCentri.App.Types;
using BioCentri.App.Types.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BioCentri.App.Features.ProtectedApps;

/// <summary>
/// Page-level VM for the Protected Apps surface. Owns:
///   * The protected-apps list (loaded + persisted via
///     <c>ILocalJsonStore</c>).
///   * The visible "Add application" command — delegates to the
///     <c>IDialogService</c>-hosted <c>AppPicker</c> for discovery.
///   * Per-row "Unprotect" commands (toggle-off semantics in M4).
///
/// Persistence file: <c>%LOCALAPPDATA%\BioCentri\ProtectedApps.json</c>
/// (matches the LocalJsonStore docstring convention).
/// Storage root POCO: <see cref="ProtectedAppsFile"/>.
/// </summary>
public sealed partial class ProtectedAppsViewModel : ObservableObject
{
    private readonly IToastService _toast;
    private readonly ILocalJsonStore _store;
    private readonly IDialogService _dialog;
    private readonly IDispatcher _dispatcher;
    private readonly IInstalledAppsDiscovery _discovery;

    private const string StorageFile = "protectedApps.json";

    private bool _initialized;

    public string Title => RouteTable.Get(Route.ProtectedApps).Title;
    public string Subtitle => RouteTable.Get(Route.ProtectedApps).Subtitle;

    /// <summary>The visible protected list — Single source of truth for
    /// the page ItemsControl. Mutations always happen on the UI thread
    /// (after <c>IDispatcher.InvokeAsync</c>). </summary>
    public ObservableCollection<ProtectedApp> Protected { get; } = new();

    /// <summary>Client-side filter — the TextBox on ProtectedAppsPage
    /// binds to this. Changing it re-filters <see cref="Filtered"/>.</summary>
    [ObservableProperty]
    private string _searchText = string.Empty;

    /// <summary>Filtered view of <see cref="Protected"/>. The ListBox
    /// binds here so typing in the search box trims the visible list
    /// in real time without mutating the source-of-truth.</summary>
    public ObservableCollection<ProtectedApp> Filtered { get; } = new();

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        var q = (SearchText ?? string.Empty).Trim();
        Filtered.Clear();
        foreach (var a in Protected)
        {
            if (q.Length == 0 ||
                (a.DisplayName ?? string.Empty).Contains(q, StringComparison.OrdinalIgnoreCase))
                Filtered.Add(a);
        }
    }

    public ProtectedAppsViewModel(
        IToastService toast,
        ILocalJsonStore store,
        IDialogService dialog,
        IDispatcher dispatcher,
        IInstalledAppsDiscovery discovery)
    {
        _toast = toast;
        _store = store;
        _dialog = dialog;
        _dispatcher = dispatcher;
        _discovery = discovery;

        // Fire-and-forget self-initialise. Failures route through the
        // existing UnobservedTaskException handler in App.xaml.cs
        // (which surfaces them via Debug output, not a UI dialog).
        _ = InitializeAsync();
    }

    /// <summary>Idempotent — safe to call repeatedly (e.g. on every
    /// navigation). Reads the JSON file and reseeds
    /// <see cref="Protected"/> on the UI thread.</summary>
    public async Task InitializeAsync()
    {
        if (_initialized) return;
        _initialized = true;

        try
        {
            var snapshot = await _store.LoadAsync<ProtectedAppsFile>(StorageFile)
                .ConfigureAwait(false);
            var apps = snapshot?.Apps ?? new List<ProtectedApp>();

            await _dispatcher.InvokeAsync(() =>
            {
                Protected.Clear();
                foreach (var a in apps) Protected.Add(a);
                ApplyFilter();
            });
        }
        catch (Exception ex)
        {
            await _dispatcher.InvokeAsync(() =>
                _toast.Show(ToastSeverity.Danger, "Couldn't load protected apps",
                    ex.Message));
        }
    }

    /// <summary>Open the picker overlay. The user picks an
    /// <see cref="InstalledApp"/> (or null = cancel); on a real pick
    /// we add to the protected list + persist.</summary>
    [RelayCommand]
    private async Task AddAsync()
    {
        var excludedPaths = Protected.Select(p => p.Path);
        var picker = new AppPickerViewModel(_discovery, _dispatcher, excludedPaths);

        InstalledApp? pick;
        try
        {
            pick = await _dialog.ShowAsync<InstalledApp?>(picker, picker);
        }
        catch (Exception ex)
        {
            _toast.Show(ToastSeverity.Danger, "Couldn't open picker", ex.Message);
            return;
        }

        if (pick is null) return; // user cancelled

        // Guard against an already-protected path (the picker filters
        // but a concurrent ProtectAsync could have raced past).
        if (Protected.Any(p => string.Equals(p.Path, pick.Path, StringComparison.OrdinalIgnoreCase)))
        {
            _toast.Show(ToastSeverity.Info, "Already protected",
                $"{pick.DisplayName} is already in your protected list.");
            return;
        }

        var entry = new ProtectedApp(
            DisplayName: pick.DisplayName,
            Path: pick.Path,
            IconKey: pick.IconKey,
            AddedUtc: DateTimeOffset.Now);

        await _dispatcher.InvokeAsync(() => { Protected.Add(entry); ApplyFilter(); });
        await PersistAsync();
        _toast.Show(ToastSeverity.Success, "Protected",
            $"{entry.DisplayName} now requires authentication.");
    }

    /// <summary>User clicked Remove / Unprotect on a row. In M4 the
    /// toggle-off path goes through the same command — FR-2's
    /// happy-path is symmetric for MVP.</summary>
    [RelayCommand]
    private async Task UnprotectAsync(ProtectedApp? app)
    {
        if (app is null) return;

        var removed = false;
        await _dispatcher.InvokeAsync(() =>
        {
            for (var i = 0; i < Protected.Count; i++)
            {
                if (string.Equals(Protected[i].Path, app.Path, StringComparison.OrdinalIgnoreCase))
                {
                    Protected.RemoveAt(i);
                    removed = true;
                    break;
                }
            }
            if (removed) ApplyFilter();
        });

        if (!removed) return;

        await PersistAsync();
        _toast.Show(ToastSeverity.Info, "Unprotected",
            $"{app.DisplayName} no longer requires authentication.");
    }

    private async Task PersistAsync()
    {
        // Take a thread-safe snapshot on the UI thread before the
        // ObservableCollection is mutated again. The Func<Task<T>>
        // overload of IDispatcher.InvokeAsync enforces definite
        // assignment by returning the computed value via the awaited
        // Task<T>, so the captured variable is provably assigned below.
        var snapshot = await _dispatcher.InvokeAsync(
            () => Task.FromResult(Protected.ToList()));

        try
        {
            await _store.SaveAsync(StorageFile, new ProtectedAppsFile { Apps = snapshot });
        }
        catch (Exception ex)
        {
            _toast.Show(ToastSeverity.Danger, "Couldn't save protected apps", ex.Message);
        }
    }
}

/// <summary>
/// JSON root for <c>protectedApps.json</c>. The wrapper exists so we
/// can add aggregate metadata (settings, schema version) later
/// without re-keying the entire file.
/// </summary>
internal sealed class ProtectedAppsFile
{
    [System.Text.Json.Serialization.JsonPropertyName("apps")]
    public List<ProtectedApp> Apps { get; set; } = new();
}
