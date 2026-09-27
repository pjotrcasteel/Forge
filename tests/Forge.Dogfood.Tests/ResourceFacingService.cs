using Forge.Sync;

namespace Forge.Dogfood.Tests;

[GenerateSync(nameof(ResourceFacingService.LogicalId))]
internal sealed record ResourceFacingService(
    string LogicalId,
    string ServiceType,
    [property: SyncNested]
    IReadOnlyList<ServiceCharacteristic> Characteristics);
