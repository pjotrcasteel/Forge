using Forge.Sync;

namespace Forge.Sync.Tests;

[GenerateSync(nameof(OrderItem.Id))]
internal sealed record OrderItem(
    Guid Id,
    string Product,
    int Quantity)
{
    public ItemConfiguration Configuration { get; init; } = new("Standard", true);
}
