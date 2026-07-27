namespace BioCentri.App.Types.Services;

/// <summary>
/// Windows Hello biometric authentication adapter. Wraps
/// <c>Windows.Security.Credentials.UI.UserConsentVerifier</c> with
/// thread-safe coalescing and a 500ms rapid-relaunch guard so that
/// double-clicking a protected app prompts the OS once, not twice.
///
/// All async methods may be invoked from any thread. Shell-state writes
/// are marshalled to the WPF UI thread internally.
/// </summary>
public interface IBiometricAuthService
{
    /// <summary>
    /// Prompt the user to verify their identity for the given protected
    /// <paramref name="appName"/>. Returns the outcome of the attempt.
    /// </summary>
    /// <remarks>
    /// * If a re-launch arrives within the 500ms dedupe window the call
    ///   returns <see cref="AuthOutcome.Deduped"/> and no OS prompt is shown.
    /// * If a re-launch arrives after 500ms but while the previous prompt
    ///   is still pending, the new call awaits the existing prompt's
    ///   outcome (coalesce on app-name). This matches the spec's "only one
    ///   auth prompt" semantics.
    /// * Cancellation is best-effort: a UI cancel dismisses the overlay
    ///   immediately; the OS dialog persists until the user dismisses it.
    /// </remarks>
    Task<AuthOutcome> AuthenticateAsync(string appName, CancellationToken cancellationToken);

    /// <summary>
    /// Probe the device's biometric capability without prompting.
    /// Returns one of <see cref="AuthCapability"/> values.
    /// </summary>
    Task<AuthCapability> GetCapabilityAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Raised when the service transitions between idle and in-progress.
    /// Subscribed by <see cref="BioCentri.App.State.ShellState"/> via
    /// <see cref="BioCentri.App.State.ShellState.AuthenticationCancelRequested"/>;
    /// not surfaced directly to UI today.
    /// </summary>
    event EventHandler<AuthStateChangedEventArgs>? StateChanged;
}
