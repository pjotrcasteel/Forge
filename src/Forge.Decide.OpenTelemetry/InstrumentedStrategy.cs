using System.Diagnostics;

namespace Forge.Decide.OpenTelemetry;

internal sealed class InstrumentedStrategy<TContext, TPlan> : IStrategy<TContext, TPlan>
{
    private readonly IStrategy<TContext, TPlan> _inner;

    public InstrumentedStrategy(IStrategy<TContext, TPlan> inner)
    {
        _inner = inner;
    }

    public StrategyId Id => _inner.Id;

    public async ValueTask<StrategyProposal<TPlan>> ProposeAsync(TContext context, CancellationToken cancellationToken)
    {
        using var activity = ForgeDecideTelemetry.Source.StartActivity("forge.decide.strategy.propose", ActivityKind.Internal);
        activity?.SetTag("forge.decide.strategy.id", Id.Value);

        try
        {
            var proposal = await _inner.ProposeAsync(context, cancellationToken).ConfigureAwait(false);
            activity?.SetTag("forge.decide.strategy.applicable", proposal is ApplicableStrategyProposal<TPlan>);
            return proposal;
        }
        catch (Exception exception)
        {
            SetError(activity, exception);
            throw;
        }
    }

    private static void SetError(Activity? activity, Exception exception)
    {
        activity?.SetStatus(ActivityStatusCode.Error, exception.GetType().Name);
        activity?.SetTag("error.type", exception.GetType().FullName);
    }
}