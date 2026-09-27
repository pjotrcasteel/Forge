using System.Runtime.CompilerServices;

namespace Forge.Sync;

/// <summary>Runs typed scenarios without owning any external side effects or execution infrastructure.</summary>
public static class ScenarioSimulator
{
    public static IEnumerable<ScenarioResult<TScenario, TResult>> Simulate<TScenario, TResult>(
        IEnumerable<TScenario> scenarios,
        IScenarioEvaluator<TScenario, TResult> evaluator)
    {
        ArgumentNullException.ThrowIfNull(scenarios);
        ArgumentNullException.ThrowIfNull(evaluator);
        foreach (var scenario in scenarios)
        {
            yield return new ScenarioResult<TScenario, TResult>(scenario, evaluator.Evaluate(scenario));
        }
    }

    public static async IAsyncEnumerable<ScenarioResult<TScenario, TResult>> SimulateAsync<TScenario, TResult>(
        IEnumerable<TScenario> scenarios,
        IAsyncScenarioEvaluator<TScenario, TResult> evaluator,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scenarios);
        ArgumentNullException.ThrowIfNull(evaluator);
        foreach (var scenario in scenarios)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await evaluator.EvaluateAsync(scenario, cancellationToken).ConfigureAwait(false);
            yield return new ScenarioResult<TScenario, TResult>(scenario, result);
        }
    }
}
