# Forge.Decide 1.20.0

> Typed, explainable strategy decisions that produce application-owned plans before side effects begin.

**Website:** https://pjotrcasteel.github.io/Forge/ · **NuGet:** https://www.nuget.org/packages/Forge.Decide · **Source:** https://github.com/pjotrcasteel/Forge

Forge.Decide is for applications that have several legitimate ways to handle a problem but need to control which strategies may compete, explain why a strategy applies, and produce a reviewable plan before execution.

## Install

```bash
dotnet add package Forge.Decide --version 1.20.0
```

## The model

Forge.Decide separates five concerns:

1. **Space** — the consumer-defined hard boundary that determines which strategies may compete.
2. **Proposal** — a strategy's side-effect-free answer for the current context.
3. **Applicability** — whether that strategy can handle this particular context.
4. **Selection** — an explicit policy that resolves applicable proposals.
5. **Decision** — the selected application-owned plan plus the evidence used to select it.

The package does not know what your spaces, strategies, contexts, or plans mean.

## Typed strategy spaces

A strategy can exist globally without being allowed to compete everywhere.

```csharp
using Forge.Decide;

public sealed class InteractiveSpace
{
}

var space = StrategySpace<InteractiveSpace>
    .Define<RequestContext, ExecutionPlan>("interactive")
    .Add(new FastPathStrategy())
    .Add(new BalancedStrategy())
    .Build();

var decision = await space.DecideAsync(
    context,
    cancellationToken);
```

A third strategy can be registered in another space without ever seeing this context:

```csharp
public sealed class BatchSpace
{
}

var batch = StrategySpace<BatchSpace>
    .Define<RequestContext, ExecutionPlan>("batch")
    .Add(new ThroughputStrategy())
    .Add(new BalancedStrategy())
    .Build();
```

The marker type makes spaces with the same context and plan types distinct at compile time. Membership is configuration, not a runtime `CanHandle` convention hidden inside every strategy.

## Strategies propose; they do not execute

```csharp
public sealed class FastPathStrategy : IStrategy<RequestContext, ExecutionPlan>
{
    public StrategyId Id => "fast-path";

    public ValueTask<StrategyProposal<ExecutionPlan>> ProposeAsync(
        RequestContext context,
        CancellationToken cancellationToken)
    {
        if (!context.LowLatencyRequired)
        {
            return ValueTask.FromResult(
                StrategyProposal<ExecutionPlan>.NotApplicable(
                    "Low latency is not required."));
        }

        var plan = new ExecutionPlan(
            ["reserve", "dispatch"]);

        return ValueTask.FromResult(
            StrategyProposal<ExecutionPlan>.Applicable(
                plan,
                "The request requires the low-latency path."));
    }
}
```

Forge.Decide stops at the plan boundary. Persisting, approving, transmitting, or executing that plan belongs to the application.

## Safe ambiguity by default

The default selection policy is `ExactlyOne`.

- zero applicable proposals → `NoApplicableStrategyException`;
- one applicable proposal → selected;
- multiple applicable proposals → `AmbiguousStrategyDecisionException`.

Forge.Decide never silently chooses a strategy because it happened to be registered first.

When several proposals are intentionally valid, provide an application-owned `IStrategySelectionPolicy<TContext, TPlan>`.

## Compare without deciding

```csharp
var comparison = await space.CompareAsync(
    context,
    cancellationToken);

foreach (var candidate in comparison.Candidates)
{
    Console.WriteLine(
        $"{candidate.StrategyId}: {candidate.IsApplicable}");
}
```

`CompareAsync` evaluates the admitted strategies and exposes their proposals or rejection reasons without selecting a plan.

This makes dry runs, diagnostics, tests, UIs, shadow evaluation, and future replay tooling possible without adding execution semantics to the core.

## Decision evidence

A decision contains:

```text
SpaceId
SelectedStrategyId
Plan
Candidates
  StrategyId
  Applicable / Rejected
  Reason
  Proposed plan when applicable
```

The core decision contains no generated ID or timestamp. Given the same strategy space, context behavior, proposals, and selection policy, Forge.Decide does not inject nondeterministic metadata into the result.

Applications that need audit receipts can wrap the decision with their own identity, timestamp, correlation, persistence, or transport metadata.

## Boundary

Forge.Decide does **not**:

- know domain operations such as add, change, delete, route, deploy, price, or ship;
- discover strategies by assembly scanning;
- require dependency injection;
- perform persistence or network I/O;
- execute plans;
- hide ambiguity behind registration order;
- turn strategy selection into a general rules or workflow engine.

## Forge family

- **Forge.Delta** — what changed inside this object?
- **Forge.Sync** — how does current state become desired state?
- **Forge.Parse** — does structured dynamic output satisfy this expectation?
- **Forge.Decide** — which valid course of action should become the plan, and why?

.NET 10 · MIT.