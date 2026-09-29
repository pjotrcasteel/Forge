using System.Text.Json;
using System.Text.Json.Nodes;

namespace Forge.Parse;

/// <summary>
/// Convenience assertions for structured JSON matching.
/// </summary>
public static class JsonAssert
{
    /// <summary>
    /// Asserts that expected JSON text matches actual JSON text.
    /// </summary>
    public static void Matches(
        string expectedJson,
        string actualJson,
        JsonMatchOptions? options = null,
        IEnumerable<IValueMatcher>? matchers = null)
    {
        var result = new JsonMatcher(matchers).Match(expectedJson, actualJson, options);
        ThrowIfFailed(result);
    }

    /// <summary>
    /// Asserts that expected JSON text matches an actual <see cref="JsonElement"/>.
    /// </summary>
    public static void Matches(
        string expectedJson,
        JsonElement actual,
        JsonMatchOptions? options = null,
        IEnumerable<IValueMatcher>? matchers = null)
    {
        var result = new JsonMatcher(matchers).Match(expectedJson, actual, options);
        ThrowIfFailed(result);
    }

    /// <summary>
    /// Asserts that expected JSON matches actual JSON.
    /// </summary>
    public static void Matches(
        JsonNode? expected,
        JsonNode? actual,
        JsonMatchOptions? options = null,
        IEnumerable<IValueMatcher>? matchers = null)
    {
        var result = new JsonMatcher(matchers).Match(expected, actual, options);
        ThrowIfFailed(result);
    }

    /// <summary>
    /// Serializes an object using System.Text.Json and asserts that it matches expected JSON text.
    /// </summary>
    public static void MatchesObject<T>(
        string expectedJson,
        T actual,
        JsonSerializerOptions? serializerOptions = null,
        JsonMatchOptions? options = null,
        IEnumerable<IValueMatcher>? matchers = null)
    {
        var result = new JsonMatcher(matchers).MatchObject(expectedJson, actual, serializerOptions, options);
        ThrowIfFailed(result);
    }

    private static void ThrowIfFailed(JsonMatchResult result)
    {
        if (!result.IsMatch)
        {
            throw new JsonMatchAssertionException(result);
        }
    }
}
