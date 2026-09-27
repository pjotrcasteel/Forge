# Post-1.8 roadmap

Forge 1.1-1.8 deliberately focused on product capability rather than infrastructure polish.

The completed feature train now covers:

- structured explanations;
- generated cross-type profiles;
- Delta-aware operation rules;
- incremental replanning;
- graph impact analysis;
- pre-execution plan constraints;
- three-way conflict resolution;
- portable plan simulation.

Future work should again require real dogfood pressure. Strong candidates are:

- incremental graph/topology replanning that preserves operation identity across node and edge changes;
- richer collection-level three-way merge semantics;
- typed adapters for materializing resolved merge patches into immutable domain models;
- plan slicing/approval scopes with explicit safety boundaries;
- provenance graphs that connect impact, rule selection, dependencies and simulation into one explanation model;
- additional packages only when a primitive is broad enough to stand independently of Delta and Sync.

Non-goals remain persistence frameworks, transport abstractions, workflow engines, hidden mutation and domain-specific orchestration.
