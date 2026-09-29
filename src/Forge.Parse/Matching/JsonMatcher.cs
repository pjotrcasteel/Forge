using System.Text.Json;
using System.Text.Json.Nodes;

namespace Forge.Parse;

/// <summary>
/// Performs structural JSON matching and placeholder evaluation.
/// </summary>
public sealed class JsonMatcher
{
    private readonly IReadOnlyList<IValueMatcher> _matchers;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonMatcher"/> class.
    /// </summary>
    /// <param name="matchers">Optional custom matchers. Custom matchers are evaluated before built-in matchers.</param>
    public JsonMatcher(IEnumerable<IValueMatcher>? matchers = null)
    {
        var configuredMatchers = matchers?.ToList() ?? [];
        configuredMatchers.Add(new BuiltInValueMatcher());
        _matchers = configuredMatchers;
    }

    /// <summary>
    /// Matches expected JSON text against actual JSON text.
    /// </summary>
    public JsonMatchResult Match(string expectedJson, string actualJson, JsonMatchOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedJson);
        ArgumentException.ThrowIfNullOrWhiteSpace(actualJson);

        var expected = ParseJson(expectedJson, "expected");
        var actual = ParseJson(actualJson, "actual");
        return Match(expected, actual, options);
    }

    /// <summary>
    /// Matches expected JSON text against an actual <see cref="JsonElement"/>.
    /// </summary>
    public JsonMatchResult Match(string expectedJson, JsonElement actual, JsonMatchOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedJson);
        var expected = ParseJson(expectedJson, "expected");
        var actualNode = JsonNode.Parse(actual.GetRawText());
        return Match(expected, actualNode, options);
    }

    /// <summary>
    /// Serializes an object using System.Text.Json and matches it against expected JSON text.
    /// </summary>
    public JsonMatchResult MatchObject<T>(
        string expectedJson,
        T actual,
        JsonSerializerOptions? serializerOptions = null,
        JsonMatchOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedJson);
        var expected = ParseJson(expectedJson, "expected");
        var actualNode = JsonSerializer.SerializeToNode(actual, serializerOptions);
        return Match(expected, actualNode, options);
    }

    /// <summary>
    /// Matches expected JSON against actual JSON.
    /// </summary>
    public JsonMatchResult Match(JsonNode? expected, JsonNode? actual, JsonMatchOptions? options = null)
    {
        var mismatches = new List<JsonMismatch>();
        var context = new MatchContext();
        Compare(expected, actual, "$", options ?? JsonMatchOptions.Partial, mismatches, context);
        return new JsonMatchResult(mismatches);
    }

    private void Compare(
        JsonNode? expected,
        JsonNode? actual,
        string path,
        JsonMatchOptions options,
        List<JsonMismatch> mismatches,
        MatchContext context)
    {
        if (options.IsIgnored(path))
        {
            return;
        }

        if (TryGetEscapedLiteral(expected, out var literal))
        {
            CompareEscapedLiteral(literal, actual, path, mismatches);
            return;
        }

        if (TryGetPlaceholder(expected, out var expression))
        {
            MatchPlaceholder(expression, actual, path, mismatches, context);
            return;
        }

        if (expected is null)
        {
            if (actual is not null)
            {
                AddMismatch(path, "null", Format(actual), "Expected null", mismatches);
            }

            return;
        }

        if (actual is null)
        {
            AddMismatch(path, Format(expected), "null", "Actual value was null", mismatches);
            return;
        }

        if (expected is JsonObject expectedObject)
        {
            if (actual is not JsonObject actualObject)
            {
                AddMismatch(path, Format(expected), Format(actual), "Expected a JSON object", mismatches);
                return;
            }

            CompareObjects(expectedObject, actualObject, path, options, mismatches, context);
            return;
        }

        if (expected is JsonArray expectedArray)
        {
            if (actual is not JsonArray actualArray)
            {
                AddMismatch(path, Format(expected), Format(actual), "Expected a JSON array", mismatches);
                return;
            }

            CompareArrays(expectedArray, actualArray, path, options, mismatches, context);
            return;
        }

        if (!JsonNode.DeepEquals(expected, actual))
        {
            AddMismatch(path, Format(expected), Format(actual), "Values differed", mismatches);
        }
    }

    private void CompareObjects(
        JsonObject expected,
        JsonObject actual,
        string path,
        JsonMatchOptions options,
        List<JsonMismatch> mismatches,
        MatchContext context)
    {
        foreach (var property in expected)
        {
            var propertyPath = JsonPathFormatter.AppendProperty(path, property.Key);
            if (options.IsIgnored(propertyPath))
            {
                continue;
            }

            var exists = TryGetProperty(actual, property.Key, options.PropertyNameComparison, out var actualValue);
            if (TryGetPresenceExpectation(property.Value, out var presenceExpectation))
            {
                ComparePresence(presenceExpectation, exists, actualValue, propertyPath, mismatches);
                continue;
            }

            if (!exists)
            {
                AddMismatch(propertyPath, Format(property.Value), "<missing>", "Property was missing", mismatches);
                continue;
            }

            Compare(property.Value, actualValue, propertyPath, options, mismatches, context);
        }

        if (options.AllowAdditionalProperties)
        {
            return;
        }

        foreach (var property in actual)
        {
            var propertyPath = JsonPathFormatter.AppendProperty(path, property.Key);
            if (options.IsIgnored(propertyPath))
            {
                continue;
            }

            if (!ContainsProperty(expected, property.Key, options.PropertyNameComparison))
            {
                AddMismatch(propertyPath, "<missing>", Format(property.Value), "Unexpected property was present", mismatches);
            }
        }
    }

    private void CompareArrays(
        JsonArray expected,
        JsonArray actual,
        string path,
        JsonMatchOptions options,
        List<JsonMismatch> mismatches,
        MatchContext context)
    {
        CompareArrayLengths(expected, actual, path, options, mismatches);

        if (options.IsUnorderedArray(path))
        {
            CompareUnorderedArrayItems(expected, actual, path, options, mismatches, context);
            return;
        }

        var count = Math.Min(expected.Count, actual.Count);
        for (var index = 0; index < count; index++)
        {
            Compare(
                expected[index],
                actual[index],
                JsonPathFormatter.AppendIndex(path, index),
                options,
                mismatches,
                context);
        }
    }

    private static void CompareArrayLengths(
        JsonArray expected,
        JsonArray actual,
        string path,
        JsonMatchOptions options,
        List<JsonMismatch> mismatches)
    {
        if (actual.Count < expected.Count)
        {
            AddMismatch(
                path,
                $"array length >= {expected.Count}",
                $"array length {actual.Count}",
                "Array contained too few items",
                mismatches);
        }
        else if (!options.AllowAdditionalArrayItems && actual.Count != expected.Count)
        {
            AddMismatch(
                path,
                $"array length {expected.Count}",
                $"array length {actual.Count}",
                "Array lengths differed",
                mismatches);
        }
    }

    private void CompareUnorderedArrayItems(
        JsonArray expected,
        JsonArray actual,
        string path,
        JsonMatchOptions options,
        List<JsonMismatch> mismatches,
        MatchContext context)
    {
        var usedActualItems = new bool[actual.Count];
        var trialContext = context.Clone();

        if (TryMatchUnordered(expected, actual, path, options, 0, usedActualItems, trialContext, out var failedIndex))
        {
            context.CopyFrom(trialContext);
            return;
        }

        var index = Math.Clamp(failedIndex, 0, Math.Max(0, expected.Count - 1));
        AddMismatch(
            JsonPathFormatter.AppendIndex(path, index),
            expected.Count == 0 ? "<item>" : Format(expected[index]),
            "<no matching item>",
            "No matching assignment could be found for the unordered array",
            mismatches);
    }

    private bool TryMatchUnordered(
        JsonArray expected,
        JsonArray actual,
        string path,
        JsonMatchOptions options,
        int expectedIndex,
        bool[] usedActualItems,
        MatchContext context,
        out int failedIndex)
    {
        if (expectedIndex >= expected.Count)
        {
            failedIndex = -1;
            return true;
        }

        for (var actualIndex = 0; actualIndex < actual.Count; actualIndex++)
        {
            if (usedActualItems[actualIndex])
            {
                continue;
            }

            var candidateContext = context.Clone();
            var candidateMismatches = new List<JsonMismatch>();
            Compare(
                expected[expectedIndex],
                actual[actualIndex],
                JsonPathFormatter.AppendIndex(path, expectedIndex),
                options,
                candidateMismatches,
                candidateContext);

            if (candidateMismatches.Count != 0)
            {
                continue;
            }

            usedActualItems[actualIndex] = true;
            if (TryMatchUnordered(
                    expected,
                    actual,
                    path,
                    options,
                    expectedIndex + 1,
                    usedActualItems,
                    candidateContext,
                    out failedIndex))
            {
                context.CopyFrom(candidateContext);
                return true;
            }

            usedActualItems[actualIndex] = false;
        }

        failedIndex = expectedIndex;
        return false;
    }

    private void MatchPlaceholder(
        string expression,
        JsonNode? actual,
        string path,
        List<JsonMismatch> mismatches,
        MatchContext context)
    {
        if (TryGetArgument(expression, "Capture", out var captureName))
        {
            MatchCapture(captureName, actual, path, mismatches, context);
            return;
        }

        if (TryGetArgument(expression, "Same", out var referenceName))
        {
            MatchReference(referenceName, actual, path, mismatches, context);
            return;
        }

        if (expression.Equals("<Present>", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (expression.Equals("<Missing>", StringComparison.OrdinalIgnoreCase))
        {
            AddMismatch(path, expression, Format(actual), "Expected the property to be missing", mismatches);
            return;
        }

        var matcher = _matchers.FirstOrDefault(x => x.CanMatch(expression));
        if (matcher is null)
        {
            AddMismatch(path, expression, Format(actual), $"No matcher is registered for '{expression}'", mismatches);
            return;
        }

        var result = matcher.Match(expression, actual);
        if (!result.IsMatch)
        {
            AddMismatch(path, expression, Format(actual), result.Message ?? "Placeholder did not match", mismatches);
        }
    }

    private static void MatchCapture(
        string name,
        JsonNode? actual,
        string path,
        List<JsonMismatch> mismatches,
        MatchContext context)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            AddMismatch(path, "<Capture:name>", Format(actual), "Capture name cannot be empty", mismatches);
            return;
        }

        if (context.TryGetCapture(name, out var existing))
        {
            if (!JsonNode.DeepEquals(existing, actual))
            {
                AddMismatch(
                    path,
                    $"same value as capture '{name}' ({Format(existing)})",
                    Format(actual),
                    $"Capture '{name}' was already bound to a different value",
                    mismatches);
            }

            return;
        }

        context.Capture(name, actual);
    }

    private static void MatchReference(
        string name,
        JsonNode? actual,
        string path,
        List<JsonMismatch> mismatches,
        MatchContext context)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            AddMismatch(path, "<Same:name>", Format(actual), "Reference name cannot be empty", mismatches);
            return;
        }

        if (!context.TryGetCapture(name, out var captured))
        {
            AddMismatch(path, $"<Same:{name}>", Format(actual), $"Capture '{name}' has not been defined", mismatches);
            return;
        }

        if (!JsonNode.DeepEquals(captured, actual))
        {
            AddMismatch(
                path,
                $"same value as capture '{name}' ({Format(captured)})",
                Format(actual),
                $"Value differed from capture '{name}'",
                mismatches);
        }
    }

    private static void ComparePresence(
        PresenceExpectation expectation,
        bool exists,
        JsonNode? actual,
        string path,
        List<JsonMismatch> mismatches)
    {
        if (expectation == PresenceExpectation.Present && !exists)
        {
            AddMismatch(path, "<Present>", "<missing>", "Expected the property to be present", mismatches);
        }
        else if (expectation == PresenceExpectation.Missing && exists)
        {
            AddMismatch(path, "<Missing>", Format(actual), "Expected the property to be missing", mismatches);
        }
    }

    private static void CompareEscapedLiteral(
        string literal,
        JsonNode? actual,
        string path,
        List<JsonMismatch> mismatches)
    {
        if (actual is JsonValue value && value.TryGetValue<string>(out var actualText) && actualText == literal)
        {
            return;
        }

        AddMismatch(path, JsonSerializer.Serialize(literal), Format(actual), "Values differed", mismatches);
    }

    private static bool TryGetPresenceExpectation(JsonNode? node, out PresenceExpectation expectation)
    {
        if (TryGetPlaceholder(node, out var expression))
        {
            if (expression.Equals("<Present>", StringComparison.OrdinalIgnoreCase))
            {
                expectation = PresenceExpectation.Present;
                return true;
            }

            if (expression.Equals("<Missing>", StringComparison.OrdinalIgnoreCase))
            {
                expectation = PresenceExpectation.Missing;
                return true;
            }
        }

        expectation = default;
        return false;
    }

    private static bool TryGetPlaceholder(JsonNode? node, out string expression)
    {
        if (node is JsonValue value && value.TryGetValue<string>(out var text) &&
            text is not null && IsPlaceholderExpression(text))
        {
            expression = text;
            return true;
        }

        expression = string.Empty;
        return false;
    }

    private static bool TryGetEscapedLiteral(JsonNode? node, out string literal)
    {
        if (node is JsonValue value && value.TryGetValue<string>(out var text) &&
            text is not null && text.Length >= 4 && text.StartsWith("<<", StringComparison.Ordinal) &&
            text.EndsWith(">>", StringComparison.Ordinal))
        {
            literal = text[1..^1];
            return true;
        }

        literal = string.Empty;
        return false;
    }

    private static bool IsPlaceholderExpression(string value) =>
        value.Length >= 3 && value[0] == '<' && value[^1] == '>';

    private static bool TryGetArgument(string expression, string name, out string argument)
    {
        var prefix = $"<{name}:";
        if (expression.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && expression.EndsWith('>'))
        {
            argument = expression[prefix.Length..^1];
            return true;
        }

        argument = string.Empty;
        return false;
    }

    private static bool TryGetProperty(
        JsonObject source,
        string propertyName,
        StringComparison comparison,
        out JsonNode? value)
    {
        if (comparison == StringComparison.Ordinal && source.TryGetPropertyValue(propertyName, out value))
        {
            return true;
        }

        foreach (var property in source)
        {
            if (string.Equals(property.Key, propertyName, comparison))
            {
                value = property.Value;
                return true;
            }
        }

        value = null;
        return false;
    }

    private static bool ContainsProperty(JsonObject source, string propertyName, StringComparison comparison) =>
        source.Any(property => string.Equals(property.Key, propertyName, comparison));

    private static JsonNode? ParseJson(string json, string role)
    {
        try
        {
            return JsonNode.Parse(json);
        }
        catch (JsonException exception)
        {
            throw new FormatException($"Unable to parse {role} JSON.", exception);
        }
    }

    private static void AddMismatch(
        string path,
        string expected,
        string actual,
        string message,
        List<JsonMismatch> mismatches) =>
        mismatches.Add(new JsonMismatch(path, expected, actual, message));

    private static string Format(JsonNode? node) => node?.ToJsonString() ?? "null";

    private enum PresenceExpectation
    {
        Present,
        Missing
    }
}
