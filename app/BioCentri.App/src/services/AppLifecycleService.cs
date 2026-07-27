using BioCentri.App.Types.Services;

namespace BioCentri.App.Services;

/// <summary>
/// Skeleton implementation. Each property is a simple <c>bool</c> setter —
/// promotions to <c>ObservableObject</c> happen in Milestone 2 when
/// reactive consumers (e.g. shell navigation) need change notifications.
/// </summary>
public sealed class AppLifecycleService : IAppLifecycleService
{
    public bool SplashShown       { get; set; }
    public bool OnboardingCompleted { get; set; }
    public bool MainWindowShown   { get; set; }
    public bool IsShuttingDown    { get; set; }
}
