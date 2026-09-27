namespace Forge.Sync;

/// <summary>
/// One executable structural operation tracked across replanning cycles.
/// </summary>
public sealed record IncrementalPlanOperation<TOperationId>(
    TOperationId OperationId,
    SyncManifestEntry Entry,
    string SemanticDigest);
