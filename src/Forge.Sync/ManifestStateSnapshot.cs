using System.Text.Json;
using Forge.Delta;

namespace Forge.Sync;

/// <summary>Creates portable key-to-state snapshots suitable for stale-plan precondition validation.</summary>
public static class ManifestStateSnapshot
{
    public static IReadOnlyDictionary<string, JsonElement> Create<T, TKey>(
        IReadOnlyList<T> items,
        Func<T, TKey> keySelector,
        Func<TKey, string> keyFormatter,
        JsonSerializerOptions? options = null)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(keySelector);
        ArgumentNullException.ThrowIfNull(keyFormatter);
        var result = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        foreach (var item in items)
        {
            ArgumentNullException.ThrowIfNull(item);
            var key = keyFormatter(keySelector(item));
            if (!result.TryAdd(key, DeltaManifest.SerializeValue(item, options)))
            {
                throw new ArgumentException($"Duplicate portable state key '{key}'.", nameof(items));
            }
        }

        return result;
    }
}
