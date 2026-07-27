namespace BioCentri.App.Types.Services;

/// <summary>
/// Toast surface. Push toasts; the host overlays them at the bottom-right
/// of the shell. Toasts auto-dismiss after a per-toast duration unless
/// the caller closes them earlier.
/// </summary>
public interface IToastService
{
    /// <summary>Push a toast of a given severity and return its controller.</summary>
    IToastController Show(ToastSeverity severity, string title, string? description = null, int? durationMs = null);
}

public enum ToastSeverity
{
    Info,
    Success,
    Warning,
    Danger,
}

public interface IToastController
{
    /// <summary>Dismiss this toast now, regardless of its auto-dismiss timer.</summary>
    void Dismiss();
}
