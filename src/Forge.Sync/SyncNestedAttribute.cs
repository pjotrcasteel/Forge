namespace Forge.Sync;

/// <summary>
/// Marks a child collection on a generated Sync type as an explicitly nested reconciliation boundary.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true)]
public sealed class SyncNestedAttribute : Attribute
{
    /// <summary>
    /// Initializes nested reconciliation with the supplied child collection mode.
    /// </summary>
    /// <param name="mode">The default reconciliation mode for the child collection.</param>
    public SyncNestedAttribute(SyncMode mode = SyncMode.Replace)
    {
        Mode = mode;
    }

    /// <summary>
    /// Gets the default reconciliation mode for the child collection.
    /// </summary>
    public SyncMode Mode { get; }
}
