namespace Forge.Sync.Tests;

[TestClass]
public sealed class PlanTemplateBuilderTests
{
    [TestMethod]
    public void Build_ShouldValidateDependenciesAndProduceDeterministicWaves()
    {
        var builder = new PlanTemplateBuilder<Operation, OperationId>();
        builder.AddOperation(new OperationId(1), new Operation(OperationKind.Add));
        builder.AddOperation(new OperationId(2), new Operation(OperationKind.Modify));
        builder.AddOperation(new OperationId(3), new Operation(OperationKind.Delete));
        builder.AddDependency(new OperationId(1), new OperationId(2));
        builder.AddDependency(new OperationId(2), new OperationId(3));

        var instance = builder.Build();

        Assert.IsTrue(instance.CanExecute);
        CollectionAssert.AreEqual(
            new[] { new OperationId(1), new OperationId(2), new OperationId(3) },
            instance.DependencyPlan.CreateWaves.SelectMany(static wave => wave.Items).Select(static item => item.Key).ToArray());
    }

    [TestMethod]
    public void Build_WhenDependencyReferencesUnknownOperation_ShouldFail()
    {
        var builder = new PlanTemplateBuilder<Operation, OperationId>();
        builder.AddOperation(new OperationId(1), new Operation(OperationKind.Add));
        builder.AddDependency(new OperationId(1), new OperationId(2));

        Assert.ThrowsExactly<InvalidOperationException>(() => builder.Build());
    }

    private sealed record Operation(OperationKind Kind);
    private readonly record struct OperationId(int Value);

    private enum OperationKind
    {
        Add,
        Modify,
        Delete
    }
}
