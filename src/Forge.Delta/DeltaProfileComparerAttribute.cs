namespace Forge.Delta;

/// <summary>
/// Configures equality for a named target property in an external-type Delta profile.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
public sealed class DeltaProfileComparerAttribute : Attribute
{
    /// <summary>
    /// Initializes custom equality for a target property.
    /// </summary>
    public DeltaProfileComparerAttribute(string propertyName, Type comparerType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);
        ArgumentNullException.ThrowIfNull(comparerType);
        PropertyName = propertyName;
        ComparerType = comparerType;
    }

    /// <summary>
    /// Gets the configured target property name.
    /// </summary>
    public string PropertyName { get; }

    /// <summary>
    /// Gets the equality comparer type.
    /// </summary>
    public Type ComparerType { get; }
}
