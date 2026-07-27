using System.Windows;
using System.Windows.Controls;

namespace BioCentri.App.Components.Surface;

/// <summary>
/// Single tile in the dashboard's BentoStats grid. Wraps
/// Label / Value / Caption as DependencyProperties so callers can
/// compose them in XAML with plain attribute syntax. Visually lifts
/// on hover via the inner <c>BorderTrace</c>.
/// </summary>
public partial class BentoStat : UserControl
{
    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(
            nameof(Label), typeof(string), typeof(BentoStat),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            nameof(Value), typeof(string), typeof(BentoStat),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty CaptionProperty =
        DependencyProperty.Register(
            nameof(Caption), typeof(string), typeof(BentoStat),
            new PropertyMetadata(string.Empty));

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string Caption
    {
        get => (string)GetValue(CaptionProperty);
        set => SetValue(CaptionProperty, value);
    }

    public BentoStat()
    {
        InitializeComponent();
    }
}
