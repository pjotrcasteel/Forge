using BenchmarkDotNet.Attributes;

namespace Forge.Benchmarks;

[MemoryDiagnoser]
public sealed class DeltaBenchmarks
{
    private readonly BenchmarkCustomer _before =
        new(Guid.NewGuid(), "Ada", "ada@old.test", 1);

    private readonly BenchmarkCustomer _changed;
    private readonly BenchmarkCustomer _equivalent;

    public DeltaBenchmarks()
    {
        _changed = _before with
        {
            Name = "Ada Lovelace",
            Email = null,
            Version = 2
        };
        _equivalent = _before with { };
    }

    [Benchmark(Baseline = true)]
    public bool ManualEquivalent()
    {
        return _before.Id == _equivalent.Id
               && string.Equals(_before.Name, _equivalent.Name, StringComparison.Ordinal)
               && string.Equals(_before.Email, _equivalent.Email, StringComparison.Ordinal)
               && _before.Version == _equivalent.Version;
    }

    [Benchmark]
    public bool GeneratedEquivalent()
    {
        return BenchmarkCustomerDelta.AreEquivalent(_before, _equivalent);
    }

    [Benchmark]
    public BenchmarkCustomerDelta GeneratedChangedDelta()
    {
        return BenchmarkCustomerDelta.Between(_before, _changed);
    }
}
