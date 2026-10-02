using Forge.Decide;
using Forge.Decide.Testing;

var space = StrategySpace<SampleSpace>
    .Define<SampleContext, SamplePlan>("sample")
    .Add(new PositiveStrategy())
    .Build();
var scenarios = new[]
{
    new StrategyScenario<SampleContext>("positive", new SampleContext(1)),
    new StrategyScenario<SampleContext>("uncovered", new SampleContext(0)),
};
var result = await StrategyScenarioMatrix.EvaluateAsync(space, scenarios, CancellationToken.None);

if (result.Scenarios[0].Status != StrategyScenarioStatus.Selected ||
    result.Scenarios[1].Status != StrategyScenarioStatus.NoApplicableStrategy)
{
    throw new InvalidOperationException("Forge.Decide.Testing package consumer validation failed.");
}

Console.WriteLine("Forge.Decide.Testing package consumer validation passed.");

internal sealed class SampleSpace
{
}

internal sealed record SampleContext(int Value);

internal sealed record SamplePlan(int Value);

internal sealed class PositiveStrategy : IStrategy<SampleContext, SamplePlan>
{
    public StrategyId Id => "positive";

    public ValueTask<StrategyProposal<SamplePlan>> ProposeAsync(SampleContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(context.Value > 0
            ? StrategyProposal<SamplePlan>.Applicable(new SamplePlan(context.Value))
            : StrategyProposal<SamplePlan>.NotApplicable("Value must be positive."));
    }
}