using BioCentri.App.Types;
using BioCentri.App.Types.Services;

namespace BioCentri.App.Services;

/// <summary>
/// Concrete activity logger. Reads the existing <c>activity.json</c>
/// file on every <see cref="LogAsync"/>, appends the new event,
/// then writes the merged list back. Cheap because <see cref="ILocalJsonStore"/>
/// uses atomic-rename +
/// in-memory caching at the ProtectedApps watcher layer pattern.
///
/// Thread-safety: serialised with a per-instance lock so concurrent
/// events from the watcher + auth pipeline don't race the merge.
/// </summary>
public sealed class ActivityLogger : IActivityLogger, IDisposable
{
    private const string StorageFile = "activity.json";
    private readonly ILocalJsonStore _store;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private bool _disposed;

    public ActivityLogger(ILocalJsonStore store)
    {
        ArgumentNullException.ThrowIfNull(store);
        _store = store;
    }

    public async Task LogAsync(ActivityEvent ev, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ev);

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var existing = await _store.LoadAsync<ActivityLogFile>(StorageFile, cancellationToken)
                .ConfigureAwait(false);
            var list = existing?.Events ?? new List<ActivityEvent>();
            list.Add(ev);
            // Bound the log to the 200 most-recent events so the file
            // can never grow unbounded. The Activity and Dashboard
            // pages only render the top 5, so this is a generous
            // buffer for the "Recent" tile + any future history view.
            if (list.Count > 200)
                list = list.OrderByDescending(e => e.TimestampUtc).Take(200).ToList();

            await _store.SaveAsync(StorageFile,
                new ActivityLogFile { Events = list }, cancellationToken)
                .ConfigureAwait(false);
        }
        catch
        {
            /* best-effort: a logged activity event is informational;
               failing to persist it must not crash the auth pipeline. */
        }
        finally
        {
            if (!_disposed) _gate.Release();
        }
    }

    /// <summary>Releases the cross-thread <see cref="SemaphoreSlim"/>
    /// used to serialise concurrent log events. ServiceHost owns the
    /// lifetime — called from DI teardown if the host exposes a
    /// dispose path, or relied on for process exit.</summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _gate.Dispose();
    }
}
