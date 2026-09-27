# Desired-state reconciliation

Forge stays transport-, persistence- and workflow-neutral. These scenarios are used as design pressure only.

## Change

Given a current keyed collection and a newly calculated desired collection, Sync classifies logical items as added, removed, updated or unchanged. Updated items expose Delta details, including nested paths.

This supports configuration changes, product reconfiguration, infrastructure desired state, service decomposition and similar flows without Forge knowing the domain.

## Delete

A full delete is simply reconciliation against an empty desired collection. A partial delete is reconciliation against a reduced desired collection.

Forge reports what is absent from desired state. The application remains responsible for dependency ordering, authorization, persistence and side effects.

## Compensation and cancellation

Cancellation admission is policy, not reconciliation. Forge must not decide whether an item is cancellable or which compensating action is appropriate.

Delta remains useful around the action itself: compare state before and after application-owned compensation to record what actually changed. If a future generic planning primitive emerges from multiple unrelated domains, it should be evaluated as a separate package rather than forced into Sync.


## Multiple identity routes

Cross-type persistence models sometimes expose both a stable instance identifier and a business identifier. `SyncIdentity<TKey>` and `CrossSyncMatchDefinition` model those routes explicitly. The canonical key is attempted first; fallback keys are attempted in declaration order only when an earlier key does not match.

Forge rejects an ambiguous fallback that resolves to more than one current item and rejects multiple desired items resolving to the same current item. This keeps reconciliation deterministic while supporting migrations, external models and persistence schemas that cannot be represented by one symmetric key selector.
