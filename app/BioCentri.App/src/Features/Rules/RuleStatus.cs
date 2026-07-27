namespace BioCentri.App.Features.Rules;

/// <summary>
/// Lifecycle state of an automation rule. M2 placeholder per
/// IMPLEMENTATION_PLAN §7 — Milestone 4 ships the actual scheduling
/// and runtime evaluation; today the enum exists so the persisted
/// <see cref="BioCentri.App.Models.RuleEntry"/> can round-trip through
/// <c>System.Text.Json</c> without losing its lifecycle field.
/// </summary>
public enum RuleStatus
{
    /// <summary>Authored but never run. Default for new entries.</summary>
    Draft,

    /// <summary>Currently evaluated.</summary>
    Active,

    /// <summary>Authored and previously Active, but muted by the user.</summary>
    Paused,
}
