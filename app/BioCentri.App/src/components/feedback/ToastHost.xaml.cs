using System.Windows.Controls;

namespace BioCentri.App.Components.Feedback;

/// <summary>
/// Anchor layer for toasts. ItemsControl bound to
/// <see cref="ToastService.Toasts"/>. Items render through a
/// <see cref="Toast"/> DataTemplate.
/// </summary>
public partial class ToastHost : ItemsControl
{
    public ToastHost()
    {
        InitializeComponent();
    }
}
