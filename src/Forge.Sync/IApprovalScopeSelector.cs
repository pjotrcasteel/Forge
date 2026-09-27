namespace Forge.Sync;

/// <summary>Assigns a plan operation to an application-defined approval scope.</summary>
public interface IApprovalScopeSelector<in TOperation, out TScope>
{
    /// <summary>Returns the strongly typed scope that owns approval for the operation.</summary>
    TScope Select(TOperation operation);
}
