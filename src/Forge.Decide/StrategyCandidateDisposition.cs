namespace Forge.Decide;

/// <summary>
/// Describes how a candidate participated in a completed decision.
/// </summary>
public enum StrategyCandidateDisposition
{
    /// <summary>
    /// The strategy was applicable but was not selected.
    /// </summary>
    Applicable,

    /// <summary>
    /// The strategy was not applicable to the evaluated context.
    /// </summary>
    Rejected,

    /// <summary>
    /// The strategy was applicable and selected.
    /// </summary>
    Selected,
}