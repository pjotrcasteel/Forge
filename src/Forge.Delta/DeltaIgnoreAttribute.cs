namespace Forge.Delta;

/// <summary>
/// Excludes a property from generated delta comparison.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true)]
public sealed class DeltaIgnoreAttribute : Attribute
{
}