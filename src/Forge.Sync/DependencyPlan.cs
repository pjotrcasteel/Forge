namespace Forge.Sync;

/// <summary>
/// Describes deterministic dependency execution waves and any keys participating in a cycle.
/// </summary>
public sealed class DependencyPlan<T, TKey>
{
    public DependencyPlan(
        IReadOnlyList<DependencyWave<T>> createWaves,
        IReadOnlyList<DependencyWave<T>> deleteWaves,
        IReadOnlyList<TKey> cycleKeys)
    {
        ArgumentNullException.ThrowIfNull(createWaves);
        ArgumentNullException.ThrowIfNull(deleteWaves);
        ArgumentNullException.ThrowIfNull(cycleKeys);
        CreateWaves = Array.AsReadOnly(createWaves.ToArray());
        DeleteWaves = Array.AsReadOnly(deleteWaves.ToArray());
        CycleKeys = Array.AsReadOnly(cycleKeys.ToArray());
    }

    /// <summary>Gets prerequisite-first waves appropriate for creation/provisioning.</summary>
    public IReadOnlyList<DependencyWave<T>> CreateWaves { get; }

    /// <summary>Gets dependent-first waves appropriate for deletion/deprovisioning.</summary>
    public IReadOnlyList<DependencyWave<T>> DeleteWaves { get; }

    /// <summary>Gets keys that could not be ordered because they participate in dependency cycles.</summary>
    public IReadOnlyList<TKey> CycleKeys { get; }

    public bool HasCycles => CycleKeys.Count != 0;
    public bool CanExecute => !HasCycles;
}
