# Forge design

## Scope

Forge is a family of small production libraries for state-oriented application code. The original two primitives remain intentionally narrow:

- Delta answers: **what changed inside one object?**
- Sync answers: **how do keyed current and desired collections differ?**

Neither package performs persistence or I/O.

## Delta semantics

Every eligible property has a generated `ValueChange<T>` exposing `Before`, `After` and `HasChanged`. Ordinary properties use `EqualityComparer<T>.Default`.

A property can override equality with `[DeltaComparer(typeof(TComparer))]`. `[DeltaIgnore]` removes a property from typed and untyped change output. Public readable inherited properties participate. Overrides replace their base declaration while keeping deterministic ordering.

For types the consumer does not own, `[GenerateDeltaProfile]` creates the Delta from a local profile. Profile attributes can exclude properties and assign comparers at compile time without runtime reflection configuration.

### Explicit nested semantics

Delta never performs reflection-based recursive graph walking.

When a property type explicitly opts into `[GenerateDelta]` or `[GenerateSync]`, the containing Delta uses generated semantics:

- separate nested instances with equivalent generated state are unchanged;
- nested changes flatten into paths such as `Address.City`;
- the nested generated Delta remains strongly typed;
- null/non-null transitions are reported at the containing property;
- a custom property comparer takes precedence.

Explicit nested state is expected to be acyclic. Domain back-references should be ignored or compared explicitly.

### Collections in Delta

Delta does not infer collection meaning. A consumer can explicitly choose sequence, unordered/multiset, set, dictionary or keyed-list equality through reusable comparers. Actual add/update/remove/preserve reconciliation belongs to Sync.

## Sync semantics

Sync indexes current state by generated logical key and validates key uniqueness on both sides.

### Replace mode

Replace represents complete desired state:

- key only in desired -> `Added`;
- key only in current -> `Removed`;
- key in both + equivalent mutable/nested state -> `Unchanged`;
- key in both + changed mutable/nested state -> `Updated`.

### Upsert mode

Upsert represents a partial desired payload:

- key only in desired -> `Added`;
- key only in current -> `Preserved`;
- key in both + equivalent state -> `Unchanged`;
- key in both + changed state -> `Updated`.

This distinction is explicit so a patch-like payload cannot silently become a deletion plan.

Key properties are identity and are excluded from the Sync-generated Delta. Identity changes are represented structurally as remove/add, while equivalent custom-key representations do not create false state updates.

### Explicit nested Sync boundaries

A property marked `[SyncNested]` declares that a keyed child collection participates in parent reconciliation. Nested boundaries can use Replace or Upsert semantics independently. Parent classification uses the child `AreEquivalent` path; a detailed child plan is produced only when the consumer calls the generated `Plan<Property>` helper.

This keeps aggregate boundaries explicit and avoids interpreting arbitrary lists as child entities.

### Ordering and snapshots

Plan collections are snapshotted before exposure. Desired-side categories follow desired order. Removed/Preserved current-only categories follow current order. Duplicate logical keys are invalid input and throw `DuplicateSyncKeyException`.

## Source generation and performance

Runtime libraries do not inspect model metadata. Roslyn generators emit direct property access and cache configured comparers. This keeps normal execution trimming/AOT-friendly and avoids reflection caches, compiled expressions and proxies.

Delta and Sync expose `AreEquivalent` paths for callers/classification logic that do not need detailed operation objects. Performance claims still require measured benchmark output; see `PERFORMANCE.md`.

## Future admission rule

A future Forge package should only be added when it solves a recurring production-code primitive with a similarly small, obvious API. Being related to state or fitting the Forge name is not enough.


## Typed planning architecture through 1.20

Planning features build on Delta and Sync but retain the application/execution boundary. Logical keys, states, readiness reasons, facts, approval scopes, scenario identities and invariant violations remain consumer CLR types.

### Extension rules

Forge follows a narrow-interface Open/Closed design:

- `IStateCondition<TState>` adds state semantics;
- readiness requirements/providers add blockers;
- `IPlanSliceSelector<TItem>` adds slice policy;
- `IExecutionStateClassifier<TState>` maps domain execution state;
- approval selectors add structural scope policy;
- plan evaluators/preferences add alternative metrics and choice policy;
- typed fact rules add derivation;
- scenario evaluators add simulation behavior;
- composite topology invariants add cross-topology rules.

None of these extension points use runtime discovery. Applications construct the implementations they want and pass them explicitly.

### Invalid states and execution safety

Results use separate typed categories where lifecycle differences matter. In particular, execution-aware replanning does not represent completed, running and pending outcomes as one record with optional fields. Completed semantic changes become compensation requirements; running semantic changes become explicit conflicts; only not-yet-started work is freely replaced or cancelled.

### Security boundary

Typed planning APIs are pure computation. They do not perform file, network, database, queue, authorization, or process I/O. Forge does not execute serialized manifests as code, scan assemblies, dynamically compile expressions, call `Activator.CreateInstance` on external input, or load types from strings. Portable manifests remain data.

### Complexity expectations

Hash-key reconciliation is expected O(n) average. Graph dependency, impact, provenance and slicing traversals are O(V + E) for the traversed graph. Scenario Cartesian products are lazy so memory does not scale with the full combination count unless the consumer materializes it. Typed fact propagation is bounded by an explicit maximum pass count.
