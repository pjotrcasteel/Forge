namespace Forge.Sync;

/// <summary>
/// Immutable snapshot of executable structural operations and their stable application-owned identities.
/// </summary>
public sealed class IncrementalPlan<TOperationId>
{
    /// <summary>
    /// Creates a snapshot of tracked operations.
    /// </summary>
    public IncrementalPlan(IReadOnlyList<IncrementalPlanOperation<TOperationId>> operations)
    {
        ArgumentNullException.ThrowIfNull(operations);
        Operations = Array.AsReadOnly(operations.ToArray());
    }

    /// <summary>Gets tracked operations in manifest order.</summary>
    public IReadOnlyList<IncrementalPlanOperation<TOperationId>> Operations { get; }
}
