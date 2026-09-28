# Forge.ServiceProvisioning.Sample

A runnable production-style Forge example.

The sample starts with an existing service state:

```text
access
└── router
    └── legacy-vpn
```

The requested desired state is:

```text
access
└── router (configuration update)
    └── firewall (new)
        └── monitoring (new)
```

Forge therefore describes one unchanged component, one update with typed Delta details, two additions, one removal, dependency-safe execution waves, and a portable manifest with a canonical digest.

The sample then deliberately changes the current router state after planning. The manifest precondition check rejects that plan as stale.

Finally, execution is assumed to have partially started:

```text
router update      completed
firewall add       running
monitoring add     waiting
legacy-vpn remove  waiting
```

A revised desired state arrives while that work is in flight. The example uses `ExecutedReplanner` to distinguish completed work that must remain locked, running work whose changed semantics require application intervention, waiting work that can be cancelled safely, and newly required work.

Forge still does not execute anything. `ProvisioningPlanBuilder` is intentionally application code: it translates the structural Sync plan into the operation model this fictional application would execute.

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

The point is not the provisioning domain. The same pattern applies to configuration rollout, infrastructure desired state, product decomposition, deployment planning and other systems where current state and desired state are separated from execution.
