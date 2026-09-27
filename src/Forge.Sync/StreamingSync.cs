using System.Runtime.CompilerServices;
using Forge.Delta;

namespace Forge.Sync;

/// <summary>
/// Performs ordered streaming reconciliation without materializing either complete collection.
/// </summary>
public static class StreamingSync
{
    public static async IAsyncEnumerable<IStreamingSyncStep<TKey>> PlanOrderedAsync<TCurrent, TDesired, TKey, TDelta>(
        IAsyncEnumerable<TCurrent> current,
        IAsyncEnumerable<TDesired> desired,
        CrossSyncDefinition<TCurrent, TDesired, TKey, TDelta> definition,
        IComparer<TKey> keyComparer,
        [EnumeratorCancellation] CancellationToken cancellationToken)
        where TKey : notnull
        where TDelta : IDelta
    {
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(desired);
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(keyComparer);

        await using var currentEnumerator = current.GetAsyncEnumerator(cancellationToken);
        await using var desiredEnumerator = desired.GetAsyncEnumerator(cancellationToken);
        var hasCurrent = await currentEnumerator.MoveNextAsync();
        var hasDesired = await desiredEnumerator.MoveNextAsync();
        var hasPreviousCurrentKey = false;
        var hasPreviousDesiredKey = false;
        TKey? previousCurrentKey = default;
        TKey? previousDesiredKey = default;

        while (hasCurrent || hasDesired)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (hasCurrent)
            {
                ArgumentNullException.ThrowIfNull(currentEnumerator.Current);
                var currentKeyForValidation = definition.CurrentKeySelector(currentEnumerator.Current);
                ValidateStrictOrder(
                    previousCurrentKey,
                    hasPreviousCurrentKey,
                    currentKeyForValidation,
                    keyComparer,
                    nameof(current));
            }

            if (hasDesired)
            {
                ArgumentNullException.ThrowIfNull(desiredEnumerator.Current);
                var desiredKeyForValidation = definition.DesiredKeySelector(desiredEnumerator.Current);
                ValidateStrictOrder(
                    previousDesiredKey,
                    hasPreviousDesiredKey,
                    desiredKeyForValidation,
                    keyComparer,
                    nameof(desired));
            }

            if (!hasCurrent)
            {
                var desiredItem = desiredEnumerator.Current;
                var key = definition.DesiredKeySelector(desiredItem);
                yield return new StreamingSyncAddition<TDesired, TKey>(key, desiredItem);
                previousDesiredKey = key;
                hasPreviousDesiredKey = true;
                hasDesired = await desiredEnumerator.MoveNextAsync();
                continue;
            }

            if (!hasDesired)
            {
                var currentItem = currentEnumerator.Current;
                var key = definition.CurrentKeySelector(currentItem);
                yield return definition.Mode == SyncMode.Upsert
                    ? new StreamingSyncPreserved<TCurrent, TKey>(key, currentItem)
                    : new StreamingSyncRemoval<TCurrent, TKey>(key, currentItem);
                previousCurrentKey = key;
                hasPreviousCurrentKey = true;
                hasCurrent = await currentEnumerator.MoveNextAsync();
                continue;
            }

            var currentItemValue = currentEnumerator.Current;
            var desiredItemValue = desiredEnumerator.Current;
            var currentKey = definition.CurrentKeySelector(currentItemValue);
            var desiredKey = definition.DesiredKeySelector(desiredItemValue);
            var comparison = keyComparer.Compare(currentKey, desiredKey);
            if (comparison < 0)
            {
                yield return definition.Mode == SyncMode.Upsert
                    ? new StreamingSyncPreserved<TCurrent, TKey>(currentKey, currentItemValue)
                    : new StreamingSyncRemoval<TCurrent, TKey>(currentKey, currentItemValue);
                previousCurrentKey = currentKey;
                hasPreviousCurrentKey = true;
                hasCurrent = await currentEnumerator.MoveNextAsync();
                continue;
            }

            if (comparison > 0)
            {
                yield return new StreamingSyncAddition<TDesired, TKey>(desiredKey, desiredItemValue);
                previousDesiredKey = desiredKey;
                hasPreviousDesiredKey = true;
                hasDesired = await desiredEnumerator.MoveNextAsync();
                continue;
            }

            if (definition.AreEquivalent(currentItemValue, desiredItemValue))
            {
                yield return new StreamingSyncUnchanged<TCurrent, TDesired, TKey>(
                    currentKey,
                    currentItemValue,
                    desiredItemValue);
            }
            else
            {
                yield return new StreamingSyncUpdate<TCurrent, TDesired, TKey, TDelta>(
                    currentKey,
                    currentItemValue,
                    desiredItemValue,
                    definition.DeltaFactory(currentItemValue, desiredItemValue));
            }

            previousCurrentKey = currentKey;
            previousDesiredKey = desiredKey;
            hasPreviousCurrentKey = true;
            hasPreviousDesiredKey = true;
            hasCurrent = await currentEnumerator.MoveNextAsync();
            hasDesired = await desiredEnumerator.MoveNextAsync();
        }
    }

    private static void ValidateStrictOrder<TKey>(
        TKey? previous,
        bool hasPrevious,
        TKey current,
        IComparer<TKey> comparer,
        string collectionName)
    {
        if (hasPrevious && comparer.Compare(previous!, current) >= 0)
        {
            throw new InvalidOperationException(
                $"{collectionName} must be strictly ordered by logical key and cannot contain duplicate keys.");
        }
    }
}
