using Forge.Delta;
using Forge.Sync;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var current = new List<Component>
{
    new("access", "Access", new ComponentConfiguration(null, 100)),
    new("subnet", "Subnet", new ComponentConfiguration("10.1.0.0/24", 100)),
    new("router", "Router", new ComponentConfiguration(null, 100))
};

app.MapGet("/components", () => current);

app.MapPut("/components", (IReadOnlyList<Component> desired) =>
{
    var plan = ComponentSync.Plan(current, desired);

    foreach (var removal in plan.Removed)
    {
        current.RemoveAll(item => ComponentSync.GetKey(item) == removal.Key);
    }

    foreach (var update in plan.Updated)
    {
        var index = current.FindIndex(item => ComponentSync.GetKey(item) == update.Key);
        current[index] = update.Desired;
    }

    foreach (var addition in plan.Added)
    {
        current.Add(addition.Desired);
    }

    return Results.Ok(new
    {
        Added = plan.Added.Count,
        Removed = plan.Removed.Count,
        Updated = plan.Updated.Count,
        Unchanged = plan.Unchanged.Count,
        Changes = plan.Updated.SelectMany(update => update.Delta.Changes)
    });
});

app.Run();

[GenerateDelta]
internal sealed record ComponentConfiguration(
    string? Network,
    int Capacity);

[GenerateSync(nameof(Component.LogicalId))]
internal sealed record Component(
    string LogicalId,
    string Kind,
    ComponentConfiguration Configuration);
