namespace Forge.Sync;

/// <summary>Typed simulation result retaining the exact scenario that produced it.</summary>
public sealed record ScenarioResult<TScenario, TResult>(TScenario Scenario, TResult Result);
