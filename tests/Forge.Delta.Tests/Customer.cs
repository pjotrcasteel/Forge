using Forge.Delta;

namespace Forge.Delta.Tests;

[GenerateDelta]
internal sealed record Customer(
    Guid Id,
    [property: DeltaComparer(typeof(CaseInsensitiveStringComparer))] string Name,
    string? Email,
    Address Address)
{
    [DeltaIgnore]
    public DateTimeOffset LastModified { get; init; }
}
