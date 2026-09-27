namespace Forge.Sync;

/// <summary>Describes why a typed conditional dependency is not currently satisfied.</summary>
public enum ConditionalDependencyBlockReason
{
    MissingPredecessorState,
    StateConditionNotSatisfied
}
