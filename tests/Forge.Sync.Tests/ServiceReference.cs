using Forge.Sync;

namespace Forge.Sync.Tests;

[GenerateSync(nameof(ServiceReference.ExternalId))]
internal sealed record ServiceReference(
    [property: SyncKeyComparer(typeof(CaseInsensitiveStringComparer))]
    string ExternalId,
    string State);
