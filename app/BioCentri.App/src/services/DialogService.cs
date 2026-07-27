using BioCentri.App.Types.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BioCentri.App.Services;

/// <summary>
/// Dialog surface for the application's overlay layer. Implementation
/// strategy: a single <see cref="ActiveDialog"/> property that the
/// overlay binds to. The awaitable <see cref="RequestCloseAsync{TResult}"/>
/// resolves when the user dismisses.
/// </summary>
public sealed class DialogService : ObservableObject, IDialogService
{
    private object? _activeDialog;

    /// <summary>The currently-rendered overlay (or null if no dialog open).</summary>
    public object? ActiveDialog
    {
        get => _activeDialog;
        private set => SetProperty(ref _activeDialog, value);
    }

    /// <inheritdoc />
    public async Task<bool> ConfirmAsync(string title, string message, string? confirmLabel = null, string? cancelLabel = null)
    {
        var vm = new ConfirmDialogViewModel(title, message, confirmLabel ?? "Confirm", cancelLabel ?? "Cancel");
        ActiveDialog = vm;
        var result = await vm.WaitForResultAsync();
        ActiveDialog = null;
        return result;
    }

    /// <inheritdoc />
    public async Task NotifyAsync(string title, string message)
    {
        var vm = new NotifyDialogViewModel(title, message);
        ActiveDialog = vm;
        await vm.WaitForResultAsync();
        ActiveDialog = null;
    }

    /// <inheritdoc />
    public async Task<TResult?> ShowAsync<TResult>(object content, IDialogHostViewModel<TResult> viewModel)
    {
        ActiveDialog = content;
        var result = await viewModel.WaitForResultAsync(CancellationToken.None);
        ActiveDialog = null;
        return result;
    }
}

/// <summary>Internal VM for two-button confirmation dialogs.</summary>
internal sealed class ConfirmDialogViewModel : IDialogHostViewModel<bool>
{
    private readonly TaskCompletionSource<bool> _tcs = new();
    public string Title { get; }
    public string Message { get; }
    public string ConfirmLabel { get; }
    public string CancelLabel { get; }

    public ConfirmDialogViewModel(string title, string message, string confirmLabel, string cancelLabel)
    {
        Title = title; Message = message;
        ConfirmLabel = confirmLabel; CancelLabel = cancelLabel;
    }

    public Task<bool> WaitForResultAsync(CancellationToken cancellationToken = default) =>
        _tcs.Task;

    public void Confirm() => _tcs.TrySetResult(true);
    public void Cancel()  => _tcs.TrySetResult(false);
}

/// <summary>Internal VM for read-only notification dialogs.</summary>
internal sealed class NotifyDialogViewModel : IDialogHostViewModel<bool>
{
    private readonly TaskCompletionSource<bool> _tcs = new();
    public string Title { get; }
    public string Message { get; }

    public NotifyDialogViewModel(string title, string message)
    {
        Title = title; Message = message;
    }

    public Task<bool> WaitForResultAsync(CancellationToken cancellationToken = default) =>
        _tcs.Task;

    public void Dismiss() => _tcs.TrySetResult(true);
}
