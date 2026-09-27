namespace Forge.Sync;

/// <summary>Immutable instantiated plan template with validated typed dependencies.</summary>
public sealed class PlanTemplateInstance<TOperation, TKey>
    where TKey : notnull
{
    public PlanTemplateInstance(
        IReadOnlyList<PlanTemplateOperation<TOperation, TKey>> operations,
        IReadOnlyList<DependencyEdge<TKey>> dependencies,
        DependencyPlan<PlanTemplateOperation<TOperation, TKey>, TKey> dependencyPlan)
    {
        ArgumentNullException.ThrowIfNull(operations);
        ArgumentNullException.ThrowIfNull(dependencies);
        ArgumentNullException.ThrowIfNull(dependencyPlan);
        Operations = Array.AsReadOnly(operations.ToArray());
        Dependencies = Array.AsReadOnly(dependencies.ToArray());
        DependencyPlan = dependencyPlan;
    }

    public IReadOnlyList<PlanTemplateOperation<TOperation, TKey>> Operations { get; }
    public IReadOnlyList<DependencyEdge<TKey>> Dependencies { get; }
    public DependencyPlan<PlanTemplateOperation<TOperation, TKey>, TKey> DependencyPlan { get; }
    public bool CanExecute => !DependencyPlan.HasCycles;
}
