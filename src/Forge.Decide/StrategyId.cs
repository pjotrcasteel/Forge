namespace Forge.Decide;

/// <summary>
/// Identifies a strategy within a strategy space.
/// </summary>
public sealed record StrategyId
{
    /// <summary>
    /// Initializes a strategy identifier.
    /// </summary>
    /// <param name="value">Stable strategy identifier.</param>
    public StrategyId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    /// <summary>
    /// Gets the stable strategy identifier.
    /// </summary>
    public string Value { get; }

    /// <inheritdoc />
    public override string ToString() => Value;

    /// <summary>
    /// Converts a string to a strategy identifier.
    /// </summary>
    /// <param name="value">Stable strategy identifier.</param>
    public static implicit operator StrategyId(string value) => new(value);
}