using Forge.Delta;

namespace Forge.Sync.Tests;

[TestClass]
public sealed class StreamingSyncTests
{
    [TestMethod]
    public async Task PlanOrderedAsync_ShouldMergeOrderedStreams()
    {
        var current = AsAsync(new Item("a", "old"), new Item("c", "remove"));
        var desired = AsAsync(new Item("a", "new"), new Item("b", "add"));
        var steps = new List<IStreamingSyncStep<string>>();
        var definition = Definition();

        await foreach (var step in StreamingSync.PlanOrderedAsync(
                           current,
                           desired,
                           definition,
                           StringComparer.Ordinal,
                           TestContext.CancellationToken))
        {
            steps.Add(step);
        }

        CollectionAssert.AreEqual(
            new[]
            {
                StreamingSyncChangeKind.Updated,
                StreamingSyncChangeKind.Added,
                StreamingSyncChangeKind.Removed
            },
            steps.Select(static step => step.Kind).ToArray());
    }

    [TestMethod]
    public async Task PlanOrderedAsync_WhenInputIsNotStrictlyOrdered_ShouldFail()
    {
        var current = AsAsync(new Item("b", "x"), new Item("a", "x"));
        var desired = AsAsync<Item>();
        var definition = Definition();

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () =>
        {
            await foreach (var _ in StreamingSync.PlanOrderedAsync(
                               current,
                               desired,
                               definition,
                               StringComparer.Ordinal,
                               TestContext.CancellationToken))
            {
            }
        });
    }

    private static CrossSyncDefinition<Item, Item, string, TestDelta> Definition()
        => new(
            static item => item.Id,
            static item => item.Id,
            static (left, right) => left.Value == right.Value,
            static (left, right) => new TestDelta(left.Value, right.Value));

    private static async IAsyncEnumerable<T> AsAsync<T>(params T[] items)
    {
        foreach (var item in items)
        {
            await Task.Yield();
            yield return item;
        }
    }

    private sealed record Item(string Id, string Value);

    private sealed class TestDelta : IDelta
    {
        public TestDelta(string before, string after)
        {
            Changes = before == after ? [] : [new PropertyChange("Value", before, after)];
        }

        public bool HasChanges => Changes.Count != 0;
        public IReadOnlyList<PropertyChange> Changes { get; }
    }
}
