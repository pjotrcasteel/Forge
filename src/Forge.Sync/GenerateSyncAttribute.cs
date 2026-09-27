namespace Forge.Sync;

/// <summary>
/// Requests generation of a keyed collection synchronizer for the annotated type.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public sealed class GenerateSyncAttribute : Attribute
{
    /// <summary>
    /// Creates a sync definition using one or more property names as the item key.
    /// </summary>
    /// <param name="keyProperties">Properties that uniquely identify an item.</param>
    public GenerateSyncAttribute(params string[] keyProperties)
    {
        ArgumentNullException.ThrowIfNull(keyProperties);
        KeyProperties = Array.AsReadOnly((string[])keyProperties.Clone());
    }

    /// <summary>
    /// Gets the configured key properties.
    /// </summary>
    public IReadOnlyList<string> KeyProperties { get; }
}