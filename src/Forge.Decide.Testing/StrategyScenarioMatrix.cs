using Forge.Decide;

namespace Forge.Decide.Testing;

/// <summary>
/// Evaluates deterministic context matrices against a configured Forge.Decide strategy space.
/// </summary>
public static class StrategyScenarioMatrix
{
    /// <summary>
    /// Evaluates scenarios sequentially in input order and reports selected, uncovered and ambiguous contexts.
    /// </summary>
    /// <typeparam name="TSpace">Consumer-defined strategy-space marker.</typeparam>
    /// <typeparam name="TContext">Decision context type.</typeparam>
    /// <typeparam name="TPlan">Application-owned plan type.</typeparam>
    /// <param name="space">Configured strategy space.</param>
    /// <param name="scenarios">Named contexts to evaluate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Scenario results in deterministic input order.</returns>
    public static async ValueTask<StrategyScenarioMatrixResult<TSpace, TPlan>> EvaluateAsync<TSpace, TContext, TPlan>(
        StrategySpace<TSpace, TContext, TPlan> space,
        IEnumerable<StrategyScenario<TContext>> scenarios,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(space);
        ArgumentNullException.ThrowIfNull(scenarios);

        var results = new List<StrategyScenarioResult<TSpace, TPlan>>();
        var names = new HashSet<string>(StringComparer.Ordinal);

        foreach (var scenario in scenarios)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ArgumentNullException.ThrowIfNull(scenario);

            if (!names.Add(scenario.Name))
            {
                throw new ArgumentException($"Scenario name '{scenario.Name}' is duplicated.", nameof(scenarios));
            }

            var comparison = await space.CompareAsync(scenario.Context, cancellationToken).ConfigureAwait(false);

            try
            {
                var decision = await space.DecideAsync(comparison, scenario.Context, cancellationToken).ConfigureAwait(false);
                results.Add(new StrategyScenarioResult<TSpace, TPlan>(
                    scenario.Name,
                    StrategyScenarioStatus.Selected,
                    comparison,
                    decision,
                    Array.Empty<StrategyId>()));
            }
            catch (NoApplicableStrategyException)
            {
                results.Add(new StrategyScenarioResult<TSpace, TPlan>(
                    scenario.Name,
                    StrategyScenarioStatus.NoApplicableStrategy,
                    comparison,
                    null,
                    Array.Empty<StrategyId>()));
            }
            catch (AmbiguousStrategyDecisionException exception)
            {
                results.Add(new StrategyScenarioResult<TSpace, TPlan>(
                    scenario.Name,
                    StrategyScenarioStatus.Ambiguous,
                    comparison,
                    null,
                    Array.AsReadOnly(exception.StrategyIds.ToArray())));
            }
        }

        return new StrategyScenarioMatrixResult<TSpace, TPlan>(Array.AsReadOnly(results.ToArray()));
    }
}