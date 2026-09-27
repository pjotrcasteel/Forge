using Forge.Delta;

namespace Forge.Benchmarks;

[GenerateDelta]
public sealed record BenchmarkCustomer(
    Guid Id,
    string Name,
    string? Email,
    int Version);