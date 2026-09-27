using BenchmarkDotNet.Attributes;

namespace Forge.Benchmarks;

[MemoryDiagnoser]
public sealed class NestedSyncBenchmarks
{
    private IReadOnlyList<BenchmarkService> _current = Array.Empty<BenchmarkService>();
    private IReadOnlyList<BenchmarkService> _equivalent = Array.Empty<BenchmarkService>();
    private IReadOnlyList<BenchmarkService> _changed = Array.Empty<BenchmarkService>();

    [Params(100, 1_000)]
    public int Count { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _current = CreateServices(changeLast: false);
        _equivalent = CreateServices(changeLast: false);
        _changed = CreateServices(changeLast: true);
    }

    [Benchmark]
    public bool EquivalentFastPath()
    {
        return BenchmarkServiceSync.AreEquivalent(_current, _equivalent);
    }

    [Benchmark]
    public bool ChangedFastPath()
    {
        return BenchmarkServiceSync.AreEquivalent(_current, _changed);
    }

    [Benchmark]
    public object ChangedPlan()
    {
        return BenchmarkServiceSync.Plan(_current, _changed);
    }

    private IReadOnlyList<BenchmarkService> CreateServices(bool changeLast)
    {
        return Enumerable.Range(0, Count)
            .Select(index => new BenchmarkService(
                index,
                "Active",
                [
                    new BenchmarkCharacteristic("bandwidth", "100"),
                    new BenchmarkCharacteristic(
                        "ipSubnet",
                        changeLast && index == Count - 1 ? "10.2.0.0/24" : "10.1.0.0/24"),
                ]))
            .ToArray();
    }
}
