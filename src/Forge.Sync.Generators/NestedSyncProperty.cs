using Microsoft.CodeAnalysis;

namespace Forge.Sync.Generators;

internal sealed class NestedSyncProperty
{
    public NestedSyncProperty(
        IPropertySymbol property,
        INamedTypeSymbol itemType,
        int mode)
    {
        Property = property;
        ItemType = itemType;
        Mode = mode;
    }

    public IPropertySymbol Property { get; }

    public INamedTypeSymbol ItemType { get; }

    public int Mode { get; }
}
