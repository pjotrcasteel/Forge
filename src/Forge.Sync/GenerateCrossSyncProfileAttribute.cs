namespace Forge.Sync;

/// <summary>
/// Requests a source-generated cross-type reconciliation profile.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class GenerateCrossSyncProfileAttribute : Attribute
{
    /// <summary>
    /// Initializes the profile target types.
    /// </summary>
    public GenerateCrossSyncProfileAttribute(Type currentType, Type desiredType)
    {
        CurrentType = currentType;
        DesiredType = desiredType;
    }

    /// <summary>Gets the current-state CLR type.</summary>
    public Type CurrentType { get; }

    /// <summary>Gets the desired-state CLR type.</summary>
    public Type DesiredType { get; }
}
