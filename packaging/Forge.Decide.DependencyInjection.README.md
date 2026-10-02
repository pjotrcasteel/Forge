# Forge.Decide.DependencyInjection 1.20.0

> Explicit Microsoft.Extensions.DependencyInjection registration for typed Forge.Decide strategy spaces.

**Website:** https://pjotrcasteel.github.io/Forge/ · **Source:** https://github.com/pjotrcasteel/Forge

The core `Forge.Decide` package does not require dependency injection. Add this package when an application already uses `Microsoft.Extensions.DependencyInjection` and wants strategy instances, their dependencies, and typed spaces resolved from the container.

## Install

```bash
dotnet add package Forge.Decide.DependencyInjection --version 1.20.0
```

## Register a typed space

```csharp
services.AddForgeDecisionSpace<InteractiveSpace, RequestContext, ExecutionPlan>(
    "interactive",
    space => space
        .Add<FastPathStrategy>()
        .Add<BalancedStrategy>()
        .SelectWith(provider =>
            StrategySelectionPolicies.LowestBy<RequestContext, ExecutionPlan, int>(
                (_, candidate) => candidate.Plan.EstimatedCost)));
```

Then inject the normal core type:

```csharp
public sealed class Planner(
    StrategySpace<InteractiveSpace, RequestContext, ExecutionPlan> space)
{
    public ValueTask<StrategyDecision<InteractiveSpace, ExecutionPlan>> PlanAsync(
        RequestContext context,
        CancellationToken cancellationToken) =>
        space.DecideAsync(context, cancellationToken);
}
```

There is no separate DI-specific decision engine.

## Existing registrations are respected

If a strategy or policy type is already registered by the application, Forge.Decide.DependencyInjection reuses that registration rather than replacing its lifetime or factory. This allows strategies to use scoped or singleton application dependencies naturally.

## Hard candidate boundaries remain explicit

Only strategy types named in a space registration are admitted to that space. The adapter does not scan assemblies and does not automatically add every `IStrategy<TContext, TPlan>` from the container.

That means two spaces with the same context and plan types can still have completely different candidate sets.

## Boundary

This package only adapts space construction to Microsoft.Extensions.DependencyInjection. It does not change strategy applicability, selection semantics, plan ownership, persistence, or execution.

.NET 10 · MIT.