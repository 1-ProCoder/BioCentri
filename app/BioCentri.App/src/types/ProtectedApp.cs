using System.Text.Json.Serialization;

namespace BioCentri.App.Types;

/// <summary>
/// A user-protected application — the persistent record saved to
/// <c>%LOCALAPPDATA%\BioCentri\ProtectedApps.json</c> via
/// <c>ILocalJsonStore</c>. <see cref="Path"/> is the primary key.
///
/// Equality (built-in for <c>record</c>) compares all members, which
/// matches the dedupe contract used by the picker (a re-protection
/// attempt with the same <see cref="Path"/> is a no-op).
/// </summary>
public sealed record ProtectedApp(
    [property: JsonPropertyName("displayName")] string DisplayName,
    [property: JsonPropertyName("path")] string Path,
    [property: JsonPropertyName("iconKey")] string IconKey,
    [property: JsonPropertyName("addedUtc")] DateTimeOffset AddedUtc);
