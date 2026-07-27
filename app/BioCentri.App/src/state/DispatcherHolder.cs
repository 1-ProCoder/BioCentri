using System.Windows.Threading;
using BioCentri.App.Types.Services;

namespace BioCentri.App.State;

/// <summary>WPF <see cref="Dispatcher"/> adapter exposed via <see cref="IDispatcher"/>.</summary>
public sealed class DispatcherHolder : IDispatcher
{
    private readonly Dispatcher _dispatcher;

    public DispatcherHolder(Dispatcher dispatcher)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);
        _dispatcher = dispatcher;
    }

    /// <inheritdoc />
    public void Post(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        _dispatcher.BeginInvoke(action);
    }

    /// <inheritdoc />
    public Task InvokeAsync(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        return _dispatcher.InvokeAsync(action).Task;
    }

    /// <inheritdoc />
    public Task InvokeAsync(Func<Task> func)
    {
        ArgumentNullException.ThrowIfNull(func);
        return _dispatcher.InvokeAsync(func).Task;
    }

    /// <inheritdoc />
    public Task<T> InvokeAsync<T>(Func<Task<T>> func)
    {
        ArgumentNullException.ThrowIfNull(func);
        // Dispatcher.InvokeAsync(Func<Task<T>>) returns a
        // DispatcherOperation<Task<Task<T>>>; calling .Task on it yields
        // Task<Task<T>>, not Task<T>. Unwrap pulls out the inner Task<T>
        // so the caller awaits the actual result type.
        return _dispatcher.InvokeAsync(func).Task.Unwrap();
    }
}
