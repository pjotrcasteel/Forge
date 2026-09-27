# Production quality contract

Forge is intended for production planning paths where incorrect classification can cause incorrect downstream work. The library therefore treats type safety, determinism, bounded algorithms and explicit application policy as product requirements.

## Type safety

Normal planning APIs use generic application types for identity, state, facts, blocking reasons, scopes, scenario cases, metrics and violations. New 1.9-1.20 APIs do not introduce string registries or untyped extension bags.

## Security posture

Core planning is pure computation. Forge performs no network, filesystem, database, queue, process, secret, authorization or credential I/O. It does not discover extensions by assembly scanning or dynamically activate types from input. Portable manifest JSON is data and never an executable rule format.

Consumers remain responsible for validating untrusted input sizes before constructing very large collections or graphs. Any application action based on a plan must re-check the application's authorization and concurrency boundaries.

## Determinism

Plan output preserves documented input ordering. Hash-based indexing is used for lookup, not to expose nondeterministic result ordering. Scenario cross-products use axis order. Alternative-plan ties preserve input order.

## Bounded behavior

- graph algorithms are iterative rather than recursively walking arbitrary object graphs;
- fact propagation has an explicit pass limit;
- async scenario simulation propagates cancellation;
- scenario matrices are lazy;
- duplicate and ambiguous identities fail before execution plans are accepted.

## SOLID extension model

Extension points are small and focused. Applications add conditions, readiness rules, selectors, state classifiers, evaluators, fact rules and topology invariants by implementing one responsibility. Forge does not require DI and does not own extension lifetime.

## Validation gates

The repository contains unit tests, generator compilation tests, API surface snapshots, package-consumer tests, Native AOT/trimming gates, package verification and benchmarks. A release should not be published until the real .NET 10 CI build/test/pack/AOT gates are green.
