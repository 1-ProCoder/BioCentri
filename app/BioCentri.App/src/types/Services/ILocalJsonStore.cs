using System.Threading;
using System.Threading.Tasks;

namespace BioCentri.App.Types.Services;

/// <summary>
/// File-based persistence contract. Reads and writes JSON records to
/// <c>%LOCALAPPDATA%\BioCentri\</c> via the concrete
/// <c>BioCentri.App.Services.LocalJsonStore</c>. Async-only — callers
/// MUST be dispatcher-aware when updating WPF-bound
/// <c>ObservableCollection</c>s after a load completes.
///
/// Methods are idempotent: <see cref="LoadAsync{T}"/> returns <c>null</c>
/// when the file does not yet exist (first-launch state); callers seed
/// defaults. <see cref="SaveAsync{T}"/> uses a temp-file + atomic
/// rename to keep the on-disk file consistent under abrupt shutdown.
/// </summary>
public interface ILocalJsonStore
{
    /// <summary>The directory under which all BioCentri data files live.</summary>
    string StorageRoot { get; }

    /// <summary>
    /// Read + deserialise <paramref name="fileName"/> into <typeparamref name="T"/>.
    /// Returns <c>null</c> when the file does not exist (first launch).
    /// </summary>
    Task<T?> LoadAsync<T>(string fileName, CancellationToken cancellationToken = default)
        where T : class;

    /// <summary>
    /// Serialise <paramref name="value"/> to <paramref name="fileName"/> using
    /// a temp-file + atomic rename so a crash mid-write cannot leave a
    /// half-written JSON file on disk.
    /// </summary>
    Task SaveAsync<T>(string fileName, T value, CancellationToken cancellationToken = default)
        where T : class;

    /// <summary>Delete the file if it exists; no-op otherwise.</summary>
    Task DeleteAsync(string fileName, CancellationToken cancellationToken = default);
}
