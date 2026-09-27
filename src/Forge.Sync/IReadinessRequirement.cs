namespace Forge.Sync;

/// <summary>Evaluates one strongly typed readiness requirement against an application context.</summary>
public interface IReadinessRequirement<in TContext, TReason>
    where TReason : notnull
{
    /// <summary>Evaluates the requirement without mutating application state.</summary>
    ReadinessRequirementResult<TReason> Evaluate(TContext context);
}
