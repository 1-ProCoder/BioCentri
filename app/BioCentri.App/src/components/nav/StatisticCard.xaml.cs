using System.Windows;
using System.Windows.Controls;

namespace BioCentri.App.Components.Nav;

/// <summary>
/// Single statistic row used on the Dashboard. Label above, value
/// (large display type), optional subtle delta caption.
/// </summary>
public partial class StatisticCard : Border
{
    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(
            nameof(Label), typeof(string), typeof(StatisticCard),
            new PropertyMetadata("Label", OnLabelChanged));

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            nameof(Value), typeof(string), typeof(StatisticCard),
            new PropertyMetadata("—", OnValueChanged));

    public static readonly DependencyProperty DeltaProperty =
        DependencyProperty.Register(
            nameof(Delta), typeof(string), typeof(StatisticCard),
            new PropertyMetadata(string.Empty, OnDeltaChanged));

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

    public string Delta
    {
        get => (string)GetValue(DeltaProperty);
        set => SetValue(DeltaProperty, value);
    }

    public StatisticCard()
    {
        InitializeComponent();
    }

    private static void OnLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StatisticCard c && c.LabelText is { } t) t.Text = (string)e.NewValue;
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StatisticCard c && c.ValueText is { } t) t.Text = (string)e.NewValue;
    }

    private static void OnDeltaChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StatisticCard c && c.DeltaText is { } t)
        {
            var value = (string)e.NewValue;
            t.Text = value;
            t.Visibility = string.IsNullOrEmpty(value) ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
