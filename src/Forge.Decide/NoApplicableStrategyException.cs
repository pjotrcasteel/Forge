namespace Forge.Decide;

/// <summary>
/// Thrown when no strategy in a space can formulate an applicable proposal.
/// </summary>
public sealed class NoApplicableStrategyException : InvalidOperationException
{
    /// <summary>
    /// Initializes the exception.
    /// </summary>
    /// <param name="spaceId">Strategy-space identifier.</param>
    public NoApplicableStrategyException(StrategySpaceId spaceId)
        : base($"Strategy space '{spaceId}' produced no applicable proposals.")
    {
        SpaceId = spaceId;
    }

    /// <summary>
    /// Gets the strategy-space identifier.
    /// </summary>
    public StrategySpaceId SpaceId { get; }
}