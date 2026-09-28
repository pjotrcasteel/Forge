using Forge.Sync;

namespace Forge.ServiceProvisioning.Sample;

[GenerateSync(nameof(ServiceComponent.Id))]
public sealed record ServiceComponent(
    string Id,
    string Kind,
    int Revision,
    string Configuration,
    string? DependsOn);
