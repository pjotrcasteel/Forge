# Forge maturity milestones

The early 1.0 prototype was intentionally pulled back to pre-1.0. Stable 1.0 is based on the capability train below.

| Milestone | Capability | Status |
| --- | --- | --- |
| 0.10 | Real-service dogfood and Sync correctness | Complete |
| 0.11 | External/unannotated Delta types | Complete |
| 0.12 | Explicit collection equality semantics | Complete |
| 0.13 | Replace vs partial-Upsert reconciliation | Complete |
| 0.14 | Explicit nested Sync | Complete |
| 0.15 | Rich external Delta configuration | Complete |
| 0.16 | Compile-time diagnostics/debuggability | Complete |
| 0.17 | Equivalence fast paths and performance instrumentation | Complete |
| 0.18 | Cross-type reconciliation | Complete |
| 0.19 | Three-way semantic conflict analysis | Complete |
| 0.20 | Dependency-aware execution waves/cycle detection | Complete |
| 0.21 | Typed operation planning | Complete |
| 0.22 | Ordered streaming reconciliation | Complete |
| 0.23 | Node/edge topology reconciliation | Complete |
| 0.24 | Reversible Delta and Sync compensation plans | Complete |
| 0.25 | Equality-consistent in-process semantic hashing | Complete |
| 0.26 | Versioned portable Delta/Sync manifests | Complete |
| 0.27 | Heterogeneous batch composition/pre-validation | Complete |
| 0.28 | Delayed-plan stale-state preconditions | Complete |
| 0.29 | Canonical SHA-256 portable plan identity | Complete |
| 0.30 | Cross-type operation/manifest/compensation parity | Complete |
| 1.0 | Stable Delta + Sync capability baseline | Release gates prepared |

Release gates are not product milestones. They include build/test/pack, package-only consumption, Native AOT, public API contract tests, structural validation and benchmark execution.
## 1.x feature train

| Milestone | Capability | Status |
| --- | --- | --- |
| 1.1 | Structured explainable plans | Complete |
| 1.2 | Generated cross-type profiles | Complete |
| 1.3 | Delta-aware operation rules | Complete |
| 1.4 | Incremental semantic replanning | Complete |
| 1.5 | Graph impact analysis | Complete |
| 1.6 | Typed plan constraints | Complete |
| 1.7 | Three-way conflict resolution policies | Complete |
| 1.8 | Portable plan simulation | Complete |

Infrastructure remains a release gate rather than a feature milestone.
