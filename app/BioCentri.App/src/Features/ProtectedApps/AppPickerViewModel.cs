using System.Collections.ObjectModel;
using BioCentri.App.Types;
using BioCentri.App.Types.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BioCentri.App.Features.ProtectedApps;

/// <summary>
/// Modal picker VM surfaced via <see cref="IDialogService.ShowAsync{T}"/>.
/// The shell's <c>DialogHost</c> selects this VM by
/// <c>DataType</c> in <c>App.xaml</c> and renders the matching view.
///
/// Why a separate VM: page-level <c>ProtectedAppsViewModel</c> stays
/// focused on the protected list + persistence; picker search/discovery
/// state belongs here. Lifetime is bounded by the dialog — the
/// <see cref="_tcs"/> resolves when the user confirms or cancels.
///
/// Discovery is awaited on the worker thread, observable updates are
/// marshalled onto the WPF UI thread via <see cref="IDispatcher.InvokeAsync(Action)"/>.
/// </summary>
public sealed partial class AppPickerViewModel : ObservableObject, IDialogHostViewModel<InstalledApp?>
{
    private readonly IInstalledAppsDiscovery _discovery;
    private readonly IDispatcher _dispatcher;
    private readonly TaskCompletionSource<InstalledApp?> _tcs = new();

    /// <summary>Paths the user has already protected — used to filter the
    /// picker list so re-adding is impossible without going through
    /// an explicit un-protect round-trip.</summary>
    private readonly HashSet<string> _excludedPaths;

    /// <summary>Backing list for the design-system list binding.</summary>
    public ObservableCollection<InstalledApp> Filtered { get; } = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    /// <inheritdoc />
    public Task<InstalledApp?> WaitForResultAsync(CancellationToken cancellationToken) =>
        _tcs.Task;

    public AppPickerViewModel(
        IInstalledAppsDiscovery discovery,
        IDispatcher dispatcher,
        IEnumerable<string> excludedPaths)
    {
        _discovery = discovery;
        _dispatcher = dispatcher;
        _excludedPaths = new HashSet<string>(excludedPaths, StringComparer.OrdinalIgnoreCase);

        _ = LoadAsync();
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var all = await _discovery.DiscoverAsync().ConfigureAwait(false);
            var usable = all.Where(a => !_excludedPaths.Contains(a.Path)).ToList();
            await _dispatcher.InvokeAsync(() =>
            {
                Filtered.Clear();
                foreach (var app in usable) Filtered.Add(app);
                ApplySearch();
            });
        }
        catch (Exception ex)
        {
            await _dispatcher.InvokeAsync(() => ErrorMessage = ex.Message);
        }
        finally
        {
            await _dispatcher.InvokeAsync(() => IsBusy = false);
        }
    }

    /// <summary>Re-filters the in-memory list when
    /// <see cref="SearchText"/> changes.</summary>
    partial void OnSearchTextChanged(string value) => ApplySearch();

    private void ApplySearch()
    {
        var query = (SearchText ?? string.Empty).Trim();
        if (query.Length == 0) return;

        var hits = Filtered.Where(a =>
            a.DisplayName.Contains(query, StringComparison.CurrentCultureIgnoreCase) ||
            (a.Publisher?.Contains(query, StringComparison.CurrentCultureIgnoreCase) ?? false))
            .ToList();

        Filtered.Clear();
        foreach (var hit in hits) Filtered.Add(hit);
    }

    /// <summary>User clicked the "Protect" button on a row — close
    /// dialog with the picked app.</summary>
    [RelayCommand]
    private void Confirm(InstalledApp? picked)
    {
        _tcs.TrySetResult(picked);
    }

    /// <summary>User clicked Cancel / hit Esc / closed the dimmer.</summary>
    [RelayCommand]
    private void Cancel()
    {
        _tcs.TrySetResult(null);
    }
}
