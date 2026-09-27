using Forge.Delta;
using Forge.Sync;

var before = new ServiceState("service-1", "pending", new Configuration(100));
var after = new ServiceState("service-1", "active", new Configuration(200));
var delta = ServiceStateDelta.Between(before, after);

IReadOnlyList<ServiceState> current = [before];
IReadOnlyList<ServiceState> desired = [after];
var plan = ServiceStateSync.Plan(current, desired);

Console.WriteLine($"Delta changes: {delta.Changes.Count}");
Console.WriteLine($"Sync updates: {plan.Updated.Count}");

[GenerateDelta]
internal sealed record Configuration(int Capacity);

[GenerateSync(nameof(ServiceState.Id))]
internal sealed record ServiceState(
    string Id,
    string State,
    Configuration Configuration);
