using BioCentri.App.Types.Services;

namespace BioCentri.App.Services;

/// <summary>
/// Stub whitelist that always answers from an in-memory
/// <see cref="HashSet{T}"/>. Default seeds contain Chrome and Discord so
/// QA can verify the auth path without touching the OS process watcher.
/// The real persistence-backed store lands in Milestone 6.
/// </summary>
public sealed class StubAuthAppRules : IAuthAppRules
{
    private readonly HashSet<string> _protected;

    public StubAuthAppRules(IEnumerable<string> protectedProcesses)
    {
        ArgumentNullException.ThrowIfNull(protectedProcesses);
        _protected = new HashSet<string>(
            protectedProcesses.Where(s => !string.IsNullOrWhiteSpace(s)).Select(Normalize),
            StringComparer.OrdinalIgnoreCase);
    }

    // Hoisted to a static field so each Defaults() call doesn't reallocate
    // the protected-process array (CA1861).
    private static readonly string[] DefaultProtectedProcesses =
        { "chrome.exe", "discord.exe", "code.exe", "spotify.exe" };

    /// <summary>Sensible default whitelist for local QA.</summary>
    public static StubAuthAppRules Defaults() =>
        new(DefaultProtectedProcesses);

    /// <inheritdoc />
    public bool IsProtected(string processName)
    {
        if (string.IsNullOrWhiteSpace(processName)) return false;
        var norm = Normalize(processName);

        if (_protected.Contains(norm)) return true;
        foreach (var p in _protected)
        {
            if (norm.EndsWith("\\" + p, StringComparison.OrdinalIgnoreCase)) return true;
            if (norm.EndsWith("/" + p, StringComparison.OrdinalIgnoreCase)) return true;
        }
        return false;
    }

    private static string Normalize(string s) => s.Trim().Replace('/', '\\').ToLowerInvariant();
}
