namespace Forge.Sync;

/// <summary>
/// Maps one ordered logical-identity route between current and desired types.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class CrossSyncIdentityAttribute : Attribute
{
    /// <summary>
    /// Initializes an identity mapping. Lower order values are tried first.
    /// </summary>
    public CrossSyncIdentityAttribute(string currentProperty, string desiredProperty, int order = 0)
    {
        CurrentProperty = currentProperty;
        DesiredProperty = desiredProperty;
        Order = order;
    }

    /// <summary>Gets the current-state property name.</summary>
    public string CurrentProperty { get; }

    /// <summary>Gets the desired-state property name.</summary>
    public string DesiredProperty { get; }

    /// <summary>Gets identity priority. Lower values are tried first.</summary>
    public int Order { get; }

    /// <summary>
    /// Gets or sets an optional comparer type implementing <see cref="IEqualityComparer{T}"/> for the identity type.
    /// </summary>
    public Type? ComparerType { get; set; }
}
