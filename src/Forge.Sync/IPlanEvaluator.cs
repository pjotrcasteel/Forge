namespace Forge.Sync;

/// <summary>Evaluates an application plan into strongly typed comparison metrics.</summary>
public interface IPlanEvaluator<in TPlan, out TMetrics>
{
    /// <summary>Calculates metrics without mutating or executing the plan.</summary>
    TMetrics Evaluate(TPlan plan);
}
