using Forge.Sync;

namespace Forge.Dogfood.Tests;

[GenerateSync(nameof(CharacteristicSyncState.CharacteristicId))]
internal sealed record CharacteristicSyncState(
    [property: SyncKeyComparer(typeof(IgnoreCaseStringComparer))] string CharacteristicId,
    string Name,
    string Value,
    string? Source,
    string? Description,
    bool IncludeInInventory);

internal sealed class IgnoreCaseStringComparer : IEqualityComparer<string>
{
    public bool Equals(string? x, string? y)
    {
        return StringComparer.OrdinalIgnoreCase.Equals(x, y);
    }

    public int GetHashCode(string obj)
    {
        return StringComparer.OrdinalIgnoreCase.GetHashCode(obj);
    }
}
