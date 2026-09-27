namespace Forge.Delta;

/// <summary>
/// Requests generation of a strongly typed delta for a type that cannot or should not be annotated directly.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public sealed class GenerateDeltaProfileAttribute : Attribute
{
    /// <summary>
    /// Initializes a profile for the supplied target type.
    /// </summary>
    /// <param name="targetType">The type whose public readable state should participate in the generated delta.</param>
    public GenerateDeltaProfileAttribute(Type targetType)
    {
        ArgumentNullException.ThrowIfNull(targetType);
        TargetType = targetType;
    }

    /// <summary>
    /// Gets the type compared by this profile.
    /// </summary>
    public Type TargetType { get; }
}
