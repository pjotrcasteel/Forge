namespace Forge.Sync;

/// <summary>Maps an application execution state to the generic disposition Forge needs for replanning.</summary>
public interface IExecutionStateClassifier<in TState>
{
    /// <summary>Classifies the supplied application state.</summary>
    ExecutionDisposition Classify(TState state);
}
