using System.IO;
using BioCentri.App.Types;
using BioCentri.App.Types.Services;
using Microsoft.Win32;

namespace BioCentri.App.Services;

/// <summary>
/// Registry-based installed-app discovery. Walks the three standard
/// Uninstall hives, returns one <see cref="InstalledApp"/> per entry
/// that has both a <c>DisplayName</c> and a resolvable executable
/// path (<c>DisplayIcon</c>, fallback <c>InstallLocation</c> +
/// candidate <c>DisplayName.exe</c>, fallback <c>UninstallString</c>).
///
/// Thread-pool execution via <c>Task.Run</c> + an internal 1500 ms
/// deadline so the UI thread stays responsive even if the registry
/// is slow (large corporate images, antivirus hooks, etc.).
///
/// Repository-only conclusion: PackageManager (UWP) and
/// Start-menu shortcut scanning are deferred to a later milestone.
/// Today the picker is populated from the classic Uninstall hive,
/// which covers all Win32 desktop apps the user could plausibly want
/// to protect.
/// </summary>
public sealed class InstalledAppsDiscovery : IInstalledAppsDiscovery
{
    /// <summary>Hard upper bound on the registry walk. Keeps the UI
    /// thread responsive even on slow / degraded machines.</summary>
    private static readonly TimeSpan DiscoveryDeadline = TimeSpan.FromMilliseconds(1500);

    /// <summary>Subkeys under which every installed Win32 app
    /// publishes an Uninstall registration.</summary>
    private static readonly (RegistryHive Hive, RegistryView View, string SubKey)[] Roots =
    {
        (RegistryHive.LocalMachine, RegistryView.Registry64,
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"),
        (RegistryHive.LocalMachine, RegistryView.Registry32,
            @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"),
        (RegistryHive.CurrentUser, RegistryView.Default,
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"),
    };

    /// <inheritdoc />
    public Task<IReadOnlyList<InstalledApp>> DiscoverAsync(
        CancellationToken cancellationToken = default)
    {
        return Task.Run<IReadOnlyList<InstalledApp>>(() =>
        {
            using var deadlineCts =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            deadlineCts.CancelAfter(DiscoveryDeadline);

            var seen = new Dictionary<string, InstalledApp>(StringComparer.OrdinalIgnoreCase);

            foreach (var (hive, view, subKey) in Roots)
            {
                if (deadlineCts.IsCancellationRequested) break;

                try
                {
                    using var baseKey = RegistryKey.OpenBaseKey(hive, view);
                    using var uninstall = baseKey.OpenSubKey(subKey);
                    if (uninstall is null) continue;

                    foreach (var childName in uninstall.GetSubKeyNames())
                    {
                        if (deadlineCts.IsCancellationRequested) break;
                        TryReadChild(uninstall, childName, seen);
                    }
                }
                catch (System.Security.SecurityException) { /* hive restricted; skip */ }
                catch (IOException) { /* hive transient; skip */ }
                catch (UnauthorizedAccessException) { /* hive restricted; skip */ }
            }

            return seen.Values
                .OrderBy(a => a.DisplayName, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        }, cancellationToken);
    }

    private static void TryReadChild(
        RegistryKey parent, string childName, Dictionary<string, InstalledApp> sink)
    {
        RegistryKey? child = null;
        try
        {
            child = parent.OpenSubKey(childName);
            if (child is null) return;

            var displayName = child.GetValue("DisplayName") as string;
            if (string.IsNullOrWhiteSpace(displayName)) return;

            var path = ResolvePath(child, displayName);
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return;

            // Already known from another hive — keep the first hit.
            if (sink.ContainsKey(path)) return;

            var publisher = child.GetValue("Publisher") as string;
            if (string.IsNullOrWhiteSpace(publisher)) publisher = null;

            var iconKey = GuessIconKey(displayName);

            sink.Add(path, new InstalledApp(
                DisplayName: displayName.Trim(),
                Path: path,
                Publisher: publisher?.Trim(),
                IconKey: iconKey));
        }
        catch (System.Security.SecurityException) { /* restricted child */ }
        catch (IOException) { /* transient child */ }
        finally
        {
            child?.Dispose();
        }
    }

    private static string? ResolvePath(RegistryKey child, string displayName)
    {
        // (1) DisplayIcon — clean the comma-suffix Windows sometimes appends.
        var icon = child.GetValue("DisplayIcon") as string;
        if (!string.IsNullOrWhiteSpace(icon))
        {
            var cleaned = icon.Trim();
            var comma = cleaned.IndexOf(',');
            if (comma > 0) cleaned = cleaned[..comma];
            cleaned = cleaned.Trim('"');
            if (Path.HasExtension(cleaned) && File.Exists(cleaned)) return Path.GetFullPath(cleaned);
        }

        // (2) InstallLocation + "<DisplayName>.exe".
        var installLocation = child.GetValue("InstallLocation") as string;
        if (!string.IsNullOrWhiteSpace(installLocation) && Directory.Exists(installLocation))
        {
            foreach (var candidate in EnumerateExeCandidates(installLocation, displayName))
                if (File.Exists(candidate)) return Path.GetFullPath(candidate);
        }

        // (3) UninstallString — best-effort regex for the embedded path.
        var uninstall = child.GetValue("UninstallString") as string;
        if (!string.IsNullOrWhiteSpace(uninstall))
        {
            var match = System.Text.RegularExpressions.Regex.Match(
                uninstall, "\"(?<p>[^\"]+\\.exe)\"", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (match.Success && File.Exists(match.Groups["p"].Value))
                return Path.GetFullPath(match.Groups["p"].Value);
        }

        return null;
    }

    private static IEnumerable<string> EnumerateExeCandidates(string installLocation, string displayName)
    {
        yield return Path.Combine(installLocation, displayName + ".exe");
        foreach (var exe in Directory.EnumerateFiles(installLocation, "*.exe", SearchOption.TopDirectoryOnly))
            yield return exe;
    }

    /// <summary>
    /// Map well-known app names to design-system icon keys. Everything
    /// else falls back to the route-level ProtectedApps glyph so the
    /// picker row remains visually consistent. Icon-extractor (read
    /// the actual exe icon) lands in a later milestone — README flag.
    /// </summary>
    private static string GuessIconKey(string displayName)
    {
        var n = displayName.ToLowerInvariant();
        if (n.Contains("chrome")  || n.Contains("edge")    || n.Contains("firefox")) return "Icons.Route.ProtectedApps";
        if (n.Contains("discord") || n.Contains("slack")   || n.Contains("teams"))  return "Icons.Route.ProtectedApps";
        if (n.Contains("steam")   || n.Contains("epic")    || n.Contains("riot"))   return "Icons.Route.ProtectedApps";
        if (n.Contains("code")    || n.Contains("visual studio") || n.Contains("intellij")) return "Icons.Route.ProtectedApps";
        return "Icons.Route.ProtectedApps";
    }
}
