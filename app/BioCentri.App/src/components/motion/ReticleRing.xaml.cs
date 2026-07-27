using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace BioCentri.App.Components.Motion;

/// <summary>
/// Decorative target-reticle ornament with a slow continuous rotation.
/// Sits behind the BioCentri logo on the dashboard hero. Reads as an
/// "always-on biometric scan" silhouette without inviting interaction.
///
/// Reduced-motion: if <c>Motion.RespectReducedMotion</c> is true at
/// load time, the rotation is suppressed and the reticle renders as a
/// static ornament (correct M7 accessibility posture).
/// </summary>
public partial class ReticleRing : UserControl
{
    private Storyboard? _rotationStoryboard;

    public ReticleRing()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (!ShouldAnimate()) return;
        BeginRotation();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _rotationStoryboard?.Stop();
        _rotationStoryboard = null;
    }

    private void BeginRotation()
    {
        // Motion.Duration.Reticle = 12000 ms
        var anim = new DoubleAnimation
        {
            From = 0,
            To = 360,
            Duration = TimeSpan.FromMilliseconds(12000),
            RepeatBehavior = RepeatBehavior.Forever,
        };

        Storyboard.SetTarget(anim, RingRotate);
        Storyboard.SetTargetProperty(anim, new PropertyPath(RotateTransform.AngleProperty));

        _rotationStoryboard = new Storyboard();
        _rotationStoryboard.Children.Add(anim);
        _rotationStoryboard.Begin();
    }

    private static bool ShouldAnimate()
    {
        var resource = Application.Current?.TryFindResource("Motion.RespectReducedMotion");
        if (resource is bool b) return !b;
        return true;
    }
}
