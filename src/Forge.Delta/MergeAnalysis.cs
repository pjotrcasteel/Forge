namespace Forge.Delta;

/// <summary>
/// Describes generated three-way change analysis without mutating or constructing domain objects.
/// </summary>
public sealed class MergeAnalysis<TDelta>
    where TDelta : IDelta
{
    public MergeAnalysis(TDelta currentDelta, TDelta desiredDelta, IReadOnlyList<MergeConflict> conflicts)
    {
        ArgumentNullException.ThrowIfNull(currentDelta);
        ArgumentNullException.ThrowIfNull(desiredDelta);
        ArgumentNullException.ThrowIfNull(conflicts);
        CurrentDelta = currentDelta;
        DesiredDelta = desiredDelta;
        Conflicts = Array.AsReadOnly(conflicts.ToArray());
    }

    /// <summary>Gets changes made from baseline to current state.</summary>
    public TDelta CurrentDelta { get; }

    /// <summary>Gets changes requested from baseline to desired state.</summary>
    public TDelta DesiredDelta { get; }

    /// <summary>Gets properties changed differently by both branches.</summary>
    public IReadOnlyList<MergeConflict> Conflicts { get; }

    /// <summary>Gets whether at least one semantic conflict exists.</summary>
    public bool HasConflicts => Conflicts.Count != 0;

    /// <summary>Gets whether the two branches can be combined without a same-property semantic conflict.</summary>
    public bool CanAutoMerge => !HasConflicts;
}
