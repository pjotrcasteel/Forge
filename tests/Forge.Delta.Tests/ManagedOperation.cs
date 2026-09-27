using Forge.Delta;

namespace Forge.Delta.Tests;

[GenerateDelta]
internal sealed record ManagedOperation(
    string State,
    string? AssignedResource,
    DateTimeOffset UpdatedAt)
{
    [DeltaIgnore]
    public string? DiagnosticNote { get; init; }
}
