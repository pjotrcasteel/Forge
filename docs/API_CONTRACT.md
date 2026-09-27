# Forge 1.x API contract

Forge 1.0 established the stable Delta/Sync baseline. Forge 1.1-1.20 add compatible planning capabilities; breaking public-contract changes still require a major version.

## Generated Delta surface

For `[GenerateDelta] T` or an external type targeted by `[GenerateDeltaProfile]`:

- `TDelta.AreEquivalent(before, after)` performs semantic equality without allocating a detailed Delta;
- `TDelta.GetSemanticHashCode(value)` produces an in-process hash consistent with that equality;
- `TDelta.Between(before, after)` creates a typed detailed Delta;
- `TDelta.AnalyzeMerge(baseline, current, desired)` reports semantic three-way conflicts;
- `delta.Invert()` reverses all generated typed changes;
- `<Property>Change` exposes `ValueChange<TProperty>`;
- `<Property>Delta` exists for explicitly generated nested Delta state;
- `Changes` exposes deterministic flattened property paths;
- `HasChanges` reports whether participating state changed.

Three-way analysis can be passed to `MergeResolver` with a `MergeResolutionPolicy` to produce an explicit semantic merge patch while leaving arbitrary domain-object materialization to the application.

## Generated Sync surface

For `[GenerateSync(...)] T`:

- `TSync.Key` and `TSync.GetKey(item)` define logical identity;
- `TSync.AreEquivalent(current, desired[, mode])` checks reconciliation equivalence;
- `TSync.Plan(current, desired[, mode])` returns a typed `SyncPlan`;
- key properties are identity and are excluded from mutable-state Delta output;
- `[SyncNested]` participates in parent update classification and emits `Plan<Property>` helpers;
- duplicate logical keys fail explicitly.

## Generated cross-type profiles

For a partial class marked with `[GenerateCrossSyncProfile]` plus explicit `[CrossSyncIdentity]` and `[CrossSyncMap]` attributes, Forge generates:

- `Plan(current, desired[, mode])`;
- `AreEquivalent(current, desired)`;
- `Between(current, desired)` returning `CrossTypeDelta`;
- ordered same-key-type identity fallback through `SyncIdentity<TKey>`.

Heterogeneous identity types continue to use the lower-level `CrossSyncMatchDefinition` rather than implicit string conversion.

## Runtime reconciliation and planning primitives

Forge 1.x exposes:

- `CrossSync` for different current/desired CLR types;
- `SyncIdentity<TKey>` / `CrossSyncMatchDefinition` for ordered canonical + fallback identity routes;
- `SyncPlanExplainer` and `PlanExplanationBuilder` for structured, enrichable explanations;
- `SyncOperationRules` / `CrossSyncOperationRules` for ordered Delta-aware operation selection;
- `IncrementalReplanner` for stable application-owned operation identity across semantic replanning;
- `ImpactAnalyzer` for direct/transitive graph impact and shortest reason paths;
- `PlanConstraintSet<TPlan>` and Sync convenience constraints for whole-plan pre-execution validation;
- `DependencyPlanner` for topological execution waves;
- `SyncOperationPlanner` / `CrossSyncOperationPlanner` for direct typed action classification;
- `StreamingSync` for strictly ordered asynchronous reconciliation;
- `TopologySync` for validated node/edge desired-state transitions;
- `SyncPlanInverter` / `CrossSyncPlanInverter` for safe complete-plan compensation;
- portable Delta/Sync manifests and canonical manifest digests;
- manifest stale-state precondition validation;
- `ManifestPlanSimulator` for portable state projection without execution;
- `ReconciliationBatch` for heterogeneous plan dependencies and batch-wide validation.
- typed conditional state dependencies through `IStateCondition<TState>` and immutable `StateSnapshot<TKey, TState>`;
- `ReadinessEvaluator` with consumer-defined requirement and blocking-reason types;
- dependency-safe `PlanSlicer` subsets;
- typed `ProvenanceGraph` cause/effect tracing;
- `PlanDelta` for semantic operation/dependency differences between plan snapshots;
- `ExecutedReplanner` for pending/running/completed-work-aware replanning;
- typed `ApprovalScopePlanner` structural approval boundaries;
- typed alternative-plan evaluation and application-owned preference selection;
- `FactKey<T>` plus bounded monotonic typed fact derivation;
- lazy typed scenario matrices and cancellation-aware simulation;
- source-generated reusable plan templates through `[GeneratePlanTemplate]`;
- typed composite topology validation through consumer-defined invariants.

## Execution boundary

Forge APIs calculate, classify, validate, explain and simulate transitions. They do not persist state, call external systems, publish messages, decide domain policy or automatically execute a plan.

## 1.x typed extension contract

New planning features use application CLR types for logical keys, execution states, facts, reasons, approval scopes, scenario identities, metrics, and invariant violations. Forge does not require string registries or `Dictionary<string, object>` extension bags on the normal typed planning path.

Consumer extension points are intentionally small and single-purpose. Forge may use interfaces for application policy boundaries, but it does not scan assemblies or activate implementations dynamically.
