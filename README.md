# Forge 1.20.0

> Strongly typed state transition planning for .NET 10.
>
> **Website:** https://pjotrcasteel.github.io/Forge/ · **Forge.Sync:** https://www.nuget.org/packages/Forge.Sync · **Forge.Delta:** https://www.nuget.org/packages/Forge.Delta · **Source:** https://github.com/pjotrcasteel/Forge

Forge turns **current state + desired state** into explicit, typed transition plans while persistence, transport, workflow policy, authorization, and execution remain in your application.

Use **Forge.Delta** when the question is _“what changed inside this object?”_  
Use **Forge.Sync** when the question is _“how does the state I have become the state I want?”_

Normal generated Delta/Sync hot paths use direct property access and dictionaries: no runtime reflection, dynamic proxies, hidden I/O, or mandatory dependency injection.

<a id="why"></a>

## When Forge is useful

Forge is a strong fit when application correctness depends on one or more of these problems:

- semantic object change detection;
- desired-state reconciliation;
- complete replacement versus partial upsert semantics;
- cross-type current/desired models;
- dependency-aware create/update/delete ordering;
- graph or topology transitions;
- plan validation before side effects begin;
- stale-plan detection after a delay or approval step;
- incremental or execution-aware replanning;
- portable, reviewable plan descriptions.

For a small CRUD update where normal equality and one direct database write are enough, ordinary application code is usually the simpler tool.

<a id="five-minute-start"></a>

## Five-minute start

Choose the smallest package that answers your first question.

For semantic object differences:

```bash
dotnet add package Forge.Delta --version 1.20.0
```

For desired-state reconciliation and planning:

```bash
dotnet add package Forge.Sync --version 1.20.0
```

Installing `Forge.Sync` also brings in `Forge.Delta`.

<a id="your-first-delta"></a>

## Your first Delta

```csharp
using Forge.Delta;

[GenerateDelta]
public sealed record Customer(
    Guid Id,
    string Name,
    string? Email);

var delta = CustomerDelta.Between(before, after);

if (delta.EmailChange.HasChanged)
{
    Console.WriteLine(
        $"{delta.EmailChange.Before} -> {delta.EmailChange.After}");
}
```

The generated API also exposes:

```csharp
CustomerDelta.AreEquivalent(before, after);
CustomerDelta.GetSemanticHashCode(after);
CustomerDelta.AnalyzeMerge(baseline, current, desired);
delta.Invert();
```

Use `[DeltaIgnore]` for bookkeeping state that should not participate, `[DeltaComparer]` for domain-specific equality, and explicit collection comparers when list/set/dictionary semantics matter.

Forge does not recursively guess arbitrary object-graph or collection semantics.

<a id="your-first-reconciliation"></a>

## Your first reconciliation

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

<a id="replace-upsert"></a>

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

This distinction is explicit so a partial payload cannot accidentally become a deletion plan.

<a id="choose-capability"></a>

## Pick the capability by the planning problem

| Planning problem | Start with |
| --- | --- |
| What changed inside one object? | `Forge.Delta` |
| Which keyed items were added/updated/removed? | generated `Forge.Sync` |
| Current and desired use different CLR types | cross-type reconciliation |
| More than one legitimate identity route exists | ordered fallback identity |
| Operations depend on other operations | `DependencyPlanner` |
| Nodes and relationships change together | `TopologySync` |
| A plan crosses an approval/outbox boundary | portable manifest + digest |
| Reality may change before execution | manifest preconditions |
| A previous plan is already partly executing | execution-aware replanning |
| Several plans form one release/change unit | reconciliation batch / composite topology |

The detailed contracts live in **[docs/API_CONTRACT.md](docs/API_CONTRACT.md)**, **[docs/DESIGN.md](docs/DESIGN.md)**, and **[docs/RECONCILIATION.md](docs/RECONCILIATION.md)**.

<a id="dependency-aware-planning"></a>

## Dependency-aware planning

```csharp
var dependencyPlan = DependencyPlanner.Plan(
    operations,
    static item => item.Id,
    static item => item.DependsOn);

foreach (var wave in dependencyPlan.CreateWaves)
{
    // Items in this wave can execute after all previous waves complete.
}

foreach (var wave in dependencyPlan.DeleteWaves)
{
    // Dependent-first order for deletion/deprovisioning.
}
```

Cycles are surfaced before execution.

<a id="portable-plans"></a>

## Portable plans and stale-state protection

```csharp
var manifest = SyncManifest.Create(
    plan,
    static key => key.ToString());

var json = manifest.ToJson();
var digest = ManifestDigest.ComputeSha256Hex(manifest);
```

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

Portable manifests are data. Forge does not execute serialized plans as code.

<a id="typed-planning"></a>

## Typed planning through 1.20

The 1.x feature train extends planning without moving execution into Forge:

- conditional state dependencies;
- readiness and application-defined blocking reasons;
- dependency-safe plan slicing;
- typed provenance and root-cause paths;
- semantic plan-to-plan Delta;
- execution-aware replanning;
- approval scopes;
- alternative-plan evaluation with application-owned preferences;
- typed derived facts;
- lazy scenario matrices;
- source-generated reusable plan templates;
- composite topology invariants and dependency ordering.

Applications keep their own identity, state, fact, scope, reason, metric, and violation types. Forge does not require string registries or `Dictionary<string, object>` extension bags on the normal typed planning path.

<a id="boundary"></a>

## The execution boundary

Forge calculates, classifies, validates, explains, and simulates transitions.

Forge does **not**:

- persist data;
- call HTTP or RPC endpoints;
- publish messages;
- perform authorization;
- scan assemblies for runtime plugins;
- dynamically compile external expressions;
- execute a plan.

That boundary keeps planning deterministic, testable, and infrastructure-independent.

<a id="design"></a>

## Design rules

1. .NET 10 first.
2. Source-generated normal Delta/Sync hot paths.
3. Native AOT and trimming-friendly runtime primitives.
4. No runtime reflection in generated comparison/reconciliation paths.
5. No mandatory dependency injection.
6. No persistence or transport assumptions.
7. No hidden I/O or mutation.
8. Strongly typed generated APIs.
9. Explicit state and identity semantics.
10. Deterministic planning and portable plan descriptions.
11. Application-owned policy and execution.
12. New features must solve broad production primitives, not framework-specific convenience.

<a id="docs"></a>

## Documentation

- **[Website](https://pjotrcasteel.github.io/Forge/):** visual introduction, interactive planning examples, package choice, and AI-agent context.
- **This README:** package choice, first Delta/Sync, planning boundary, and common capabilities.
- **[API_CONTRACT.md](docs/API_CONTRACT.md):** stable generated/runtime contracts across Forge 1.x.
- **[DESIGN.md](docs/DESIGN.md):** semantics, source-generation model, extension rules, safety, and complexity.
- **[RECONCILIATION.md](docs/RECONCILIATION.md):** reconciliation details and identity semantics.
- **[Production-style service provisioning sample](samples/Forge.ServiceProvisioning.Sample/README.md):** Delta → Sync → dependencies → manifest → stale-state protection → execution-aware replanning.
- **[llms.txt](https://pjotrcasteel.github.io/Forge/llms.txt):** concise machine-readable map for coding agents.

<a id="license"></a>

## License

MIT. See [LICENSE](LICENSE).

---

Forge is an open-source project by **[Pjotr Casteel](https://github.com/pjotrcasteel)**.
