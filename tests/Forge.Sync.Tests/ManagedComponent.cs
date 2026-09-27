using Forge.Sync;

namespace Forge.Sync.Tests;

[GenerateSync(nameof(ManagedComponent.LogicalId))]
internal sealed record ManagedComponent(
    string LogicalId,
    string Kind,
    ManagedComponentConfiguration Configuration);
