using Forge.Delta;

namespace Forge.Sync.Tests;

[GenerateDelta]
internal sealed record ManagedComponentConfiguration(
    string? Network,
    int Capacity);
