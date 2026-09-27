namespace Forge.Delta;

/// <summary>
/// Uses a custom equality comparer when determining whether a property changed.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true)]
public sealed class DeltaComparerAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeltaComparerAttribute"/> class.
    /// </summary>
    /// <param name="comparerType">
    /// A concrete type implementing <see cref="IEqualityComparer{T}"/> for the annotated property type.
    /// </param>
    public DeltaComparerAttribute(Type comparerType)
    {
        ArgumentNullException.ThrowIfNull(comparerType);
        ComparerType = comparerType;
    }

    /// <summary>
    /// Gets the equality comparer type.
    /// </summary>
    public Type ComparerType { get; }
}
