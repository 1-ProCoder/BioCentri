using System.Windows;
using System.Windows.Controls;
using BioCentri.App.Components.Motion;

namespace BioCentri.App.Features.Diagnostics;

public partial class DiagnosticsPage : Page
{
    public DiagnosticsPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (Content is ScrollViewer sv && sv.Content is Panel panel)
            EntranceAnimation.Play(panel);
    }
}
