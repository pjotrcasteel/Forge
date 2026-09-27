namespace Forge.Sync;

/// <summary>Represents an item whose complete readiness requirement set is currently satisfied.</summary>
public sealed record ReadyPlanItem<TItem, TKey>(TKey Key, TItem Item)
    where TKey : notnull;
