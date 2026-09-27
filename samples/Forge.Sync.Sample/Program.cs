using Forge.Sync;

var removedId = Guid.NewGuid();
var updatedId = Guid.NewGuid();
var addedId = Guid.NewGuid();

IReadOnlyList<OrderItem> current =
[
    new OrderItem(removedId, "Old modem", 1),
    new OrderItem(updatedId, "Router", 1)
];

IReadOnlyList<OrderItem> desired =
[
    new OrderItem(updatedId, "Router", 2),
    new OrderItem(addedId, "Bracket", 1)
];

var plan = OrderItemSync.Plan(current, desired);

Console.WriteLine($"Added: {plan.Added.Count}");
Console.WriteLine($"Removed: {plan.Removed.Count}");
Console.WriteLine($"Updated: {plan.Updated.Count}");

foreach (var update in plan.Updated)
{
    foreach (var change in update.Delta.Changes)
    {
        Console.WriteLine($"{update.Current.Id}: {change.Path}: {change.Before} -> {change.After}");
    }
}

[GenerateSync(nameof(OrderItem.Id))]
internal sealed record OrderItem(
    Guid Id,
    string Product,
    int Quantity);