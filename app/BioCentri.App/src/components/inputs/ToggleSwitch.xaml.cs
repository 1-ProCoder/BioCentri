using System.Windows;
using System.Windows.Controls;

namespace BioCentri.App.Components.Inputs;

/// <summary>
/// iOS-style toggle switch. The host <see cref="Border"/> exposes a
/// labeled description on the left and a <see cref="System.Windows.Controls.Primitives.ToggleButton"/>
/// on the right. The toggle button's own TwoWay binding drives
/// <see cref="IsChecked"/>; we do NOT add a click handler on the
/// Border because <c>PreviewMouseLeftButtonDown</c> would cause a
/// double-toggle when the user clicks the thumb directly. Pointer
/// activation of the row is delegated to focus-forwarding on the
/// inner ToggleButton (Tab will hit it; Space/Enter activates; the
/// visual track is also the click target).
/// </summary>
public partial class ToggleSwitch : Border
{
    public static readonly DependencyProperty IsCheckedProperty =
        DependencyProperty.Register(
            nameof(IsChecked), typeof(bool), typeof(ToggleSwitch),
            new FrameworkPropertyMetadata(false,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(
            nameof(Label), typeof(string), typeof(ToggleSwitch),
            new PropertyMetadata("Setting", OnLabelChanged));

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(
            nameof(Description), typeof(string), typeof(ToggleSwitch),
            new PropertyMetadata(string.Empty, OnDescriptionChanged));

    public bool IsChecked
    {
        get => (bool)GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public ToggleSwitch()
    {
        InitializeComponent();
        // Click semantics are delegated to the embedded ToggleButton
        // (`Switch`), whose `IsChecked` is TwoWay-bound to this Border's
        // `IsChecked` DP. We deliberately do NOT add a click handler on
        // the Border — that would cause a double-toggle when the user
        // clicks the thumb directly.
    }

    private static void OnLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ToggleSwitch s && s.LabelText is { } t) t.Text = (string)e.NewValue;
    }

    private static void OnDescriptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ToggleSwitch s && s.DescriptionText is { } t)
        {
            var v = (string)e.NewValue;
            t.Text = v;
            t.Visibility = string.IsNullOrWhiteSpace(v) ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
