# Forge

**Compile-time state transition tooling for .NET 10.**

Forge turns current state and desired state into typed, explainable transition plans while keeping persistence, transports, workflow policy and execution in the application.

The first stable family contains:

- **Forge.Delta** — semantic object differences, nested changes, three-way conflict analysis and reversible deltas.
- **Forge.Sync** — keyed reconciliation, partial upserts, nested/cross-type/streaming/topology reconciliation and portable execution planning.

Normal generated Delta/Sync hot paths use direct property access and dictionaries: no reflection, no dynamic proxies and no hidden I/O.

## Install

```bash
dotnet add package Forge.Delta
# or, for reconciliation (includes Forge.Delta):
dotnet add package Forge.Sync
```

## Delta

```csharp
using Forge.Delta;

[GenerateDelta]
public sealed record Customer(Guid Id, string Name, string? Email);

var delta = CustomerDelta.Between(before, after);

if (delta.EmailChange.HasChanged)
{
    Console.WriteLine($"{delta.EmailChange.Before} -> {delta.EmailChange.After}");
}
```

Generated Delta also supports:

```csharp
CustomerDelta.AreEquivalent(before, after);
CustomerDelta.GetSemanticHashCode(after);
CustomerDelta.AnalyzeMerge(baseline, current, desired);
delta.Invert();
```

### Explicit semantics

Forge supports:

- `[DeltaIgnore]` for non-domain bookkeeping state;
- `[DeltaComparer]` for domain equality;
- nested generated Delta state with flattened paths such as `Address.City`;
- external/unannotated models through `[GenerateDeltaProfile]`;
- explicit sequence, unordered, set, dictionary and keyed-list collection comparers.

Forge does not guess collection semantics or recursively walk arbitrary object graphs.

## Sync

```csharp
using Forge.Sync;

[GenerateSync(nameof(OrderItem.Id))]
public sealed record OrderItem(string Id, string Product, int Quantity);

var plan = OrderItemSync.Plan(current, desired);

plan.Added;
plan.Updated;
plan.Removed;
plan.Unchanged;
```

Every update carries its generated Delta:

```csharp
foreach (var update in plan.Updated)
{
    foreach (var change in update.Delta.Changes)
    {
        Console.WriteLine($"{update.Key}: {change.Path}: {change.Before} -> {change.After}");
    }
}
```

### Replace vs partial Upsert

Complete desired state is the default:

```csharp
var replace = OrderItemSync.Plan(current, desired);
```

For partial input where omitted current items must remain untouched:

```csharp
var upsert = OrderItemSync.Plan(current, payload, SyncMode.Upsert);

upsert.Preserved;
```

The distinction is explicit so a partial update payload cannot accidentally become a delete plan.

### Nested reconciliation

```csharp
[GenerateSync(nameof(Service.Id))]
public sealed record Service(
    string Id,
    [property: SyncNested]
    IReadOnlyList<Characteristic> Characteristics);
```

Parent classification includes child changes and generated `PlanCharacteristics(...)` helpers expose the child plan.

## Cross-type reconciliation

Current and desired state do not need to use the same CLR type:

```csharp
var definition = new CrossSyncDefinition<ServiceCharacteristicNode, CharacteristicModel, string, CharacteristicDelta>(
    static node => node.CharacteristicId,
    static model => model.CharacteristicId,
    CharacteristicDelta.AreEquivalent,
    CharacteristicDelta.Between,
    SyncMode.Upsert,
    StringComparer.OrdinalIgnoreCase);

var plan = CrossSync.Plan(existing, incoming, definition);
```

Cross-type plans support operation classification, portable manifests and compensation inversion too.

### Ordered fallback identity

Some systems have more than one legitimate identity route, for example a persisted instance ID first and a business key as fallback. Model that explicitly instead of normalizing away the distinction:

```csharp
var definition = new CrossSyncMatchDefinition<StoredCharacteristic, IncomingCharacteristic, string, CharacteristicDelta>(
    static current => new SyncIdentity<string>(
        $"id:{current.Id}",
        [$"characteristic:{current.CharacteristicId}"]),
    static desired => desired.Id == Guid.Empty
        ? new SyncIdentity<string>($"characteristic:{desired.CharacteristicId}")
        : new SyncIdentity<string>(
            $"id:{desired.Id}",
            [$"characteristic:{desired.CharacteristicId}"]),
    CharacteristicDelta.AreEquivalent,
    CharacteristicDelta.Between,
    SyncMode.Replace,
    StringComparer.OrdinalIgnoreCase);

var plan = CrossSync.Plan(current, desired, definition);
```

Matching tries the canonical key first and only then the fallback keys in order. If a fallback identity resolves to multiple current items, Forge fails before producing an ambiguous plan. Matched results keep the current item's canonical identity; additions use the desired item's canonical identity.

## Dependency-aware planning

```csharp
var dependencyPlan = DependencyPlanner.Plan(
    operations,
    static item => item.Id,
    static item => item.DependsOn);

foreach (var wave in dependencyPlan.CreateWaves)
{
    // Items in one wave can execute after all previous waves complete.
}

foreach (var wave in dependencyPlan.DeleteWaves)
{
    // Dependent-first reverse order for deletion/deprovisioning.
}
```

Cycles are surfaced before execution.

## Typed operation planning

Forge can translate structural changes to application-defined operations without executing them:

```csharp
var operations = SyncOperationPlanner.Classify(
    plan,
    static _ => Operation.Create,
    static update => update.Delta.ProductChange.HasChanged
        ? Operation.Replace
        : Operation.Update,
    static _ => Operation.Delete);
```

## Streaming reconciliation

Large ordered sources can be reconciled without materializing both complete datasets:

```csharp
var streamDefinition = new CrossSyncDefinition<Resource, Resource, string, ResourceDelta>(
    static item => item.Id,
    static item => item.Id,
    ResourceDelta.AreEquivalent,
    ResourceDelta.Between);

await foreach (var step in StreamingSync.PlanOrderedAsync(
                   currentStream,
                   desiredStream,
                   streamDefinition,
                   StringComparer.Ordinal,
                   cancellationToken))
{
    // Added / Updated / Removed / Unchanged / Preserved
}
```

Both sources must be strictly ordered by logical key. Cancellation is explicit.

## Topology reconciliation

Forge can reconcile graph nodes and relationships as one validated transition:

```csharp
var topology = TopologySync.Plan(
    currentNodes,
    desiredNodes,
    currentEdges,
    desiredEdges,
    nodeDefinition,
    edgeDefinition);
```

Edges are validated against their snapshot's nodes before reconciliation. The plan exposes graph-safe structural phases:

```text
RemoveEdges -> RemoveNodes -> AddNodes -> UpdateNodes -> UpdateEdges -> AddEdges
```

## Three-way conflict analysis

```csharp
var merge = CustomerDelta.AnalyzeMerge(
    baseline,
    current,
    desired);

if (merge.HasConflicts)
{
    foreach (var conflict in merge.Conflicts)
    {
        Console.WriteLine(conflict.Path);
    }
}
```

Forge reports semantic same-property conflicts; the application owns resolution policy.

## Reversible transitions

```csharp
var reverseDelta = delta.Invert();
var compensation = SyncPlanInverter.Invert(
    plan,
    static itemDelta => itemDelta.Invert());
```

Upsert plans containing preserved state are deliberately rejected as non-reversible because omitted desired state is unknown.

## Portable plans

```csharp
var manifest = SyncManifest.Create(
    plan,
    static key => key.ToString());

var json = manifest.ToJson();
var digest = ManifestDigest.ComputeSha256Hex(manifest);
```

Portable manifests are versioned JSON-safe descriptions suitable for audit, approval and outbox boundaries. Canonical SHA-256 digesting gives a cross-process identity for the exact plan.

Before executing a delayed plan, validate that reality still matches the state the plan was calculated against:

```csharp
var snapshot = ManifestStateSnapshot.Create(
    current,
    OrderItemSync.GetKey,
    static key => key.ToString());

var preconditions = ManifestPreconditions.Validate(
    manifest,
    snapshot);
```

If the state changed meanwhile, Forge reports stale-plan failures rather than silently overwriting newer state.

## Batch composition

Multiple heterogeneous manifests can be assessed as one unit:

```csharp
var batch = ReconciliationBatch.Create(
    [
        new NamedSyncManifest("services", serviceManifest),
        new NamedSyncManifest("resources", resourceManifest),
        new NamedSyncManifest("relationships", relationshipManifest)
    ],
    [
        new PlanDependency("resources", "services"),
        new PlanDependency("relationships", "resources")
    ]);

var validation = batch.Validate(
    value => value.TotalOperationCount > 500
        ? new BatchValidationIssue("LIMIT", "Approval required.")
        : null);
```

The application still decides whether and how to execute the batch.


## 1.9-1.20 typed planning train

Forge 1.20 keeps planning strongly typed while extending the production planning surface:

- **1.9 conditional dependencies** — predecessor state requirements through `IStateCondition<TState>`.
- **1.10 readiness** — composable typed requirements and application-defined blocking reasons.
- **1.11 plan slicing** — dependency-safe subsets with explicit direct-vs-required membership.
- **1.12 provenance** — typed cause/effect graphs and shortest root-cause traces.
- **1.13 plan Delta** — semantic operation and dependency changes between plan snapshots.
- **1.14 execution-aware replanning** — pending/running/completed work is handled safely and explicitly.
- **1.15 approval scopes** — typed structural approval boundaries without authorization logic in Forge.
- **1.16 alternatives** — consumer-defined plan metrics and preference selection with explicit ties.
- **1.17 derived facts** — `FactKey<T>` and bounded monotonic typed propagation.
- **1.18 scenario matrices** — lazy typed Cartesian scenarios and cancellation-aware simulation.
- **1.19 plan templates** — source-generated typed template instantiation with application-owned build logic.
- **1.20 composite topology** — cross-topology invariants and one typed dependency order across composed transitions.

### Type-safety and extension constitution

New planning APIs do not use string operation identities, string state names, or `Dictionary<string, object>` extension bags. Applications keep their own key/state/reason/fact/scope/violation types. Forge extension points are intentionally narrow (`IStateCondition`, readiness providers, slice selectors, execution-state classifiers, approval selectors, evaluators, fact rules, scenario evaluators, and topology invariants).

Forge continues to perform pure planning only: no network, database, queue, filesystem, authorization, dynamic compilation, assembly scanning, or runtime plugin activation is introduced by these features.

## Design rules

1. .NET 10 first.
2. Source-generated normal Delta/Sync hot paths.
3. AOT and trimming friendly runtime primitives.
4. No reflection in normal generated comparison/reconciliation.
5. No mandatory dependency injection.
6. No persistence or transport assumptions.
7. No hidden I/O or mutation.
8. Strongly typed generated APIs.
9. Explicit state and identity semantics.
10. Deterministic planning and portable plan descriptions.
11. Application-owned policy and execution.
12. Features must solve broad production primitives rather than framework-specific convenience.

See `docs/API_CONTRACT.md`, `docs/DESIGN.md`, `docs/RECONCILIATION.md`, `docs/DOGFOOD.md` and the milestone reviews for the detailed contracts.

## 1.1-1.8 planning features

The post-1.0 feature train adds planning capabilities on top of Delta and Sync without moving execution into Forge:

```text
Current + Desired
      ↓
structural Delta / Sync
      ↓
explanation + operation rules
      ↓
impact + constraints
      ↓
incremental replanning
      ↓
portable simulation
```

### Explain and classify

```csharp
var explanation = SyncPlanExplainer.Explain(plan);

var rules = new SyncOperationRules<OrderItem, OrderItemSync.Key, OrderItemDelta, Operation>(
        Operation.Add,
        Operation.Delete,
        Operation.Modify)
    .WhenUpdated(
        "replace.product",
        "Changing product requires replacement.",
        update => update.Delta.ProductChange.HasChanged,
        Operation.Replace);

var operations = rules.Plan(plan);
```

### Generated cross-type profiles

```csharp
[GenerateCrossSyncProfile(typeof(StoredLine), typeof(RequestedLine))]
[CrossSyncIdentity(nameof(StoredLine.Id), nameof(RequestedLine.Id))]
[CrossSyncMap(nameof(StoredLine.Name), nameof(RequestedLine.Name))]
[CrossSyncMap(nameof(StoredLine.Quantity), nameof(RequestedLine.Quantity))]
public partial class StoredLineProfile
{
}

var crossTypePlan = StoredLineProfile.Plan(current, desired);
```

### Incremental replanning

```csharp
var manifest = SyncManifest.Create(plan, key => key.ToString());
var tracked = IncrementalReplanner.Create(manifest, CreateOperationId);
var next = IncrementalReplanner.Replan(tracked, nextManifest, CreateOperationId);

next.Retained;
next.NewlyRequired;
next.NoLongerRequired;
next.Replaced;
```

### Impact and constraints

```csharp
var impact = ImpactAnalyzer.Analyze(changedKeys, propagationEdges);

var validation = SyncPlanConstraints
    .For<OrderItem, OrderItemSync.Key, OrderItemDelta>()
    .MaximumChanges(100)
    .RequireNoRemovals()
    .Validate(plan);
```

### Resolve three-way conflicts

```csharp
var analysis = CustomerDelta.AnalyzeMerge(baseline, current, desired);
var resolution = MergeResolver.Resolve(
    analysis,
    new MergeResolutionPolicy()
        .PreferDesired("Description")
        .PreferCurrent("LastObservedAt"));
```

### Simulate before execution

```csharp
var simulation = ManifestPlanSimulator.Simulate(manifest, currentSnapshot);

if (simulation.Applied && simulation.Matches(expectedDesiredSnapshot))
{
    // The structural plan projects to the expected state.
}
```
