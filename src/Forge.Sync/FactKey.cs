namespace Forge.Sync;

/// <summary>Strongly typed identity for one derived fact.</summary>
public sealed class FactKey<T>
{
    private FactKey(long id)
    {
        Id = id;
    }

    internal long Id { get; }

    /// <summary>Creates a new typed fact key without a string registry.</summary>
    public static FactKey<T> Create() => new(FactKeyIdentity.Next());
}
