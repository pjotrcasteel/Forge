namespace Forge.Sync;

/// <summary>Evaluates one typed scenario synchronously.</summary>
public interface IScenarioEvaluator<in TScenario, out TResult>
{
    TResult Evaluate(TScenario scenario);
}
