namespace Forge.Sync;

/// <summary>
/// Preserves application-owned operation identities across semantic replanning when an operation is genuinely unchanged.
/// </summary>
public static class IncrementalReplanner
{
    /// <summary>
    /// Creates the first tracked plan from a portable structural manifest.
    /// </summary>
    public static IncrementalPlan<TOperationId> Create<TOperationId>(
        SyncManifestDocument manifest,
        Func<SyncManifestEntry, TOperationId> operationIdFactory)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentNullException.ThrowIfNull(operationIdFactory);
        var operations = manifest.Operations
            .Select(entry => CreateOperation(entry, operationIdFactory(entry), manifest.ItemType))
            .ToArray();
        return new IncrementalPlan<TOperationId>(operations);
    }

    /// <summary>
    /// Replans against a new structural manifest, retaining identities only when operation semantics are unchanged.
    /// </summary>
    public static IncrementalReplanResult<TOperationId> Replan<TOperationId>(
        IncrementalPlan<TOperationId> previous,
        SyncManifestDocument nextManifest,
        Func<SyncManifestEntry, TOperationId> operationIdFactory)
    {
        ArgumentNullException.ThrowIfNull(previous);
        ArgumentNullException.ThrowIfNull(nextManifest);
        ArgumentNullException.ThrowIfNull(operationIdFactory);

        var previousBySlot = previous.Operations.ToDictionary(
            static operation => Slot(operation.Entry),
            StringComparer.Ordinal);
        var nextOperations = new List<IncrementalPlanOperation<TOperationId>>(nextManifest.OperationCount);
        var retained = new List<IncrementalPlanOperation<TOperationId>>();
        var newlyRequired = new List<IncrementalPlanOperation<TOperationId>>();
        var replaced = new List<IncrementalPlanReplacement<TOperationId>>();
        var seenSlots = new HashSet<string>(StringComparer.Ordinal);

        foreach (var entry in nextManifest.Operations)
        {
            var slot = Slot(entry);
            if (!seenSlots.Add(slot))
            {
                throw new ArgumentException("A manifest cannot contain the same key and operation more than once.", nameof(nextManifest));
            }

            var digest = DigestEntry(entry, nextManifest.ItemType);
            if (previousBySlot.TryGetValue(slot, out var old) && string.Equals(old.SemanticDigest, digest, StringComparison.Ordinal))
            {
                nextOperations.Add(old);
                retained.Add(old);
                continue;
            }

            var created = new IncrementalPlanOperation<TOperationId>(operationIdFactory(entry), entry, digest);
            nextOperations.Add(created);
            if (old is null)
            {
                newlyRequired.Add(created);
            }
            else
            {
                replaced.Add(new IncrementalPlanReplacement<TOperationId>(old, created));
            }
        }

        var noLongerRequired = previous.Operations
            .Where(operation => !seenSlots.Contains(Slot(operation.Entry)))
            .ToArray();

        return new IncrementalReplanResult<TOperationId>(
            new IncrementalPlan<TOperationId>(nextOperations),
            retained,
            newlyRequired,
            noLongerRequired,
            replaced);
    }

    private static IncrementalPlanOperation<TOperationId> CreateOperation<TOperationId>(
        SyncManifestEntry entry,
        TOperationId operationId,
        string itemType)
        => new(operationId, entry, DigestEntry(entry, itemType));

    private static string DigestEntry(SyncManifestEntry entry, string itemType)
        => ManifestDigest.ComputeSha256Hex(
            new SyncManifestDocument(
                SyncManifestDocument.CurrentSchemaVersion,
                itemType,
                [entry]));

    private static string Slot(SyncManifestEntry entry)
        => ((int)entry.Operation).ToString(System.Globalization.CultureInfo.InvariantCulture) + "\u001f" + entry.Key;
}
