namespace Forge.Sync;

/// <summary>Declares that one named plan must execute after another named plan.</summary>
public sealed record PlanDependency(string Plan, string DependsOn);
