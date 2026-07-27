namespace BioCentri.App.Types.Services;

/// <summary>
/// App-wide lifecycle state. Splash shown → onboarded → main window shown
/// → shutting down. Implemented as a simple mutable service in Milestone 1;
/// promoted to an <c>ObservableObject</c> in Milestone 2 when reactive
/// consumers (e.g. shell navigation) need change notifications.
///
/// Setter accessor is exposed here intentionally: the only writer at M1 is
/// <c>App.xaml.cs</c>, and casting to the concrete class for writes is a
/// leak. When promote-to-<c>ObservableObject</c> happens at M2, replace
/// each public setter with a fenced <c>SetProperty(ref …)</c> call.
/// </summary>
public interface IAppLifecycleService
{
    bool SplashShown { get; set; }

    bool OnboardingCompleted { get; set; }

    bool MainWindowShown { get; set; }

    bool IsShuttingDown { get; set; }
}
