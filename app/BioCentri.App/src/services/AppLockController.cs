using System.Diagnostics;
using BioCentri.App.Types.Services;

namespace BioCentri.App.Services;

/// <summary>
/// Process termination surface for the M6 app-locking pipeline.
/// <c>ProcessWatcher</c> injects this and calls
/// <see cref="KillAsync"/> after the biometric challenge results in
/// anything other than <see cref="BioCentri.App.Types.Services.AuthOutcome.Verified"/>.
///
/// Thread-safety: may be invoked from any thread (the watcher's
/// background event handler). The underlying <c>Process.Kill()</c> is
/// thread-safe per MSDN.
/// </summary>
public sealed class AppLockController
{
    private readonly IToastService _toast;

    public AppLockController(IToastService toast)
    {
        ArgumentNullException.ThrowIfNull(toast);
        _toast = toast;
    }

    /// <summary>
    /// Kill the process identified by <paramref name="pid"/>. Best-effort:
    /// if the process has already exited by the time the auth challenge
    /// completes, the call is silently ignored.
    /// </summary>
    /// <param name="displayName">Friendly name for toast feedback.</param>
    /// <param name="pid">OS process ID from the launch event.</param>
    /// <param name="outcome">Why the process was blocked (logged, not
    /// acted on).</param>
    public void Kill(int pid, string displayName, string? outcome = null)
    {
        try
        {
            using var process = Process.GetProcessById(pid);

            // Best-effort: try a graceful close first (WM_CLOSE), then
            // force-kill if the process is still alive after 2 seconds.
            if (!process.HasExited)
            {
                process.CloseMainWindow();
                if (!process.WaitForExit(2000))
                    process.Kill();
            }

            var reason = outcome is not null ? $" ({outcome})" : string.Empty;
            _toast.Show(
                BioCentri.App.Types.Services.ToastSeverity.Warning,
                "Blocked",
                $"{displayName} was blocked by BioCentri.{reason}");
        }
        catch (ArgumentException)
        {
            // Process already exited — nothing to kill.
        }
        catch (InvalidOperationException)
        {
            // Process already exited or cannot be killed.
        }
        catch (System.ComponentModel.Win32Exception)
        {
            // Permission denied (unlikely for a user-mode process).
        }
    }
}
