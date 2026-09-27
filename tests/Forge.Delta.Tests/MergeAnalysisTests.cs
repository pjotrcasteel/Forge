namespace Forge.Delta.Tests;

[TestClass]
public sealed class MergeAnalysisTests
{
    [TestMethod]
    public void AnalyzeMerge_WhenDifferentPropertiesChange_ShouldNotConflict()
    {
        var baseline = new Customer(Guid.NewGuid(), "Pjotr", "old@example.com", new Address("Apeldoorn", "1"));
        var current = baseline with { Email = "new@example.com" };
        var desired = baseline with { Name = "Peter" };

        var analysis = CustomerDelta.AnalyzeMerge(baseline, current, desired);

        Assert.IsFalse(analysis.HasConflicts);
        Assert.IsTrue(analysis.CanAutoMerge);
        Assert.IsTrue(analysis.CurrentDelta.EmailChange.HasChanged);
        Assert.IsTrue(analysis.DesiredDelta.NameChange.HasChanged);
    }

    [TestMethod]
    public void AnalyzeMerge_WhenSamePropertyChangesDifferently_ShouldReportConflict()
    {
        var baseline = new Customer(Guid.NewGuid(), "Pjotr", "old@example.com", new Address("Apeldoorn", "1"));
        var current = baseline with { Email = "work@example.com" };
        var desired = baseline with { Email = "private@example.com" };

        var analysis = CustomerDelta.AnalyzeMerge(baseline, current, desired);

        Assert.IsTrue(analysis.HasConflicts);
        Assert.AreEqual("Email", analysis.Conflicts[0].Path);
    }

    [TestMethod]
    public void AnalyzeMerge_ShouldRespectConfiguredComparer()
    {
        var baseline = new Customer(Guid.NewGuid(), "Pjotr", null, new Address("Apeldoorn", "1"));
        var current = baseline with { Name = "PJOTR" };
        var desired = baseline with { Name = "pjotr" };

        var analysis = CustomerDelta.AnalyzeMerge(baseline, current, desired);

        Assert.IsFalse(analysis.HasConflicts);
    }
}
