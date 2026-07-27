using System.Windows;
using System.Windows.Controls;
using BioCentri.App.Components.Motion;

namespace BioCentri.App.Features.Rules;

public partial class RulesPage : Page
{
    public RulesPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (Content is Grid g) EntranceAnimation.Play(g);
    }
}
