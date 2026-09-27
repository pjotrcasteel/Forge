namespace Forge.Sync;

/// <summary>
/// Thrown when a synchronization identity key resolves to more than one current item.
/// </summary>
public sealed class AmbiguousSyncIdentityException : InvalidOperationException
{
    /// <summary>
    /// Initializes the exception.
    /// </summary>
    public AmbiguousSyncIdentityException(Type itemType, string key)
        : base($"Sync identity key '{key}' resolves to more than one current '{itemType.FullName}' item.")
    {
        ItemType = itemType;
        Key = key;
    }

    /// <summary>Gets the current item type whose identity was ambiguous.</summary>
    public Type ItemType { get; }

    /// <summary>Gets the formatted identity key that matched multiple current items.</summary>
    public string Key { get; }
}
