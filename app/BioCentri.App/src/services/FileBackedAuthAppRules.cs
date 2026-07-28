using BioCentri.App.Types;
using BioCentri.App.Types.Services;

namespace BioCentri.App.Services;

/// <summary>
/// Real <see cref="IAuthAppRules"/> backed by
/// <c>%LOCALAPPDATA%\BioCentri\protectedApps.json</c> — the same file
/// that <see cref="BioCentri.App.Features.ProtectedApps.ProtectedAppsViewModel"/>
/// writes via <see cref="ILocalJsonStore"/>.
///
/// Strategy (per M6 thinker validation):
///   * Load the file once at construction and cache a <c>HashSet&lt;string&gt;</c>
///     of lower-cased exe-leaf names (e.g. "brave.exe").
///   * On every <see cref="IsProtected"/> call, check whether the file's
///     <c>LastWriteTimeUtc</c> has changed since the last cache fill. If
///     so, re-read and replace the cache. This keeps per-call overhead at
///     O(1) (hash lookup) with rare O(file I/O) on save from the UI.
///   * The store already uses atomic write, so partial reads cannot
///     happen.
///
/// Normalization: the cache key is the bare executable name extracted
/// from <c>ProtectedApp.Path</c> via <see cref="System.IO.Path.GetFileName(string)"/>.
/// This matches the WMI <c>Win32_ProcessStartTrace.ProcessName</c> and
/// <c>Process.GetProcesses()</c> output, both of which return bare exe
/// names like "brave.exe" — full paths are unreliable for elevated
/// processes running in different sessions. Two installs of the same
/// exe (e.g. brave.exe in two locations) intentionally collapse to
/// the same rule; the user wants the app protected regardless of
/// install path.
/// </summary>
public sealed class FileBackedAuthAppRules : IAuthAppRules
{
    private readonly string _filePath;

    private HashSet<string> _cache = new(StringComparer.OrdinalIgnoreCase);
    private DateTime _lastWriteUtc = DateTime.MinValue;
    private static readonly System.Text.Json.JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly object _gate = new();

    public FileBackedAuthAppRules(ILocalJsonStore store)
    {
        ArgumentNullException.ThrowIfNull(store);
        // Use ILocalJsonStore.StorageRoot for the canonical path so the
        // watcher reads the same file the UI writes. Actual I/O is sync
        // (File.ReadAllText / File.GetLastWriteTime) because IsProtected
        // must be O(1) and runs on a background thread; async would
        // require GetAwaiter().GetResult() which is noisy and unnecessary
        // here (the store's atomic rename guarantees the target file is
        // always complete).
        _filePath = System.IO.Path.Combine(store.StorageRoot, "protectedApps.json");
        Reload();
    }

    /// <inheritdoc />
    public bool IsProtected(string processName)
    {
        if (string.IsNullOrWhiteSpace(processName)) return false;

        // Lightweight TTL: the JSON file can change at any time (the
        // user toggles protection on the UI). Check last-write before
        // every lookup so the watcher never runs behind.
        lock (_gate)
        {
            var writeTime = GetLastWriteUtc();
            if (writeTime > _lastWriteUtc)
                ReloadUnderLock();
        }

        // Match by exe leaf name. The cache stores leaves (see
        // ReloadUnderLock), so a single HashSet.Contains check is
        // both correct and O(1).
        var leaf = System.IO.Path.GetFileName(Normalize(processName));
        if (string.IsNullOrEmpty(leaf)) return false;

        HashSet<string> snapshot;
        lock (_gate) { snapshot = _cache; }

        if (snapshot.Contains(leaf))
        {
            System.Diagnostics.Debug.WriteLine($"[AuthAppRules] Match: {leaf}");
            return true;
        }

        return false;
    }

    private void Reload()
    {
        lock (_gate)
        {
            ReloadUnderLock();
        }
    }

    private void ReloadUnderLock()
    {
        try
        {
            if (!System.IO.File.Exists(_filePath))
            {
                _cache = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                _lastWriteUtc = DateTime.MinValue;
                return;
            }

            var json = System.IO.File.ReadAllText(_filePath);
            var root = System.Text.Json.JsonSerializer.Deserialize<ProtectedAppsFile>(
                json, JsonOptions);

            _cache = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (root?.Apps is { } apps)
            {
                foreach (var app in apps)
                {
                    if (!string.IsNullOrWhiteSpace(app.Path))
                    {
                        // Store just the exe leaf — the watcher input
                        // from WMI / Process.GetProcesses() is also a
                        // bare exe name, so this is the only key shape
                        // that actually matches. See IsProtected() above.
                        var leaf = System.IO.Path.GetFileName(Normalize(app.Path));
                        if (!string.IsNullOrEmpty(leaf))
                            _cache.Add(leaf);
                    }
                }
            }

            _lastWriteUtc = GetLastWriteUtc();
        }
        catch
        {
            // File may be locked mid-save — keep the existing cache and
            // try again on the next IsProtected call.
        }
    }

    private DateTime GetLastWriteUtc()
    {
        try { return System.IO.File.GetLastWriteTimeUtc(_filePath); }
        catch { return DateTime.MinValue; }
    }

    private static string Normalize(string s)
        => s.Trim().Replace('/', '\\').ToLowerInvariant();

    /// <summary>Schema must match <c>BioCentri.App.Features.ProtectedApps.ProtectedAppsFile</c>.</summary>
    private sealed class ProtectedAppsFile
    {
        [System.Text.Json.Serialization.JsonPropertyName("apps")]
        public List<ProtectedApp> Apps { get; set; } = new();
    }
}
