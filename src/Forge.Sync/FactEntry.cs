namespace Forge.Sync;

internal sealed record FactEntry<T>(T Value) : IFactEntry;
