namespace Forge.Sync;

/// <summary>Represents named plan components that may execute in parallel after prior waves complete.</summary>
public sealed class BatchExecutionWave
{
    public BatchExecutionWave(int index, IReadOnlyList<NamedSyncManifest> plans)
    {
        ArgumentNullException.ThrowIfNull(plans);
        Index = index;
        Plans = Array.AsReadOnly(plans.ToArray());
    }

    public int Index { get; }
    public IReadOnlyList<NamedSyncManifest> Plans { get; }
}
