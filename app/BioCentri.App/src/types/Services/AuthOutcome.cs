namespace BioCentri.App.Types.Services;

/// <summary>
/// Outcome of a single biometric authentication attempt. The semantics
/// intentionally mirror <see cref="Windows.Security.Credentials.UI.UserConsentVerificationResult"/>
/// so that callers do not have to reason about the WinRT enum directly.
/// </summary>
public enum AuthOutcome
{
    /// <summary>The user verified their identity. Process launch proceeds.</summary>
    Verified,

    /// <summary>The user dismissed the OS biometrics prompt.</summary>
    UserCancelled,

    /// <summary>The device's biometric hardware is unavailable.</summary>
    DeviceUnavailable,

    /// <summary>Policy blocks biometric authentication for this user/machine.</summary>
    DisabledByPolicy,

    /// <summary>The OS reports biometrics are not configured for the current user.</summary>
    NotConfiguredForUser,

    /// <summary>The user exhausted the OS-provided retry window.</summary>
    RetriesExhausted,

    /// <summary>A 500ms rapid-relaunch window swallowed the call to avoid double prompts.</summary>
    Deduped,

    /// <summary>An internal error occurred (COMException, marshalling failure, etc).</summary>
    Error,

    /// <summary>
    /// The OS biometric prompt did not resolve within the
    /// <see cref="BioCentri.App.Services.BiometricAuthService"/> timeout
    /// (60 s). Treated like any other non-Verified outcome: the
    /// process is blocked and the kill chain runs. Distinct from
    /// <see cref="UserCancelled"/> so the audit trail shows whether
    /// the user actively hit cancel versus walked away.
    /// </summary>
    Timeout,
}

/// <summary>
/// Long-lived capability of the device's biometric stack. Distinct from
/// <see cref="AuthOutcome"/> because it answers a different question: can
/// we ever verify, not did this attempt succeed?
/// </summary>
public enum AuthCapability
{
    Available,
    NotConfiguredForUser,
    DisabledByPolicy,
    NotAvailableForHardware,
    Unknown,
}

/// <summary>Carries the latest auth state to subscribers of <see cref="IBiometricAuthService.StateChanged"/>.</summary>
public sealed class AuthStateChangedEventArgs : EventArgs
{
    public AuthStateChangedEventArgs(bool isInProgress, string pendingAppName)
    {
        IsInProgress = isInProgress;
        PendingAppName = pendingAppName;
    }

    public bool IsInProgress { get; }
    public string PendingAppName { get; }
}
