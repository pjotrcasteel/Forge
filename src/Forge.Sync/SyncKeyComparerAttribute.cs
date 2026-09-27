namespace Forge.Sync;

/// <summary>
/// Configures equality for a property that participates in a generated Sync key.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true)]
public sealed class SyncKeyComparerAttribute : Attribute
{
    /// <summary>
    /// Creates a key comparer configuration.
    /// </summary>
    /// <param name="comparerType">
    /// A constructible type implementing <see cref="IEqualityComparer{T}"/> for the key property type.
    /// </param>
    public SyncKeyComparerAttribute(Type comparerType)
    {
        ArgumentNullException.ThrowIfNull(comparerType);
        ComparerType = comparerType;
    }

    /// <summary>
    /// Gets the configured comparer type.
    /// </summary>
    public Type ComparerType { get; }
}
