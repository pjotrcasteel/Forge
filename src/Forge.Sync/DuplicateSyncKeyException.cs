namespace Forge.Sync;

/// <summary>
/// Thrown when a current or desired collection contains more than one item with the same generated sync key.
/// </summary>
public sealed class DuplicateSyncKeyException : InvalidOperationException
{
    /// <summary>
    /// Initializes the exception.
    /// </summary>
    public DuplicateSyncKeyException(Type itemType, string collectionName, string key)
        : base($"Duplicate sync key '{key}' found in {collectionName} collection for '{itemType.FullName}'.")
    {
        ItemType = itemType;
        CollectionName = collectionName;
        Key = key;
    }

    /// <summary>
    /// Gets the synchronized item type.
    /// </summary>
    public Type ItemType { get; }

    /// <summary>
    /// Gets the input collection name.
    /// </summary>
    public string CollectionName { get; }

    /// <summary>
    /// Gets the formatted duplicate key.
    /// </summary>
    public string Key { get; }
}