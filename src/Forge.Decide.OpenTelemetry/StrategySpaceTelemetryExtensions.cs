using System.Diagnostics;

namespace Forge.Decide.OpenTelemetry;

/// <summary>
/// Adds ActivitySource spans around strategy-space operations without changing core decision semantics.
/// </summary>
public static class StrategySpaceTelemetryExtensions
{
    /// <summary>
    /// Evaluates a strategy space inside a comparison activity.
    /// </summary>
    public static async ValueTask<StrategyComparison<TSpace, TPlan>> CompareWithOpenTelemetryAsync<TSpace, TContext, TPlan>(
        this StrategySpace<TSpace, TContext, TPlan> space,
        TContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(space);

        using var activity = Start("forge.decide.compare", space.Id);

        try
        {
            var comparison = await space.CompareAsync(context, cancellationToken).ConfigureAwait(false);
            SetCandidateTags(activity, comparison.Candidates.Count, comparison.ApplicableCandidates.Count, comparison.RejectedCandidates.Count);
            return comparison;
        }
        catch (Exception exception)
        {
            SetError(activity, exception);
            throw;
        }
    }

    /// <summary>
    /// Makes a strategy decision inside a decision activity.
    /// </summary>
    public static async ValueTask<StrategyDecision<TSpace, TPlan>> DecideWithOpenTelemetryAsync<TSpace, TContext, TPlan>(
        this StrategySpace<TSpace, TContext, TPlan> space,
        TContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(space);

        using var activity = Start("forge.decide.decide", space.Id);

        try
        {
            var decision = await space.DecideAsync(context, cancellationToken).ConfigureAwait(false);
            activity?.SetTag("forge.decide.selected_strategy.id", decision.SelectedStrategyId.Value);
            SetCandidateTags(
                activity,
                decision.Candidates.Count,
                decision.Candidates.Count(candidate => candidate.IsApplicable),
                decision.Candidates.Count(candidate => !candidate.IsApplicable));
            return decision;
        }
        catch (Exception exception)
        {
            SetError(activity, exception);
            throw;
        }
    }

    /// <summary>
    /// Applies production and shadow policies to one frozen comparison inside a shadow-decision activity.
    /// </summary>
    public static async ValueTask<StrategyShadowDecision<TSpace, TPlan>> DecideWithShadowOpenTelemetryAsync<TSpace, TContext, TPlan>(
        this StrategyComparison<TSpace, TPlan> comparison,
        TContext context,
        IStrategySelectionPolicy<TContext, TPlan> productionPolicy,
        IStrategySelectionPolicy<TContext, TPlan> shadowPolicy,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(comparison);

        using var activity = Start("forge.decide.shadow", comparison.SpaceId);

        try
        {
            var decision = await comparison.DecideWithShadowAsync(context, productionPolicy, shadowPolicy, cancellationToken).ConfigureAwait(false);
            activity?.SetTag("forge.decide.selected_strategy.id", decision.Production.SelectedStrategyId.Value);
            activity?.SetTag("forge.decide.shadow_strategy.id", decision.Shadow.SelectedStrategyId.Value);
            activity?.SetTag("forge.decide.selection_changed", decision.SelectionChanged);
            return decision;
        }
        catch (Exception exception)
        {
            SetError(activity, exception);
            throw;
        }
    }

    private static Activity? Start(string operationName, StrategySpaceId spaceId)
    {
        var activity = ForgeDecideTelemetry.Source.StartActivity(operationName, ActivityKind.Internal);
        activity?.SetTag("forge.decide.space.id", spaceId.Value);
        return activity;
    }

    private static void SetCandidateTags(Activity? activity, int total, int applicable, int rejected)
    {
        activity?.SetTag("forge.decide.candidates.total", total);
        activity?.SetTag("forge.decide.candidates.applicable", applicable);
        activity?.SetTag("forge.decide.candidates.rejected", rejected);
    }

    private static void SetError(Activity? activity, Exception exception)
    {
        activity?.SetStatus(ActivityStatusCode.Error, exception.GetType().Name);
        activity?.SetTag("error.type", exception.GetType().FullName);
    }
}