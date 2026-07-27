using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace BioCentri.App.Components.Feedback;

/// <summary>
/// Modal overlay. Bound to <see cref="DialogService.ActiveDialog"/>:
/// when the property is set, the dimmer fades in and the dialog content
/// is presented; when cleared, both fade out.
/// </summary>
public partial class DialogHost : Grid
{
    public static readonly DependencyProperty ActiveDialogProperty =
        DependencyProperty.Register(
            nameof(ActiveDialog), typeof(object), typeof(DialogHost),
            new PropertyMetadata(null, OnActiveDialogChanged));

    public object? ActiveDialog
    {
        get => GetValue(ActiveDialogProperty);
        set => SetValue(ActiveDialogProperty, value);
    }

    public DialogHost()
    {
        InitializeComponent();
    }

    private static void OnActiveDialogChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DialogHost host) return;

        if (e.NewValue is null)
        {
            host.Visibility = Visibility.Collapsed;
            host.Dimmer.Opacity = 0;
            host.DialogPresenter.Content = null;
            return;
        }

        host.Visibility = Visibility.Visible;
        host.Dimmer.Opacity = 0;
        host.DialogPresenter.Content = e.NewValue;

        // Fade-in
        host.Dimmer.BeginStoryboard(((Storyboard)host.TryFindResource("Transitions.Dimmer.Show")!).Clone());
        if (((Storyboard)host.TryFindResource("Transitions.Dialog.PopIn")!) is { } sb)
        {
            Storyboard.SetTarget(sb, host.DialogPresenter);
            host.BeginStoryboard(sb);
        }
    }
}
