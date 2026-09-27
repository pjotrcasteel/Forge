namespace Forge.Sync;

/// <summary>Minimal extensibility point for monotonic fact propagation.</summary>
public interface IFactRule<in TContext>
{
    /// <summary>Reads typed facts and may add new typed facts to the monotonic builder.</summary>
    FactRuleApplication Apply(TContext context, FactSetBuilder facts);
}
