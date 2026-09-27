namespace Forge.Sync.Tests;

[TestClass]
public sealed class PlanSlicerTests
{
    [TestMethod]
    public void Slice_ShouldIncludeTransitivePredecessorClosureWithoutIncludingDependents()
    {
        Operation[] items =
        [
            new(new OperationId(1), [], false),
            new(new OperationId(2), [new OperationId(1)], false),
            new(new OperationId(3), [new OperationId(2)], true),
            new(new OperationId(4), [new OperationId(3)], false)
        ];

        var slice = PlanSlicer.Slice(
            items,
            static item => item.Id,
            static item => item.DependsOn,
            new RequestedSelector());

        CollectionAssert.AreEqual(
            new[] { new OperationId(1), new OperationId(2), new OperationId(3) },
            slice.Included.Select(static item => item.Id).ToArray());
        CollectionAssert.AreEqual(
            new[] { new OperationId(3) },
            slice.DirectlySelected.Select(static item => item.Id).ToArray());
        CollectionAssert.AreEqual(
            new[] { new OperationId(1), new OperationId(2) },
            slice.RequiredDependencies.Select(static item => item.Id).ToArray());
        CollectionAssert.AreEqual(
            new[] { new OperationId(4) },
            slice.Excluded.Select(static item => item.Id).ToArray());
    }

    [TestMethod]
    public void Slice_WhenDependenciesContainCycle_ShouldTerminateAndReturnClosureOnce()
    {
        Operation[] items =
        [
            new(new OperationId(1), [new OperationId(2)], true),
            new(new OperationId(2), [new OperationId(1)], false)
        ];

        var slice = PlanSlicer.Slice(
            items,
            static item => item.Id,
            static item => item.DependsOn,
            new RequestedSelector());

        Assert.AreEqual(2, slice.Included.Count);
        Assert.AreEqual(1, slice.RequiredDependencies.Count);
    }

    private sealed record Operation(OperationId Id, IReadOnlyList<OperationId> DependsOn, bool Requested);
    private readonly record struct OperationId(int Value);

    private readonly struct RequestedSelector : IPlanSliceSelector<Operation>
    {
        public bool Include(Operation item) => item.Requested;
    }
}
