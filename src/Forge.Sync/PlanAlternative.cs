namespace Forge.Sync;

/// <summary>One strongly typed candidate plan with an application-owned identity.</summary>
public sealed record PlanAlternative<TAlternativeId, TPlan>(TAlternativeId Id, TPlan Plan)
    where TAlternativeId : notnull;
