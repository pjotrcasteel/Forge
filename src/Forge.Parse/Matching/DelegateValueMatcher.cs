using System.Text.Json.Nodes;

namespace Forge.Parse;

/// <summary>
/// Creates a custom placeholder matcher from delegates without requiring a dedicated matcher class.
/// </summary>
public sealed class DelegateValueMatcher : IValueMatcher
{
    private readonly string _expression;
    private readonly Func<JsonNode?, ValueMatchResult> _match;

    /// <summary>
    /// Initializes a matcher for one exact placeholder expression.
    /// </summary>
    /// <param name="expression">Placeholder expression including angle brackets.</param>
    /// <param name="match">Function that evaluates an actual JSON value.</param>
    public DelegateValueMatcher(string expression, Func<JsonNode?, ValueMatchResult> match)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(expression);
        ArgumentNullException.ThrowIfNull(match);

        if (expression.Length < 3 || expression[0] != '<' || expression[^1] != '>')
        {
            throw new ArgumentException("A matcher expression must be enclosed in angle brackets.", nameof(expression));
        }

        _expression = expression;
        _match = match;
    }

    /// <inheritdoc />
    public bool CanMatch(string expression) =>
        string.Equals(_expression, expression, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public ValueMatchResult Match(string expression, JsonNode? actual) => _match(actual);
}
