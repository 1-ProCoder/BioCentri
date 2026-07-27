using System.Windows;
using System.Windows.Controls;
using BioCentri.App.State;

namespace BioCentri.App.Components.Nav;

/// <summary>
/// Top-of-window header. Shows the current route title and a global
/// status pill. Bound to <see cref="ShellState"/> so every nav event
/// updates the title automatically.
/// </summary>
public partial class TopBar : Grid
{
    public static readonly DependencyProperty ShellStateProperty =
        DependencyProperty.Register(
            nameof(ShellState), typeof(ShellState), typeof(TopBar),
            new PropertyMetadata(null, OnShellStateChanged));

    public ShellState? ShellState
    {
        get => (ShellState?)GetValue(ShellStateProperty);
        set => SetValue(ShellStateProperty, value);
    }

    public TopBar()
    {
        InitializeComponent();
    }

    private static void OnShellStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TopBar b && e.NewValue is ShellState s)
        {
            b.TitleText.SetBinding(TextBlock.TextProperty,
                new System.Windows.Data.Binding("CurrentTitle") { Source = s });
        }
    }
}
