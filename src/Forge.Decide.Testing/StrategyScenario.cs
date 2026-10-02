namespace Forge.Decide.Testing;

/// <summary>
/// Names one context in a strategy-space scenario matrix.
/// </summary>
/// <typeparam name="TContext">Decision context type.</typeparam>
public sealed record StrategyScenario<TContext>
{
    /// <summary>
    /// Initializes a named strategy scenario.
    /// </summary>
    /// <param name="name">Stable scenario name used in diagnostics.</param>
    /// <param name="context">Decision context to evaluate.</param>
    public StrategyScenario(string name, TContext context)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
        Context = context;
    }

    /// <summary>
    /// Gets the stable scenario name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the decision context.
    /// </summary>
    public TContext Context { get; }
}