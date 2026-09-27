namespace Forge.Sync;

/// <summary>Immutable result of bounded monotonic fact propagation.</summary>
public sealed record FactDerivationResult(FactSet Facts, int Passes);
