using System.Collections.Concurrent;
using System.Diagnostics;
using BioCentri.App.State;
using BioCentri.App.Types.Services;
using BioCentri.Core.Model;
using BioCentri.Core.Services;

namespace BioCentri.App.Services;

/// <summary>
/// Concrete <see cref="IBiometricAuthService"/> that orchestrates
/// <see cref="IHelloService"/> (from <c>BioCentri.Core</c>) with
/// UI-thread marshalling, coalescing, dedupe, ShellState mutation,
/// and user feedback via toast.
///
/// Threading model:
///   * <see cref="IHelloService.RequestVerificationAsync"/> must be
///     called on the STA / UI thread (WinRT requirement). This service
///     marshals calls onto the WPF dispatcher via <see cref="IDispatcher"/>.
///   * Callers may invoke <see cref="AuthenticateAsync"/> from any
///     thread; coalescing and the 500ms guard use
///     <see cref="ConcurrentDictionary{TKey,TValue}"/>.
///
/// Cancellation model:
///   * When <see cref="ShellState.CancelAuthentication"/> runs (the
///     user clicks Cancel on the overlay),
///     <see cref="ShellState.AuthenticationCancelRequested"/> fires.
///     The service force-completes the in-flight
///     <see cref="TaskCompletionSource{TResult}"/> with
///     <see cref="AuthOutcome.UserCancelled"/> so any awaiter unblocks
///     immediately. The OS-level prompt cannot be cancelled from
///     outside (WinRT surface does not accept a
///     <see cref="CancellationToken"/>); when the user eventually
///     dismisses it, the result is dropped by
///     <see cref="TaskCompletionSource{TResult}.TrySetResult"/>'s
///     idempotence.
/// </summary>
public sealed class BiometricAuthService : IBiometricAuthService
{
    private readonly IDispatcher _dispatcher;
    private readonly IToastService _toast;
    private readonly ShellState _shellState;
    private readonly IHelloService _hello;
    private readonly IActivityLogger _activity;

    private readonly ConcurrentDictionary<string, TaskCompletionSource<AuthOutcome>> _pending = new();
    private readonly ConcurrentDictionary<string, DateTimeOffset> _lastChallengedAt = new();

    private static readonly TimeSpan DedupeWindow = TimeSpan.FromMilliseconds(500);

    /// <summary>
    /// Hard upper bound on how long the OS biometric prompt is
    /// allowed to remain unanswered before the chain treats it as a
    /// non-verification. Real users verify in &lt;5 s; 60 s covers a
    /// user stepping away without making the kill chain wait
    /// forever (which would leave the protected process running
    /// without an audit trail entry).
    ///
    /// Hardcoded, NOT settings-driven: a malicious actor could
    /// otherwise raise the value to bypass the block-and-kill flow.
    /// </summary>
    private static readonly TimeSpan AuthTimeout = TimeSpan.FromSeconds(60);

    public BiometricAuthService(
        IDispatcher dispatcher,
        IToastService toast,
        ShellState shellState,
        IHelloService hello,
        IActivityLogger activity)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);
        ArgumentNullException.ThrowIfNull(toast);
        ArgumentNullException.ThrowIfNull(shellState);
        ArgumentNullException.ThrowIfNull(hello);
        ArgumentNullException.ThrowIfNull(activity);
        _dispatcher = dispatcher;
        _toast = toast;
        _shellState = shellState;
        _hello = hello;
        _activity = activity;
        _shellState.AuthenticationCancelRequested += OnShellStateCancelRequested;
    }

    /// <inheritdoc />
    public event EventHandler<AuthStateChangedEventArgs>? StateChanged;

    /// <inheritdoc />
    public async Task<AuthOutcome> AuthenticateAsync(string appName, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(appName);

        // Coalesce: if a prompt is already pending for this appName, ride
        // along and observe the same outcome.
        if (_pending.TryGetValue(appName, out var existing))
        {
            try
            {
                return await existing.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return AuthOutcome.UserCancelled;
            }
        }

        // 500ms rapid-relaunch guard. Drop duplicates silently — a
        // double-clicked app shouldn't prompt twice.
        var now = DateTimeOffset.UtcNow;
        if (_lastChallengedAt.TryGetValue(appName, out var last) && (now - last) < DedupeWindow)
        {
            return AuthOutcome.Deduped;
        }
        _lastChallengedAt[appName] = now;

        // Drive ShellState via the UI thread so PropertyChanged events
        // fire from the right context (WPF INPC contract assumes the
        // dispatcher thread for binding consumers).
        await _dispatcher.InvokeAsync(() =>
        {
            _shellState.PendingAppName = appName;
            _shellState.IsAuthenticationInProgress = true;
            StateChanged?.Invoke(this, new AuthStateChangedEventArgs(true, appName));
        }).ConfigureAwait(false);

        var tcs = new TaskCompletionSource<AuthOutcome>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pending[appName] = tcs;

        try
        {
            var message = $"Verify your identity to launch {appName}.";
            // Linked CTS owns the 60s timeout policy. The adapter
            // forwards the token to the WinRT async, so OS-side
            // cancellation still works if the user clicks Cancel on
            // the overlay (ShellState.AuthenticationCancelRequested
            // → OnShellStateCancelRequested → tcs.TrySetResult,
            // which is idempotent if our timeout also fires).
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(AuthTimeout);
            Debug.WriteLine($"[BiometricAuthService] Prompt initiated for {appName}");

            // Marshal the WinRT call onto the WPF STA thread.
            var helloResult = await _dispatcher.InvokeAsync(async () =>
                await _hello.RequestVerificationAsync(message, cts.Token)
            ).ConfigureAwait(false);

            // If our CTS fired but the caller's token didn't, we
            // timed out — distinct from UserCancelled so the audit
            // log shows "user walked away" vs "user actively hit
            // cancel".
            var timedOut = cts.IsCancellationRequested && !cancellationToken.IsCancellationRequested;
            var outcome = timedOut ? AuthOutcome.Timeout : Translate(helloResult);
            Debug.WriteLine($"[BiometricAuthService] Prompt resolved: {outcome} (timedOut={timedOut})");
            tcs.TrySetResult(outcome);
            _ = _activity.LogAsync(new BioCentri.App.Types.ActivityEvent(
                TimestampUtc: now,
                Severity:     outcome == AuthOutcome.Verified ? "INFO" : "BLOCKED",
                AppName:      appName,
                Outcome:      outcome.ToString(),
                Description:  outcome == AuthOutcome.Verified
                    ? "Verified by Windows Hello"
                    : $"Blocked by BioCentri ({outcome})"), cancellationToken);
            return tcs.Task.Result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[BiometricAuthService] {appName} threw {ex.GetType().Name}: {ex.Message}");
            tcs.TrySetResult(AuthOutcome.Error);
            return tcs.Task.Result;
        }
        finally
        {
            // Only clear our own entry: a coalesced waiter could already
            // have taken over and become the canonical owner. ReferenceEquals
            // confirms we are still it before we strip _pending + clear state.
            if (_pending.TryGetValue(appName, out var current) && ReferenceEquals(current, tcs))
            {
                _pending.TryRemove(appName, out _);
                await _dispatcher.InvokeAsync(() =>
                {
                    _shellState.IsAuthenticationInProgress = false;
                    _shellState.PendingAppName = string.Empty;
                    StateChanged?.Invoke(this, new AuthStateChangedEventArgs(false, string.Empty));
                }).ConfigureAwait(false);
            }
        }
    }

    /// <inheritdoc />
    public async Task<AuthCapability> GetCapabilityAsync(CancellationToken cancellationToken)
    {
        var avail = await _dispatcher.InvokeAsync(async () =>
            await _hello.CheckAvailabilityAsync(cancellationToken)
        ).ConfigureAwait(false);
        return TranslateCapability(avail);
    }

    private void OnShellStateCancelRequested(object? sender, EventArgs e)
    {
        var appName = _shellState.PendingAppName;
        if (string.IsNullOrEmpty(appName)) return;
        if (_pending.TryGetValue(appName, out var tcs))
        {
            // Force-complete. Awaiters unblock immediately. The OS
            // prompt continues to run until the user dismisses it; its
            // eventual result is dropped by TrySetResult idempotence.
            tcs.TrySetResult(AuthOutcome.UserCancelled);
        }
    }

    /// <summary>
    /// Maps a <see cref="HelloOutcome"/> from Core to the app-layer
    /// <see cref="AuthOutcome"/> enum. The mapping is one-to-one at M5;
    /// the app enum adds <c>Deduped</c> and <c>Error</c> which the
    /// service produces internally without going through the adapter.
    /// </summary>
    private static AuthOutcome Translate(HelloOutcome outcome) => outcome switch
    {
        HelloOutcome.Verified              => AuthOutcome.Verified,
        HelloOutcome.UserCancelled         => AuthOutcome.UserCancelled,
        HelloOutcome.DeviceUnavailable     => AuthOutcome.DeviceUnavailable,
        HelloOutcome.DisabledByPolicy      => AuthOutcome.DisabledByPolicy,
        HelloOutcome.NotConfiguredForUser => AuthOutcome.NotConfiguredForUser,
        HelloOutcome.RetriesExhausted      => AuthOutcome.RetriesExhausted,
        HelloOutcome.Error                 => AuthOutcome.Error,
        _ => AuthOutcome.Error,
    };

    private static AuthCapability TranslateCapability(HelloCapability cap) => cap switch
    {
        HelloCapability.Available              => AuthCapability.Available,
        HelloCapability.NotConfiguredForUser  => AuthCapability.NotConfiguredForUser,
        HelloCapability.DisabledByPolicy      => AuthCapability.DisabledByPolicy,
        HelloCapability.NotAvailableForHardware => AuthCapability.NotAvailableForHardware,
        HelloCapability.Unknown                => AuthCapability.Unknown,
        _ => AuthCapability.Unknown,
    };
}
