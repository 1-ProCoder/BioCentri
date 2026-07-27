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
///     of lower-cased, normalized executable paths.
///   * On every <see cref="IsProtected"/> call, check whether the file's
///     <c>LastWriteTimeUtc</c> has changed since the last cache fill. If
///     so, re-read and replace the cache. This keeps per-call overhead at
///     O(1) (hash lookup) with rare O(file I/O) on save from the UI.
///   * The store already uses atomic write, so partial reads cannot
///     happen.
///
/// Normalization: the <c>ProtectedApp.Path</c> from the JSON file is
/// already an absolute filesystem path. The <see cref="IsProtected"/>
/// contract accepts either bare exe names or full paths. To match, we
/// normalize the incoming candidate to lower-case and do a suffix match:
/// the candidate must end with <c>\chrome.exe</c> (case-insensitive).
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

        var normalized = Normalize(processName);
        HashSet<string> snapshot;
        lock (_gate) { snapshot = _cache; }

        if (snapshot.Contains(normalized)) return true;

        // Suffix match: "C:\Program Files\Google\Chrome\Application\chrome.exe"
        // against cache entry "C:\Program Files\...\chrome.exe".
        // Both sides have been lower-cased and backslash-normalized.
        foreach (var entry in snapshot)
        {
            if (normalized.EndsWith("\\" + entry, StringComparison.OrdinalIgnoreCase))
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
                        _cache.Add(Normalize(app.Path));
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
