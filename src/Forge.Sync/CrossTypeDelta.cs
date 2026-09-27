using Forge.Delta;

namespace Forge.Sync;

/// <summary>
/// Source-generated Delta for a cross-type profile whose two CLR types represent the same semantic state.
/// </summary>
public sealed class CrossTypeDelta : IDelta
{
    /// <summary>
    /// Creates an immutable cross-type Delta.
    /// </summary>
    public CrossTypeDelta(IReadOnlyList<PropertyChange> changes)
    {
        ArgumentNullException.ThrowIfNull(changes);
        Changes = Array.AsReadOnly(changes.ToArray());
    }

    /// <inheritdoc />
    public bool HasChanges => Changes.Count != 0;

    /// <inheritdoc />
    public IReadOnlyList<PropertyChange> Changes { get; }
}
