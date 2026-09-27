namespace Forge.Sync;

/// <summary>Thrown when monotonic fact derivation attempts to assign a different value to an existing fact key.</summary>
public sealed class FactConflictException : InvalidOperationException
{
    public FactConflictException(Type factType)
        : base($"A derived fact of type '{factType.FullName}' already exists with a different value.")
    {
        FactType = factType;
    }

    public Type FactType { get; }
}
