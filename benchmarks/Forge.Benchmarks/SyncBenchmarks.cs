using BenchmarkDotNet.Attributes;

namespace Forge.Benchmarks;

[MemoryDiagnoser]
public sealed class SyncBenchmarks
{
    private IReadOnlyList<BenchmarkItem> _current = Array.Empty<BenchmarkItem>();
    private IReadOnlyList<BenchmarkItem> _mostlyUnchanged = Array.Empty<BenchmarkItem>();
    private IReadOnlyList<BenchmarkItem> _mixed = Array.Empty<BenchmarkItem>();

    [Params(100, 10_000)]
    public int Count { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _current = Enumerable.Range(0, Count)
            .Select(index => new BenchmarkItem(index, "Item " + index, 1))
            .ToArray();

        _mostlyUnchanged = Enumerable.Range(0, Count)
            .Select(index => new BenchmarkItem(index, "Item " + index, 1))
            .ToArray();

        _mixed = Enumerable.Range(Count / 10, Count)
            .Select(index => new BenchmarkItem(
                index,
                "Item " + index,
                index % 10 == 0 ? 2 : 1))
            .ToArray();
    }

    [Benchmark]
    public bool MostlyUnchangedEquivalent()
    {
        return BenchmarkItemSync.AreEquivalent(_current, _mostlyUnchanged);
    }

    [Benchmark]
    public object MostlyUnchangedPlan()
    {
        return BenchmarkItemSync.Plan(_current, _mostlyUnchanged);
    }

    [Benchmark]
    public bool MixedEquivalent()
    {
        return BenchmarkItemSync.AreEquivalent(_current, _mixed);
    }

    [Benchmark]
    public object MixedChanges()
    {
        return BenchmarkItemSync.Plan(_current, _mixed);
    }
}
