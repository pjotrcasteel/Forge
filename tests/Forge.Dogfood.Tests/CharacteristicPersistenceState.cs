using Forge.Delta;

namespace Forge.Dogfood.Tests;

[GenerateDelta]
internal readonly record struct CharacteristicPersistenceState(
    string Name,
    string Value,
    string? Source,
    string? Description,
    bool IncludeInInventory);
