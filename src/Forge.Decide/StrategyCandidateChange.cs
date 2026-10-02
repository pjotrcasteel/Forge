namespace Forge.Decide;

/// <summary>
/// Describes evidence changes for one strategy across two evaluations.
/// </summary>
[Flags]
public enum StrategyCandidateChange
{
    /// <summary>
    /// No observable evidence changed.
    /// </summary>
    None = 0,

    /// <summary>
    /// The strategy exists only in the later evaluation.
    /// </summary>
    Added = 1,

    /// <summary>
    /// The strategy exists only in the earlier evaluation.
    /// </summary>
    Removed = 2,

    /// <summary>
    /// The strategy changed between applicable and rejected.
    /// </summary>
    ApplicabilityChanged = 4,

    /// <summary>
    /// The application-provided explanation changed.
    /// </summary>
    ReasonChanged = 8,

    /// <summary>
    /// The application-owned plan changed according to the supplied canonicalizer.
    /// </summary>
    PlanChanged = 16,
}