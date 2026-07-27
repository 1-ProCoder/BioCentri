using System.Diagnostics;
using BioCentri.App.Types.Services;

namespace BioCentri.App.Services;

/// <summary>
/// Stub <see cref="IProcessMonitor"/> fires no events on its own —
/// callers invoke <see cref="Trigger"/> explicitly to simulate a
/// protected-app launch. This is the milestone-4 seam: the real
/// <c>Win32_ProcessStartTrace</c> subscriber lands in Milestone 6 and
/// replaces this class via DI without touching the
/// <see cref="ProcessWatcher"/> or <see cref="BiometricAuthService"/>.
/// </summary>
public sealed class StubProcessMonitor : IProcessMonitor
{
    private int _nextPid = 4000;

    /// <inheritdoc />
    public event EventHandler<ProcessLaunchDetectedEventArgs>? ProcessLaunchDetected;

    private bool _started;

    /// <inheritdoc />
    public void Start() => _started = true;

    /// <inheritdoc />
    public void Stop() => _started = false;

    /// <summary>
    /// Manually emit a synthetic launch event. Useful for QA: call once
    /// after the shell has rendered to exercise the auth + overlay path.
    /// No-op when the monitor is not started.
    /// </summary>
    public void Trigger(string processName, int? pid = null)
    {
        if (!_started) return;
        ArgumentException.ThrowIfNullOrEmpty(processName);
        var args = new ProcessLaunchDetectedEventArgs(
            processName,
            pid ?? Interlocked.Increment(ref _nextPid),
            DateTimeOffset.UtcNow);
        try
        {
            ProcessLaunchDetected?.Invoke(this, args);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[StubProcessMonitor] subscriber threw: {ex}");
        }
    }
}
