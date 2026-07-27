using BioCentri.App.Services;
using BioCentri.App.State;
using BioCentri.App.Types.Services;
using BioCentri.Core.Model;
using BioCentri.Core.Services;
using FluentAssertions;
using Xunit;

namespace BioCentri.Tests.Unit;

/// <summary>
/// Headless tests for <see cref="BiometricAuthService"/> orchestration
/// (coalescing, dedupe, ShellState mirroring). Uses
/// <see cref="FakeHelloService"/> to inject pre-canned outcomes, a
/// synchronous <see cref="FakeDispatcher"/>, and a real
/// <see cref="ShellState"/> (lightweight ObservableObject — no WPF
/// thread-affinity needed in headless mode).
/// </summary>
public sealed class BiometricAuthServiceTests
{
    /// <summary>
    /// Create a fresh system-under-test with factory-new fakes.
    /// Every test gets its own FakeDispatcher, FakeHelloService,
    /// ShellState, and FakeToastService so state (message logs,
    /// event subscriptions, accumulated called-records) cannot
    /// leak between tests.
    /// </summary>
    private static BiometricAuthService CreateSut(
        out FakeHelloService hello,
        out FakeToastService toast,
        out ShellState shellState,
        out FakeActivityLogger activity)
    {
        var dispatcher = new FakeDispatcher();
        hello = new FakeHelloService();
        toast = new FakeToastService();
        shellState = new ShellState();
        activity = new FakeActivityLogger();
        return new BiometricAuthService(dispatcher, toast, shellState, hello, activity);
    }

    [Fact]
    public async Task AuthenticateAsync_Verified_SetsShellStateAndReturnsVerified()
    {
        var sut = CreateSut(out var hello, out _, out var shellState, out var activity);
        hello.NextOutcome = HelloOutcome.Verified;

        var outcome = await sut.AuthenticateAsync("Chrome", CancellationToken.None);

        outcome.Should().Be(AuthOutcome.Verified);
        shellState.PendingAppName.Should().BeEmpty("cleared after the call completes");
        shellState.IsAuthenticationInProgress.Should().BeFalse();
        activity.Events.Should().ContainSingle("every successful auth writes a Verified event");
        activity.Events[0].Outcome.Should().Be(nameof(AuthOutcome.Verified));
        activity.Events[0].Severity.Should().Be("INFO");
    }

    [Fact]
    public async Task AuthenticateAsync_UserCancelled_ReturnsUserCancelled()
    {
        var sut = CreateSut(out var hello, out _, out _, out _);
        hello.NextOutcome = HelloOutcome.UserCancelled;

        var outcome = await sut.AuthenticateAsync("Chrome", CancellationToken.None);

        outcome.Should().Be(AuthOutcome.UserCancelled);
    }

    [Fact]
    public async Task AuthenticateAsync_SameAppName_CoalescesToOnePrompt()
    {
        var sut = CreateSut(out var hello, out _, out _, out _);
        hello.NextOutcome = HelloOutcome.Verified;
        hello.Delay = TimeSpan.FromMilliseconds(80); // force overlap

        var t1 = sut.AuthenticateAsync("Discord", CancellationToken.None);
        var t2 = sut.AuthenticateAsync("Discord", CancellationToken.None);

        var results = await Task.WhenAll(t1, t2);

        results.Should().AllBeEquivalentTo(AuthOutcome.Verified);
        // Exactly one Hello request was made (coalesced).
        hello.Messages.Should().HaveCount(1);
    }

    [Fact]
    public async Task AuthenticateAsync_DifferentAppNames_DoNotCoalesce()
    {
        var sut = CreateSut(out var hello, out _, out _, out _);
        hello.NextOutcome = HelloOutcome.Verified;            var t1 = sut.AuthenticateAsync("Chrome", CancellationToken.None);
            var t2 = sut.AuthenticateAsync("Discord", CancellationToken.None);

            var results = await Task.WhenAll(t1, t2);

            results.Should().AllBeEquivalentTo(AuthOutcome.Verified);
            hello.Messages.Should().HaveCount(2);
        }


    [Fact]
    public async Task AuthenticateAsync_WithinDedupeWindow_ReturnsDeduped()
    {
        var sut = CreateSut(out var hello, out _, out _, out _);
        hello.NextOutcome = HelloOutcome.Verified;
        await sut.AuthenticateAsync("Chrome", CancellationToken.None); // first challenge

        var outcome = await sut.AuthenticateAsync("Chrome", CancellationToken.None); // immediate retry

        outcome.Should().Be(AuthOutcome.Deduped);
    }

    [Fact]
    public async Task GetCapabilityAsync_Available_ReturnsAvailable()
    {
        var sut = CreateSut(out var hello, out _, out _, out _);
        hello.NextCapability = HelloCapability.Available;

        var cap = await sut.GetCapabilityAsync(CancellationToken.None);

        cap.Should().Be(AuthCapability.Available);
    }

    [Fact]
    public async Task AuthenticateAsync_CancelOverlay_ReturnsUserCancelled()
    {
        var sut = CreateSut(out var hello, out _, out var shellState, out _);
        hello.NextOutcome = HelloOutcome.Verified;
        hello.Delay = TimeSpan.FromSeconds(5); // long-running; cancel will race

        var authTask = sut.AuthenticateAsync("Chrome", CancellationToken.None);

        // The auth service sets PendingAppName synchronously (FakeDispatcher)
        // before it blocks on hello's delay. Cancel now to force-complete the TCS.
        shellState.CancelAuthentication();

        var outcome = await authTask;
        outcome.Should().Be(AuthOutcome.UserCancelled);
        shellState.IsAuthenticationInProgress.Should().BeFalse();
    }
}

/// <summary>Minimal toast spy — records calls for assertion.</summary>
internal sealed class FakeToastService : IToastService
{
    public List<(ToastSeverity Severity, string Title, string? Desc)> Calls { get; } = new();

    public IToastController Show(ToastSeverity severity, string title, string? description = null, int? durationMs = null)
    {
        Calls.Add((severity, title, description));
        return new FakeToastController();
    }

    private sealed class FakeToastController : IToastController
    {
        public void Dismiss() { }
    }
}
