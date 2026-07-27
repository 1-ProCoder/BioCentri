using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BioCentri.App.Components.Inputs;

/// <summary>
/// Pill-shaped filter chip used in lists (Protected Apps, Activity
/// filters, etc.). Two-way bindable <see cref="IsChecked"/> + a
/// bindable <see cref="Label"/>. Visual treatment (gradient fill
/// when selected) is declared in <c>FilterChip.xaml</c> as a
/// DataTrigger; this code-behind only forwards DP changes into the
/// visible label and translates a click on the host into a toggle.
/// </summary>
public partial class FilterChip : Border
{
    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(
            nameof(Label), typeof(string), typeof(FilterChip),
            new PropertyMetadata("Chip", OnLabelChanged));

    public static readonly DependencyProperty IsCheckedProperty =
        DependencyProperty.Register(
            nameof(IsChecked), typeof(bool), typeof(FilterChip),
            new FrameworkPropertyMetadata(false,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public bool IsChecked
    {
        get => (bool)GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    public FilterChip()
    {
        InitializeComponent();
        MouseLeftButtonUp += OnClicked;
    }

    private void OnClicked(object sender, MouseButtonEventArgs e)
    {
        IsChecked = !IsChecked;
    }

    private static void OnLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FilterChip c && c.LabelText is { } t) t.Text = (string)e.NewValue;
    }
}
