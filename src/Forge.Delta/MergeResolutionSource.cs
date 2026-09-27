namespace Forge.Delta;

/// <summary>
/// Identifies where a resolved three-way merge value came from.
/// </summary>
public enum MergeResolutionSource
{
    /// <summary>Uses the value from the current branch.</summary>
    Current,

    /// <summary>Uses the value from the desired branch.</summary>
    Desired,

    /// <summary>Uses the original baseline value.</summary>
    Baseline,

    /// <summary>Uses a value produced by an application-supplied resolver.</summary>
    Custom,

    /// <summary>Uses a value that does not require conflict resolution.</summary>
    NonConflicting
}
