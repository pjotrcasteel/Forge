namespace Forge.Sync;

/// <summary>One strongly typed value on a scenario axis.</summary>
public sealed record ScenarioCase<TCaseId, TValue>(TCaseId Id, TValue Value)
    where TCaseId : notnull;
