using Forge.Delta;

namespace Forge.Delta.Tests;

[TestClass]
public sealed class MergeResolutionTests
{
    [TestMethod]
    public void Resolve_WhenBranchesChangeDifferentProperties_ShouldAutoResolveBoth()
    {
        var baseline = new Customer(Guid.NewGuid(), "Alice", "old@example.com", new Address("Amsterdam", "NL"));
        var current = baseline with { Name = "Alicia" };
        var desired = baseline with { Email = "new@example.com" };
        var analysis = CustomerDelta.AnalyzeMerge(baseline, current, desired);

        var result = MergeResolver.Resolve(analysis);

        Assert.IsTrue(result.IsFullyResolved);
        Assert.AreEqual(2, result.Values.Count);
        Assert.AreEqual("Alicia", result.Values.Single(value => value.Path == "Name").Value);
        Assert.AreEqual("new@example.com", result.Values.Single(value => value.Path == "Email").Value);
    }

    [TestMethod]
    public void Resolve_WhenConflictHasPathPolicy_ShouldUseConfiguredBranch()
    {
        var baseline = new Customer(Guid.NewGuid(), "Alice", "old@example.com", new Address("Amsterdam", "NL"));
        var current = baseline with { Email = "work@example.com" };
        var desired = baseline with { Email = "private@example.com" };
        var analysis = CustomerDelta.AnalyzeMerge(baseline, current, desired);
        var policy = new MergeResolutionPolicy().PreferDesired("Email");

        var result = MergeResolver.Resolve(analysis, policy);

        Assert.IsTrue(result.IsFullyResolved);
        var resolved = result.Values.Single();
        Assert.AreEqual("private@example.com", resolved.Value);
        Assert.AreEqual(MergeResolutionSource.Desired, resolved.Source);
    }

    [TestMethod]
    public void Resolve_WhenConflictHasNoPolicy_ShouldRemainExplicitlyUnresolved()
    {
        var baseline = new Customer(Guid.NewGuid(), "Alice", "old@example.com", new Address("Amsterdam", "NL"));
        var current = baseline with { Email = "work@example.com" };
        var desired = baseline with { Email = "private@example.com" };

        var result = MergeResolver.Resolve(CustomerDelta.AnalyzeMerge(baseline, current, desired));

        Assert.IsFalse(result.IsFullyResolved);
        Assert.AreEqual("Email", result.UnresolvedConflicts.Single().Path);
    }
}
