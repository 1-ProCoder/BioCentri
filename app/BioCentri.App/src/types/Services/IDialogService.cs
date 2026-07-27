namespace BioCentri.App.Types.Services;

/// <summary>
/// Heart of the dialog overlay. Three flavours:
///   * Confirm — Yes/No, returns a bool
///   * Notify  — info-only dismissable
///   * Custom  — caller provides content, lifecycle hooks to close
///
/// All three render in the central DialogHost overlay so the dimmer and
/// focus-trap are uniform across the app.
/// </summary>
public interface IDialogService
{
    Task<bool> ConfirmAsync(string title, string message, string? confirmLabel = null, string? cancelLabel = null);
    Task NotifyAsync(string title, string message);
    Task<TResult?> ShowAsync<TResult>(object content, IDialogHostViewModel<TResult> viewModel);
}

/// <summary>Implemented by dialog VMs that want strict lifecycle parity with the host.</summary>
public interface IDialogHostViewModel<TResult>
{
    Task<TResult> WaitForResultAsync(CancellationToken cancellationToken);
}
