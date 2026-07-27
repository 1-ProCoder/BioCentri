namespace BioCentri.App.Services;

/// <summary>
/// Tiny in-house DI host. Replaces
/// <c>Microsoft.Extensions.DependencyInjection</c> at Milestone 1 while the
/// offline NuGet cache on this build machine is dep-light (Decision 9).
/// Resolves to singleton instances and rejects unknown keys with a clear
/// error. Will be swapped for <c>AddSingleton</c> + <c>BuildServiceProvider</c>
/// in Milestone 2 the moment <c>Microsoft.Extensions.DependencyInjection</c>
/// is restored to the feed.
/// </summary>
public sealed class ServiceHost
{
    private readonly Dictionary<Type, object> _singletons = new();

    /// <summary>Register a singleton instance for <typeparamref name="TService"/>.</summary>
    public ServiceHost AddSingleton<TService>(TService instance) where TService : class
    {
        ArgumentNullException.ThrowIfNull(instance);
        _singletons[typeof(TService)] = instance;
        return this;
    }

    /// <summary>Resolve <typeparamref name="TService"/> — throws when unknown.</summary>
    public T Get<T>() where T : class
    {
        if (_singletons.TryGetValue(typeof(T), out var value))
            return (T)value;
        throw new InvalidOperationException(
            $"Service {typeof(T).FullName} is not registered with the host.");
    }

    /// <summary>True if the host has a registration for the given service type.</summary>
    public bool IsRegistered<T>() where T : class => _singletons.ContainsKey(typeof(T));
}
