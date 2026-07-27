namespace BioCentri.App.Types.Services;

/// <summary>
/// Process-launch monitor. The watcher subscribes to
/// <see cref="ProcessLaunchDetected"/>; on each protected launch it asks
/// the biometric auth service to verify the user before allowing the
/// launch to continue.
///
/// In Production (Milestone 6) this is implemented by a WMI
/// <c>Win32_ProcessStartTrace</c> subscriber with a polling fallback.
/// In Milestone 4 the only concrete impl is <c>StubProcessMonitor</c>
/// which fires synthetic events on demand, so the auth + overlay pipeline
/// can be exercised end-to-end without the real watcher in place.
/// </summary>
public interface IProcessMonitor
{
    /// <summary>
    /// Raised when a process launch is detected. Handlers may run on a
    /// background thread (WMI / timer / threadpool) so subscribers must
    /// marshal to the UI thread before touching any XAML-bound state.
    /// </summary>
    event EventHandler<ProcessLaunchDetectedEventArgs>? ProcessLaunchDetected;

    /// <summary>Begin emitting events. Idempotent on most impls.</summary>
    void Start();

    /// <summary>Stop emitting events. Idempotent on most impls.</summary>
    void Stop();
}

/// <summary>Event payload for a detected process launch.</summary>
public sealed class ProcessLaunchDetectedEventArgs : EventArgs
{
    public ProcessLaunchDetectedEventArgs(string processName, int pid, DateTimeOffset timestampUtc)
    {
        ProcessName = processName;
        Pid = pid;
        TimestampUtc = timestampUtc;
    }

    public string ProcessName { get; }
    public int Pid { get; }
    public DateTimeOffset TimestampUtc { get; }
}
