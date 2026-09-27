using Forge.Sync;

namespace Forge.Sync.Tests;

[GenerateSync(nameof(OrderLine.OrderId), nameof(OrderLine.LineNumber))]
internal sealed record OrderLine(
    Guid OrderId,
    int LineNumber,
    string Product,
    int Quantity);