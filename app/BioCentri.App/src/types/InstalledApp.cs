using System.Text.Json.Serialization;

namespace BioCentri.App.Types;

/// <summary>
/// A discovered installed application on the user's machine. Surfaced
/// transparently to the picker overlay; never persisted (only
/// <see cref="ProtectedApp"/> is). Equality is by <see cref="Path"/>
/// so the picker can dedupe via
/// <c>protectedPaths.Contains(app.Path)</c> without a second key.
///
/// Discovery source (HKLM Uninstall, HKCU Uninstall, etc.) is kept
/// off this model on purpose: caller doesn't care, and we may add
/// Start-menu + MSI enumeration later without changing the model.
/// </summary>
public sealed record InstalledApp(
    [property: JsonPropertyName("displayName")] string DisplayName,
    [property: JsonPropertyName("path")] string Path,
    [property: JsonPropertyName("publisher")] string? Publisher,
    [property: JsonPropertyName("iconKey")] string IconKey);
