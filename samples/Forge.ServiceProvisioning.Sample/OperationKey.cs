namespace Forge.ServiceProvisioning.Sample;

public readonly record struct OperationKey(
    ProvisioningAction Action,
    string TargetId)
{
    public override string ToString() => $"{Action}:{TargetId}";
}
