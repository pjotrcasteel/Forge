namespace Forge.Sync;

/// <summary>Immutable group of operations that share one application-defined approval scope.</summary>
public sealed class ApprovalScopeGroup<TScope, TOperation>
    where TScope : notnull
{
    public ApprovalScopeGroup(TScope scope, IReadOnlyList<TOperation> operations)
    {
        ArgumentNullException.ThrowIfNull(operations);
        Scope = scope;
        Operations = Array.AsReadOnly(operations.ToArray());
    }

    public TScope Scope { get; }
    public IReadOnlyList<TOperation> Operations { get; }
}
