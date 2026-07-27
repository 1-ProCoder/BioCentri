using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace BioCentri.App.Components.Nav;

/// <summary>
/// Hero strip for a routed feature page. Eyebrow / title / subtitle on the
/// left, optional action area injected into <see cref="TrailingActions"/>
/// on the right (search box, primary button, etc.).
/// </summary>
[ContentProperty(nameof(TrailingActions))]
public partial class PageHeader : StackPanel
{
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title), typeof(string), typeof(PageHeader),
            new PropertyMetadata("Title", OnTitleChanged));

    public static readonly DependencyProperty SubtitleProperty =
        DependencyProperty.Register(
            nameof(Subtitle), typeof(string), typeof(PageHeader),
            new PropertyMetadata("Subtitle", OnSubtitleChanged));

    public static readonly DependencyProperty EyebrowProperty =
        DependencyProperty.Register(
            nameof(Eyebrow), typeof(string), typeof(PageHeader),
            new PropertyMetadata(string.Empty, OnEyebrowChanged));

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

    public string Eyebrow
    {
        get => (string)GetValue(EyebrowProperty);
        set => SetValue(EyebrowProperty, value);
    }

    /// <summary>Right-side action slot (search box, primary button, etc.).
    /// Child XAML placed inside <c>&lt;PageHeader&gt;…&lt;/PageHeader&gt;</c> lands
    /// here; we forward into the inner ContentPresenter.</summary>
    public object? TrailingActions
    {
        get => TrailingActionsHost.Content;
        set => TrailingActionsHost.Content = value;
    }

    public PageHeader()
    {
        InitializeComponent();
    }

    private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PageHeader h && h.TitleText is { } t) t.Text = (string)e.NewValue;
    }

    private static void OnSubtitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PageHeader h && h.SubtitleText is { } t) t.Text = (string)e.NewValue;
    }

    private static void OnEyebrowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PageHeader h && h.EyebrowText is { } t) t.Text = (string)e.NewValue;
    }
}
