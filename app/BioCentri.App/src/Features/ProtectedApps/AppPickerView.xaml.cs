using System.Windows.Controls;
using System.Windows.Input;

namespace BioCentri.App.Features.ProtectedApps;

/// <summary>
/// View surface for <see cref="AppPickerViewModel"/>. The shell's
/// DialogHost renders this when the running dialog's
/// <c>DataType</c> matches <c>AppPickerViewModel</c> (registered in
/// <c>App.xaml</c>). Code-behind is intentionally minimal — UI
/// lifecycle only.
/// </summary>
public partial class AppPickerView : UserControl
{
    public AppPickerView()
    {
        InitializeComponent();
    }

    /// <summary>Milestone 7 accessibility: Escape key closes the picker
    /// dialog by invoking the Cancel command on the DataContext.</summary>
    private void OnEscapeKey(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && DataContext is AppPickerViewModel vm)
            vm.CancelCommand.Execute(null);
    }
}
