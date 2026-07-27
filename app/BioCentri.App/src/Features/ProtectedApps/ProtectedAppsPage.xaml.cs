using System.Windows;
using System.Windows.Controls;
using BioCentri.App.Components.Motion;

namespace BioCentri.App.Features.ProtectedApps;

public partial class ProtectedAppsPage : Page
{
    public ProtectedAppsPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Page root is a 3-row Grid; animate the three row children.
        if (Content is Grid g) EntranceAnimation.Play(g);
    }
}
