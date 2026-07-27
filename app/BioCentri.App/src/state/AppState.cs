using CommunityToolkit.Mvvm.ComponentModel;

namespace BioCentri.App.State;

/// <summary>
/// Process-wide app state. Holds facts about the running state of the
/// application that the dashboard, diagnostics, and chrome surfaces
/// read from. Promoted to <see cref="ObservableObject"/> at M3 so
/// reactive consumers (cross-feature notifications, dashboard refresh)
/// can subscribe via INPC. No business logic lives here — the source
/// of truth for protection state, hello challenges, and process
/// tracking is M5+ in <c>BioCentri.Core</c>.
/// </summary>
public sealed partial class AppState : ObservableObject
{
    /// <summary>Build label surfaced in chrome and About.</summary>
    [ObservableProperty]
    private string _buildLabel = "0.2.0+milestone.3";

    /// <summary>Seconds elapsed since the app marked itself ready.</summary>
    [ObservableProperty]
    private double _idleSinceBootSeconds = 0;

    /// <summary>UTC stamp of the most recent shell event (challenge, blocked
    /// launch, audit signal). Drives the dashboard's "last event" glance.</summary>
    [ObservableProperty]
    private DateTimeOffset? _lastEventUtc = null;

    /// <summary>Active theme variant — only "Dark" shipped at v1.</summary>
    [ObservableProperty]
    private string _themeStyle = "Dark";

    /// <summary>Culture name (drives About / Diagnostics copy).</summary>
    [ObservableProperty]
    private string _cultureName = "en-US";

    /// <summary>UTC stamp the app marked itself ready.</summary>
    [ObservableProperty]
    private DateTimeOffset _appReadyAtUtc = DateTimeOffset.UtcNow;

    /// <summary>Friendly machine name surfaced in Diagnostics.</summary>
    [ObservableProperty]
    private string _machineName = Environment.MachineName;

    /// <summary>Processors count surfaced in Diagnostics.</summary>
    [ObservableProperty]
    private int _processorCount = Environment.ProcessorCount;

    /// <summary>Working-set memory in MB surfaced in Diagnostics.</summary>
    [ObservableProperty]
    private long _workingSetMb = GC.GetTotalMemory(false) / 1024 / 1024;
}
