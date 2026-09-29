# Forge 1.0 — Generic Distributed Rollout Example

This example pressure-tests Forge against a production-shaped distributed application without borrowing terminology or models from another project.

The fictional system contains independently versioned components with dependencies:

```text
database
└── api
    ├── legacy-worker
    └── policy-engine
        ├── telemetry
        └── queue
```

The important boundary remains:

> **Forge calculates and describes state differences and transition plans. The consuming application owns persistence, transport, authorization and execution.**

---

## 1. Partial configuration updates

Assume persisted state and incoming configuration use different CLR models:

```text
StoredSetting             current persisted state
ConfigurationPatch        incoming partial desired state
```

Only selected fields participate in semantic comparison:

```csharp
using Forge.Delta;

[GenerateDelta]
internal readonly record struct SettingState(
    string Name,
    string Value,
    string? Source,
    string? Description,
    bool IsExported);
```

Both models can be mapped into that explicit comparison state. Forge does not require persistence models to carry library-specific behavior.

For partial input, use explicit Upsert semantics:

```csharp
var plan = CrossSync.Plan(
    existingSettings,
    incomingPatch,
    settingDefinition);
```

The result distinguishes:

```text
Added       new logical settings
Updated     existing settings whose participating state changed
Unchanged   supplied settings that are semantically equal
Preserved   existing settings omitted from the patch
Removed     empty for this Upsert scenario
```

Duplicate-key policy still belongs to the application. If "last value wins" is required, normalize before reconciliation.

---

## 2. Complete desired-state rollout

Now consider a full rollout where the application has a complete desired component graph.

Current:

```text
database
└── api (revision 1, stable)
    └── legacy-worker
```

Desired:

```text
database
└── api (revision 2, optimized)
    └── policy-engine
        └── telemetry
```

A simple model can remain application-owned:

```csharp
[GenerateSync(nameof(ApplicationComponent.Id))]
internal sealed record ApplicationComponent(
    string Id,
    string Kind,
    int Revision,
    string Configuration,
    string? DependsOn);
```

Then:

```csharp
var plan = ApplicationComponentSync.Plan(
    current,
    desired);
```

produces:

```text
Unchanged
    database

Updated
    api

Added
    policy-engine
    telemetry

Removed
    legacy-worker
```

The API update carries its generated Delta, so the application can explain that both `Revision` and `Configuration` changed.

---

## 3. Dependency ordering

Structural changes are not enough when ordering is part of correctness.

The application can translate structural changes into its own operation type and then ask Forge for waves:

```csharp
var dependencyPlan = DependencyPlanner.Plan(
    operations,
    static operation => operation.Key,
    static operation => operation.DependsOn);
```

For the desired graph:

```text
Wave 1
    Update api
    Remove legacy-worker

Wave 2
    Add policy-engine

Wave 3
    Add telemetry
```

If a dependency cycle appears, Forge reports it before any side effect begins.

---

## 4. Portable plan identity

A Sync plan can cross an application boundary as data:

```csharp
var manifest = SyncManifest.Create(
    plan,
    static key => key.Id);

var json = manifest.ToJson();
var digest = ManifestDigest.ComputeSha256Hex(manifest);
```

The manifest is versioned data, not executable code.

The digest can bind approval, storage or delayed execution to the exact plan.

---

## 5. Stale-state protection

Planning and execution may be separated in time.

Imagine the plan was calculated against:

```text
api
revision = 1
configuration = stable
```

but another actor changes it before execution:

```text
api
revision = 7
configuration = emergency-hotfix
```

Create a fresh snapshot and validate the manifest:

```csharp
var snapshot = ManifestStateSnapshot.Create(
    latestCurrent,
    ApplicationComponentSync.GetKey,
    static key => key.Id);

var preconditions = ManifestPreconditions.Validate(
    manifest,
    snapshot);
```

The old plan is rejected because its assumptions no longer match current reality.

---

## 6. Three-way conflict analysis

Suppose the baseline API state is:

```text
revision      = 1
configuration = stable
timeout       = 30
```

Another actor changes only `timeout`, while the desired rollout changes only `configuration`.

Those changes can be independent.

If both current reality and desired state change `configuration` differently, Delta conflict analysis surfaces the same-property conflict.

The application still decides whether to reject, replan or request approval.

---

## 7. Execution-aware replanning

Now assume execution already started:

```text
Update api                 completed
Add policy-engine          running
Add telemetry              waiting
Remove legacy-worker       waiting
```

A revised desired state arrives:

```text
database
└── api
    ├── legacy-worker
    └── policy-engine (strict)
        └── queue
```

Using `ExecutedReplanner`, the application can distinguish:

```text
Completed work
    remains locked into reality

Running work with changed semantics
    requires intervention

Waiting work no longer required
    can be cancelled safely

Newly required work
    enters the next plan
```

Forge does not invent cancellation or rollback policy. It classifies what changed around already-started work.

---

## 8. Topology reconciliation

For systems with explicit nodes and edges, `TopologySync` can reconcile both together.

Example current graph:

```text
api -> database
legacy-worker -> api
```

Desired graph:

```text
api -> database
policy-engine -> api
telemetry -> policy-engine
```

Forge can validate that every edge refers to a known node and produce graph-safe structural phases.

This applies equally to:

- application components;
- infrastructure resources;
- deployment units;
- workflow graphs;
- product configurations;
- data-pipeline stages.

---

## 9. Batch composition

A rollout may combine several independently reconciled categories:

```text
components
configuration
relationships
```

They can be composed into a batch with application-owned dependencies:

```csharp
var batch = ReconciliationBatch.Create(
    [
        new NamedSyncManifest("components", componentManifest),
        new NamedSyncManifest("configuration", configurationManifest),
        new NamedSyncManifest("relationships", relationshipManifest)
    ],
    [
        new PlanDependency("configuration", "components"),
        new PlanDependency("relationships", "configuration")
    ]);
```

The whole proposal can be validated before execution begins.

---

## 10. Compensation

If application policy allows compensation, a structural plan can be inverted:

```text
Forward

Added policy-engine
Removed legacy-worker
Updated api stable -> optimized

Reverse

Removed policy-engine
Added legacy-worker
Updated api optimized -> stable
```

Forge calculates the reverse description. Whether it is legal to execute remains application policy.

---

## Why this is useful dogfood

The scenario exercises production concerns without tying Forge to any particular company or platform:

```text
different current/desired CLR models
partial-upsert semantics
complete desired-state semantics
nested state
dependency ordering
topology changes
portable plans
canonical plan identity
stale-state detection
three-way conflicts
execution-aware replanning
batch-wide validation
compensation
```

The consuming domain can change completely while the Forge primitives remain the same. That is the abstraction boundary this example is intended to prove.
