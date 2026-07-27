namespace BioCentri.App.Types.Services;

/// <summary>
/// UI thread dispatcher abstraction. Hides <c>System.Windows.Threading.Dispatcher</c>
/// behind a small surface so non-UI services can schedule work without
/// taking a hard dependency on WPF.
/// </summary>
public interface IDispatcher
{
    /// <summary>Queue <paramref name="action"/> on the UI thread (BeginInvoke).</summary>
    void Post(Action action);

    /// <summary>
    /// Run <paramref name="action"/> on the UI thread and await its
    /// synchronous completion. Use for fire-and-forget UI writes like
    /// INPC property setters.
    /// </summary>
    Task InvokeAsync(Action action);

    /// <summary>
    /// Run <paramref name="func"/> on the UI thread and await its
    /// asynchronous completion. Essential for awaiting WinRT calls
    /// (e.g. <c>UserConsentVerifier.RequestVerificationAsync</c>) which
    /// must be marshalled to the UI thread; passing a <c>Func&lt;Task&gt;</c>
    /// keeps the inner <c>await</c> inside the dispatcher frame instead
    /// of leaking out as <c>async void</c>.
    /// </summary>
    Task InvokeAsync(Func<Task> func);

    /// <summary>Typed result variant of <see cref="InvokeAsync(Func{Task})"/>.</summary>
    Task<T> InvokeAsync<T>(Func<Task<T>> func);
}
