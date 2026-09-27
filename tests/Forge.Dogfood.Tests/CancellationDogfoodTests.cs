namespace Forge.Dogfood.Tests;

[TestClass]
public sealed class CancellationDogfoodTests
{
    [TestMethod]
    public void Between_AfterApplicationOwnedCompensation_CapturesActualStateEffect()
    {
        var timestamp = DateTimeOffset.UtcNow;
        var before = new CancellationOutcome("completed", "resource-42", timestamp)
        {
            AssessmentReason = "RevertAfterComplete"
        };
        var after = new CancellationOutcome("removed", null, timestamp)
        {
            AssessmentReason = "Policy already evaluated by application"
        };

        var delta = CancellationOutcomeDelta.Between(before, after);

        CollectionAssert.AreEqual(
            new[] { "ExecutionState", "ResourceReference" },
            delta.Changes.Select(change => change.Path).ToArray());
    }
}
