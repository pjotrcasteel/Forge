using Forge.Delta;

namespace Forge.Sync;

/// <summary>Compares two complete typed plan snapshots, including both operations and dependencies.</summary>
public static class PlanDelta
{
    /// <summary>Calculates semantic plan changes without flattening operations or dependencies to strings.</summary>
    public static PlanDeltaResult<TOperation, TOperationKey, TOperationDelta, TDependency, TDependencyKey, TDependencyDelta>
        Between<TOperation, TOperationKey, TOperationDelta, TDependency, TDependencyKey, TDependencyDelta>(
            IReadOnlyList<TOperation> previousOperations,
            IReadOnlyList<TOperation> nextOperations,
            PlanElementDefinition<TOperation, TOperationKey, TOperationDelta> operationDefinition,
            IReadOnlyList<TDependency> previousDependencies,
            IReadOnlyList<TDependency> nextDependencies,
            PlanElementDefinition<TDependency, TDependencyKey, TDependencyDelta> dependencyDefinition)
        where TOperationKey : notnull
        where TDependencyKey : notnull
        where TOperationDelta : IDelta
        where TDependencyDelta : IDelta
    {
        ArgumentNullException.ThrowIfNull(operationDefinition);
        ArgumentNullException.ThrowIfNull(dependencyDefinition);
        var operationSync = Definition(operationDefinition);
        var dependencySync = Definition(dependencyDefinition);
        var operations = CrossSync.Plan(previousOperations, nextOperations, operationSync);
        var dependencies = CrossSync.Plan(previousDependencies, nextDependencies, dependencySync);
        return new PlanDeltaResult<TOperation, TOperationKey, TOperationDelta, TDependency, TDependencyKey, TDependencyDelta>(
            operations,
            dependencies);
    }

    private static CrossSyncDefinition<TItem, TItem, TKey, TDelta> Definition<TItem, TKey, TDelta>(
        PlanElementDefinition<TItem, TKey, TDelta> definition)
        where TKey : notnull
        where TDelta : IDelta
        => new(
            definition.KeySelector,
            definition.KeySelector,
            definition.AreEquivalent,
            definition.DeltaFactory,
            SyncMode.Replace,
            definition.KeyComparer);
}
