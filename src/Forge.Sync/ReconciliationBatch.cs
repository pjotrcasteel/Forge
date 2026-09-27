namespace Forge.Sync;

/// <summary>
/// Composes heterogeneous portable reconciliation plans into a dependency-ordered, pre-validatable unit.
/// </summary>
public sealed class ReconciliationBatch
{
    private ReconciliationBatch(
        IReadOnlyList<NamedSyncManifest> plans,
        IReadOnlyList<PlanDependency> dependencies,
        IReadOnlyList<BatchExecutionWave> executionWaves,
        IReadOnlyList<string> cyclePlans)
    {
        Plans = plans;
        Dependencies = dependencies;
        ExecutionWaves = executionWaves;
        CyclePlans = cyclePlans;
    }

    public IReadOnlyList<NamedSyncManifest> Plans { get; }
    public IReadOnlyList<PlanDependency> Dependencies { get; }
    public IReadOnlyList<BatchExecutionWave> ExecutionWaves { get; }
    public IReadOnlyList<string> CyclePlans { get; }
    public bool HasDependencyCycles => CyclePlans.Count != 0;
    public bool HasChanges => Plans.Any(static plan => plan.Manifest.HasChanges);
    public int TotalOperationCount => Plans.Sum(static plan => plan.Manifest.OperationCount);

    public static ReconciliationBatch Create(
        IReadOnlyList<NamedSyncManifest> plans,
        IReadOnlyList<PlanDependency> dependencies)
    {
        ArgumentNullException.ThrowIfNull(plans);
        ArgumentNullException.ThrowIfNull(dependencies);
        var byName = new Dictionary<string, NamedSyncManifest>(StringComparer.Ordinal);
        foreach (var plan in plans)
        {
            ArgumentNullException.ThrowIfNull(plan);
            ArgumentException.ThrowIfNullOrWhiteSpace(plan.Name);
            if (!byName.TryAdd(plan.Name, plan))
            {
                throw new ArgumentException($"Duplicate plan name '{plan.Name}'.", nameof(plans));
            }
        }

        var dependencyMap = plans.ToDictionary(
            static plan => plan.Name,
            static _ => new List<string>(),
            StringComparer.Ordinal);
        foreach (var dependency in dependencies)
        {
            ArgumentNullException.ThrowIfNull(dependency);
            if (!byName.ContainsKey(dependency.Plan))
            {
                throw new ArgumentException($"Unknown dependent plan '{dependency.Plan}'.", nameof(dependencies));
            }

            if (!byName.ContainsKey(dependency.DependsOn))
            {
                throw new ArgumentException($"Unknown prerequisite plan '{dependency.DependsOn}'.", nameof(dependencies));
            }

            dependencyMap[dependency.Plan].Add(dependency.DependsOn);
        }

        var dependencyPlan = DependencyPlanner.Plan(
            plans,
            static plan => plan.Name,
            plan => dependencyMap[plan.Name],
            StringComparer.Ordinal);
        var waves = dependencyPlan.CreateWaves
            .Select(wave => new BatchExecutionWave(wave.Index, wave.Items))
            .ToArray();

        return new ReconciliationBatch(
            Array.AsReadOnly(plans.ToArray()),
            Array.AsReadOnly(dependencies.ToArray()),
            Array.AsReadOnly(waves),
            dependencyPlan.CycleKeys);
    }

    public BatchValidationResult Validate(params Func<ReconciliationBatch, BatchValidationIssue?>[] rules)
    {
        ArgumentNullException.ThrowIfNull(rules);
        var issues = new List<BatchValidationIssue>();
        foreach (var rule in rules)
        {
            ArgumentNullException.ThrowIfNull(rule);
            var issue = rule(this);
            if (issue is not null)
            {
                issues.Add(issue);
            }
        }

        return new BatchValidationResult(HasDependencyCycles, issues);
    }
}
