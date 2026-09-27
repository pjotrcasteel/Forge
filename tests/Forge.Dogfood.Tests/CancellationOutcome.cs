using Forge.Delta;

namespace Forge.Dogfood.Tests;

[GenerateDelta]
internal sealed record CancellationOutcome(
    string ExecutionState,
    string? ResourceReference,
    DateTimeOffset LastUpdated)
{
    [DeltaIgnore]
    public string? AssessmentReason { get; init; }
}
