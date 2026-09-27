using Forge.Delta;

namespace Forge.Delta.Tests;

[TestClass]
public sealed class CollectionComparerTests
{
    [TestMethod]
    public void SequenceComparer_WhenOrderChanges_ShouldNotBeEqual()
    {
        var comparer = new ReadOnlyListSequenceComparer<int>();

        Assert.IsFalse(comparer.Equals([1, 2, 3], [3, 2, 1]));
    }

    [TestMethod]
    public void UnorderedComparer_WhenOrderChanges_ShouldRemainEqual()
    {
        var comparer = new ReadOnlyListUnorderedComparer<int>();

        IReadOnlyList<int> first = [1, 2, 2, 3];
        IReadOnlyList<int> second = [3, 2, 1, 2];

        Assert.IsTrue(comparer.Equals(first, second));
        Assert.AreEqual(comparer.GetHashCode(first), comparer.GetHashCode(second));
        Assert.IsFalse(comparer.Equals([1, 2, 2], [1, 2, 3]));
    }

    [TestMethod]
    public void SetComparer_WhenOnlyDuplicateMultiplicityChanges_ShouldRemainEqual()
    {
        var comparer = new ReadOnlyListSetComparer<int>();

        Assert.IsTrue(comparer.Equals([1, 2, 2], [2, 1]));
    }

    [TestMethod]
    public void DictionaryComparer_WhenEnumerationOrderDiffers_ShouldRemainEqual()
    {
        IReadOnlyDictionary<string, int> first = new Dictionary<string, int>
        {
            ["a"] = 1,
            ["b"] = 2,
        };
        IReadOnlyDictionary<string, int> second = new Dictionary<string, int>
        {
            ["b"] = 2,
            ["a"] = 1,
        };
        var comparer = new ReadOnlyDictionaryComparer<string, int>();

        Assert.IsTrue(comparer.Equals(first, second));
        Assert.AreEqual(comparer.GetHashCode(first), comparer.GetHashCode(second));
    }

    [TestMethod]
    public void KeyedComparer_WhenItemsReorder_ShouldMatchByLogicalKey()
    {
        var comparer = new ReadOnlyListKeyedComparer<KeyedItem, string, KeyedItemSelector>();
        IReadOnlyList<KeyedItem> first = [new("a", "one"), new("b", "two")];
        IReadOnlyList<KeyedItem> second = [new("b", "two"), new("a", "one")];

        Assert.IsTrue(comparer.Equals(first, second));
        Assert.AreEqual(comparer.GetHashCode(first), comparer.GetHashCode(second));
    }

    [TestMethod]
    public void KeyedComparer_WhenValueChangesForSameKey_ShouldNotBeEqual()
    {
        var comparer = new ReadOnlyListKeyedComparer<KeyedItem, string, KeyedItemSelector>();
        IReadOnlyList<KeyedItem> first = [new("a", "one")];
        IReadOnlyList<KeyedItem> second = [new("a", "changed")];

        Assert.IsFalse(comparer.Equals(first, second));
    }

    private sealed record KeyedItem(string Id, string Value);

    private sealed class KeyedItemSelector : IKeySelector<KeyedItem, string>
    {
        public KeyedItemSelector()
        {
        }

        public string GetKey(KeyedItem item) => item.Id;
    }
}
