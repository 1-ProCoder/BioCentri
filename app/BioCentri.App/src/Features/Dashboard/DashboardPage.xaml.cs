using System.Windows;
using System.Windows.Controls;
using BioCentri.App.Components.Motion;

namespace BioCentri.App.Features.Dashboard;

/// <summary>Milestone UI: staggered fade-up on first navigation to the
/// dashboard so the hero / stat tiles / activity list animate in as
/// one calm motion. Honours <c>Motion.RespectReducedMotion</c> via
/// <see cref="EntranceAnimation"/>.</summary>
public partial class DashboardPage : Page
{
    public DashboardPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Find the outermost StackPanel of the ScrollViewer content
        // (ScrollViewer.Child) and animate its direct children.
        if (this.FindName("Root") is Panel p) EntranceAnimation.Play(p);
        else if (Content is FrameworkElement fe && fe.Parent is ScrollViewer sv && sv.Content is Panel panel)
            EntranceAnimation.Play(panel);
    }
}
