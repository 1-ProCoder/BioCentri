using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace BioCentri.App.Components.Iconography;

/// <summary>
/// Vector-icon surface that resolves a geometry key from the app's
/// <c>Icons.xaml</c> dictionary. Consumers set <see cref="GeometryKey"/>
/// in XAML or bind it; the icon then renders the corresponding path
/// at the requested size (defaulting to <c>Icons.Size.Default</c>).
/// </summary>
public partial class Icon : UserControl
{
    public static readonly DependencyProperty GeometryKeyProperty =
        DependencyProperty.Register(
            nameof(GeometryKey), typeof(string), typeof(Icon),
            new PropertyMetadata(null, OnGeometryKeyChanged));

    public static readonly DependencyProperty IconSizeProperty =
        DependencyProperty.Register(
            nameof(IconSize), typeof(double), typeof(Icon),
            new PropertyMetadata(20.0, OnIconSizeChanged));

    public static readonly DependencyProperty StrokeProperty =
        DependencyProperty.Register(
            nameof(Stroke), typeof(Brush), typeof(Icon),
            new PropertyMetadata(Brushes.Transparent));

    public static readonly DependencyProperty FillProperty =
        DependencyProperty.Register(
            nameof(Fill), typeof(Brush), typeof(Icon),
            new PropertyMetadata(null));

    public static readonly DependencyProperty StrokeThicknessProperty =
        DependencyProperty.Register(
            nameof(StrokeThickness), typeof(double), typeof(Icon),
            new PropertyMetadata(1.5, OnStrokeThicknessChanged));

    /// <summary>Resource key into the Icons.xaml dictionary (e.g. "Icons.Route.Dashboard").</summary>
    public string? GeometryKey
    {
        get => (string?)GetValue(GeometryKeyProperty);
        set => SetValue(GeometryKeyProperty, value);
    }

    /// <summary>Edge length of the square icon, in DIPs.</summary>
    public double IconSize
    {
        get => (double)GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    /// <summary>Optional stroke color override. Default: theme Text.Primary.</summary>
    public Brush Stroke
    {
        get => (Brush)GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }

    /// <summary>Optional fill color override. Default: transparent.</summary>
    public Brush Fill
    {
        get => (Brush)GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }

    /// <summary>Stroke width applied to the vector path. Default: 1.5.</summary>
    public double StrokeThickness
    {
        get => (double)GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    /// <summary>Resolved geometry, exposed for binding to the inner path.</summary>
    public Geometry? Geometry
    {
        get
        {
            var key = GeometryKey;
            return key is null ? null : (Geometry?)TryFindResource(key);
        }
    }

    public Icon()
    {
        InitializeComponent();
        IsHitTestVisible = false;
        Loaded += (_, _) => ApplySize();
    }

    private static void OnGeometryKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Icon icon)
        {
            icon.Root.SetBinding(FrameworkElement.DataContextProperty,
                new System.Windows.Data.Binding("Geometry") { Source = icon });
        }
    }

    private static void OnIconSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Icon icon) icon.ApplySize();
    }

    private static void OnStrokeThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Icon icon && icon.IconPath is { } p)
        {
            p.StrokeThickness = (double)e.NewValue;
        }
    }

    private void ApplySize()
    {
        Width = IconSize;
        Height = IconSize;
    }
}
