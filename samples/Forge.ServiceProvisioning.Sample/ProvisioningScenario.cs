using Forge.Sync;

namespace Forge.ServiceProvisioning.Sample;

public static class ProvisioningScenario
{
    public static ProvisioningScenarioResult Run()
    {
        var current = CreateCurrentState();
        var desired = CreateDesiredState();

        var plan = ServiceComponentSync.Plan(current, desired);
        var apiUpdate = plan.Updated.Single(update =>
            string.Equals(update.Desired.Id, "api", StringComparison.Ordinal));
        var apiChangePaths = apiUpdate.Delta.Changes
            .Select(static change => change.Path)
            .ToArray();

        var operations = ProvisioningPlanBuilder.Build(current, desired);
        var dependencyPlan = DependencyPlanner.Plan(
            operations,
            static operation => operation.Key,
            static operation => operation.DependsOn);

        if (dependencyPlan.HasCycles)
        {
            throw new InvalidOperationException(
                $"The sample provisioning plan contains a dependency cycle: {string.Join(", ", dependencyPlan.CycleKeys)}");
        }

        var executionWaves = dependencyPlan.CreateWaves
            .Select(wave => new ExecutionWaveSummary(
                wave.Index + 1,
                wave.Items.Select(static operation => operation.ToString()).ToArray()))
            .ToArray();

        var manifest = SyncManifest.Create(
            plan,
            static key => key.Id);
        var manifestJson = manifest.ToJson();
        var manifestDigest = ManifestDigest.ComputeSha256Hex(manifest);

        var originalSnapshot = ManifestStateSnapshot.Create(
            current,
            ServiceComponentSync.GetKey,
            static key => key.Id);
        var originalPreconditions = ManifestPreconditions.Validate(
            manifest,
            originalSnapshot);

        var changedMeanwhile = CreateChangedCurrentState();
        var changedSnapshot = ManifestStateSnapshot.Create(
            changedMeanwhile,
            ServiceComponentSync.GetKey,
            static key => key.Id);
        var changedPreconditions = ManifestPreconditions.Validate(
            manifest,
            changedSnapshot);

        var replan = ReplanAroundExecution(
            operations,
            current,
            CreateRevisedDesiredState());

        return new ProvisioningScenarioResult(
            plan.Added.Count,
            plan.Updated.Count,
            plan.Removed.Count,
            plan.Unchanged.Count,
            apiChangePaths,
            executionWaves,
            manifest.OperationCount,
            manifestJson,
            manifestDigest,
            originalPreconditions.IsSatisfied,
            changedPreconditions.IsSatisfied,
            changedPreconditions.Failures
                .Select(static failure => failure.Key)
                .ToArray(),
            replan);
    }

    public static IReadOnlyList<ServiceComponent> CreateCurrentState()
    {
        return
        [
            new ServiceComponent("database", "Database", 1, "schema-v3", null),
            new ServiceComponent("api", "ApiService", 1, "stable", "database"),
            new ServiceComponent("legacy-worker", "Worker", 1, "legacy-v1", "api")
        ];
    }

    public static IReadOnlyList<ServiceComponent> CreateDesiredState()
    {
        return
        [
            new ServiceComponent("database", "Database", 1, "schema-v3", null),
            new ServiceComponent("api", "ApiService", 2, "optimized", "database"),
            new ServiceComponent("policy-engine", "PolicyEngine", 1, "standard", "api"),
            new ServiceComponent("telemetry", "Telemetry", 1, "enhanced", "policy-engine")
        ];
    }

    public static IReadOnlyList<ServiceComponent> CreateChangedCurrentState()
    {
        return
        [
            new ServiceComponent("database", "Database", 1, "schema-v3", null),
            new ServiceComponent("api", "ApiService", 7, "emergency-hotfix", "database"),
            new ServiceComponent("legacy-worker", "Worker", 1, "legacy-v1", "api")
        ];
    }

    public static IReadOnlyList<ServiceComponent> CreateRevisedDesiredState()
    {
        return
        [
            new ServiceComponent("database", "Database", 1, "schema-v3", null),
            new ServiceComponent("api", "ApiService", 2, "optimized", "database"),
            new ServiceComponent("policy-engine", "PolicyEngine", 2, "strict", "api"),
            new ServiceComponent("queue", "MessageQueue", 1, "durable-v1", "policy-engine"),
            new ServiceComponent("legacy-worker", "Worker", 1, "legacy-v1", "api")
        ];
    }

    private static ReplanSummary ReplanAroundExecution(
        IReadOnlyList<ProvisioningOperation> previousOperations,
        IReadOnlyList<ServiceComponent> current,
        IReadOnlyList<ServiceComponent> revisedDesired)
    {
        var definition = new ExecutedReplanDefinition<ProvisioningOperation, OperationKey>(
            static operation => operation.Key,
            static (left, right) => left.IsEquivalentTo(right));

        var nextOperationId = 1;
        var tracked = ExecutedReplanner.Create(
            previousOperations,
            definition,
            _ => new OperationId(nextOperationId++));

        var executionStates = tracked.Operations.ToDictionary(
            static operation => operation.OperationId,
            static operation => operation.Operation.TargetId switch
            {
                "api" => ExecutionState.Completed,
                "policy-engine" => ExecutionState.Running,
                "telemetry" => ExecutionState.Waiting,
                "legacy-worker" => ExecutionState.Waiting,
                _ => throw new InvalidOperationException(
                    $"No execution state was defined for '{operation.Operation.TargetId}'.")
            });

        var revisedOperations = ProvisioningPlanBuilder.Build(
            current,
            revisedDesired);

        var result = ExecutedReplanner.Replan(
            tracked,
            revisedOperations,
            new ExecutionSnapshot<OperationId, ExecutionState>(executionStates),
            new ExecutionStateClassifier(),
            definition,
            _ => new OperationId(nextOperationId++));

        return new ReplanSummary(
            result.LockedWork.Completed.Count,
            result.Interventions.RunningReplacements.Count,
            result.SafeChanges.Cancelled.Count,
            result.SafeChanges.NewlyPlanned.Count,
            result.CanProceedWithoutIntervention);
    }
}
