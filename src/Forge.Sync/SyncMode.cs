namespace Forge.Sync;

/// <summary>
/// Defines how current items that are absent from the supplied desired collection are treated.
/// </summary>
public enum SyncMode
{
    /// <summary>
    /// The desired collection is complete. Current-only items are removals.
    /// </summary>
    Replace = 0,

    /// <summary>
    /// The desired collection is a partial upsert. Current-only items are preserved.
    /// </summary>
    Upsert = 1,
}
