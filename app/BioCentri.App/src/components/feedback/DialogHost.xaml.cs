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

        // IMPORTANT: BOTH storyboards are StaticResources pulled from the
        // application MergedDictionaries. ResourceDictionary animations are
        // *frozen* by WPF for thread-safety. Retargeting + beginning a
        // frozen storyboard without first cloning throws
        // InvalidOperationException (\"Cannot modify a Frozen object\")
        // on every render tick. Pre-fix this exception fired synchronously
        // here, hit OnDispatcherUnhandledException (e.Handled = true), and
        // was re-raised on the next ~16ms composition pass — producing the
        // MessageBox cascade the user observed. The Dimmer path already
        // .Clone()s; the PopIn path did not. Fix matches the Dimmer pattern.
        host.Dimmer.BeginStoryboard(((Storyboard)host.TryFindResource("Transitions.Dimmer.Show")!).Clone());

        // Cast MUST wrap the resource lookup, THEN invoke Clone() on the
        // typed Storyboard. The earlier `(Storyboard)....Clone()` shape
        // called Clone() on `object` (TryFindResource's compile-time return
        // type) before the cast ran, which the compiler rejected as CS1061.
        // The `as` cast + `?.Clone()` + pattern test null-safe handles both
        // a missing resource AND a wrong-typed resource without throwing.
        if ((host.TryFindResource("Transitions.Dialog.PopIn") as Storyboard)?.Clone() is { } sb)
        {
            Storyboard.SetTarget(sb, host.DialogPresenter);
            host.BeginStoryboard(sb);
        }
    }
}
