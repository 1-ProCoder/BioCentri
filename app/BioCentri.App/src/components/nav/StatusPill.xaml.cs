using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BioCentri.App.Components.Nav;

/// <summary>
/// Compact "pill" surface. Used in the TopBar for system status and in
/// toast summaries. Status drives the dot colour.
/// </summary>
public partial class StatusPill : Border
{
    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(
            nameof(Label), typeof(string), typeof(StatusPill),
            new PropertyMetadata("Status", OnLabelChanged));

    public static readonly DependencyProperty StatusProperty =
        DependencyProperty.Register(
            nameof(Status), typeof(StatusPillKind), typeof(StatusPill),
            new PropertyMetadata(StatusPillKind.Active, OnStatusChanged));

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public StatusPillKind Status
    {
        get => (StatusPillKind)GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    public StatusPill()
    {
        InitializeComponent();
    }

    private static void OnLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StatusPill pill && pill.LabelText is { } label) label.Text = (string)e.NewValue;
    }

    private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StatusPill pill) pill.ApplyStatus((StatusPillKind)e.NewValue);
    }

    private void ApplyStatus(StatusPillKind kind)
    {
        var resource = kind switch
        {
            StatusPillKind.Warning => "Brushes.Status.Warn",
            StatusPillKind.Danger  => "Brushes.Status.Danger",
            StatusPillKind.Idle    => "Brushes.Ink.500",
            _                       => "Brushes.Status.Success",
        };
        Dot.Fill = (Brush)FindResource(resource);
    }
}

public enum StatusPillKind
{
    Active,
    Idle,
    Warning,
    Danger,
}
