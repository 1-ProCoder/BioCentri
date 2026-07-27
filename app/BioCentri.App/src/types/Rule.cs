using System;
using System.Text.Json.Serialization;

namespace BioCentri.App.Types;

/// <summary>
/// One automation rule. JSON root lives in
/// <c>%LOCALAPPDATA%/BioCentri/rules.json</c>.
/// Phase-1 MVP scope: user can create, edit, enable/disable, and
/// delete rules. Time-window enforcement (per
/// <c>FEATURE_ROADMAP.md</c> Phase 2) is not yet wired — the rule
/// is just a stored intent today.
/// </summary>
public sealed record Rule(
    [property: JsonPropertyName("id")]          Guid Id,
    [property: JsonPropertyName("name")]        string Name,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("trigger")]     string TriggerText,
    [property: JsonPropertyName("isEnabled")]   bool IsEnabled,
    [property: JsonPropertyName("createdUtc")]  DateTimeOffset CreatedUtc);

/// <summary>JSON root wrapper for <c>rules.json</c>.</summary>
public sealed class RulesFile
{
    [JsonPropertyName("rules")]
    public List<Rule> Rules { get; set; } = new();
}
