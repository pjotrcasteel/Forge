namespace Forge.ServiceProvisioning.Sample;

public sealed record ProvisioningOperation(
    OperationKey Key,
    ProvisioningAction Action,
    string TargetId,
    string StateFingerprint,
    IReadOnlyList<OperationKey> DependsOn)
{
    public bool IsEquivalentTo(ProvisioningOperation other)
    {
        ArgumentNullException.ThrowIfNull(other);

        return Action == other.Action
            && string.Equals(TargetId, other.TargetId, StringComparison.Ordinal)
            && string.Equals(StateFingerprint, other.StateFingerprint, StringComparison.Ordinal)
            && DependsOn.SequenceEqual(other.DependsOn);
    }

    public override string ToString() => Key.ToString();
}
