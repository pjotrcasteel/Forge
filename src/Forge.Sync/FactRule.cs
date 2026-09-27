namespace Forge.Sync;

/// <summary>Typed base class for deriving one target fact from context and already-known facts.</summary>
public abstract class FactRule<TContext, TFact> : IFactRule<TContext>
{
    protected FactRule(FactKey<TFact> target)
    {
        ArgumentNullException.ThrowIfNull(target);
        Target = target;
    }

    public FactKey<TFact> Target { get; }

    public FactRuleApplication Apply(TContext context, FactSetBuilder facts)
    {
        ArgumentNullException.ThrowIfNull(facts);
        if (!TryDerive(context, facts, out var value))
        {
            return FactRuleApplication.NoChange;
        }

        return facts.TryAdd(Target, value) ? FactRuleApplication.Added : FactRuleApplication.NoChange;
    }

    protected abstract bool TryDerive(TContext context, FactSetBuilder facts, out TFact value);
}
