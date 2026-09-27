namespace Forge.Delta;

/// <summary>
/// Represents an immutable description of differences between two object states.
/// </summary>
public interface IDelta
{
    /// <summary>
    /// Gets a value indicating whether at least one participating property changed.
    /// </summary>
    bool HasChanges { get; }

    /// <summary>
    /// Gets the changed properties in declaration order.
    /// </summary>
    IReadOnlyList<PropertyChange> Changes { get; }
}