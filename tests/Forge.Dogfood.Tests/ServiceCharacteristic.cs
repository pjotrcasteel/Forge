using Forge.Sync;

namespace Forge.Dogfood.Tests;

[GenerateSync(nameof(ServiceCharacteristic.Name))]
internal sealed record ServiceCharacteristic(
    string Name,
    string? Value);
