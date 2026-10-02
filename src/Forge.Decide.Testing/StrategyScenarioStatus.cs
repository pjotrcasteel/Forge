namespace Forge.Decide.Testing;

/// <summary>
/// Describes the resolution state of one scenario.
/// </summary>
public enum StrategyScenarioStatus
{
    /// <summary>
    /// The configured selection policy produced one decision.
    /// </summary>
    Selected,

    /// <summary>
    /// No strategy produced an applicable proposal.
    /// </summary>
    NoApplicableStrategy,

    /// <summary>
    /// The configured selection policy could not resolve multiple applicable proposals.
    /// </summary>
    Ambiguous,
}