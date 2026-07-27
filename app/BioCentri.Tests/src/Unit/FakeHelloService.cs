using BioCentri.Core.Model;
using BioCentri.Core.Services;

namespace BioCentri.Tests.Unit;

/// <summary>
/// Test double for <see cref="IHelloService"/>. Inject pre-canned
/// outcomes via <see cref="NextOutcome"/> and
/// <see cref="NextCapability"/> before the system under test calls
/// through the interface. All calls are synchronous (no real OS prompt
/// surfaces) and recorded for later assertion.
/// </summary>
public sealed class FakeHelloService : IHelloService
{
    private HelloOutcome _outcome = HelloOutcome.Verified;
    private HelloCapability _capability = HelloCapability.Available;

    /// <summary>Outcome the next call to <see cref="RequestVerificationAsync"/> will return.</summary>
    public HelloOutcome NextOutcome
    {
        get => _outcome;
        set
        {
            _outcome = value;
            Outcomes.Add(value);
        }
    }

    /// <summary>Capability the next call to <see cref="CheckAvailabilityAsync"/> will return.</summary>
    public HelloCapability NextCapability
    {
        get => _capability;
        set => _capability = value;
    }

    /// <summary>Every outcome returned (most-recent-first). Allows the
    /// test to assert that dedupe coalescing or cancellation paths
    /// produced exactly the expected sequence.</summary>
    public List<HelloOutcome> Outcomes { get; } = new();

    /// <summary>Messages passed to RequestVerificationAsync (recorded
    /// per call; most-recent-first).</summary>
    public List<string> Messages { get; } = new();

    /// <summary>How long the adapter should block (simulate a real WinRT
    /// dialog). Default 0 — immediately returns the canned result.</summary>
    public TimeSpan Delay { get; set; } = TimeSpan.Zero;

    public async Task<HelloOutcome> RequestVerificationAsync(
        string message, CancellationToken cancellationToken = default)
    {
        Messages.Add(message);

        if (Delay > TimeSpan.Zero)
        {
            try { await Task.Delay(Delay, cancellationToken); }
            catch (OperationCanceledException) { return HelloOutcome.UserCancelled; }
        }

        return NextOutcome;
    }

    public Task<HelloCapability> CheckAvailabilityAsync(
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(NextCapability);
    }
}
