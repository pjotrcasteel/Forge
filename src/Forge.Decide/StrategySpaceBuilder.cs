namespace Forge.Decide;

/// <summary>
/// Builds an explicit typed strategy space.
/// </summary>
/// <typeparam name="TSpace">Consumer-defined strategy-space marker.</typeparam>
/// <typeparam name="TContext">Decision context type.</typeparam>
/// <typeparam name="TPlan">Application-owned plan type.</typeparam>
public sealed class StrategySpaceBuilder<TSpace, TContext, TPlan>
{
    private readonly StrategySpaceId _id;
    private readonly List<IStrategy<TContext, TPlan>> _strategies = [];
    private IStrategySelectionPolicy<TContext, TPlan> _selectionPolicy = StrategySelectionPolicies.ExactlyOne<TContext, TPlan>();

    internal StrategySpaceBuilder(StrategySpaceId id)
    {
        _id = id;
    }

    /// <summary>
    /// Adds a strategy that is allowed to compete in this space.
    /// </summary>
    /// <param name="strategy">Strategy to add.</param>
    /// <returns>This builder.</returns>
    public StrategySpaceBuilder<TSpace, TContext, TPlan> Add(IStrategy<TContext, TPlan> strategy)
    {
        ArgumentNullException.ThrowIfNull(strategy);

        if (_strategies.Any(existing => existing.Id == strategy.Id))
        {
            throw new ArgumentException($"Strategy '{strategy.Id}' is already registered in strategy space '{_id}'.", nameof(strategy));
        }

        _strategies.Add(strategy);
        return this;
    }

    /// <summary>
    /// Configures how applicable proposals are resolved when more than one strategy can handle the context.
    /// </summary>
    /// <param name="selectionPolicy">Selection policy.</param>
    /// <returns>This builder.</returns>
    public StrategySpaceBuilder<TSpace, TContext, TPlan> SelectWith(IStrategySelectionPolicy<TContext, TPlan> selectionPolicy)
    {
        ArgumentNullException.ThrowIfNull(selectionPolicy);
        _selectionPolicy = selectionPolicy;
        return this;
    }

    /// <summary>
    /// Creates the immutable strategy space.
    /// </summary>
    /// <returns>A configured strategy space.</returns>
    public StrategySpace<TSpace, TContext, TPlan> Build()
    {
        if (_strategies.Count == 0)
        {
            throw new InvalidOperationException($"Strategy space '{_id}' must contain at least one strategy.");
        }

        return new StrategySpace<TSpace, TContext, TPlan>(_id, _strategies.ToArray(), _selectionPolicy);
    }
}