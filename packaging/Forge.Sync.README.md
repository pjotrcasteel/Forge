# Forge.Sync

**Compile-time desired-state reconciliation for .NET 10.**

Forge.Sync turns current and desired keyed state into an explicit reconciliation plan while leaving persistence, transports, workflow policy, and execution in your application.

Installing `Forge.Sync` also brings in `Forge.Delta`.

## Install

```bash
dotnet add package Forge.Sync
```

## Quick start

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

Every update carries a strongly typed generated Delta:

```csharp
foreach (var update in plan.Updated)
{
    foreach (var change in update.Delta.Changes)
    {
        Console.WriteLine($"{update.Key}: {change.Path}: {change.Before} -> {change.After}");
    }
}
```

## Replace and partial Upsert

Complete desired state is the default:

```csharp
var replace = OrderItemSync.Plan(current, desired);
```

For partial input where omitted current items must survive:

```csharp
var upsert = OrderItemSync.Plan(current, payload, SyncMode.Upsert);

upsert.Preserved;
```

The distinction is explicit so a partial patch cannot accidentally become a deletion plan.

## More than same-type collections

Forge.Sync also supports:

- nested keyed reconciliation with `[SyncNested]`;
- current and desired values with different CLR types;
- ordered fallback identities such as instance ID first, business ID second;
- dependency planning and cycle detection;
- typed operation classification;
- ordered streaming reconciliation with `IAsyncEnumerable<T>`;
- node/edge topology reconciliation;
- reversible Replace plans;
- portable manifests, canonical plan digests, stale-plan checks, and batch composition.

## Boundary

Forge calculates and explains the transition. It does **not** persist data, call external systems, publish events, or decide application-specific business policy.

## Runtime characteristics

- .NET 10
- source-generated
- Native AOT and trimming friendly
- no runtime reflection in generated reconciliation paths
- no mandatory DI
- no infrastructure dependency

## Planning beyond structural reconciliation

Forge.Sync 1.8 also includes:

- structured plan explanations;
- source-generated cross-type profiles;
- ordered Delta-aware operation rules;
- incremental semantic replanning with stable application-owned operation IDs;
- graph impact analysis;
- typed pre-execution plan constraints;
- portable plan simulation with stale-plan protection.

These features remain planning primitives: Forge still performs no persistence or external execution.


## Typed planning through 1.20

Forge.Sync 1.20 adds typed conditional dependencies, readiness, safe slices, provenance, plan-to-plan Delta, execution-aware replanning, approval scopes, alternative plans, typed derived facts, lazy scenario matrices, source-generated plan templates, and composite topology invariants.

The application retains its own identity/state/fact/scope/reason types; Forge does not require string registries, reflection-driven rule discovery, or an execution engine.
