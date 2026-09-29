# Generic Distributed Rollout Sample

A runnable production-style Forge.Sync example using a fictional distributed application.

**Two-minute visual story:** https://pjotrcasteel.github.io/Forge/service-provisioning.html

The current application state is:

```text
database
└── api
    └── legacy-worker
```

The requested desired state is:

```text
database
└── api (revision/configuration update)
    └── policy-engine (new)
        └── telemetry (new)
```

Forge therefore describes one unchanged component, one update with typed Delta details, two additions, one removal, dependency-safe execution waves, and a portable manifest with a canonical digest.

The sample then deliberately changes the current API state after planning. The manifest precondition check rejects that plan as stale.

Finally, execution is assumed to have partially started:

```text
api update             completed
policy-engine add      running
telemetry add          waiting
legacy-worker remove   waiting
```

A revised desired state arrives while that work is in flight. The example uses `ExecutedReplanner` to distinguish completed work that must remain locked, running work whose changed semantics require application intervention, waiting work that can be cancelled safely, and newly required work such as a message queue.

Forge still does not execute anything. `ProvisioningPlanBuilder` is intentionally application code: it translates the structural Sync plan into the operation model this fictional distributed application would execute.

## Run

```bash
dotnet run --project samples/Forge.ServiceProvisioning.Sample/Forge.ServiceProvisioning.Sample.csproj
```

## Test

```bash
dotnet test tests/Forge.ServiceProvisioning.Sample.Tests/Forge.ServiceProvisioning.Sample.Tests.csproj
```

## What this sample exercises

```text
generated Delta
    ↓
generated Sync
    ↓
application-owned operation classification
    ↓
DependencyPlanner
    ↓
SyncManifest + ManifestDigest
    ↓
ManifestPreconditions
    ↓
ExecutedReplanner
```

The domain is intentionally generic. The same pattern can apply to application rollouts, infrastructure desired state, deployment planning, workflow graphs, product configuration, or data-pipeline orchestration.
