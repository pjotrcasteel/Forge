namespace Forge.Sync.Tests;

[TestClass]
public sealed class ApprovalScopePlannerTests
{
    [TestMethod]
    public void Plan_ShouldCollapseOperationDependenciesIntoTypedScopeDependencies()
    {
        Operation[] operations =
        [
            new(new OperationId(1), ApprovalScope.Automatic),
            new(new OperationId(2), ApprovalScope.Network),
            new(new OperationId(3), ApprovalScope.Network),
            new(new OperationId(4), ApprovalScope.Security)
        ];
        DependencyEdge<OperationId>[] dependencies =
        [
            new(new OperationId(1), new OperationId(2)),
            new(new OperationId(2), new OperationId(3)),
            new(new OperationId(3), new OperationId(4))
        ];

        var plan = ApprovalScopePlanner.Plan(
            operations,
            static operation => operation.Id,
            dependencies,
            new ScopeSelector());

        Assert.AreEqual(3, plan.Scopes.Count);
        Assert.AreEqual(2, plan.Dependencies.Count);
        Assert.IsFalse(plan.HasCycles);
        CollectionAssert.AreEqual(
            new[] { ApprovalScope.Automatic, ApprovalScope.Network, ApprovalScope.Security },
            plan.DependencyPlan.CreateWaves.SelectMany(static wave => wave.Items).Select(static group => group.Scope).ToArray());
    }

    [TestMethod]
    public void Plan_WhenScopeCollapseCreatesCycle_ShouldSurfaceCycleBeforeApproval()
    {
        Operation[] operations =
        [
            new(new OperationId(1), ApprovalScope.Network),
            new(new OperationId(2), ApprovalScope.Security),
            new(new OperationId(3), ApprovalScope.Network)
        ];
        DependencyEdge<OperationId>[] dependencies =
        [
            new(new OperationId(1), new OperationId(2)),
            new(new OperationId(2), new OperationId(3))
        ];

        var plan = ApprovalScopePlanner.Plan(
            operations,
            static operation => operation.Id,
            dependencies,
            new ScopeSelector());

        Assert.IsTrue(plan.HasCycles);
    }

    private sealed record Operation(OperationId Id, ApprovalScope Scope);
    private readonly record struct OperationId(int Value);

    private enum ApprovalScope
    {
        Automatic,
        Network,
        Security
    }

    private sealed class ScopeSelector : IApprovalScopeSelector<Operation, ApprovalScope>
    {
        public ApprovalScope Select(Operation operation) => operation.Scope;
    }
}
