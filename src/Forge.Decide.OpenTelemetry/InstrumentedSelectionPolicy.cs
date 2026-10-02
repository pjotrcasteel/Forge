using System.Diagnostics;

namespace Forge.Decide.OpenTelemetry;

internal sealed class InstrumentedSelectionPolicy<TContext, TPlan> : IStrategySelectionPolicy<TContext, TPlan>
{
    private readonly IStrategySelectionPolicy<TContext, TPlan> _inner;

    public InstrumentedSelectionPolicy(IStrategySelectionPolicy<TContext, TPlan> inner)
    {
        _inner = inner;
    }

    public async ValueTask<ApplicableStrategyCandidate<TPlan>> SelectAsync(
        TContext context,
        IReadOnlyList<ApplicableStrategyCandidate<TPlan>> candidates,
        CancellationToken cancellationToken)
    {
        using var activity = ForgeDecideTelemetry.Source.StartActivity("forge.decide.selection", ActivityKind.Internal);
        activity?.SetTag("forge.decide.candidates.applicable", candidates.Count);

        try
        {
            var selected = await _inner.SelectAsync(context, candidates, cancellationToken).ConfigureAwait(false);
            activity?.SetTag("forge.decide.selected_strategy.id", selected.StrategyId.Value);
            return selected;
        }
        catch (Exception exception)
        {
            activity?.SetStatus(ActivityStatusCode.Error, exception.GetType().Name);
            activity?.SetTag("error.type", exception.GetType().FullName);
            throw;
        }
    }
}