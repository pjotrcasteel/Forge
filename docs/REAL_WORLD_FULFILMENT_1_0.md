# Forge 1.0 — Real Fulfilment Example

This example uses the shape of the **current FulfilmentManagement code** rather than a toy customer/order model.

It has two parts:

1. Replace the hand-written comparison inside the current `CharacteristicUpsertPlanner` with Forge semantics.
2. Show how the same primitives can grow into the upcoming Service Change flow without making Forge IRMA-specific.

The important boundary remains:

> **Forge calculates and describes the transition. Fulfilment owns policy, persistence, SPIS calls, events and workflow execution.**

---

# 1. Current Fulfilment code: `CharacteristicUpsertPlanner`

The current service has:

```text
ServiceCharacteristicNode       current persisted graph state
CharacteristicModel             incoming characteristic update
```

The current planner:

- indexes existing nodes by `CharacteristicId`;
- compares identifiers case-insensitively;
- updates an existing node in place;
- adds a node when the key does not exist;
- leaves current characteristics untouched when they are omitted from the payload;
- manually compares `Name`, `Value`, `Source`, `Description` and `IncludeInInventory`.

That last part currently looks conceptually like:

```csharp
private static bool HasChanges(
    ServiceCharacteristicNode existingNode,
    CharacteristicModel characteristic,
    string value)
    => !string.Equals(existingNode.Name, characteristic.Name, StringComparison.Ordinal)
       || !string.Equals(existingNode.Value, value, StringComparison.Ordinal)
       || !string.Equals(existingNode.Source, characteristic.Source, StringComparison.Ordinal)
       || !string.Equals(existingNode.Description, characteristic.Description, StringComparison.Ordinal)
       || existingNode.IncludeInInventory != characteristic.IncludeInInventory;
```

This is a good real Forge use case because the **current and desired CLR types are different** and the payload has **partial-upsert semantics**.

---

## 2. Define the comparable state once

Do not put Forge annotations on the GORM node just to make the library happy.

Instead define the exact state that matters to the persistence decision:

```csharp
using Forge.Delta;

[GenerateDelta]
internal readonly record struct CharacteristicPersistenceState(
    string Name,
    string Value,
    string? Source,
    string? Description,
    bool IncludeInInventory);
```

Then map both architectural sides into that state:

```csharp
private static CharacteristicPersistenceState ToState(
    ServiceCharacteristicNode node)
{
    return new CharacteristicPersistenceState(
        node.Name,
        node.Value,
        node.Source,
        node.Description,
        node.IncludeInInventory);
}

private static CharacteristicPersistenceState ToState(
    CharacteristicModel characteristic)
{
    return new CharacteristicPersistenceState(
        characteristic.Name,
        CharacteristicValueStorage.Encode(characteristic.Value),
        characteristic.Source,
        characteristic.Description,
        characteristic.IncludeInInventory);
}
```

The generated Delta is now the single definition of:

> "When does a characteristic persistence row materially differ?"

---

# 3. Cross-type partial Upsert

Define the reconciliation semantics:

```csharp
using Forge.Sync;

private static readonly CrossSyncDefinition<
    ServiceCharacteristicNode,
    CharacteristicModel,
    string,
    CharacteristicPersistenceStateDelta> CharacteristicSyncDefinition =
        new(
            static node => node.CharacteristicId,
            static characteristic => characteristic.CharacteristicId,
            static (node, characteristic) =>
                CharacteristicPersistenceStateDelta.AreEquivalent(
                    ToState(node),
                    ToState(characteristic)),
            static (node, characteristic) =>
                CharacteristicPersistenceStateDelta.Between(
                    ToState(node),
                    ToState(characteristic)),
            SyncMode.Upsert,
            StringComparer.OrdinalIgnoreCase);
```

Why `SyncMode.Upsert`?

Because this current event is **not** the complete desired characteristic collection.

If Fulfilment currently has:

```text
rfs.materiaaltype
rfs.bandbreedte
rfs.ipSubnet
```

and SPIS sends only:

```text
rfs.ipSubnet
```

then:

```text
rfs.materiaaltype
rfs.bandbreedte
```

must remain untouched.

Forge expresses that explicitly:

```csharp
var plan = CrossSync.Plan(
    existingNodes,
    incomingCharacteristics,
    CharacteristicSyncDefinition);
```

The result is:

```text
Added
    incoming characteristics that do not exist yet

Updated
    existing logical characteristics whose participating state differs

Unchanged
    characteristics supplied again with identical state

Preserved
    existing characteristics omitted from this partial payload

Removed
    always empty in this Upsert scenario
```

That maps directly to the current service behavior.

---

# 4. Applying the plan remains Fulfilment code

Forge does not mutate GORM nodes.

The application does:

```csharp
foreach (var update in plan.Updated)
{
    var existingNode = update.Current;
    var characteristic = update.Desired;

    existingNode.Name = characteristic.Name;
    existingNode.Value = CharacteristicValueStorage.Encode(
        characteristic.Value);
    existingNode.Source = characteristic.Source;
    existingNode.Description = characteristic.Description;
    existingNode.IncludeInInventory =
        characteristic.IncludeInInventory;

    nodesToUpdate.Add(existingNode);
}

foreach (var addition in plan.Added)
{
    var characteristic = addition.Desired;

    nodesToAdd.Add(
        new ServiceCharacteristicNode
        {
            Id = characteristic.Id,
            CharacteristicId = characteristic.CharacteristicId,
            Name = characteristic.Name,
            Value = CharacteristicValueStorage.Encode(
                characteristic.Value),
            Source = characteristic.Source,
            Description = characteristic.Description,
            IncludeInInventory =
                characteristic.IncludeInInventory,
            CreatedAt = createdAt,
        });
}
```

The useful difference is that the comparison policy is no longer duplicated here.

---

# 5. You also get the reason for an update

Today the planner knows only:

```text
this node needs updating
```

Forge also gives:

```csharp
foreach (var update in plan.Updated)
{
    foreach (var change in update.Delta.Changes)
    {
        logger.LogInformation(
            "Characteristic {CharacteristicId} changed {Path}: {Before} -> {After}",
            update.Key,
            change.Path,
            change.Before,
            change.After);
    }
}
```

A real SPIS response could therefore explain:

```text
rfs.ipSubnet
Value:
json:null
->
"10.20.0.0/24"
```

without adding another hand-written comparison block.

---

# 6. Important current-service nuance: duplicate incoming keys

The current `CharacteristicUpsertPlanner` deliberately accepts the same `CharacteristicId` twice in one payload.

Its current behavior is effectively:

```text
first occurrence -> add/update
second occurrence -> update the same newly planned node
last value wins
```

Forge rejects duplicate keys because a reconciliation snapshot should not contain ambiguous logical identity.

That is intentional.

If Fulfilment must preserve its current "last occurrence wins" behavior, normalize that application policy **before** invoking Forge:

```csharp
var normalizedCharacteristics = characteristics
    .GroupBy(
        static characteristic => characteristic.CharacteristicId,
        StringComparer.OrdinalIgnoreCase)
    .Select(static group => group.Last())
    .ToArray();

var plan = CrossSync.Plan(
    existingNodes,
    normalizedCharacteristics,
    CharacteristicSyncDefinition);
```

This is a useful example of the boundary:

```text
"What does duplicate input mean?"
        -> application policy

"Given one desired item per logical key, what changed?"
        -> Forge
```

---

# 7. Future Change flow: complete desired RFS topology

The upcoming Change flow is different from the partial characteristic event.

After decomposition, Fulfilment can eventually have:

```text
CURRENT topology
        versus
DESIRED topology
```

That is true complete desired-state reconciliation.

For example:

```text
CURRENT

access
subnet
router

router -> access
subnet -> access


DESIRED

access
subnet'
firewall

firewall -> access
subnet -> access
```

Where `subnet'` is the same logical RFS with changed state.

---

## 8. Example RFS state

```csharp
[GenerateSync(nameof(RfsState.Id))]
internal sealed record RfsState(
    string Id,
    string ServiceType,
    string State,
    [property: SyncNested]
    IReadOnlyList<RfsCharacteristic> Characteristics,
    IReadOnlyList<string> DependsOn);

[GenerateSync(nameof(RfsCharacteristic.Id))]
internal sealed record RfsCharacteristic(
    string Id,
    string? Value);

[GenerateSync(nameof(RfsRelationship.Id))]
internal sealed record RfsRelationship(
    string Id,
    string SourceId,
    string TargetId,
    string Type);
```

The model remains generic from Forge's perspective.

---

# 9. Reconcile nodes and relationships together

Define node semantics:

```csharp
var nodeDefinition =
    new TopologyNodeDefinition<
        RfsState,
        string,
        RfsStateDelta>(
            static node => node.Id,
            RfsStateDelta.AreEquivalent,
            RfsStateDelta.Between,
            StringComparer.Ordinal);
```

Define relationship semantics:

```csharp
var edgeDefinition =
    new TopologyEdgeDefinition<
        RfsRelationship,
        string,
        string,
        RfsRelationshipDelta>(
            static edge => edge.Id,
            static edge => edge.SourceId,
            static edge => edge.TargetId,
            RfsRelationshipDelta.AreEquivalent,
            RfsRelationshipDelta.Between,
            StringComparer.Ordinal);
```

Then:

```csharp
var topology = TopologySync.Plan(
    currentRfs,
    desiredRfs,
    currentRelationships,
    desiredRelationships,
    nodeDefinition,
    edgeDefinition);
```

Forge validates that every relationship points to nodes that actually exist in its topology snapshot.

It then produces:

```text
Nodes

Added
    firewall

Removed
    router

Updated
    subnet

Unchanged
    access


Relationships

Added
    firewall -> access

Removed
    router -> access

Unchanged
    subnet -> access
```

---

# 10. Ask exactly what changed inside `subnet`

Because the RFS is a generated Sync type, its update carries a generated Delta.

The parent Delta excludes the nested collection as scalar state because `[SyncNested]` owns it.

You can ask for the nested characteristic plan:

```csharp
var subnetUpdate = topology.Nodes.Updated
    .Single(update => update.Key == "subnet");

var characteristicPlan =
    RfsStateSync.PlanCharacteristics(
        subnetUpdate.Current,
        subnetUpdate.Desired);
```

That can say:

```text
Updated characteristic

ipSubnet
    Value:
    10.1.0.0/24
    ->
    10.2.0.0/24
```

So Change gets two levels of explanation:

```text
RFS "subnet" changed

because

Characteristic "ipSubnet" changed
```

---

# 11. Turn structural changes into operations

Fulfilment can classify the RFS changes without putting provisioning rules into Forge:

```csharp
enum RfsOperation
{
    Provision,
    Modify,
    Replace,
    Delete
}
```

```csharp
var operations = CrossSyncOperationPlanner.Classify(
    topology.Nodes,
    static _ => RfsOperation.Provision,
    static update =>
        update.Delta.ServiceTypeChange.HasChanged
            ? RfsOperation.Replace
            : RfsOperation.Modify,
    static _ => RfsOperation.Delete);
```

For example:

```text
firewall
    -> Provision

subnet
    -> Modify

router
    -> Delete
```

Forge records the classification.

Fulfilment decides what `Provision`, `Modify`, `Replace` and `Delete` actually mean for SPIS.

---

# 12. Dependency ordering

Suppose:

```text
access
  ^
  |
subnet

access
  ^
  |
firewall
```

The desired state can carry dependencies:

```csharp
var dependencyPlan = DependencyPlanner.Plan(
    desiredRfs,
    static rfs => rfs.Id,
    static rfs => rfs.DependsOn,
    StringComparer.Ordinal);
```

For provisioning:

```csharp
foreach (var wave in dependencyPlan.CreateWaves)
{
    // all previous waves are complete
    // items in this wave may execute concurrently
}
```

For deletion:

```csharp
foreach (var wave in dependencyPlan.DeleteWaves)
{
    // dependent-first ordering
}
```

If the new decomposition accidentally contains:

```text
A -> B
B -> C
C -> A
```

Forge reports the cycle before SPIS receives anything.

---

# 13. Produce an approval/outbox manifest

The node and relationship plans can become portable documents:

```csharp
var rfsManifest = CrossSyncManifest.Create(
    topology.Nodes,
    static key => key);

var relationshipManifest = CrossSyncManifest.Create(
    topology.Edges,
    static key => key);
```

These are JSON-safe versioned descriptions rather than serialized source-generator implementation classes.

They can be stored in an outbox or presented for approval.

---

# 14. Bind approval to the exact plan

```csharp
var rfsDigest =
    ManifestDigest.ComputeSha256Hex(
        rfsManifest);
```

The digest is canonical.

Changing:

```text
operation
key
current state
desired state
Delta path/value
```

changes the digest.

Formatting JSON differently does not.

An approval record can therefore store:

```text
Change ServiceOrder
PlanDigest = 74c...
ApprovedBy = ...
ApprovedAt = ...
```

and the executor can verify it is still looking at the exact approved plan.

---

# 15. Detect stale state before executing the approved Change

Planning and execution may be separated by seconds, minutes or longer.

Meanwhile SPIS could have updated an RFS.

Create a fresh current-state snapshot:

```csharp
var currentSnapshot = ManifestStateSnapshot.Create(
    latestRfs,
    static rfs => rfs.Id,
    static id => id);
```

Validate:

```csharp
var preconditions = ManifestPreconditions.Validate(
    rfsManifest,
    currentSnapshot);

if (!preconditions.IsSatisfied)
{
    // do not execute an obsolete plan
    // re-plan or run conflict analysis
}
```

Forge can distinguish:

```text
expected item no longer exists

new item already exists

current item changed after planning
```

That protects an approved desired-state plan from blindly overwriting newer state.

---

# 16. Three-way conflict analysis after a stale plan

Suppose:

```text
baseline subnet
    bandwidth = 100
    ipSubnet  = 10.1.0.0/24

current subnet
    bandwidth = 200
    ipSubnet  = 10.1.0.0/24

desired Change
    bandwidth = 100
    ipSubnet  = 10.2.0.0/24
```

Then:

```csharp
var merge = RfsStateDelta.AnalyzeMerge(
    baseline,
    current,
    desired);
```

These changes are independent:

```text
CURRENT changed bandwidth
DESIRED changed ipSubnet
```

so Forge reports no same-property conflict.

But:

```text
CURRENT:
    ipSubnet = 10.3.0.0/24

DESIRED:
    ipSubnet = 10.2.0.0/24
```

produces a conflict on:

```text
ipSubnet
```

The application then decides whether to:

```text
reject
re-plan
prefer current
prefer desired
ask for approval
```

Forge does not make that policy decision.

---

# 17. Compose the whole Change as one batch

A Change can affect multiple categories:

```text
RFS state
Resources
Relationships
```

Build separate manifests and compose them:

```csharp
var batch = ReconciliationBatch.Create(
    [
        new NamedSyncManifest(
            "rfs",
            rfsManifest),

        new NamedSyncManifest(
            "resources",
            resourceManifest),

        new NamedSyncManifest(
            "relationships",
            relationshipManifest)
    ],
    [
        new PlanDependency(
            "resources",
            "rfs"),

        new PlanDependency(
            "relationships",
            "resources")
    ]);
```

Forge now gives execution waves:

```text
Wave 1
    rfs

Wave 2
    resources

Wave 3
    relationships
```

And application-wide validation can happen **before anything executes**:

```csharp
var validation = batch.Validate(
    value => value.TotalOperationCount > 500
        ? new BatchValidationIssue(
            "CHANGE_TOO_LARGE",
            "Manual approval is required.")
        : null);

if (!validation.IsValid)
{
    return;
}
```

This maps naturally to an all-or-nothing assessment model without putting workflow execution inside Forge.

---

# 18. Compensation

If a forward Replace-mode plan was calculated and execution later needs compensating:

```csharp
var reverse = CrossSyncPlanInverter.Invert(
    topology.Nodes,
    static delta => delta.Invert());
```

Conceptually:

```text
Forward

Added firewall
Removed router
Updated subnet old -> new


Reverse

Removed firewall
Added router
Updated subnet new -> old
```

Forge calculates the reverse description.

Fulfilment still decides whether the reverse operations are legal and whether point-of-no-return or external-system state prevents compensation.

---

# 19. Where Cancel fits

Cancel is still **not** `Sync`.

Your cancellation policy needs information such as:

```text
execution state
cancellation policy
point of no return
completed/not completed
```

That decides:

```text
Suppress
Cancel
Delete
Reject
```

Forge should not own that decision.

Forge becomes useful around it:

```text
Cancellation assessment
    -> application policy

Resulting resource/service state
    -> Delta

Delete/compensation topology
    -> Sync / TopologySync

Safe reverse structural plan
    -> Invert

All proposed operations valid?
    -> ReconciliationBatch
```

That is the boundary we want.

---

# 20. Why this is a better Forge dogfood example

This example pressures almost every production concern the package claims to solve:

```text
different current/desired CLR models
case-insensitive logical identity
partial-upsert semantics
complete desired-state semantics
nested keyed collections
node + edge topology
dependency ordering
typed operation classification
portable plans
approval identity
stale-plan detection
three-way conflicts
batch-wide validation
compensation
```

Yet Forge still contains no knowledge of:

```text
KPN
IRMA
Fulfilment
SPIS
TMF641
RFS
ServiceOrder
Dapr
GORM
```

Those concepts exist only in the consuming application.

That is a strong signal that the abstraction is staying generic while being directly useful in the core services.
