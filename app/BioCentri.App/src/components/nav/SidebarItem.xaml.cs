using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using BioCentri.App.Routing;

namespace BioCentri.App.Components.Nav;

/// <summary>
/// One row in the sidebar. Title, icon, and a left-edge selection stripe
/// are toggled via <see cref="IsSelected"/>. <see cref="NavigateCommand"/>
/// bubbles to the parent shell's <see cref="INavigationService.NavigateTo"/>.
/// </summary>
public partial class SidebarItem : Border
{
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title), typeof(string), typeof(SidebarItem),
            new PropertyMetadata("Item", OnTitleChanged));

    public static readonly DependencyProperty RouteProperty =
        DependencyProperty.Register(
            nameof(Route), typeof(Route), typeof(SidebarItem),
            new PropertyMetadata(Route.Dashboard));

    public static readonly DependencyProperty IconKeyProperty =
        DependencyProperty.Register(
            nameof(IconKey), typeof(string), typeof(SidebarItem),
            new PropertyMetadata("Icons.Route.Dashboard", OnIconKeyChanged));

    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register(
            nameof(IsSelected), typeof(bool), typeof(SidebarItem),
            new PropertyMetadata(false, OnIsSelectedChanged));

    public static readonly DependencyProperty NavigateCommandProperty =
        DependencyProperty.Register(
            nameof(NavigateCommand), typeof(ICommand), typeof(SidebarItem));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public Route Route
    {
        get => (Route)GetValue(RouteProperty);
        set => SetValue(RouteProperty, value);
    }

    public string IconKey
    {
        get => (string)GetValue(IconKeyProperty);
        set => SetValue(IconKeyProperty, value);
    }

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public ICommand? NavigateCommand
    {
        get => (ICommand?)GetValue(NavigateCommandProperty);
        set => SetValue(NavigateCommandProperty, value);
    }

    public SidebarItem()
    {
        InitializeComponent();
        Loaded += (_, _) => ApplySelection();
    }

    private void OnSelected(object sender, MouseButtonEventArgs e)
    {
        if (NavigateCommand is { } cmd && cmd.CanExecute(Route))
        {
            cmd.Execute(Route);
        }
    }

    private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SidebarItem i && i.TitleText is { } t) t.Text = (string)e.NewValue;
    }

    private static void OnIconKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SidebarItem i && i.LeadingIcon is { } icon) icon.GeometryKey = (string)e.NewValue;
    }

    private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SidebarItem i) i.ApplySelection();
    }

    private Storyboard? _stripeIn;
    private Storyboard? _stripeOut;

    private void ApplySelection()
    {
        if (SelectionStripe is null) return;

        if (IsSelected)
        {
            _stripeIn ??= BuildStripeStoryboard(true);
            BeginStoryboard(_stripeIn);
            Background = (Brush)TryFindResource("Brushes.Subtle.Surface")!;
            TitleText.Foreground = (Brush)TryFindResource("Brushes.Text.Primary")!;
            LeadingIcon.Stroke = (Brush)TryFindResource("Brushes.Accent.Indigo")!;
        }
        else
        {
            _stripeOut ??= BuildStripeStoryboard(false);
            BeginStoryboard(_stripeOut);
            Background = Brushes.Transparent;
            TitleText.Foreground = (Brush)TryFindResource("Brushes.Text.Muted")!;
            LeadingIcon.Stroke = (Brush)TryFindResource("Brushes.Text.Muted")!;
        }
    }

    private Storyboard BuildStripeStoryboard(bool visible)
    {
        // Selection stripe fade. Decision 9 / followup: easings live in C#
        // at /app/BioCentri.App/src/styles/Motion.cs — we inline the
        // website's `out-expo` cubic-bezier(0.16, 1, 0.3, 1) here so
        // this selection animation works without a baml hiccup.
        var sb = new Storyboard();
        var anim = new DoubleAnimationUsingKeyFrames();
        Storyboard.SetTarget(anim, SelectionStripe!);
        Storyboard.SetTargetProperty(anim, new PropertyPath("Opacity"));
        anim.KeyFrames.Add(new SplineDoubleKeyFrame(
            visible ? 1.0 : 0.0,
            KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(220)))
        {
            KeySpline = new System.Windows.Media.Animation.KeySpline(0.16, 1.0, 0.3, 1.0),
        });
        sb.Children.Add(anim);
        return sb;
    }
}
