using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace BioCentri.App.Components.Nav;

/// <summary>
/// One row in a list with an icon, title, optional subtitle, and an
/// optional trailing action area (e.g. a chevron or a status pill).
/// </summary>
[ContentProperty("TrailingActions")]
public partial class ListTile : Border
{
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title), typeof(string), typeof(ListTile),
            new PropertyMetadata("Title", OnTitleChanged));

    public static readonly DependencyProperty SubtitleProperty =
        DependencyProperty.Register(
            nameof(Subtitle), typeof(string), typeof(ListTile),
            new PropertyMetadata(string.Empty, OnSubtitleChanged));

    public static readonly DependencyProperty IconKeyProperty =
        DependencyProperty.Register(
            nameof(IconKey), typeof(string), typeof(ListTile),
            new PropertyMetadata("Icons.Brand.Placeholder", OnIconKeyChanged));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Subtitle
    {
        get => (string)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    public string IconKey
    {
        get => (string)GetValue(IconKeyProperty);
        set => SetValue(IconKeyProperty, value);
    }

    public ListTile()
    {
        InitializeComponent();
    }

    private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ListTile t && t.TitleText is { } x) x.Text = (string)e.NewValue;
    }

    private static void OnSubtitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ListTile t && t.SubtitleText is { } x)
        {
            var value = (string)e.NewValue;
            x.Text = value;
            x.Visibility = string.IsNullOrEmpty(value) ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    private static void OnIconKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ListTile t && t.LeadingIcon is { } icon) icon.GeometryKey = (string)e.NewValue;
    }
}
