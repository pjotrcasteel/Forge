using BenchmarkDotNet.Attributes;
using Forge.Delta;

namespace Forge.Benchmarks;

[MemoryDiagnoser]
public sealed class CollectionComparerBenchmarks
{
    private readonly ReadOnlyListUnorderedComparer<int> _unordered = new();
    private readonly ReadOnlyDictionaryComparer<int, string> _dictionary = new();
    private IReadOnlyList<int> _ordered = Array.Empty<int>();
    private IReadOnlyList<int> _reversed = Array.Empty<int>();
    private IReadOnlyDictionary<int, string> _firstDictionary = new Dictionary<int, string>();
    private IReadOnlyDictionary<int, string> _secondDictionary = new Dictionary<int, string>();

    [Params(100, 10_000)]
    public int Count { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _ordered = Enumerable.Range(0, Count).ToArray();
        _reversed = Enumerable.Range(0, Count).Reverse().ToArray();
        _firstDictionary = Enumerable.Range(0, Count).ToDictionary(index => index, index => index.ToString());
        _secondDictionary = Enumerable.Range(0, Count).Reverse().ToDictionary(index => index, index => index.ToString());
    }

    [Benchmark]
    public bool UnorderedListEquality()
    {
        return _unordered.Equals(_ordered, _reversed);
    }

    [Benchmark]
    public bool DictionaryEquality()
    {
        return _dictionary.Equals(_firstDictionary, _secondDictionary);
    }
}
