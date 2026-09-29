namespace Forge.ServiceProvisioning.Sample;

public sealed record ProvisioningScenarioResult(
    int AddedCount,
    int UpdatedCount,
    int RemovedCount,
    int UnchangedCount,
    IReadOnlyList<string> ApiChangePaths,
    IReadOnlyList<ExecutionWaveSummary> ExecutionWaves,
    int ManifestOperationCount,
    string ManifestJson,
    string ManifestDigest,
    bool OriginalStateAccepted,
    bool ChangedStateAccepted,
    IReadOnlyList<string> StaleKeys,
    ReplanSummary Replan);
