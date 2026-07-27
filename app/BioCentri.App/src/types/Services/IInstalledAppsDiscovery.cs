namespace BioCentri.App.Types.Services;

/// <summary>
/// Discovers installed desktop applications on the user's Windows
/// machine. Implementations enumerate registry Uninstall keys
/// (HKLM, HKLM Wow6432Node, HKCU) and return a stable, sorted list of
/// <see cref="BioCentri.App.Types.InstalledApp"/> records.
///
/// All methods are async + cancellable; the concrete implementation
/// runs the registry walk on a thread-pool task so the WPF UI
/// thread is never blocked. <c>ILocalJsonStore</c> handles persistence
/// of the protected list — this service is stateless.
/// </summary>
public interface IInstalledAppsDiscovery
{
    /// <summary>
    /// Enumerate installed desktop applications. Returns an empty
    /// list — never throws — if the registry walk was cancelled or
    /// timed out. The caller surfaces any failure via
    /// <c>IToastService.Show(ToastSeverity.Error, ...)</c>.
    /// </summary>
    Task<IReadOnlyList<BioCentri.App.Types.InstalledApp>> DiscoverAsync(
        CancellationToken cancellationToken = default);
}
