namespace Forge.Sync;

/// <summary>Typed approval partition with dependency-safe scope ordering.</summary>
public sealed class ApprovalScopePlan<TScope, TOperation>
    where TScope : notnull
{
    public ApprovalScopePlan(
        IReadOnlyList<ApprovalScopeGroup<TScope, TOperation>> scopes,
        IReadOnlyList<ApprovalScopeDependency<TScope>> dependencies,
        DependencyPlan<ApprovalScopeGroup<TScope, TOperation>, TScope> dependencyPlan)
    {
        ArgumentNullException.ThrowIfNull(scopes);
        ArgumentNullException.ThrowIfNull(dependencies);
        ArgumentNullException.ThrowIfNull(dependencyPlan);
        Scopes = Array.AsReadOnly(scopes.ToArray());
        Dependencies = Array.AsReadOnly(dependencies.ToArray());
        DependencyPlan = dependencyPlan;
    }

    public IReadOnlyList<ApprovalScopeGroup<TScope, TOperation>> Scopes { get; }
    public IReadOnlyList<ApprovalScopeDependency<TScope>> Dependencies { get; }
    public DependencyPlan<ApprovalScopeGroup<TScope, TOperation>, TScope> DependencyPlan { get; }
    public bool HasCycles => DependencyPlan.HasCycles;
}
