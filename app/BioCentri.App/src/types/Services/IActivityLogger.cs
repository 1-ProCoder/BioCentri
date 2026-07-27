using BioCentri.App.Types;

namespace BioCentri.App.Types.Services;

/// <summary>
/// Local-only destination for runtime events that should appear in the
/// Activity page + Dashboard's "Recent activity" timeline. The
/// implementation writes through <c>ILocalJsonStore</c> so all writes
/// stay on-device and follow the same atomic-rename durability
/// contract as the protected-apps and rules persistence layers.
///
/// Fired from two producers today:
///   * <c>BiometricAuthService</c> — every completed challenge.
///   * <c>ProcessWatcher</c>      — every detected protected launch.
/// </summary>
public interface IActivityLogger
{
    /// <summary>Append one event to <c>activity.json</c>. Safe to
    /// call from any thread. Returns when the file write completes
    /// or on best-effort skip if the file is locked.</summary>
    Task LogAsync(ActivityEvent ev, CancellationToken cancellationToken = default);
}
