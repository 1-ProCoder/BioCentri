using Windows.Security.Credentials.UI;
using BioCentri.Core.Model;
using BioCentri.Core.Services;

namespace BioCentri.Core.Interop;

/// <summary>
/// Concrete <see cref="IHelloService"/> over
/// <c>Windows.Security.Credentials.UI.UserConsentVerifier</c>.
///
/// Threading: the WinRT APIs require STA (UI thread in WPF). This
/// adapter does NOT marshal — the caller (<c>BiometricAuthService</c>
/// in <c>BioCentri.App</c>) marshals the calls via <c>IDispatcher</c>
/// before calling this adapter. That split keeps Core free of WPF
/// dependencies while still allowing headless tests to inject a
/// <c>FakeHelloService</c> that returns pre-canned outcomes.
/// </summary>
public sealed class UserConsentVerifierAdapter : IHelloService
{
    /// <inheritdoc />
    public async Task<HelloOutcome> RequestVerificationAsync(
        string message, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await UserConsentVerifier.RequestVerificationAsync(message)
                .AsTask(cancellationToken)
                .ConfigureAwait(false);
            return Translate(result);
        }
        catch (OperationCanceledException)
        {
            return HelloOutcome.UserCancelled;
        }
        catch (Exception)
        {
            return HelloOutcome.Error;
        }
    }

    /// <inheritdoc />
    public async Task<HelloCapability> CheckAvailabilityAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var avail = await UserConsentVerifier.CheckAvailabilityAsync()
                .AsTask(cancellationToken)
                .ConfigureAwait(false);
            return TranslateAvailability(avail);
        }
        catch (Exception)
        {
            return HelloCapability.Unknown;
        }
    }

    private static HelloOutcome Translate(UserConsentVerificationResult result) => result switch
    {
        UserConsentVerificationResult.Verified              => HelloOutcome.Verified,
        UserConsentVerificationResult.Canceled              => HelloOutcome.UserCancelled,
        UserConsentVerificationResult.DisabledByPolicy      => HelloOutcome.DisabledByPolicy,
        UserConsentVerificationResult.NotConfiguredForUser => HelloOutcome.NotConfiguredForUser,
        UserConsentVerificationResult.RetriesExhausted      => HelloOutcome.RetriesExhausted,
        _ => HelloOutcome.Error,
    };

    /// <summary>
    /// Maps WinRT availability to our capability enum. The 19041 projection
    /// does not expose <c>NotAvailableForHardware</c>; on those machines
    /// the OS reports <c>DisabledByPolicy</c> or <c>NotConfiguredForUser</c>,
    /// both of which are mapped above. Unknown covers the gap safely.
    /// </summary>
    private static HelloCapability TranslateAvailability(UserConsentVerifierAvailability avail) => avail switch
    {
        UserConsentVerifierAvailability.Available             => HelloCapability.Available,
        UserConsentVerifierAvailability.NotConfiguredForUser => HelloCapability.NotConfiguredForUser,
        UserConsentVerifierAvailability.DisabledByPolicy     => HelloCapability.DisabledByPolicy,
        _ => HelloCapability.Unknown,
    };
}
