namespace Forge.Sync;

/// <summary>Immutable validated transition spanning multiple application-defined topology components.</summary>
public sealed class CompositeTopologyPlan<TContext, TOperation, TKey, TViolation>
    where TKey : notnull
{
    public CompositeTopologyPlan(
        TContext context,
        IReadOnlyList<TOperation> operations,
        IReadOnlyList<DependencyEdge<TKey>> dependencies,
        DependencyPlan<TOperation, TKey> dependencyPlan,
        IReadOnlyList<TViolation> violations)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(operations);
        ArgumentNullException.ThrowIfNull(dependencies);
        ArgumentNullException.ThrowIfNull(dependencyPlan);
        ArgumentNullException.ThrowIfNull(violations);
        Context = context;
        Operations = Array.AsReadOnly(operations.ToArray());
        Dependencies = Array.AsReadOnly(dependencies.ToArray());
        DependencyPlan = dependencyPlan;
        Violations = Array.AsReadOnly(violations.ToArray());
    }

    public TContext Context { get; }
    public IReadOnlyList<TOperation> Operations { get; }
    public IReadOnlyList<DependencyEdge<TKey>> Dependencies { get; }
    public DependencyPlan<TOperation, TKey> DependencyPlan { get; }
    public IReadOnlyList<TViolation> Violations { get; }
    public bool HasCycles => DependencyPlan.HasCycles;
    public bool IsValid => !HasCycles && Violations.Count == 0;
}
