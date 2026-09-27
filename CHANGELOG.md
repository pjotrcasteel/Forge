# Changelog

## 1.20.0

- Added typed composite topology transitions that combine application-defined topology plans without erasing their domain types.
- Added consumer-extensible cross-topology invariants, composite dependency ordering, and cycle-aware validity.

## 1.19.0

- Added source-generated strongly typed reusable plan templates.
- Added a validated `PlanTemplateBuilder` for typed operations and dependencies with deterministic execution waves.

## 1.18.0

- Added lazy strongly typed Cartesian scenario matrices.
- Added synchronous and cancellation-aware asynchronous scenario simulation through small evaluator interfaces.

## 1.17.0

- Added `FactKey<T>` and immutable typed fact snapshots without string/object fact bags.
- Added bounded monotonic fact propagation with typed rule extensibility and conflicting-fact detection.

## 1.16.0

- Added typed alternative-plan sets and consumer-defined metric evaluation.
- Added deterministic application-owned preference selection with explicit ties instead of Forge-defined scoring assumptions.

## 1.15.0

- Added typed approval scopes with application-owned scope selection.
- Added automatic cross-scope dependency derivation and cycle detection without introducing users, roles, or approval storage into Forge.

## 1.14.0

- Added execution-aware typed replanning that preserves safe work and surfaces compensation or running-work conflicts.
- Added separate typed outcome records for pending, running and completed work instead of enum-plus-null result models.

## 1.13.0

- Added strongly typed plan-to-plan semantic Delta for operation and dependency snapshots.
- Added reusable typed plan-element definitions so applications control identity and equality without reflection.

## 1.12.0

- Added strongly typed provenance graphs with consumer-defined cause node types.
- Added deterministic ancestor tracing and shortest root-cause paths for plan explanations and support tooling.

## 1.11.0

- Added dependency-safe typed plan slicing with transitive predecessor closure.
- Added an extensible `IPlanSliceSelector<TItem>` boundary and explicit direct-vs-required slice membership.

## 1.10.0

- Added composable strongly typed readiness requirements and typed blocking reasons.
- Added deterministic readiness evaluation that accumulates every blocker per operation without dispatching work.

## 1.9.0

- Added strongly typed conditional dependencies with consumer-extensible state conditions.
- Added immutable typed state snapshots and explicit missing-vs-unsatisfied dependency results.

## 1.8.0

- Added portable plan simulation with stale-plan precondition enforcement and projected-state comparison.

## 1.7.0

- Added conflict-resolution policies on top of generated three-way merge analysis.
- Added explicit resolved merge patches with unresolved-conflict reporting.

## 1.6.0

- Added composable typed plan constraints with accumulated validation violations.
- Added Sync-specific maximum-change, no-removal and protected-key constraints plus dependency-cycle validation.

## 1.5.0

- Added graph impact analysis with direct/transitive impact and shortest reason paths.

## 1.4.0

- Added incremental replanning with application-owned operation identity retention based on canonical semantic plan digests.
- Added retained, newly-required, no-longer-required and replaced operation classifications.

## 1.3.0

- Added ordered Delta-aware operation rules with structured rule explanations.

## 1.2.0

- Added source-generated cross-type Sync profiles with explicit identity and state mappings.
- Added generated cross-type semantic deltas without runtime reflection.

## 1.1.0

- Added structured plan explanations and application-owned explanation enrichment.

## 1.0.0

- Stabilized Forge.Delta and Forge.Sync after the 0.10-0.30 capability train.
- Added cross-type, nested, streaming and topology reconciliation.
- Added three-way Delta conflict analysis, reversible deltas and compensation plans.
- Added dependency waves and typed operation classification.
- Added versioned portable manifests, stale-plan preconditions and canonical SHA-256 plan identity.
- Added heterogeneous reconciliation batches with dependency ordering and batch-wide validation.
- Added ordered fallback identities for cross-type reconciliation where instance IDs and business keys coexist.
- Preserved the no-reflection generated hot path and application-owned execution boundary.
- Hardened NuGet packaging with package-specific READMEs, SDK package validation, Source Link repository metadata, XML docs,
  symbol-package verification, isolated packed-package consumers, and retained CI package artifacts.

## 0.30.0

- Added cross-type operation classification, portable manifests and compensation inversion.

## 0.29.0

- Added canonical SHA-256 identity for portable Sync manifests.

## 0.28.0

- Added portable current-state snapshots and stale-plan precondition validation.

## 0.27.0

- Added heterogeneous reconciliation batch composition, component dependencies and batch-wide validation.

## 0.26.0

- Added versioned JSON-safe Delta and Sync manifests.

## 0.25.0

- Added generated semantic hash codes consistent with Delta equality semantics inside the process.

## 0.24.0

- Added generated Delta inversion and complete Sync compensation-plan inversion.

## 0.23.0

- Added validated node/edge topology reconciliation and graph-safe structural phases.

## 0.22.0

- Added cancellation-aware ordered `IAsyncEnumerable<T>` streaming reconciliation.

## 0.21.0

- Added typed operation classification from structural Sync plans.

## 0.20.0

- Added dependency execution waves, reverse deletion waves and cycle detection.

## 0.19.0

- Added generated three-way semantic conflict analysis.

## 0.18.0

- Added strongly typed reflection-free cross-type reconciliation.

## 0.17.0

- Added generated `Sync.AreEquivalent` overloads for allocation-light replace/upsert equivalence checks.
- Changed nested Sync classification to use child equivalence checks instead of allocating child plans.
- Reworked unordered-list, keyed-list and dictionary comparers around indexed/counting algorithms rather than repeated scans.
- Expanded BenchmarkDotNet coverage for Sync equivalence, nested reconciliation and collection comparers.
- Added a manually triggered benchmark workflow that publishes JSON and Markdown results as CI artifacts.
- Documented the performance methodology and explicitly kept benchmark claims out of the product until measured on a real .NET 10 runner.

## 0.16.0

- Added warnings for empty Deltas and ineffective comparer-on-ignored-property configuration.
- Added an error when a `SyncNested` collection is also configured as a logical key.
- Added debugger displays for generated Delta types, generated Sync keys, `ValueChange`, `PropertyChange` and `SyncPlan`.
- Expanded the diagnostics reference through all current Delta and Sync diagnostic IDs.

## 0.15.0

- Added `DeltaProfileIgnore` and `DeltaProfileComparer` for rich compile-time configuration of external/unannotated types.
- Added diagnostics for missing properties, invalid comparers and conflicting profile configuration.
- Fixed comparer constructibility validation so profile-generated code uses the consumer/profile assembly as the accessibility boundary.

## 0.14.0

- Added `SyncNestedAttribute` for explicit nested reconciliation boundaries.
- Parent Sync classification now includes nested child plan changes without treating child collections as scalar Delta state.
- Generated `Plan<Property>` helpers expose child add/update/remove/unchanged plans for parent updates.
- Updated Fulfilment-shaped dogfood so RFS characteristic changes are reconciled as child Sync updates.

## 0.13.0

- Added explicit `SyncMode.Replace` and `SyncMode.Upsert` semantics.
- Added a `Preserved` result bucket for current-only items intentionally retained during partial upserts.
- Kept the existing two-argument `Plan` overload as complete desired-state/replace behavior.
- Added current-Fulfilment-shaped partial-characteristic dogfood coverage.

## 0.12.0

- Added explicit order-sensitive, order-insensitive and set semantics for read-only lists.
- Added order-independent read-only dictionary equality.
- Added logical-key list equality through `IKeySelector<T, TKey>`.
- Kept all collection semantics opt-in through `DeltaComparer`; Delta does not guess collection intent.

## 0.11.0

- Added `GenerateDeltaProfileAttribute` for types consumers cannot or do not want to annotate directly.
- Profile-generated deltas retain the strongly typed `Between`, `AreEquivalent` and property-change API.
- Added generator and runtime tests for unannotated target types.


## 0.10.0

- Reclassified the previous 1.0 prototype as a pre-1.0 baseline while the products mature.
- Fixed duplicated desired-item loop emission in generated Sync plans.
- Added current-Fulfilment-shaped characteristic persistence dogfood tests for Delta.
- Added explicit milestone alignment reviews.

## 0.9.0-rc.1

- Freeze feature work for the 1.0 release candidate.
- Add automated `.nupkg` layout inspection for runtime and analyzer assets.
- Require Forge.Sync packages to declare a Forge.Delta package dependency.
- Validate the package consumer after layout inspection and before NuGet publishing.
- Add release-tag/version matching for GitHub Release-triggered publishing.
- Require the Native AOT smoke publish in both CI and the NuGet publish workflow.
- Document the release gates and post-1.0 compatibility commitment.

## 0.8.0-preview.1

- Stabilize the intended 1.0 runtime and generated API contracts.
- Add exported-runtime-type snapshots for Forge.Delta and Forge.Sync.
- Add compile-time contract coverage for generated Delta and Sync names/signatures.
- Make detailed Delta change collections read-only instead of exposing a mutable array implementation.
- Hide generated-code-only construction helpers from normal IntelliSense where possible.
- Document the 1.x API contract and identity-vs-state semantics.

## 0.7.5-preview.1

- Centralize package versioning with `ForgeVersion`.
- Add a package-consumer project that references the packed `Forge.Sync` package instead of project references.
- Validate Sync source generation from the packed package.
- Validate transitive Forge.Delta runtime/analyzer availability when only Forge.Sync is installed.
- Add packed-consumer validation to CI and the publish workflow before NuGet push.
- Re-check provisional `Forge.Delta` / `Forge.Sync` package IDs; exact current NuGet search returned no matches.

## 0.7.0-preview.1

- Dogfood Delta and Sync against service-orchestration-shaped Change/Delete/cancellation scenarios.
- Add `ReadOnlyListSequenceComparer<T>` for explicit order-sensitive state equality.
- Prove re-materialized equivalent list state does not have to create false Sync updates.
- Validate full and partial delete as ordinary desired-state reconciliation.
- Validate cancellation/compensation as application policy with Delta describing only the resulting state effect.
- Document why dependency ordering and cancellation admission remain outside Sync.

## 0.6.0-preview.1

- Add warnings for ineffective Sync comparer placement.
- Add a dedicated diagnostic for unsupported logical-key property types.
- Add XML documentation to generated Delta and Sync public APIs.
- Add a diagnostic reference document.
- Add an ASP.NET Core application sample showing application-owned reconciliation execution.
- Improve NuGet metadata while keeping the public brand marked as provisional.

## 0.5.0-preview.1

- Add generated `AreEquivalent` fast paths for Delta models.
- Use `AreEquivalent` in Sync before constructing a detailed Delta.
- Avoid allocating change lists for unchanged Delta results.
- Replace the desired-value dictionary with a desired-key `HashSet` in generated Sync code.
- Expand BenchmarkDotNet coverage for equivalent and mixed-change workloads.
- Add a Native AOT smoke sample and CI publish gate.
- Keep performance claims measurement-based; no unverified benchmark numbers are documented.

## 0.4.0-preview.1

- Validate desired-state reconciliation against change, full-delete and partial-delete scenarios.
- Exclude Sync key properties from the Sync-generated Delta.
- Treat identity changes as remove/add instead of mutable-state updates.
- Prevent equivalent custom-key representations from becoming false updates.
- Add nested change scenario coverage with flattened Delta paths.
- Add compensating-operation Delta coverage without introducing workflow policy into Forge.

## 0.3.0-preview.1

- Harden Sync around explicit logical identity.
- Generate a strongly typed public key for every synchronizer.
- Add `[SyncKeyComparer]` for per-key custom equality.
- Use identical key equality for reconciliation, duplicate detection and exposed keys.
- Replace raw add/remove/unchanged values with operation records carrying logical keys.
- Snapshot all plan collections before exposing them.
- Add current/desired item counts to reconciliation plans.
- Cover delete-to-empty, ordering, duplicate custom keys and custom-key diagnostics.

## 0.1.0-preview.2

- Add semantic nested generated deltas.
- Flatten nested changes into paths such as `Address.City`.
- Avoid reference-identity false positives for nested Forge models.
- Add nullable nested reference and nullable nested struct handling.
- Add `[DeltaComparer]` with compile-time comparer validation.
- Add inherited public property support to Delta.
- Add inherited key-property support to Sync.
- Keep custom comparer instances cached in generated static fields.
- Expand Delta, Sync and generator edge-case tests.
- Keep the Delta analyzer available through the Forge.Sync package dependency.

## 0.1.0-preview.1

- Initial Delta source generator.
- Initial keyed Sync reconciliation source generator.
- Add composite keys and duplicate-key detection.
- Add runtime tests, generator tests, samples, benchmarks and CI.
