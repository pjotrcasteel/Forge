using Forge.Sync;

namespace Forge.Benchmarks;

[GenerateSync(nameof(BenchmarkItem.Id))]
internal sealed record BenchmarkItem(
    int Id,
    string Name,
    int Quantity);