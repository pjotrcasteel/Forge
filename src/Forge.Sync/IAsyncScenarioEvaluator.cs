namespace Forge.Sync;

/// <summary>Evaluates one typed scenario asynchronously with explicit cancellation.</summary>
public interface IAsyncScenarioEvaluator<TScenario, TResult>
{
    ValueTask<TResult> EvaluateAsync(TScenario scenario, CancellationToken cancellationToken);
}
