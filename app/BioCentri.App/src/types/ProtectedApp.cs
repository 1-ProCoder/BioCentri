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
///
/// <see cref="IsEnabled"/> is the per-row "gate state" toggle (M7+);
/// defaults to <c>true</c> so legacy <c>protectedApps.json</c> files
/// that pre-date the toggle keep their apps gated on first launch.
/// </summary>
public sealed record ProtectedApp(
    [property: JsonPropertyName("displayName")] string DisplayName,
    [property: JsonPropertyName("path")] string Path,
    [property: JsonPropertyName("iconKey")] string IconKey,
    [property: JsonPropertyName("addedUtc")] DateTimeOffset AddedUtc)
{
    /// <summary>"Gate State" toggle in Protected Apps row. Defaults to
    /// <c>true</c> so JSON files missing this field still gate apps.
    /// <para>
    /// Must be <c>set</c> (not <c>init</c>) so the per-row
    /// <see cref="System.Windows.Controls.Primitives.ToggleButton"/>
    /// TwoWay binding can write through. Equality compares <see cref="Path"/>
    /// semantically (the record's primary key), so toggling this flag
    /// doesn't break dedupe / re-protection logic.
    /// </para></summary>
    [JsonPropertyName("isEnabled")]
    public bool IsEnabled { get; set; } = true;
}
