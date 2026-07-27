using System;
using System.Text.Json.Serialization;

namespace BioCentri.App.Types;

/// <summary>
/// One row in the activity log. JSON root lives in
/// <c>%LOCALAPPDATA%/BioCentri/activity.json</c> as
/// <c>{ "events": [ ... ] }</c> via <see cref="ActivityLogFile"/>.
/// </summary>
public sealed record ActivityEvent(
    [property: JsonPropertyName("ts")]         DateTimeOffset TimestampUtc,
    [property: JsonPropertyName("severity")]   string Severity,
    [property: JsonPropertyName("app")]        string AppName,
    [property: JsonPropertyName("outcome")]    string Outcome,
    [property: JsonPropertyName("description")] string Description);

/// <summary>JSON root wrapper for <c>activity.json</c>.</summary>
public sealed class ActivityLogFile
{
    [JsonPropertyName("events")]
    public List<ActivityEvent> Events { get; set; } = new();
}
