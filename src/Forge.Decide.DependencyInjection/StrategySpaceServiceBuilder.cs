using Microsoft.Extensions.DependencyInjection;

namespace Forge.Decide.DependencyInjection;

/// <summary>
/// Configures one typed strategy space for dependency injection.
/// </summary>
/// <typeparam name="TSpace">Consumer-defined strategy-space marker.</typeparam>
/// <typeparam name="TContext">Decision context type.</typeparam>
/// <typeparam name="TPlan">Application-owned plan type.</typeparam>
public sealed class StrategySpaceServiceBuilder<TSpace, TContext, TPlan>
{
    private readonly StrategySpaceId _id;
    private readonly List<Action<IServiceCollection>> _registrations = [];
    private readonly List<Func<IServiceProvider, IStrategy<TContext, TPlan>>> _strategyFactories = [];
    private Func<IServiceProvider, IStrategySelectionPolicy<TContext, TPlan>>? _selectionPolicyFactory;

    internal StrategySpaceServiceBuilder(StrategySpaceId id)
    {
        _id = id;
    }

    /// <summary>
    /// Adds one strategy type to this space. Existing application registrations for the strategy type are respected.
    /// </summary>
    /// <typeparam name="TStrategy">Strategy implementation type.</typeparam>
    /// <param name="lifetime">Lifetime used only when the strategy type is not already registered.</param>
    /// <returns>This builder.</returns>
    public StrategySpaceServiceBuilder<TSpace, TContext, TPlan> Add<TStrategy>(ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TStrategy : class, IStrategy<TContext, TPlan>
    {
        _registrations.Add(services => AddIfMissing(services, typeof(TStrategy), typeof(TStrategy), lifetime));
        _strategyFactories.Add(static provider => provider.GetRequiredService<TStrategy>());
        return this;
    }

    /// <summary>
    /// Adds a strategy factory to this space without adding a service registration.
    /// </summary>
    /// <param name="factory">Factory that resolves or creates the admitted strategy.</param>
    /// <returns>This builder.</returns>
    public StrategySpaceServiceBuilder<TSpace, TContext, TPlan> Add(Func<IServiceProvider, IStrategy<TContext, TPlan>> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _strategyFactories.Add(factory);
        return this;
    }

    /// <summary>
    /// Uses a selection-policy type. Existing application registrations for the policy type are respected.
    /// </summary>
    /// <typeparam name="TPolicy">Selection-policy implementation type.</typeparam>
    /// <param name="lifetime">Lifetime used only when the policy type is not already registered.</param>
    /// <returns>This builder.</returns>
    public StrategySpaceServiceBuilder<TSpace, TContext, TPlan> SelectWith<TPolicy>(ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TPolicy : class, IStrategySelectionPolicy<TContext, TPlan>
    {
        _registrations.Add(services => AddIfMissing(services, typeof(TPolicy), typeof(TPolicy), lifetime));
        _selectionPolicyFactory = static provider => provider.GetRequiredService<TPolicy>();
        return this;
    }

    /// <summary>
    /// Uses an application-owned selection-policy factory.
    /// </summary>
    /// <param name="factory">Policy factory.</param>
    /// <returns>This builder.</returns>
    public StrategySpaceServiceBuilder<TSpace, TContext, TPlan> SelectWith(
        Func<IServiceProvider, IStrategySelectionPolicy<TContext, TPlan>> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _selectionPolicyFactory = factory;
        return this;
    }

    internal void Register(IServiceCollection services)
    {
        var spaceType = typeof(StrategySpace<TSpace, TContext, TPlan>);

        if (services.Any(descriptor => descriptor.ServiceType == spaceType))
        {
            throw new InvalidOperationException($"Strategy space '{spaceType.FullName}' is already registered.");
        }

        foreach (var registration in _registrations)
        {
            registration(services);
        }

        services.Add(new ServiceDescriptor(
            spaceType,
            provider => Build(provider),
            ServiceLifetime.Transient));
    }

    private StrategySpace<TSpace, TContext, TPlan> Build(IServiceProvider provider)
    {
        var builder = StrategySpace<TSpace>.Define<TContext, TPlan>(_id);

        foreach (var strategyFactory in _strategyFactories)
        {
            builder.Add(strategyFactory(provider));
        }

        if (_selectionPolicyFactory is not null)
        {
            builder.SelectWith(_selectionPolicyFactory(provider));
        }

        return builder.Build();
    }

    private static void AddIfMissing(IServiceCollection services, Type serviceType, Type implementationType, ServiceLifetime lifetime)
    {
        if (!services.Any(descriptor => descriptor.ServiceType == serviceType))
        {
            services.Add(new ServiceDescriptor(serviceType, implementationType, lifetime));
        }
    }
}