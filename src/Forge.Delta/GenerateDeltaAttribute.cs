namespace Forge.Delta;

/// <summary>
/// Requests generation of a strongly typed delta for the annotated type.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public sealed class GenerateDeltaAttribute : Attribute
{
}