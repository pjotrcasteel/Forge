using System.Text;

namespace Forge.Decide;

/// <summary>
/// Portable diagnostic explanation of a completed strategy decision.
/// </summary>
public sealed record StrategyDecisionExplanation
{
    internal StrategyDecisionExplanation(
        StrategySpaceId spaceId,
        StrategyId selectedStrategyId,
        IReadOnlyList<StrategyCandidateExplanation> candidates)
    {
        SpaceId = spaceId;
        SelectedStrategyId = selectedStrategyId;
        Candidates = candidates;
    }

    /// <summary>
    /// Gets the strategy-space identifier.
    /// </summary>
    public StrategySpaceId SpaceId { get; }

    /// <summary>
    /// Gets the selected strategy identifier.
    /// </summary>
    public StrategyId SelectedStrategyId { get; }

    /// <summary>
    /// Gets the complete candidate explanation in evaluation order.
    /// </summary>
    public IReadOnlyList<StrategyCandidateExplanation> Candidates { get; }

    /// <inheritdoc />
    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.Append("Space: ").AppendLine(SpaceId.Value);
        builder.Append("Selected: ").AppendLine(SelectedStrategyId.Value);

        foreach (var candidate in Candidates)
        {
            builder.Append('[').Append(candidate.Disposition).Append("] ").Append(candidate.StrategyId.Value);

            if (!string.IsNullOrWhiteSpace(candidate.Reason))
            {
                builder.Append(": ").Append(candidate.Reason);
            }

            builder.AppendLine();
        }

        return builder.ToString().TrimEnd();
    }
}