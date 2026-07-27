using System.Diagnostics;
using BioCentri.App.State;
using BioCentri.App.Types.Services;
using ActivityEvent = BioCentri.App.Types.ActivityEvent;

namespace BioCentri.App.Services;

/// <summary>
/// Join point between <see cref="IProcessMonitor"/> (raw launch events)
/// and <see cref="IBiometricAuthService"/> (Windows Hello prompts).
///
/// Lifecycle:
///   * ctor wires dependencies.
///   * <see cref="Start"/> subscribes to the monitor's
///     <see cref="IProcessMonitor.ProcessLaunchDetected"/> event so the
///     subscriber exists BEFORE the monitor can fire. Critically, the
///     monitor's <c>Start()</c> is called last so any events emitted
///     during the start handshake are still picked up.
///   * <see cref="Stop"/> reverses both. <see cref="App.OnExit"/> invokes it.
///
/// The watcher deliberately does NOT contain the 500ms dedupe or
/// coalescing logic — both live inside <see cref="BiometricAuthService"/>
/// because that's the surface that owns the in-flight WinRT task.
/// Splitting them keeps the dedupe invariant next to the actual user
/// experience, not buried in an event handler.
///
/// Every observed protected launch is logged via <see cref="IActivityLogger"/>
/// so the Dashboard "Recent activity" + Activity page timelines stay
/// in sync with the on-device audit trail.
/// </summary>
public sealed class ProcessWatcher : IDisposable
{
    private readonly IProcessMonitor _monitor;
    private readonly IAuthAppRules _rules;
    private readonly IBiometricAuthService _biometric;
    private readonly ShellState _shellState;
    private readonly IAppLifecycleService _lifecycle;
    private readonly IDispatcher _dispatcher;
    private readonly AppLockController _lock;
    private readonly IActivityLogger _activity;
    private bool _started;
    private bool _disposed;

    public ProcessWatcher(
        IProcessMonitor monitor,
        IAuthAppRules rules,
        IBiometricAuthService biometric,
        ShellState shellState,
        IAppLifecycleService lifecycle,
        IDispatcher dispatcher,
        AppLockController @lock,
        IActivityLogger activity)
    {
        ArgumentNullException.ThrowIfNull(monitor);
        ArgumentNullException.ThrowIfNull(rules);
        ArgumentNullException.ThrowIfNull(biometric);
        ArgumentNullException.ThrowIfNull(shellState);
        ArgumentNullException.ThrowIfNull(lifecycle);
        ArgumentNullException.ThrowIfNull(dispatcher);
        ArgumentNullException.ThrowIfNull(@lock);
        ArgumentNullException.ThrowIfNull(activity);
        _monitor = monitor;
        _rules = rules;
        _biometric = biometric;
        _shellState = shellState;
        _lifecycle = lifecycle;
        _dispatcher = dispatcher;
        _lock = @lock;
        _activity = activity;
    }

    public void Start()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_started) return;
        _monitor.ProcessLaunchDetected += OnProcessLaunchDetected;
        _monitor.Start();
        _started = true;
    }

    public void Stop()
    {
        if (!_started) return;
        _monitor.ProcessLaunchDetected -= OnProcessLaunchDetected;
        _monitor.Stop();
        _started = false;
    }

    private void OnProcessLaunchDetected(object? sender, ProcessLaunchDetectedEventArgs e)
    {
        // The handler may be invoked on any thread (WMI / timer / stub).
        // We do NOT block here: a synchronous await would block the
        // monitor's thread on the OS prompt, which is exactly the
        // failure mode this architecture exists to avoid.
        if (!_rules.IsProtected(e.ProcessName))
        {
            Debug.WriteLine($"[ProcessWatcher] Allow (unprotected): {e.ProcessName}");
            return;
        }
        _ = HandleProtectedLaunchAsync(e);
    }

    private async Task HandleProtectedLaunchAsync(ProcessLaunchDetectedEventArgs e)
    {
        try
        {
            var outcome = await _biometric.AuthenticateAsync(e.ProcessName, default).ConfigureAwait(false);
            if (outcome == AuthOutcome.Verified)
            {
                Debug.WriteLine($"[ProcessWatcher] Allow (verified): {e.ProcessName}");
                await _activity.LogAsync(new ActivityEvent(
                    TimestampUtc: DateTimeOffset.UtcNow,
                    Severity:     "INFO",
                    AppName:      e.ProcessName,
                    Outcome:      nameof(AuthOutcome.Verified),
                    Description:  "Verified by Windows Hello")).ConfigureAwait(false);
            }
            else
            {
                Debug.WriteLine($"[ProcessWatcher] Block ({outcome}): {e.ProcessName}");
                _lock.Kill(e.Pid, e.ProcessName, outcome.ToString());
                await _activity.LogAsync(new ActivityEvent(
                    TimestampUtc: DateTimeOffset.UtcNow,
                    Severity:     "BLOCKED",
                    AppName:      e.ProcessName,
                    Outcome:      outcome.ToString(),
                    Description:  $"Blocked by BioCentri ({outcome})")).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            // Bug in the auth path must NOT crash the shell. Per
            // docs/DECISIONS.md Decision 6 the surface has zero
            // telemetry — Debug.WriteLine is the local breadcrumb.
            Debug.WriteLine($"[ProcessWatcher] {e.ProcessName} threw {ex.GetType().Name}: {ex.Message}");
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Stop();
    }
}
