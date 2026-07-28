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
///   * Per-row "Unprotect" commands (delete semantics in M7+).
///   * Per-row "Gate State" toggle (M7+ — persists to
///     <c>protectedApps.json</c> via the <see cref="IsEnabled"/>
///     column on <see cref="ProtectedApp"/>).
///
/// Persistence file: <c>%LOCALAPPDATA%\BioCentri\protectedApps.json</c>
/// Storage root POCO: <see cref="ProtectedAppsFile"/>.
///
/// M7 polish: on first run with no persisted apps, the VM seeds
/// IN-MEMORY sample entries so the polished table has content to
/// render. The samples are NOT written to disk — the user can add
/// their real apps via "Protect New App" and those will persist.
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
    /// (after <c>IDispatcher.InvokeAsync</c>).</summary>
    public ObservableCollection<ProtectedApp> Protected { get; } = new();

    /// <summary>Client-side filter — the TextBox on ProtectedAppsPage
    /// binds to this. Changing it re-filters <see cref="Filtered"/>.</summary>
    [ObservableProperty]
    private string _searchText = string.Empty;

    /// <summary>Client-side "Gate State" filter (matches the
    /// "Filter by Gate State" ComboBox shown in the polished UI).</summary>
    [ObservableProperty]
    private string _gateStateFilter = "All";

    /// <summary>Filtered view of <see cref="Protected"/>. The ListBox
    /// binds here so typing in the search box trims the visible list
    /// in real time without mutating the source-of-truth.</summary>
    public ObservableCollection<ProtectedApp> Filtered { get; } = new();

    /// <summary>Static navigation over Gate State filter choices.</summary>
    public IReadOnlyList<string> GateStateOptions { get; } =
        new[] { "All", "Gated", "Open" };

    partial void OnSearchTextChanged(string value) => ApplyFilter();
    partial void OnGateStateFilterChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        var q = (SearchText ?? string.Empty).Trim();
        var gate = GateStateFilter ?? "All";
        Filtered.Clear();
        foreach (var a in Protected)
        {
            if (q.Length > 0 &&
                !(a.DisplayName ?? string.Empty).Contains(q, StringComparison.OrdinalIgnoreCase))
                continue;
            if (gate == "Gated" && !a.IsEnabled) continue;
            if (gate == "Open" && a.IsEnabled) continue;
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
        _ = InitializeAsync();
    }

    /// <summary>Idempotent — safe to call repeatedly (e.g. on every
    /// navigation). On first run with an empty store, seeds IN-MEMORY
    /// sample apps so the UI shows the polished table; only the user's
    /// real additions ever hit disk.</summary>
    public async Task InitializeAsync()
    {
        if (_initialized) return;
        _initialized = true;
        try
        {
            var snapshot = await _store.LoadAsync<ProtectedAppsFile>(StorageFile)
                .ConfigureAwait(false);
            var apps = snapshot?.Apps ?? new List<ProtectedApp>();
            if (apps.Count == 0)
                apps = SampleApps(); // in-memory only; never persisted

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
            AddedUtc: DateTimeOffset.Now)
        { IsEnabled = true };

        await _dispatcher.InvokeAsync(() => { Protected.Add(entry); ApplyFilter(); });
        await PersistAsync();
        _toast.Show(ToastSeverity.Success, "Protected",
            $"{entry.DisplayName} now requires authentication.");
    }

    /// <summary>User clicked Remove / Unprotect on a row.</summary>
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

    /// <summary>Removed in M7+: the per-row Gate State TwoWay binding
    /// on <see cref="ProtectedApp.IsEnabled"/> handles persistence via
    /// <see cref="PersistAsync"/> (raised by <see cref="ObservableCollection{T}"/>
    /// replace inside the binding's setter path). Keeping a parallel
    /// ToggleGateCommand would create a dual-write race.</summary>

    private async Task PersistAsync()
    {
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

    /// <summary>Demo entries used only when the on-disk store is empty.
    /// These never persist — they're transient UI scaffolding so the
    /// polished table renders content on a brand-new install.</summary>
    private static List<ProtectedApp> SampleApps() => new()
    {
        new("Brave Browser",
            @"C:\Program Files\BraveSoftware\Brave-Browser\Application\brave.exe",
            "Icons.Route.ProtectedApps",
            new DateTimeOffset(2023, 10, 28, 12, 0, 0, TimeSpan.Zero))
        { IsEnabled = true },
        new("Microsoft Teams",
            @"C:\Users\User\AppData\Local\Microsoft\Teams\current\Teams.exe",
            "Icons.Route.ProtectedApps",
            new DateTimeOffset(2023, 11, 15, 12, 0, 0, TimeSpan.Zero))
        { IsEnabled = true },
        new("Slack",
            @"C:\Users\User\AppData\Local\slack\app-4.36.0\slack.exe",
            "Icons.Route.ProtectedApps",
            new DateTimeOffset(2024, 1, 12, 12, 0, 0, TimeSpan.Zero))
        { IsEnabled = true },
        new("Visual Studio Code",
            @"C:\Users\User\AppData\Local\Programs\Microsoft VS Code\Code.exe",
            "Icons.Route.ProtectedApps",
            new DateTimeOffset(2024, 2, 28, 12, 0, 0, TimeSpan.Zero))
        { IsEnabled = false },
        new("Adobe Photoshop 2024",
            @"C:\Program Files\Adobe\Adobe Photoshop 2024\Photoshop.exe",
            "Icons.Route.ProtectedApps",
            new DateTimeOffset(2024, 3, 10, 12, 0, 0, TimeSpan.Zero))
        { IsEnabled = true },
    };
}

/// <summary>JSON root for <c>protectedApps.json</c>.</summary>
internal sealed class ProtectedAppsFile
{
    [System.Text.Json.Serialization.JsonPropertyName("apps")]
    public List<ProtectedApp> Apps { get; set; } = new();
}
