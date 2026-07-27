using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace BioCentri.App.Components.Motion;

/// <summary>
/// Wraps a single content child in a gentle vertical "float" — the
/// child bobs between Y=0 and Y=-6 over 6 s, autoreversing forever.
/// Used on the dashboard hero logo to surface the security/identity
/// aesthetic without inviting user interaction.
///
/// Reduced-motion: if <c>Motion.RespectReducedMotion</c> is true at
/// load time, the float is suppressed. (M7 will wire that resource to
/// <c>SystemParameters</c>; for M3 the resource ships as a default-true
/// opt-in so the visual works out of the box.)
/// </summary>
public partial class HologramFloat : UserControl
{
    private Storyboard? _floatStoryboard;

    public HologramFloat()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (!ShouldAnimate()) return;
        BeginFloat();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _floatStoryboard?.Stop();
        _floatStoryboard = null;
    }

    private void BeginFloat()
    {
        Body.RenderTransform = new TranslateTransform(0, 0);
        var anim = new DoubleAnimationUsingKeyFrames
        {
            Duration = TimeSpan.FromMilliseconds(6000),
            RepeatBehavior = RepeatBehavior.Forever,
            AutoReverse = true,
        };
        // 0 -> -6 -> 0 with an out-expo spline (Decision 9 followup:
        // easings stay C# because baml refuses SplineEase).
        anim.KeyFrames.Add(new SplineDoubleKeyFrame(
            0, KeyTime.FromTimeSpan(TimeSpan.Zero),
            new KeySpline(0.45, 0, 0.55, 1)));
        anim.KeyFrames.Add(new SplineDoubleKeyFrame(
            -6, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(3000)),
            new KeySpline(0.45, 0, 0.55, 1)));

        Storyboard.SetTarget(anim, (TranslateTransform)Body.RenderTransform);
        Storyboard.SetTargetProperty(anim, new PropertyPath(TranslateTransform.YProperty));

        _floatStoryboard = new Storyboard();
        _floatStoryboard.Children.Add(anim);
        _floatStoryboard.Begin();
    }

    private static bool ShouldAnimate()
    {
        // Consult the design-system flag. Default true so the float ships.
        var resource = Application.Current?.TryFindResource("Motion.RespectReducedMotion");
        if (resource is bool b) return !b;
        return true;
    }
}
