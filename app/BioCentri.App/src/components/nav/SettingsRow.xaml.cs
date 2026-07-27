using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace BioCentri.App.Components.Nav;

/// <summary>
/// Standard "row" inside a settings list: leading icon, title,
/// description, and a trailing control area. Trailing area accepts
/// CheckBox, ComboBox, Toggle, etc. via the XAML property element.
/// </summary>
[ContentProperty("TrailingAction")]
public partial class SettingsRow : Border
{
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title), typeof(string), typeof(SettingsRow),
            new PropertyMetadata("Setting", OnTitleChanged));

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(
            nameof(Description), typeof(string), typeof(SettingsRow),
            new PropertyMetadata("Description", OnDescriptionChanged));

    public static readonly DependencyProperty IconKeyProperty =
        DependencyProperty.Register(
            nameof(IconKey), typeof(string), typeof(SettingsRow),
            new PropertyMetadata("Icons.Route.Settings", OnIconKeyChanged));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public string IconKey
    {
        get => (string)GetValue(IconKeyProperty);
        set => SetValue(IconKeyProperty, value);
    }

    public SettingsRow()
    {
        InitializeComponent();
    }

    private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SettingsRow r && r.TitleText is { } x) x.Text = (string)e.NewValue;
    }

    private static void OnDescriptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SettingsRow r && r.DescriptionText is { } x) x.Text = (string)e.NewValue;
    }

    private static void OnIconKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SettingsRow r && r.LeadingIcon is { } icon) icon.GeometryKey = (string)e.NewValue;
    }
}
