# Forge.Sync 1.20.0

> Source-generated desired-state reconciliation and typed transition planning for .NET 10.
>
> **Website:** https://pjotrcasteel.github.io/Forge/ · **NuGet:** https://www.nuget.org/packages/Forge.Sync · **Source:** https://github.com/pjotrcasteel/Forge

Forge.Sync answers one question first:

> **How does the state I have become the state I want?**

It produces an explicit reconciliation/transition plan while persistence, transport, workflow policy, authorization, and execution stay in your application.

Installing `Forge.Sync` also brings in `Forge.Delta`.

## Install

```bash
dotnet add package Forge.Sync --version 1.20.0
```

## Five-minute start

```csharp
using Forge.Sync;

[GenerateSync(nameof(OrderItem.Id))]
public sealed record OrderItem(
    string Id,
    string Product,
    int Quantity);

var plan = OrderItemSync.Plan(current, desired);

plan.Added;
plan.Updated;
plan.Removed;
plan.Unchanged;
```

Every update carries its generated typed Delta:

```csharp
foreach (var update in plan.Updated)
{
    foreach (var change in update.Delta.Changes)
    {
        Console.WriteLine(
            $"{update.Key}: {change.Path}: {change.Before} -> {change.After}");
    }
}
```

## Replace versus partial Upsert

Complete desired state is the default:

```csharp
var replace = OrderItemSync.Plan(current, desired);
```

For partial input where omitted current items must remain untouched:

```csharp
var upsert = OrderItemSync.Plan(
    current,
    payload,
    SyncMode.Upsert);

upsert.Preserved;
```

The distinction is explicit so a partial payload cannot accidentally become a deletion plan.

## When Forge.Sync is useful

Forge.Sync is a strong fit for:

- desired-state APIs;
- provisioning/deprovisioning planning;
- nested keyed collections;
- cross-type current/desired models;
- ordered fallback identities;
- dependency-aware create/delete waves;
- topology reconciliation;
- portable plans and stale-state checks;
- plan validation and explanation;
- incremental and execution-aware replanning.

## Dependency-aware planning

```csharp
var dependencyPlan = DependencyPlanner.Plan(
    operations,
    static item => item.Id,
    static item => item.DependsOn);

foreach (var wave in dependencyPlan.CreateWaves)
{
    // Safe after every previous wave completed.
}
```

Cycles are surfaced before execution.

## Planning boundary

Forge calculates, classifies, validates, explains, and simulates the transition.

It does **not** persist state, call external systems, publish events, authorize operations, discover runtime plugins, or execute plans.

## Typed planning through 1.20

Forge.Sync 1.20 includes typed conditional dependencies, readiness, safe plan slices, provenance, plan-to-plan Delta, execution-aware replanning, approval scopes, alternative plans, derived facts, scenario matrices, plan templates, and composite topology invariants.

Applications retain their own identity, state, fact, scope, reason, metric, and violation types; Forge does not require string registries or reflection-driven rule discovery.

## Runtime characteristics

- .NET 10;
- source-generated normal reconciliation hot path;
- Native AOT and trimming friendly;
- no runtime reflection in generated reconciliation paths;
- no mandatory DI;
- no infrastructure dependency.

Full documentation: https://pjotrcasteel.github.io/Forge/
