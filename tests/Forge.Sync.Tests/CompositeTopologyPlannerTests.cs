namespace Forge.Sync.Tests;

[TestClass]
public sealed class CompositeTopologyPlannerTests
{
    [TestMethod]
    public void Compose_ShouldOrderCrossTopologyOperationsAndEvaluateTypedInvariants()
    {
        var context = new CompositeContext(
            RemovedResources: [new ResourceId(7)],
            DesiredServiceReferences: [new ResourceId(7)]);
        CompositeOperation[] operations =
        [
            new(new CompositeOperationId(1), CompositeOperationKind.UpdateService),
            new(new CompositeOperationId(2), CompositeOperationKind.RemoveResource)
        ];
        DependencyEdge<CompositeOperationId>[] dependencies =
        [
            new(new CompositeOperationId(1), new CompositeOperationId(2))
        ];

        var plan = CompositeTopologyPlanner.Compose(
            context,
            operations,
            static operation => operation.Id,
            dependencies,
            new ICompositeTopologyInvariant<CompositeContext, TopologyViolation>[]
            {
                new ReferencedResourceCannotBeRemoved()
            });

        Assert.IsFalse(plan.IsValid);
        Assert.AreEqual(new TopologyViolation(new ResourceId(7)), plan.Violations.Single());
        CollectionAssert.AreEqual(
            new[] { CompositeOperationKind.UpdateService, CompositeOperationKind.RemoveResource },
            plan.DependencyPlan.CreateWaves.SelectMany(static wave => wave.Items).Select(static item => item.Kind).ToArray());
    }

    [TestMethod]
    public void Compose_WhenCrossTopologyStateIsValid_ShouldBeExecutable()
    {
        var context = new CompositeContext(
            RemovedResources: [new ResourceId(7)],
            DesiredServiceReferences: [new ResourceId(8)]);
        CompositeOperation[] operations = [new(new CompositeOperationId(1), CompositeOperationKind.RemoveResource)];

        var plan = CompositeTopologyPlanner.Compose(
            context,
            operations,
            static operation => operation.Id,
            [],
            new ICompositeTopologyInvariant<CompositeContext, TopologyViolation>[]
            {
                new ReferencedResourceCannotBeRemoved()
            });

        Assert.IsTrue(plan.IsValid);
        Assert.AreEqual(0, plan.Violations.Count);
    }

    private sealed record CompositeContext(
        IReadOnlyList<ResourceId> RemovedResources,
        IReadOnlyList<ResourceId> DesiredServiceReferences);

    private sealed record CompositeOperation(CompositeOperationId Id, CompositeOperationKind Kind);
    private readonly record struct CompositeOperationId(int Value);
    private readonly record struct ResourceId(int Value);
    private sealed record TopologyViolation(ResourceId ReferencedResource);

    private enum CompositeOperationKind
    {
        UpdateService,
        RemoveResource
    }

    private sealed class ReferencedResourceCannotBeRemoved : ICompositeTopologyInvariant<CompositeContext, TopologyViolation>
    {
        public IReadOnlyList<TopologyViolation> Validate(CompositeContext context)
        {
            var referenced = context.RemovedResources
                .Where(context.DesiredServiceReferences.Contains)
                .Select(static resource => new TopologyViolation(resource))
                .ToArray();
            return referenced;
        }
    }
}
