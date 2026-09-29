namespace Forge.Parse;

/// <summary>
/// Represents the result of matching a placeholder expression against one JSON value.
/// </summary>
public sealed record ValueMatchResult
{
    private ValueMatchResult(bool isMatch, string? message)
    {
        IsMatch = isMatch;
        Message = message;
    }

    /// <summary>
    /// Gets a value indicating whether the value matched.
    /// </summary>
    public bool IsMatch { get; }

    /// <summary>
    /// Gets the mismatch message when matching failed.
    /// </summary>
    public string? Message { get; }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static ValueMatchResult Success() => new(true, null);

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    /// <param name="message">Reason the value did not match.</param>
    public static ValueMatchResult Failure(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        return new ValueMatchResult(false, message);
    }
}
