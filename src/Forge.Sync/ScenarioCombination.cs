namespace Forge.Sync;

/// <summary>One lazily produced pair from two strongly typed scenario axes.</summary>
public sealed record ScenarioCombination<TLeftId, TRightId, TLeft, TRight>(
    ScenarioCase<TLeftId, TLeft> Left,
    ScenarioCase<TRightId, TRight> Right)
    where TLeftId : notnull
    where TRightId : notnull;
