using Microsoft.Extensions.DependencyInjection;

namespace Forge.Decide.DependencyInjection;

/// <summary>
/// Registers typed Forge.Decide strategy spaces with Microsoft.Extensions.DependencyInjection.
/// </summary>
public static class StrategySpaceServiceCollectionExtensions
{
    /// <summary>
    /// Registers one explicitly configured typed strategy space.
    /// </summary>
    /// <typeparam name="TSpace">Consumer-defined strategy-space marker.</typeparam>
    /// <typeparam name="TContext">Decision context type.</typeparam>
    /// <typeparam name="TPlan">Application-owned plan type.</typeparam>
    /// <param name="services">Service collection.</param>
    /// <param name="id">Stable strategy-space identifier.</param>
    /// <param name="configure">Explicit strategy-space registration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddForgeDecisionSpace<TSpace, TContext, TPlan>(
        this IServiceCollection services,
        StrategySpaceId id,
        Action<StrategySpaceServiceBuilder<TSpace, TContext, TPlan>> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var builder = new StrategySpaceServiceBuilder<TSpace, TContext, TPlan>(id);
        configure(builder);
        builder.Register(services);

        return services;
    }
}