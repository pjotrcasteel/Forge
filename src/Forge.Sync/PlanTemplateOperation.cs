namespace Forge.Sync;

/// <summary>One strongly typed operation emitted by a reusable plan template.</summary>
public sealed record PlanTemplateOperation<TOperation, TKey>(TKey Key, TOperation Operation)
    where TKey : notnull;
