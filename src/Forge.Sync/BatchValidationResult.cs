namespace Forge.Sync;

/// <summary>Describes whether a reconciliation batch is structurally and application-valid before execution.</summary>
public sealed class BatchValidationResult
{
    public BatchValidationResult(bool hasDependencyCycles, IReadOnlyList<BatchValidationIssue> issues)
    {
        ArgumentNullException.ThrowIfNull(issues);
        HasDependencyCycles = hasDependencyCycles;
        Issues = Array.AsReadOnly(issues.ToArray());
    }

    public bool HasDependencyCycles { get; }
    public IReadOnlyList<BatchValidationIssue> Issues { get; }
    public bool IsValid => !HasDependencyCycles && Issues.Count == 0;
}
