using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BioCentri.App.Components.Inputs;

/// <summary>
/// Pill-shaped segmented control. Bound <see cref="ItemsSource"/>
/// drives the labels; <see cref="SelectedIndex"/> (two-way) is the
/// canonical selection state. Visual treatment (selected segment
/// gets an Indigo pill background + white text) is computed in
/// code-behind after layout so the inner TextBlocks can be re-
/// styled on every selection change.
/// </summary>
public partial class SegmentedControl : Border
{
    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(
            nameof(ItemsSource), typeof(IEnumerable), typeof(SegmentedControl),
            new PropertyMetadata(null, OnItemsSourceChanged));

    public static readonly DependencyProperty SelectedIndexProperty =
        DependencyProperty.Register(
            nameof(SelectedIndex), typeof(int), typeof(SegmentedControl),
            new FrameworkPropertyMetadata(0,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedIndexChanged));

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    public SegmentedControl()
    {
        InitializeComponent();
        Loaded += (_, _) => ApplySelectionVisual();
    }

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SegmentedControl c)
        {
            // (IEnumerable)e.NewValue — explicit cast per CS0266 fix.
            c.ItemsHost.ItemsSource = (IEnumerable?)e.NewValue;
            c.Loaded -= OnSegmentLoadedOnceApplied; // ensure single-shot hook
            c.Loaded += OnSegmentLoadedOnceApplied;
        }
    }

    private static void OnSegmentLoadedOnceApplied(object? sender, RoutedEventArgs e)
    {
        if (sender is SegmentedControl c)
        {
            c.Loaded -= OnSegmentLoadedOnceApplied;
            c.ApplySelectionVisual();
        }
    }

    private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SegmentedControl c) c.ApplySelectionVisual();
    }

    private void ApplySelectionVisual()
    {
        if (ItemsHost is null) return;
        for (int i = 0; i < ItemsHost.Items.Count; i++)
        {
            if (ItemsHost.ItemContainerGenerator.ContainerFromIndex(i) is not ContentPresenter cp) continue;

            var fe = VisualTreeHelper.GetChild(cp, 0) as FrameworkElement;
            while (fe is ContentPresenter p && VisualTreeHelper.GetChild(p, 0) is FrameworkElement inner)
                fe = inner;

            if (fe is TextBlock tb)
            {
                if (i == SelectedIndex)
                {
                    tb.Background = (Brush)TryFindResource("Brushes.Accent.Indigo")!;
                    tb.Foreground = Brushes.White;
                    tb.FontWeight = FontWeights.SemiBold;
                    tb.Padding = new Thickness(14, 6, 14, 6);
                }
                else
                {
                    tb.Background = Brushes.Transparent;
                    tb.Foreground = (Brush)TryFindResource("Brushes.Text.Muted")!;
                    tb.FontWeight = FontWeights.Medium;
                    tb.Padding = new Thickness(14, 6, 14, 6);
                }
            }
        }
    }
}
