using System.Diagnostics;
using System.Management;
using System.Timers;
using BioCentri.App.Types.Services;

namespace BioCentri.App.Services;

/// <summary>
/// Real <see cref="IProcessMonitor"/> backed by WMI
/// <c>Win32_ProcessStartTrace</c>. Replaces <see cref="StubProcessMonitor"/>
/// in Milestone 6.
///
/// WMI events fire on a background MTA thread — subscribers must marshal
/// to the UI thread before touching WPF-bound state. The watcher already
/// uses fire-and-forget (<c>_ = HandleProtectedLaunchAsync(e)</c>) so no
/// dispatcher marshal is needed here; the downstream
/// <see cref="BiometricAuthService"/> marshals its own WinRT calls via
/// <see cref="IDispatcher"/>.
///
/// Polling fallback (every 5 s): if WMI fails to subscribe (rare but
/// possible on restricted enterprise images), a background timer polls
/// <c>Process.GetProcesses()</c> and emits for newly-seen PIDs.
/// Both paths share the same dedupe set so a process seen via WMI is
/// never double-reported by the poller.
/// </summary>
public sealed class ProcessMonitor : IProcessMonitor, IDisposable
{
    private ManagementEventWatcher? _wmiWatcher;
    private System.Timers.Timer? _poller;
    private readonly HashSet<int> _seenPids = new();
    private readonly object _gate = new();
    private bool _disposed;

    /// <inheritdoc />
    public event EventHandler<ProcessLaunchDetectedEventArgs>? ProcessLaunchDetected;

    /// <inheritdoc />
    public void Start()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        // Seed the dedupe set with every PID running right now so the
        // poller doesn't re-report pre-existing processes on the first tick.
        foreach (var p in Process.GetProcesses())
        {
            try { lock (_gate) _seenPids.Add(p.Id); }
            catch { /* process exited between enum + property read */ }
        }

        try
        {
            var query = new WqlEventQuery(
                "SELECT * FROM Win32_ProcessStartTrace");
            _wmiWatcher = new ManagementEventWatcher(query);
            _wmiWatcher.EventArrived += OnWmiProcessStarted;
            _wmiWatcher.Start();
            Debug.WriteLine("[ProcessMonitor] WMI watcher subscribed.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ProcessMonitor] WMI subscribe failed: {ex.Message}. Using poller only.");
            _wmiWatcher?.Dispose();
            _wmiWatcher = null;
        }

        // Always run the poller as a safety net — catches fast-launch
        // processes that WMI might miss, and keeps working when WMI fails.
        _poller = new System.Timers.Timer(5000) { AutoReset = true };
        _poller.Elapsed += OnPollerTick;
        _poller.Start();
    }

    /// <inheritdoc />
    public void Stop()
    {
        if (_wmiWatcher is not null)
        {
            try { _wmiWatcher.Stop(); } catch { /* WMI already down */ }
            _wmiWatcher.EventArrived -= OnWmiProcessStarted;
            _wmiWatcher.Dispose();
            _wmiWatcher = null;
        }

        if (_poller is not null)
        {
            _poller.Stop();
            _poller.Elapsed -= OnPollerTick;
            _poller.Dispose();
            _poller = null;
        }
    }

    private void OnWmiProcessStarted(object sender, EventArrivedEventArgs e)
    {
        var name = e.NewEvent.Properties["ProcessName"]?.Value as string;
        var pidRaw = e.NewEvent.Properties["ProcessID"]?.Value;
        if (string.IsNullOrWhiteSpace(name) || pidRaw is null) return;

        var pid = Convert.ToInt32(pidRaw, System.Globalization.CultureInfo.InvariantCulture);
        if (TryDedupe(pid))
            Emit(name, pid);
    }

    private void OnPollerTick(object? sender, System.Timers.ElapsedEventArgs e)
    {
        Process[] snapshot;
        try { snapshot = Process.GetProcesses(); }
        catch { return; /* rare permission failure */ }

        foreach (var p in snapshot)
        {
            try
            {
                if (TryDedupe(p.Id))
                    Emit(p.ProcessName, p.Id);
            }
            catch
            {
                // Process exited mid-enumeration — skip.
            }
        }
    }

    /// <summary>Returns true when <paramref name="pid"/> is new.</summary>
    private bool TryDedupe(int pid)
    {
        lock (_gate)
        {
            if (_seenPids.Contains(pid)) return false;
            _seenPids.Add(pid);
            return true;
        }
    }

    private void Emit(string processName, int pid)
    {
        var args = new ProcessLaunchDetectedEventArgs(processName, pid, DateTimeOffset.UtcNow);
        try
        {
            ProcessLaunchDetected?.Invoke(this, args);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ProcessMonitor] subscriber threw: {ex}");
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Stop();
    }
}
