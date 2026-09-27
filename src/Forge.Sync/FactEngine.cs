namespace Forge.Sync;

/// <summary>Runs bounded monotonic strongly typed fact derivation.</summary>
public static class FactEngine
{
    public static FactDerivationResult Derive<TContext>(
        TContext context,
        FactSet initial,
        IReadOnlyList<IFactRule<TContext>> rules,
        int maxPasses = 64)
    {
        ArgumentNullException.ThrowIfNull(initial);
        ArgumentNullException.ThrowIfNull(rules);
        if (maxPasses <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxPasses));
        }

        var builder = new FactSetBuilder(initial);
        for (var pass = 1; pass <= maxPasses; pass++)
        {
            var changed = false;
            foreach (var rule in rules)
            {
                ArgumentNullException.ThrowIfNull(rule);
                changed |= rule.Apply(context, builder) == FactRuleApplication.Added;
            }

            if (!changed)
            {
                return new FactDerivationResult(builder.Build(), pass);
            }
        }

        throw new InvalidOperationException("Fact derivation exceeded the configured pass limit.");
    }
}
