using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BioCentri.App.Routing;
using BioCentri.App.State;
using BioCentri.App.Types.Services;

namespace BioCentri.App.Components.Nav;

/// <summary>
/// Vertical rail. Hosts one <see cref="SidebarItem"/> per registered
/// <see cref="Route"/>. Listens to <see cref="ShellState"/> for the
/// current selection and expanded state. The M3 polish dropped the
/// inline toggle-text label so the bottom row is just the Status
/// pill + a Border collapse-button (the chevron icon flips based
/// on ShellState.IsSidebarExpanded).
/// </summary>
public partial class Sidebar : Grid
{
    public static readonly DependencyProperty ShellStateProperty =
        DependencyProperty.Register(
            nameof(ShellState), typeof(ShellState), typeof(Sidebar),
            new PropertyMetadata(null, OnShellStateChanged));

    public static readonly DependencyProperty NavigateCommandProperty =
        DependencyProperty.Register(
            nameof(NavigateCommand), typeof(ICommand), typeof(Sidebar));

    public ShellState? ShellState
    {
        get => (ShellState?)GetValue(ShellStateProperty);
        set => SetValue(ShellStateProperty, value);
    }

    public ICommand? NavigateCommand
    {
        get => (ICommand?)GetValue(NavigateCommandProperty);
        set => SetValue(NavigateCommandProperty, value);
    }

    public Sidebar()
    {
        InitializeComponent();
        Populate();
    }

    private static void OnShellStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Sidebar s) s.Rebind();
    }

    private void Populate()
    {
        ItemsHost.Children.Clear();
        foreach (var route in new[]
                 {
                     Route.Dashboard,
                     Route.ProtectedApps,
                     Route.Rules,
                     Route.Activity,
                     Route.Settings,
                     Route.About,
                     Route.Diagnostics,
                 })
        {
            var meta = RouteTable.Get(route);
            var item = new SidebarItem
            {
                Title = meta.Title,
                Route = route,
                IconKey = meta.IconKey,
                NavigateCommand = NavigateCommand,
            };
            // Bind IsSelected against ShellState.CurrentRoute via the
            // border-relative property path to keep this MVVM-correct
            // without code-behind reach-in.
            item.SetBinding(SidebarItem.IsSelectedProperty,
                new System.Windows.Data.Binding("CurrentRoute")
                {
                    Source = ShellState,
                    Converter = new RouteEqualsConverter(),
                    ConverterParameter = route,
                });
            ItemsHost.Children.Add(item);
        }
        UpdateLayout();
    }

    private void Rebind()
    {
        if (ShellState is null) return;
        Populate();
    }

    private void OnToggleClicked(object sender, RoutedEventArgs e)
    {
        if (ShellState is null) return;
        ShellState.IsSidebarExpanded = !ShellState.IsSidebarExpanded;
        ApplyExpansion();
    }

    private void ApplyExpansion()
    {
        if (ToggleIcon is null || ShellState is null) return;
        var expanded = ShellState.IsSidebarExpanded;
        ToggleIcon.GeometryKey = expanded ? "Icons.Action.ChevronLeft" : "Icons.Action.ChevronRight";
    }
}

/// <summary>
/// One-way converter: returns true when the bound <see cref="Route"/>
/// equals the converter parameter. Used by the sidebar to mark the
/// active route without bespoke code-behind.
/// </summary>
internal sealed class RouteEqualsConverter : System.Windows.Data.IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        => value is Route r && parameter is Route target && r == target;

    public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        => throw new NotSupportedException();
}
