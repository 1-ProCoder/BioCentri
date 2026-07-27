using BioCentri.Core.Model;

namespace BioCentri.Core.Services;

/// <summary>
/// Decoupled Windows Hello biometric adapter. <see cref="RequestVerificationAsync"/>
/// wraps <c>Windows.Security.Credentials.UI.UserConsentVerifier</c>; this
/// interface exists in <c>BioCentri.Core</c> (no WPF, no dispatcher) so the
/// concrete adapter and its fakes can be tested headlessly.
///
/// The app-layer <c>BiometricAuthService</c> (in <c>BioCentri.App</c>) injects
/// this interface and handles all UI-thread marshalling, coalescing, dedupe,
/// ShellState mutation, and toast feedback.
/// </summary>
public interface IHelloService
{
    /// <summary>
    /// Prompt the user to verify their identity. The message is shown
    /// in the OS biometric prompt. Returns the raw outcome of the attempt.
    /// Callers must marshal this call onto the correct thread
    /// (<c>UserConsentVerifier</c> requires STA / UI thread in WPF).
    /// </summary>
    Task<HelloOutcome> RequestVerificationAsync(string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Probe whether biometric hardware is available and configured for
    /// the current user without showing a prompt. Returns a capability
    /// value that the app layer surfaces via its diagnostics page.
    /// </summary>
    Task<HelloCapability> CheckAvailabilityAsync(CancellationToken cancellationToken = default);
}
