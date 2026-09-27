namespace Forge.Sync;

/// <summary>Thrown when a topology edge references a node that is absent from the same topology snapshot.</summary>
public sealed class InvalidTopologyException : InvalidOperationException
{
    public InvalidTopologyException(string collectionName, string? edgeKey, string? missingNodeKey)
        : base($"Topology '{collectionName}' edge '{edgeKey}' references missing node '{missingNodeKey}'.")
    {
        CollectionName = collectionName;
        EdgeKey = edgeKey;
        MissingNodeKey = missingNodeKey;
    }

    public string CollectionName { get; }
    public string? EdgeKey { get; }
    public string? MissingNodeKey { get; }
}
