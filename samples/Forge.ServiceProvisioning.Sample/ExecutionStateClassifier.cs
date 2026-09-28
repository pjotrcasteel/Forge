using Forge.Sync;

namespace Forge.ServiceProvisioning.Sample;

public sealed class ExecutionStateClassifier : IExecutionStateClassifier<ExecutionState>
{
    public ExecutionDisposition Classify(ExecutionState state)
    {
        return state switch
        {
            ExecutionState.Waiting => ExecutionDisposition.NotStarted,
            ExecutionState.Running => ExecutionDisposition.Running,
            ExecutionState.Completed => ExecutionDisposition.Completed,
            _ => throw new ArgumentOutOfRangeException(nameof(state))
        };
    }
}
