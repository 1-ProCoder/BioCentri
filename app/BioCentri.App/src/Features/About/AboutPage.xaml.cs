using System.Windows;
using System.Windows.Controls;
using BioCentri.App.Components.Motion;

namespace BioCentri.App.Features.About;

public partial class AboutPage : Page
{
    public AboutPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // ScrollViewer.Child is the page StackPanel — animate its direct children.
        if (Content is ScrollViewer sv && sv.Content is Panel panel)
            EntranceAnimation.Play(panel);
    }
}
