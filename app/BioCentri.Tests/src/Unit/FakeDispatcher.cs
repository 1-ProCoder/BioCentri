using BioCentri.App.Types.Services;

namespace BioCentri.Tests.Unit;

/// <summary>
/// Test double for <see cref="IDispatcher"/>. Runs every posted action
/// synchronously on the calling thread — no WPF Dispatcher needed in
/// headless tests.
/// </summary>
public sealed class FakeDispatcher : IDispatcher
{
    public void Post(Action action) => action();

    public Task InvokeAsync(Action action)
    {
        action();
        return Task.CompletedTask;
    }

    public Task InvokeAsync(Func<Task> func) => func();

    public async Task<T> InvokeAsync<T>(Func<Task<T>> func) => await func();
}
