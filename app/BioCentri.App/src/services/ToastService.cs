using System.Collections.ObjectModel;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using BioCentri.App.Types.Services;

namespace BioCentri.App.Services;

/// <summary>
/// Concrete toast service. Maintains an observable collection of
/// toasts; the host overlay collects this collection directly.
/// Toasts auto-dismiss after their declared duration via a
/// <see cref="DispatcherTimer"/>.
/// </summary>
public sealed class ToastService : IToastService
{
    private readonly DispatcherTimer _timer;
    private readonly ObservableCollection<ToastViewModel> _toasts = new();

    /// <summary>Bound by <c>ToastHost.ItemsSource</c>.</summary>
    public ReadOnlyObservableCollection<ToastViewModel> Toasts { get; }

    public ToastService()
    {
        Toasts = new ReadOnlyObservableCollection<ToastViewModel>(_toasts);
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
        _timer.Tick += (_, _) => FlushExpired();
        _timer.Start();
    }

    /// <inheritdoc />
    public IToastController Show(ToastSeverity severity, string title, string? description = null, int? durationMs = null)
    {
        var vm = new ToastViewModel(severity, title, description, durationMs ?? DefaultDuration(severity));
        _toasts.Add(vm);
        return vm;
    }

    private void FlushExpired()
    {
        for (int i = _toasts.Count - 1; i >= 0; i--)
        {
            if (_toasts[i].IsExpired) _toasts.RemoveAt(i);
        }
    }

    private static int DefaultDuration(ToastSeverity severity) => severity switch
    {
        ToastSeverity.Danger  => 8000,
        ToastSeverity.Warning => 6000,
        ToastSeverity.Success => 4000,
        _                     => 3500,
    };
}

/// <summary>Backing VM for a single toast entry.</summary>
public sealed class ToastViewModel : ObservableObject, IToastController
{
    public ToastSeverity Severity { get; }
    public string Title { get; }
    public string? Description { get; }
    public DateTimeOffset ExpiresAt { get; }

    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;

    public ToastViewModel(ToastSeverity severity, string title, string? description, int durationMs)
    {
        Severity = severity; Title = title; Description = description;
        ExpiresAt = DateTimeOffset.UtcNow.AddMilliseconds(durationMs);
    }

    /// <inheritdoc />
    public void Dismiss() { /* pulled out of the collection by FlushExpired or by host */ }
}
