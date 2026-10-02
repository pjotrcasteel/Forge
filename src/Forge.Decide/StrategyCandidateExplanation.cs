namespace Forge.Decide;

/// <summary>
/// Describes one candidate's role in an explainable strategy decision.
/// </summary>
/// <param name="StrategyId">Strategy identifier.</param>
/// <param name="Disposition">Candidate disposition.</param>
/// <param name="Reason">Application-provided proposal or rejection explanation.</param>
public sealed record StrategyCandidateExplanation(StrategyId StrategyId, StrategyCandidateDisposition Disposition, string? Reason);