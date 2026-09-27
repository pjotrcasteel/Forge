using System.Text.Json;

namespace Forge.Sync;

/// <summary>
/// Result of simulating a portable Sync manifest against a current-state snapshot.
/// </summary>
public sealed class ManifestSimulationResult
{
    /// <summary>
    /// Creates an immutable simulation result.
    /// </summary>
    public ManifestSimulationResult(
        ManifestPreconditionResult preconditions,
        IReadOnlyDictionary<string, JsonElement> projectedState,
        bool applied)
    {
        ArgumentNullException.ThrowIfNull(preconditions);
        ArgumentNullException.ThrowIfNull(projectedState);
        Preconditions = preconditions;
        ProjectedState = projectedState;
        Applied = applied;
    }

    /// <summary>Gets stale-plan/precondition validation performed before simulation.</summary>
    public ManifestPreconditionResult Preconditions { get; }

    /// <summary>Gets the projected state. When <see cref="Applied"/> is false this is the unchanged input snapshot.</summary>
    public IReadOnlyDictionary<string, JsonElement> ProjectedState { get; }

    /// <summary>Gets whether the plan was applicable and therefore simulated.</summary>
    public bool Applied { get; }

    /// <summary>
    /// Compares the projected state with another portable state snapshot using Forge's canonical JSON semantics.
    /// </summary>
    public bool Matches(IReadOnlyDictionary<string, JsonElement> expected)
    {
        ArgumentNullException.ThrowIfNull(expected);
        if (ProjectedState.Count != expected.Count)
        {
            return false;
        }

        foreach (var item in ProjectedState)
        {
            if (!expected.TryGetValue(item.Key, out var expectedValue)
                || !PortableJsonComparer.AreEquivalent(item.Value, expectedValue))
            {
                return false;
            }
        }

        return true;
    }
}
