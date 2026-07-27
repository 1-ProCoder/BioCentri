using System;
using System.Collections.Generic;
using BioCentri.App.Components.Feedback;
using BioCentri.App.Features.Rules;

namespace BioCentri.App.Models;

/// <summary>
/// One user-protected desktop application. <see cref="ExecutablePath"/> is
/// the full path to the .exe the user picked (used by the future Win32
/// watcher in Milestone 6). <see cref="IsProtected"/> is the toggle
/// state — true means launch attempts should be challenged.
/// </summary>
public sealed record ProtectedAppEntry
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string DisplayName { get; init; } = string.Empty;
    public string ExecutablePath { get; init; } = string.Empty;
    public string IconKey { get; init; } = "Icons.Brand.Placeholder";
    public bool IsProtected { get; set; } = true;
    public DateTimeOffset AddedAtUtc { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastSeenUtc { get; set; }
}

/// <summary>
/// One automation rule. The MVP wires enable/disable + free-form
/// name/description; the schedule / time-window fields land at Milestone 7.
/// </summary>
public sealed record RuleEntry
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RuleStatus Status { get; set; } = RuleStatus.Draft;
    public bool IsEnabled { get; set; } = true;
    public string IconKey { get; init; } = "Icons.Route.Rules";
    public DateTimeOffset CreatedAtUtc { get; init; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// One timeline event in the activity log. The display grouping
/// (Today / Yesterday / Earlier) is derived at render time, not stored.
/// </summary>
public sealed record ActivityEventEntry
{
    public DateTimeOffset TimestampUtc { get; init; } = DateTimeOffset.UtcNow;
    public string Title { get; init; } = string.Empty;
    public string Detail { get; init; } = string.Empty;
    public TimelineSeverity Severity { get; init; } = TimelineSeverity.Info;
}

/// <summary>
/// Roll-up container for the activity log. Bounded to a rolling window
/// (default 500 entries) to keep disk usage flat over a long-running
/// session. Oldest entries are pruned on save.
/// </summary>
public sealed record ActivityLogFile
{
    public List<ActivityEventEntry> Events { get; init; } = new();
    public const int RollingWindow = 500;
}

/// <summary>
/// Flat snapshot of every setting the user has customised. Stored as
/// one row in <c>settings.json</c>. Each property's default mirrors
/// the conservative MVP defaults declared in <c>SettingsViewModel</c>.
/// </summary>
public sealed record SettingsState
{
    public string Theme { get; init; } = "Dark";
    public string Accent { get; init; } = "Indigo";
    public string Density { get; init; } = "Comfortable";
    public bool ReducedMotion { get; init; }
    public bool HighContrast { get; init; }
    public bool KeyboardFocusRings { get; init; } = true;
    public bool LocalFirstNetworkGuard { get; init; } = true;
    public bool TelemetryOptOut { get; init; } = true;
    public string Retention { get; init; } = "90 days";
    public bool LaunchOnBoot { get; init; } = true;
    public bool MinimizeToTray { get; init; } = true;
    public bool SingleInstance { get; init; } = true;
    public string UpdateChannel { get; init; } = "Stable";
    public bool AutoDownload { get; init; }
}
