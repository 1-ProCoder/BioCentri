using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BioCentri.App.Components.Feedback;

/// <summary>
/// One entry on the Activity timeline. Bound from
/// <c>ActivityTimelineEntry</c> records. Visual treatment: a
/// 14-pixel dot anchored on a vertical rail; the dot colour reflects
/// <see cref="Severity"/>; the card to the right of the rail shows
/// the title, severity badge, timestamp, and detail line.
/// </summary>
public partial class TimelineEntry : Grid
{
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title), typeof(string), typeof(TimelineEntry),
            new PropertyMetadata("Entry", OnTitleChanged));

    public static readonly DependencyProperty DetailProperty =
        DependencyProperty.Register(
            nameof(Detail), typeof(string), typeof(TimelineEntry),
            new PropertyMetadata(string.Empty, OnDetailChanged));

    public static readonly DependencyProperty TimestampProperty =
        DependencyProperty.Register(
            nameof(Timestamp), typeof(string), typeof(TimelineEntry),
            new PropertyMetadata(string.Empty, OnTimestampChanged));

    public static readonly DependencyProperty SeverityProperty =
        DependencyProperty.Register(
            nameof(Severity), typeof(TimelineSeverity), typeof(TimelineEntry),
            new PropertyMetadata(TimelineSeverity.Info, OnSeverityChanged));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Detail
    {
        get => (string)GetValue(DetailProperty);
        set => SetValue(DetailProperty, value);
    }

    public string Timestamp
    {
        get => (string)GetValue(TimestampProperty);
        set => SetValue(TimestampProperty, value);
    }

    public TimelineSeverity Severity
    {
        get => (TimelineSeverity)GetValue(SeverityProperty);
        set => SetValue(SeverityProperty, value);
    }

    public TimelineEntry()
    {
        InitializeComponent();
    }

    private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TimelineEntry t && t.TitleText is { } x) x.Text = (string)e.NewValue;
    }

    private static void OnDetailChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TimelineEntry t && t.DetailText is { } x) x.Text = (string)e.NewValue;
    }

    private static void OnTimestampChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TimelineEntry t && t.TimestampText is { } x) x.Text = (string)e.NewValue;
    }

    private static void OnSeverityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TimelineEntry entry) return;
        var sev = (TimelineSeverity)e.NewValue;
        var dot = sev switch
        {
            TimelineSeverity.Success => (Brush)(entry.TryFindResource("Brushes.Status.Success") ?? Brushes.LimeGreen),
            TimelineSeverity.Warning => (Brush)(entry.TryFindResource("Brushes.Status.Warn") ?? Brushes.Goldenrod),
            TimelineSeverity.Danger  => (Brush)(entry.TryFindResource("Brushes.Status.Danger") ?? Brushes.IndianRed),
            _                        => (Brush)(entry.TryFindResource("Brushes.Accent.Indigo") ?? Brushes.MediumSlateBlue),
        };
        entry.Dot.Background = dot;
        entry.SeverityBadge.Background = (Brush)(entry.TryFindResource("Brushes.Subtle.Surface") ?? new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)));
        entry.SeverityText.Text = sev.ToString().ToUpperInvariant();
        var badgeColor = sev switch
        {
            TimelineSeverity.Success => (Brush)(entry.TryFindResource("Brushes.Status.Success") ?? Brushes.LimeGreen),
            TimelineSeverity.Warning => (Brush)(entry.TryFindResource("Brushes.Status.Warn") ?? Brushes.Goldenrod),
            TimelineSeverity.Danger  => (Brush)(entry.TryFindResource("Brushes.Status.Danger") ?? Brushes.IndianRed),
            _                        => (Brush)(entry.TryFindResource("Brushes.Accent.IndigoLight") ?? Brushes.Lavender),
        };
        entry.SeverityText.Foreground = badgeColor;
    }
}

/// <summary>Severity tiers for a <see cref="TimelineEntry"/>.</summary>
public enum TimelineSeverity
{
    Info,
    Success,
    Warning,
    Danger,
}
