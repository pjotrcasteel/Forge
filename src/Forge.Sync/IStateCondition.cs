namespace Forge.Sync;

/// <summary>
/// Evaluates whether a strongly typed observed state satisfies a dependency condition.
/// </summary>
public interface IStateCondition<in TState>
{
    /// <summary>Returns whether the supplied state satisfies the condition.</summary>
    bool IsSatisfied(TState state);
}
