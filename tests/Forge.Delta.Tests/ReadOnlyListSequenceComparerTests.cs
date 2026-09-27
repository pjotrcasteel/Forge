using Forge.Delta;

namespace Forge.Delta.Tests;

[TestClass]
public sealed class ReadOnlyListSequenceComparerTests
{
    private readonly ReadOnlyListSequenceComparer<string> _comparer = new();

    [TestMethod]
    public void Equals_WhenDifferentInstancesContainSameSequence_ReturnsTrue()
    {
        IReadOnlyList<string> first = ["a", "b"];
        IReadOnlyList<string> second = ["a", "b"];

        Assert.IsTrue(_comparer.Equals(first, second));
        Assert.AreEqual(_comparer.GetHashCode(first), _comparer.GetHashCode(second));
    }

    [TestMethod]
    public void Equals_WhenOrderDiffers_ReturnsFalse()
    {
        IReadOnlyList<string> first = ["a", "b"];
        IReadOnlyList<string> second = ["b", "a"];

        Assert.IsFalse(_comparer.Equals(first, second));
    }

    [TestMethod]
    public void Equals_WhenOneValueIsNull_ReturnsFalse()
    {
        Assert.IsFalse(_comparer.Equals(["a"], null));
    }
}
