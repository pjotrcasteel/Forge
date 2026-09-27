namespace Forge.Sync;

/// <summary>
/// Maps one semantic state property between current and desired types.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class CrossSyncMapAttribute : Attribute
{
    /// <summary>
    /// Initializes a state mapping.
    /// </summary>
    public CrossSyncMapAttribute(string currentProperty, string desiredProperty)
    {
        CurrentProperty = currentProperty;
        DesiredProperty = desiredProperty;
    }

    /// <summary>Gets the current-state property name.</summary>
    public string CurrentProperty { get; }

    /// <summary>Gets the desired-state property name.</summary>
    public string DesiredProperty { get; }

    /// <summary>Gets or sets the path exposed by the generated Delta.</summary>
    public string? Path { get; set; }

    /// <summary>
    /// Gets or sets an optional comparer type implementing <see cref="IEqualityComparer{T}"/> for the mapped property type.
    /// </summary>
    public Type? ComparerType { get; set; }
}
