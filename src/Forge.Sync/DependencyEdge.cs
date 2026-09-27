namespace Forge.Sync;

/// <summary>
/// Represents a typed prerequisite edge where <see cref="Predecessor"/> must be satisfied before <see cref="Successor"/>.
/// </summary>
public readonly record struct DependencyEdge<TKey>(TKey Predecessor, TKey Successor)
    where TKey : notnull;
