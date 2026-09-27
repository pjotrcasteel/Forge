namespace Forge.Sync;

/// <summary>Represents a typed directed provenance relationship from cause to effect.</summary>
public readonly record struct ProvenanceEdge<TKey>(TKey Cause, TKey Effect)
    where TKey : notnull;
