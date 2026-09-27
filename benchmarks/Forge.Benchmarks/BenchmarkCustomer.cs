using Forge.Delta;

namespace Forge.Benchmarks;

[GenerateDelta]
internal sealed record BenchmarkCustomer(
    Guid Id,
    string Name,
    string? Email,
    int Version);