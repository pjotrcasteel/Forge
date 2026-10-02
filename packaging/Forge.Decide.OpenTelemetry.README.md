# Forge.Decide.OpenTelemetry 1.20.0

> Lightweight ActivitySource instrumentation for Forge.Decide decisions.

**Website:** https://pjotrcasteel.github.io/Forge/ · **Source:** https://github.com/pjotrcasteel/Forge

Forge.Decide.OpenTelemetry emits standard .NET `ActivitySource` spans around strategy proposals, selection, comparison, decisions and shadow decisions. It intentionally does not configure exporters, providers, sampling or transport.

## Install

```bash
dotnet add package Forge.Decide.OpenTelemetry --version 1.20.0
```

## Subscribe from OpenTelemetry

Register the Forge source in your existing OpenTelemetry setup:

```csharp
tracing.AddSource(ForgeDecideTelemetry.ActivitySourceName);
```

The adapter has no runtime dependency on the OpenTelemetry SDK. `ActivitySource` is the standard .NET tracing primitive consumed by OpenTelemetry.

## Instrument the strategy boundary

```csharp
var space = StrategySpace<InteractiveSpace>
    .Define<RequestContext, ExecutionPlan>("interactive")
    .Add(new FastPathStrategy().WithOpenTelemetry())
    .Add(new BalancedStrategy().WithOpenTelemetry())
    .SelectWith(
        StrategySelectionPolicies
            .LowestBy<RequestContext, ExecutionPlan, int>(
                (_, candidate) => candidate.Plan.EstimatedCost)
            .WithOpenTelemetry())
    .Build();

var decision = await space.DecideWithOpenTelemetryAsync(
    context,
    cancellationToken);
```

## Shadow decisions

A frozen comparison can trace production and shadow selection without re-running strategy proposals:

```csharp
var comparison = await space.CompareWithOpenTelemetryAsync(
    context,
    cancellationToken);

var shadow = await comparison.DecideWithShadowOpenTelemetryAsync(
    context,
    productionPolicy.WithOpenTelemetry(),
    shadowPolicy.WithOpenTelemetry(),
    cancellationToken);
```

## Activities

The package emits activities such as:

```text
forge.decide.decide
forge.decide.compare
forge.decide.strategy.propose
forge.decide.selection
forge.decide.shadow
```

Tags include stable identifiers and counts such as strategy space, strategy id, selected strategy, applicability and candidate counts. Application plan contents and application-provided explanation text are not emitted as tags by default.

## Boundary

This package observes Forge.Decide. It does not change candidate membership, applicability, selection policy behavior, plan contents or execution.

.NET 10 · MIT.