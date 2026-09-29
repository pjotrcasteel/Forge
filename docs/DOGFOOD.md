# Dogfood conclusions

The 0.7 milestone applies Forge to production-shaped models without adding those concepts to runtime packages.

## Desired-state rollout

A logical component collection can be reconciled with a newly computed desired collection. Sync determines add, update, remove and unchanged items; Delta explains the mutable-state differences of updates.

Key properties remain identity, not mutable state.

## Characteristics and other re-materialized lists

A freshly materialized `IReadOnlyList<T>` is a different CLR object even when its elements are identical. `ReadOnlyListSequenceComparer<T>` provides an explicit order-sensitive state equality option for that common case.

Forge does not assume that every collection is ordered or keyed. If order is not domain-significant, consumers should provide their own `DeltaComparer` or reconcile that collection separately with Sync.

## Delete

Full delete is current state reconciled against an empty desired collection. Partial delete is current state reconciled against a reduced desired collection. Forge does not order removals by dependencies; that is orchestration policy and remains application-owned.

## Cancel / compensation

Cancellation admission and action selection remain application policy. Delta is useful after the application executes suppression, cancellation or compensation to describe the actual state effect.

No cancellation-specific API was added. This is intentional.

## Forge 1.0 extended dogfood

The 1.0 capability train adds cross-type reconciliation, operation planning, dependency waves, topology planning, reversible plans, portable manifests, stale-plan validation and batch composition. See `GENERIC_DISTRIBUTED_ROLLOUT.md` for a concrete distributed application example covering partial configuration updates, complete desired state, dependency waves, stale-state protection and execution-aware replanning.
