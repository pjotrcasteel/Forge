using Forge.Sync;

namespace Forge.Benchmarks;

[GenerateSync(nameof(BenchmarkCharacteristic.Name))]
internal sealed record BenchmarkCharacteristic(
    string Name,
    string Value);

[GenerateSync(nameof(BenchmarkService.Id))]
internal sealed record BenchmarkService(
    int Id,
    string State,
    [property: SyncNested]
    IReadOnlyList<BenchmarkCharacteristic> Characteristics);
