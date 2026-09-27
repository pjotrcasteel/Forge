using Forge.Delta;

namespace Forge.Sync.Tests;

[TestClass]
public sealed class PlanDeltaTests
{
    [TestMethod]
    public void Between_ShouldDescribeOperationAndDependencyChangesIndependently()
    {
        Operation[] previousOperations = [new(new OperationId(1), "old"), new(new OperationId(2), "same")];
        Operation[] nextOperations = [new(new OperationId(1), "new"), new(new OperationId(2), "same"), new(new OperationId(3), "add")];
        Dependency[] previousDependencies = [new(new DependencyId(1), new OperationId(1), new OperationId(2))];
        Dependency[] nextDependencies = [new(new DependencyId(1), new OperationId(1), new OperationId(3))];

        var delta = PlanDelta.Between(
            previousOperations,
            nextOperations,
            OperationDefinition(),
            previousDependencies,
            nextDependencies,
            DependencyDefinition());

        Assert.IsTrue(delta.HasChanges);
        Assert.AreEqual(1, delta.Operations.Added.Count);
        Assert.AreEqual(1, delta.Operations.Updated.Count);
        Assert.AreEqual(1, delta.Operations.Unchanged.Count);
        Assert.AreEqual(1, delta.Dependencies.Updated.Count);
        Assert.AreEqual("Target", delta.Dependencies.Updated[0].Delta.Changes.Single().Path);
    }

    private static PlanElementDefinition<Operation, OperationId, TestDelta> OperationDefinition()
        => new(
            static operation => operation.Id,
            static (left, right) => left.Value == right.Value,
            static (left, right) => new TestDelta("Value", left.Value, right.Value));

    private static PlanElementDefinition<Dependency, DependencyId, TestDelta> DependencyDefinition()
        => new(
            static dependency => dependency.Id,
            static (left, right) => left.Source == right.Source && left.Target == right.Target,
            static (left, right) => new TestDelta("Target", left.Target, right.Target));

    private sealed record Operation(OperationId Id, string Value);
    private sealed record Dependency(DependencyId Id, OperationId Source, OperationId Target);
    private readonly record struct OperationId(int Value);
    private readonly record struct DependencyId(int Value);

    private sealed class TestDelta : IDelta
    {
        public TestDelta(string path, object? before, object? after)
        {
            Changes = Equals(before, after) ? [] : [new PropertyChange(path, before, after)];
        }

        public bool HasChanges => Changes.Count != 0;
        public IReadOnlyList<PropertyChange> Changes { get; }
    }
}
