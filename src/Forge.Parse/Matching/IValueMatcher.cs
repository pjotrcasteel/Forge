using System.Text.Json.Nodes;

namespace Forge.Parse;

/// <summary>
/// Matches a placeholder expression such as &lt;Guid&gt; against an actual JSON value.
/// </summary>
public interface IValueMatcher
{
    /// <summary>
    /// Determines whether this matcher handles the supplied expression.
    /// </summary>
    /// <param name="expression">Complete placeholder expression including angle brackets.</param>
    bool CanMatch(string expression);

    /// <summary>
    /// Matches an expression against an actual JSON value.
    /// </summary>
    /// <param name="expression">Complete placeholder expression including angle brackets.</param>
    /// <param name="actual">Actual JSON value. A JSON null is represented as <see langword="null"/>.</param>
    ValueMatchResult Match(string expression, JsonNode? actual);
}
