namespace Forge.Sync;

/// <summary>Describes one currently blocked conditional dependency.</summary>
public sealed record BlockedConditionalDependency<TKey, TState>(
    ConditionalDependency<TKey, TState> Dependency,
    ConditionalDependencyBlockReason Reason,
    ObservedState<TState> ObservedState)
    where TKey : notnull;
