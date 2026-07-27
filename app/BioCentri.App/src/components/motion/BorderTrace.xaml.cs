using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace BioCentri.App.Components.Motion;

/// <summary>
/// Border whose outline "traces" on hover — the hairline border
/// brightens to the indigo accent and gains +1 thickness, then fades
/// back. Applied to each BentoStat tile for "lift that responds to
/// interest" polish.
///
/// Reduced-motion: if <c>Motion.RespectReducedMotion</c> is true, the
/// trace is collapsed to a static hover state (no animation) so the
/// border simply toggles between resting and active brushes.
/// </summary>
public partial class BorderTrace : UserControl
{
    private Color _restingColor;
    private Color _activeColor;

    public BorderTrace()
    {
        InitializeComponent();
        CaptureBrushColors();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Host.MouseEnter += OnHostMouseEnter;
        Host.MouseLeave += OnHostMouseLeave;
    }

    private void CaptureBrushColors()
    {
        if (Application.Current?.TryFindResource("Brushes.Border.Hairline") is SolidColorBrush resting)
            _restingColor = resting.Color;
        else
            _restingColor = Color.FromRgb(0x33, 0x33, 0x33);

        if (Application.Current?.TryFindResource("Brushes.Accent.Indigo") is SolidColorBrush active)
            _activeColor = active.Color;
        else
            _activeColor = Color.FromRgb(0x81, 0x8C, 0xF8);
    }

    private void OnHostMouseEnter(object sender, MouseEventArgs e)
    {
        AnimateBorder(active: true);
    }

    private void OnHostMouseLeave(object sender, MouseEventArgs e)
    {
        AnimateBorder(active: false);
    }

    private void AnimateBorder(bool active)
    {
        var targetColor = active ? _activeColor : _restingColor;
        var targetThickness = active ? 2.0 : 1.0;

        if (!ShouldAnimate())
        {
            Host.BorderBrush = new SolidColorBrush(targetColor);
            Host.BorderThickness = new Thickness(targetThickness);
            return;
        }

        var colorAnim = new ColorAnimation
        {
            To = targetColor,
            Duration = TimeSpan.FromMilliseconds(220),
        };
        var brush = new SolidColorBrush(_restingColor);
        Host.BorderBrush = brush;
        brush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);

        var thicknessAnim = new DoubleAnimation
        {
            To = targetThickness,
            Duration = TimeSpan.FromMilliseconds(220),
        };
        Host.BeginAnimation(Border.BorderThicknessProperty, thicknessAnim);
    }

    private static bool ShouldAnimate()
    {
        var resource = Application.Current?.TryFindResource("Motion.RespectReducedMotion");
        if (resource is bool b) return !b;
        return true;
    }
}
