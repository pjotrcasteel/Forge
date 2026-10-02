using Forge.Decide;
using Forge.Decide.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddForgeDecisionSpace<ConsumerSpace, ConsumerContext, ConsumerPlan>(
    "consumer",
    space => space.Add<ConsumerStrategy>());

await using var provider = services.BuildServiceProvider();
var decisionSpace = provider.GetRequiredService<StrategySpace<ConsumerSpace, ConsumerContext, ConsumerPlan>>();
var decision = await decisionSpace.DecideAsync(new ConsumerContext(), CancellationToken.None);

if (decision.SelectedStrategyId.Value != "consumer")
{
    throw new InvalidOperationException("Forge.Decide.DependencyInjection package consumer validation failed.");
}

Console.WriteLine("Forge.Decide.DependencyInjection package consumer validation passed.");

internal sealed class ConsumerSpace
{
}

internal sealed record ConsumerContext;

internal sealed record ConsumerPlan(string Value);

internal sealed class ConsumerStrategy : IStrategy<ConsumerContext, ConsumerPlan>
{
    public StrategyId Id => "consumer";

    public ValueTask<StrategyProposal<ConsumerPlan>> ProposeAsync(ConsumerContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(StrategyProposal<ConsumerPlan>.Applicable(new ConsumerPlan("ok")));
    }
}