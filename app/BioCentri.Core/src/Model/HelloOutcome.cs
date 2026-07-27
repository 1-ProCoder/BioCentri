namespace BioCentri.Core.Model;

/// <summary>
/// Outcome of a single biometric authentication attempt. Semantics
/// mirror <c>Windows.Security.Credentials.UI.UserConsentVerificationResult</c>
/// so callers never need to reference the WinRT enum directly.
///
/// The app layer (<c>BioCentri.App.Services.BiometricAuthService</c>)
/// maps these values to its own <c>AuthOutcome</c> enum, which adds
/// orchestrator-specific outcomes (<c>Deduped</c>, <c>Error</c>) that
/// are not represented in the OS contract.
/// </summary>
public enum HelloOutcome
{
    /// <summary>The user verified their identity.</summary>
    Verified,

    /// <summary>The user dismissed the OS biometric prompt.</summary>
    UserCancelled,

    /// <summary>The device biometric hardware is unavailable.</summary>
    DeviceUnavailable,

    /// <summary>Policy blocks biometric authentication.</summary>
    DisabledByPolicy,

    /// <summary>Biometrics are not configured for the current user.</summary>
    NotConfiguredForUser,

    /// <summary>The user exhausted the OS retry window.</summary>
    RetriesExhausted,

    /// <summary>An unexpected OS-level error occurred.</summary>
    Error,
}

/// <summary>
/// Long-lived capability of the device biometric stack — answers
/// "can we verify?" separate from "did this attempt succeed?"
/// </summary>
public enum HelloCapability
{
    Available,
    NotConfiguredForUser,
    DisabledByPolicy,
    NotAvailableForHardware,
    Unknown,
}
