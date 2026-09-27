namespace Forge.Sync;

/// <summary>Provides the readiness requirements that apply to an operation or plan item.</summary>
public interface IReadinessRequirementProvider<in TItem, TContext, TReason>
    where TReason : notnull
{
    /// <summary>Gets requirements in deterministic evaluation order.</summary>
    IReadOnlyList<IReadinessRequirement<TContext, TReason>> GetRequirements(TItem item);
}
