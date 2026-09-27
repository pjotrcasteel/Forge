# Generator diagnostics

Forge reports unsafe or ineffective configuration at compile time wherever generation can determine the problem.

## Delta

| ID | Severity | Meaning |
| --- | --- | --- |
| `FORGEDELTA001` | Error | Unsupported Delta target shape. |
| `FORGEDELTA002` | Error | `DeltaComparer` does not implement the required `IEqualityComparer<T>`. |
| `FORGEDELTA003` | Error | A configured comparer cannot be constructed by generated code. |
| `FORGEDELTA004` | Error | A participating property uses a ref-like or pointer type. |
| `FORGEDELTA005` | Error | An external Delta profile targets an unsupported type. |
| `FORGEDELTA006` | Error | A profile references a property that does not exist or is not publicly readable. |
| `FORGEDELTA007` | Error | A profile comparer has the wrong `IEqualityComparer<T>` type. |
| `FORGEDELTA008` | Error | A profile comparer is not constructible from the consumer/profile assembly. |
| `FORGEDELTA009` | Error | A profile property is configured more than once or has conflicting ignore/comparer configuration. |
| `FORGEDELTA010` | Warning | A property has both `DeltaIgnore` and `DeltaComparer`; the comparer cannot have an effect. |
| `FORGEDELTA011` | Warning | A generated Delta has no participating state and will always be empty. |

## Sync

| ID | Severity | Meaning |
| --- | --- | --- |
| `FORGESYNC001` | Error | Unsupported Sync target shape. |
| `FORGESYNC002` | Error | No logical key was configured. |
| `FORGESYNC003` | Error | A configured key property is invalid or inaccessible. |
| `FORGESYNC004` | Error | A key property is configured more than once. |
| `FORGESYNC005` | Error | A Delta comparer on synchronized state has the wrong type. |
| `FORGESYNC006` | Error | A Delta comparer on synchronized state cannot be constructed. |
| `FORGESYNC007` | Error | Participating Delta state uses a ref-like or pointer type. |
| `FORGESYNC008` | Error | A logical-key comparer has the wrong type. |
| `FORGESYNC009` | Error | A logical-key comparer cannot be constructed. |
| `FORGESYNC010` | Warning | `SyncKeyComparer` is applied to a property that is not part of the key. |
| `FORGESYNC011` | Warning | `DeltaComparer` is applied to a Sync key and therefore cannot affect generated Delta state. |
| `FORGESYNC012` | Error | A key property uses a ref-like or pointer type. |
| `FORGESYNC013` | Error | `SyncNested` is applied to an unsupported or nullable child collection/type. |
| `FORGESYNC014` | Warning | `DeltaComparer` is applied to a `SyncNested` property and cannot affect parent Delta state. |
| `FORGESYNC015` | Error | `SyncNested` configures an unknown `SyncMode`. |
| `FORGESYNC016` | Error | A property is simultaneously configured as a Sync key and a nested Sync collection. |

## Tooling behavior

Generated Delta types, generated Sync keys, `ValueChange<T>`, `PropertyChange` and `SyncPlan` expose debugger displays so the important state is visible without expanding implementation details.

Warnings mean Forge can still generate deterministic code but the configuration is almost certainly unintended. Errors mean generation cannot safely define the requested semantics.
