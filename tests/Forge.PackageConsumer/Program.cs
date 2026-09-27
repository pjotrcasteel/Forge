using Forge.Delta;
using Forge.Sync;

var current = new Item("item-1", new Configuration("old"));
var desired = new Item("item-1", new Configuration("new"));

var delta = ConfigurationDelta.Between(current.Configuration, desired.Configuration);
if (!delta.ValueChange.HasChanged)
{
    throw new InvalidOperationException("Transitive Forge.Delta generator was not available.");
}

var plan = ItemSync.Plan([current], [desired]);
if (plan.Updated.Count != 1 || plan.Updated[0].Delta.Changes.Count != 1)
{
    throw new InvalidOperationException("Packed Forge.Sync did not generate the expected reconciliation API.");
}

var stored = new StoredCharacteristic(
    Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
    "ipSubnet",
    "old");
var incoming = new IncomingCharacteristic(
    Guid.Empty,
    "IPSUBNET",
    "new");

var matchDefinition = new CrossSyncMatchDefinition<StoredCharacteristic, IncomingCharacteristic, string, ConfigurationDelta>(
    static value => new SyncIdentity<string>(
        $"id:{value.Id}",
        [$"characteristic:{value.CharacteristicId}"]),
    static value => value.Id == Guid.Empty
        ? new SyncIdentity<string>($"characteristic:{value.CharacteristicId}")
        : new SyncIdentity<string>(
            $"id:{value.Id}",
            [$"characteristic:{value.CharacteristicId}"]),
    static (left, right) => ConfigurationDelta.AreEquivalent(
        new Configuration(left.Value),
        new Configuration(right.Value)),
    static (left, right) => ConfigurationDelta.Between(
        new Configuration(left.Value),
        new Configuration(right.Value)),
    SyncMode.Replace,
    StringComparer.OrdinalIgnoreCase);

var crossPlan = CrossSync.Plan([stored], [incoming], matchDefinition);
if (crossPlan.Updated.Count != 1 || crossPlan.Added.Count != 0 || crossPlan.Removed.Count != 0)
{
    throw new InvalidOperationException("Packed Forge.Sync did not expose ordered fallback identity reconciliation.");
}

Console.WriteLine("Forge.Sync package consumer validation passed.");

[GenerateDelta]
internal sealed record Configuration(string Value);

[GenerateSync(nameof(Item.Id))]
internal sealed record Item(
    string Id,
    Configuration Configuration);

internal sealed record StoredCharacteristic(
    Guid Id,
    string CharacteristicId,
    string Value);

internal sealed record IncomingCharacteristic(
    Guid Id,
    string CharacteristicId,
    string Value);
