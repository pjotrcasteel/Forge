namespace Forge.ServiceProvisioning.Sample;

public sealed record ExecutionWaveSummary(
    int Number,
    IReadOnlyList<string> Operations);
