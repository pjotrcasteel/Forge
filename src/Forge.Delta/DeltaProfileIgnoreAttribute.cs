namespace Forge.Delta;

/// <summary>
/// Excludes a named target property from a generated external-type Delta profile.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
public sealed class DeltaProfileIgnoreAttribute : Attribute
{
    /// <summary>
    /// Initializes an exclusion for a target property.
    /// </summary>
    public DeltaProfileIgnoreAttribute(string propertyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);
        PropertyName = propertyName;
    }

    /// <summary>
    /// Gets the excluded target property name.
    /// </summary>
    public string PropertyName { get; }
}
