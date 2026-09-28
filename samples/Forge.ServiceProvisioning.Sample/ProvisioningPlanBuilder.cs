using Forge.Sync;

namespace Forge.ServiceProvisioning.Sample;

public static class ProvisioningPlanBuilder
{
    public static IReadOnlyList<ProvisioningOperation> Build(
        IReadOnlyList<ServiceComponent> current,
        IReadOnlyList<ServiceComponent> desired)
    {
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(desired);

        var plan = ServiceComponentSync.Plan(current, desired);
        var operations = new List<ProvisioningOperation>(plan.ChangeCount);

        foreach (var addition in plan.Added)
        {
            operations.Add(CreateOperation(
                ProvisioningAction.Add,
                addition.Desired,
                addition.Desired));
        }

        foreach (var update in plan.Updated)
        {
            operations.Add(CreateOperation(
                ProvisioningAction.Update,
                update.Desired,
                update.Desired));
        }

        foreach (var removal in plan.Removed)
        {
            operations.Add(CreateOperation(
                ProvisioningAction.Remove,
                removal.Current,
                removal.Current));
        }

        var operationByTarget = operations.ToDictionary(
            static operation => operation.TargetId,
            StringComparer.Ordinal);

        return operations
            .Select(operation => operation.Action == ProvisioningAction.Remove
                ? operation
                : operation with
                {
                    DependsOn = ResolveOperationDependencies(
                        operation.TargetId,
                        desired,
                        operationByTarget)
                })
            .ToArray();
    }

    private static ProvisioningOperation CreateOperation(
        ProvisioningAction action,
        ServiceComponent component,
        ServiceComponent state)
    {
        return new ProvisioningOperation(
            new OperationKey(action, component.Id),
            action,
            component.Id,
            Fingerprint(state),
            Array.Empty<OperationKey>());
    }

    private static IReadOnlyList<OperationKey> ResolveOperationDependencies(
        string targetId,
        IReadOnlyList<ServiceComponent> desired,
        IReadOnlyDictionary<string, ProvisioningOperation> operationByTarget)
    {
        var component = desired.Single(item =>
            string.Equals(item.Id, targetId, StringComparison.Ordinal));

        if (component.DependsOn is null
            || !operationByTarget.TryGetValue(component.DependsOn, out var dependency))
        {
            return Array.Empty<OperationKey>();
        }

        return [dependency.Key];
    }

    private static string Fingerprint(ServiceComponent component)
    {
        return string.Join(
            '|',
            component.Kind,
            component.Revision,
            component.Configuration,
            component.DependsOn ?? "<none>");
    }
}
