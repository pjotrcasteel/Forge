namespace Forge.Sync.Tests;

[TestClass]
public sealed class PlanAlternativeTests
{
    [TestMethod]
    public void Select_ShouldUseApplicationMetricsAndPreferenceWithoutForgeAssumingWhatBestMeans()
    {
        var alternatives = new PlanAlternativeSet<AlternativeId, CandidatePlan>(
        [
            new(new AlternativeId(1), new CandidatePlan(3, 10)),
            new(new AlternativeId(2), new CandidatePlan(5, 2)),
            new(new AlternativeId(3), new CandidatePlan(5, 2))
        ]);

        var evaluated = alternatives.Evaluate(new MetricsEvaluator());
        var selected = evaluated.Select(new PreferLowDisruptionThenOperations());

        Assert.AreEqual(new AlternativeId(2), selected.Selected.Id);
        Assert.IsTrue(selected.HasTie);
        CollectionAssert.AreEqual(
            new[] { new AlternativeId(2), new AlternativeId(3) },
            selected.EquallyPreferred.Select(static item => item.Id).ToArray());
    }

    private sealed record CandidatePlan(int Operations, int Disruption);
    private readonly record struct AlternativeId(int Value);
    private readonly record struct Metrics(int Operations, int Disruption);

    private sealed class MetricsEvaluator : IPlanEvaluator<CandidatePlan, Metrics>
    {
        public Metrics Evaluate(CandidatePlan plan) => new(plan.Operations, plan.Disruption);
    }

    private sealed class PreferLowDisruptionThenOperations : IPlanPreference<Metrics>
    {
        public int Compare(Metrics left, Metrics right)
        {
            var disruption = left.Disruption.CompareTo(right.Disruption);
            return disruption != 0 ? disruption : left.Operations.CompareTo(right.Operations);
        }
    }
}
