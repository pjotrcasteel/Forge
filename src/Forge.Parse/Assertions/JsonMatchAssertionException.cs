namespace Forge.Parse;

/// <summary>
/// Thrown when a Forge.Parse assertion fails.
/// </summary>
public sealed class JsonMatchAssertionException : Exception
{
    /// <summary>
    /// Initializes an assertion exception with a custom message.
    /// </summary>
    public JsonMatchAssertionException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes an assertion exception from a match result.
    /// </summary>
    public JsonMatchAssertionException(JsonMatchResult result)
        : base(GetMessage(result))
    {
        Result = result;
    }

    /// <summary>
    /// Gets the structured match result when this exception originated from a Forge.Parse match.
    /// </summary>
    public JsonMatchResult? Result { get; }

    private static string GetMessage(JsonMatchResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return result.ToString();
    }
}
