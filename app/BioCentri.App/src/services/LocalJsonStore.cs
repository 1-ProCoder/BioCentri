using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using BioCentri.App.Types.Services;

namespace BioCentri.App.Services;

/// <summary>
/// Local JSON-file persistence backed by <c>%LOCALAPPDATA%\BioCentri\</c>.
/// One concrete file per logical domain (settings, protected-apps, rules,
/// activity). Reads return <c>null</c> on first-launch; writes use a
/// temp-file + atomic rename so a crash mid-write cannot leave a
/// half-written JSON file on disk.
///
/// MVP choice rationale (see docs/DECISIONS.md Decision 12):
///   * <see cref="System.Text.Json"/> is in-box on .NET 8, no NuGet.
///   * Async-only matches WPF's UI-thread dispatcher model — callers
///     await and update ObservableCollections on the UI thread.
///   * Atomic rename guarantees a consistent on-disk file even on
///     abrupt shutdown (matching the durability posture of Win32
///     write-through APIs).
/// </summary>
public sealed class LocalJsonStore : ILocalJsonStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly string _root;

    public LocalJsonStore()
    {
        _root = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "BioCentri");
        Directory.CreateDirectory(_root);
    }

    /// <inheritdoc />
    public string StorageRoot => _root;

    /// <inheritdoc />
    public async Task<T?> LoadAsync<T>(string fileName, CancellationToken cancellationToken = default)
        where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        var path = ResolvePath(fileName);
        if (!File.Exists(path)) return null;

        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task SaveAsync<T>(string fileName, T value, CancellationToken cancellationToken = default)
        where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentNullException.ThrowIfNull(value);

        var path = ResolvePath(fileName);
        var temp = path + ".tmp";

        await using (var stream = File.Create(temp))
        {
            await JsonSerializer.SerializeAsync(stream, value, JsonOptions, cancellationToken)
                .ConfigureAwait(false);
            await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        // Atomic replace. On Windows, File.Move with overwrite is rename
        // when both paths are on the same volume (which they always are
        // here — the temp file lives next to the target).
        File.Move(temp, path, overwrite: true);
    }

    /// <inheritdoc />
    public Task DeleteAsync(string fileName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        var path = ResolvePath(fileName);
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }

    private string ResolvePath(string fileName)
    {
        // Reject path traversal — fileName must be a bare name, not a
        // relative or absolute path. StorageRoot is the only valid
        // destination for BioCentri data.
        if (fileName.Contains('/') || fileName.Contains('\\') || fileName.Contains(".."))
            throw new ArgumentException(
                $"fileName must be a bare name (no path separators); got '{fileName}'.",
                nameof(fileName));
        return Path.Combine(_root, fileName);
    }
}
