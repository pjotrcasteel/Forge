namespace Forge.Decide;

/// <summary>
/// Identifies an explicit candidate boundary in which strategies may compete.
/// </summary>
public sealed record StrategySpaceId
{
    /// <summary>
    /// Initializes a strategy-space identifier.
    /// </summary>
    /// <param name="value">Stable strategy-space identifier.</param>
    public StrategySpaceId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    /// <summary>
    /// Gets the stable strategy-space identifier.
    /// </summary>
    public string Value { get; }

    /// <inheritdoc />
    public override string ToString() => Value;

    /// <summary>
    /// Converts a string to a strategy-space identifier.
    /// </summary>
    /// <param name="value">Stable strategy-space identifier.</param>
    public static implicit operator StrategySpaceId(string value) => new(value);
}