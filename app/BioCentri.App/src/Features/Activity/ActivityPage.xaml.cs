using System.Windows;
using System.Windows.Controls;
using BioCentri.App.Components.Motion;

namespace BioCentri.App.Features.Activity;

public partial class ActivityPage : Page
{
    public ActivityPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Top-level Grid has named rows; animate the three row children.
        if (Content is Grid g) EntranceAnimation.Play(g);
    }
}
