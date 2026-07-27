using System.Windows;
using System.Windows.Controls;
using BioCentri.App.Services;
using BioCentri.App.Types.Services;

namespace BioCentri.App.Components.Feedback;

/// <summary>
/// Floating notification card. Bound to <see cref="ToastViewModel"/> via
/// DataContext. Severity drives icon + accent stripe colour.
/// </summary>
public partial class Toast : Border
{
    public static readonly DependencyProperty SeverityProperty =
        DependencyProperty.Register(
            nameof(Severity), typeof(ToastSeverity), typeof(Toast),
            new PropertyMetadata(ToastSeverity.Info, OnSeverityChanged));

    public ToastSeverity Severity
    {
        get => (ToastSeverity)GetValue(SeverityProperty);
        set => SetValue(SeverityProperty, value);
    }

    /// <summary>Raised when the user clicks the close button.</summary>
    public event EventHandler? DismissRequested;

    public Toast()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => Rebind();
    }

    private void Rebind()
    {
        if (DataContext is ToastViewModel vm)
        {
            Severity = vm.Severity;
            TitleText.Text = vm.Title;
            if (!string.IsNullOrEmpty(vm.Description))
            {
                DescriptionText.Text = vm.Description;
                DescriptionText.Visibility = Visibility.Visible;
            }
            else
            {
                DescriptionText.Visibility = Visibility.Collapsed;
            }
        }
    }

    private static void OnSeverityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Toast toast) return;
        var kind = (ToastSeverity)e.NewValue;
        toast.LeadingIcon.GeometryKey = kind switch
        {
            ToastSeverity.Success => "Icons.Status.Check",
            ToastSeverity.Warning => "Icons.Status.Warning",
            ToastSeverity.Danger  => "Icons.Status.Warning",
            _                     => "Icons.Status.Info",
        };
        toast.SeverityStripe.SetResourceReference(Border.BackgroundProperty,
            kind switch
            {
                ToastSeverity.Success => "Brushes.Status.Success",
                ToastSeverity.Warning => "Brushes.Status.Warn",
                ToastSeverity.Danger  => "Brushes.Status.Danger",
                _                     => "Brushes.Accent.Indigo",
            });
    }

    private void OnCloseClicked(object sender, RoutedEventArgs e)
    {
        DismissRequested?.Invoke(this, EventArgs.Empty);
    }
}
