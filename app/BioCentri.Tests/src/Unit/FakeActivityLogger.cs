using BioCentri.App.Types;
using BioCentri.App.Types.Services;

namespace BioCentri.Tests.Unit;

/// <summary>
/// In-memory <see cref="IActivityLogger"/> for headless tests. Stores
/// every <see cref="ActivityEvent"/> passed to <see cref="LogAsync"/>
/// in a thread-safe list so tests can assert outcomes without
/// touching the on-device <c>activity.json</c> file.
///
/// Mirrors the shape of <see cref="FakeHelloService"/>: zero side
/// effects, no threading primitives the test framework has to
/// coordinate, all calls return a completed Task deterministically.
/// </summary>
public sealed class FakeActivityLogger : IActivityLogger
{
    private readonly object _gate = new();
    public List<ActivityEvent> Events { get; } = new();

    public Task LogAsync(ActivityEvent ev, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ev);
        lock (_gate) Events.Add(ev);
        return Task.CompletedTask;
    }
}
