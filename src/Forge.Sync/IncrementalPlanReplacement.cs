namespace Forge.Sync;

/// <summary>
/// Describes an operation whose logical slot remains but whose semantics changed enough to require a new operation identity.
/// </summary>
public sealed record IncrementalPlanReplacement<TOperationId>(
    IncrementalPlanOperation<TOperationId> Previous,
    IncrementalPlanOperation<TOperationId> Current);
