using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Forge.Parse;

internal sealed class BuiltInValueMatcher : IValueMatcher
{
    private static readonly HashSet<string> FixedExpressions = new(StringComparer.OrdinalIgnoreCase)
    {
        "<Any>",
        "<Null>",
        "<NotNull>",
        "<Object>",
        "<EmptyObject>",
        "<NotEmptyObject>",
        "<Array>",
        "<EmptyArray>",
        "<NotEmptyArray>",
        "<String>",
        "<EmptyString>",
        "<NotEmptyString>",
        "<NotWhiteSpaceString>",
        "<WhiteSpaceString>",
        "<Int>",
        "<Long>",
        "<Number>",
        "<Decimal>",
        "<Bool>",
        "<Guid>",
        "<NotEmptyGuid>",
        "<DateTime>",
        "<DateTimeOffset>",
        "<DateOnly>",
        "<TimeOnly>",
        "<Uri>"
    };

    private static readonly string[] ParameterizedPrefixes =
    [
        "Regex",
        "OneOf",
        "StartsWith",
        "EndsWith",
        "Contains",
        "Length",
        "MinLength",
        "MaxLength",
        "GreaterThan",
        "GreaterThanOrEqual",
        "LessThan",
        "LessThanOrEqual",
        "ArrayCount",
        "ArrayMinCount",
        "ArrayMaxCount"
    ];

    public bool CanMatch(string expression)
    {
        if (FixedExpressions.Contains(expression))
        {
            return true;
        }

        return ParameterizedPrefixes.Any(prefix => TryGetArgument(expression, prefix, out _));
    }

    public ValueMatchResult Match(string expression, JsonNode? actual)
    {
        if (expression.Equals("<Any>", StringComparison.OrdinalIgnoreCase))
        {
            return ValueMatchResult.Success();
        }

        if (expression.Equals("<Null>", StringComparison.OrdinalIgnoreCase))
        {
            return actual is null
                ? ValueMatchResult.Success()
                : ValueMatchResult.Failure("Expected null");
        }

        if (expression.Equals("<NotNull>", StringComparison.OrdinalIgnoreCase))
        {
            return actual is null
                ? ValueMatchResult.Failure("Expected a non-null value")
                : ValueMatchResult.Success();
        }

        if (expression.Equals("<Object>", StringComparison.OrdinalIgnoreCase))
        {
            return actual is JsonObject
                ? ValueMatchResult.Success()
                : ValueMatchResult.Failure("Expected a JSON object");
        }

        if (expression.Equals("<EmptyObject>", StringComparison.OrdinalIgnoreCase))
        {
            return actual is JsonObject emptyObject && emptyObject.Count == 0
                ? ValueMatchResult.Success()
                : ValueMatchResult.Failure("Expected an empty JSON object");
        }

        if (expression.Equals("<NotEmptyObject>", StringComparison.OrdinalIgnoreCase))
        {
            return actual is JsonObject nonEmptyObject && nonEmptyObject.Count > 0
                ? ValueMatchResult.Success()
                : ValueMatchResult.Failure("Expected a non-empty JSON object");
        }

        if (expression.Equals("<Array>", StringComparison.OrdinalIgnoreCase))
        {
            return actual is JsonArray
                ? ValueMatchResult.Success()
                : ValueMatchResult.Failure("Expected a JSON array");
        }

        if (expression.Equals("<EmptyArray>", StringComparison.OrdinalIgnoreCase))
        {
            return actual is JsonArray emptyArray && emptyArray.Count == 0
                ? ValueMatchResult.Success()
                : ValueMatchResult.Failure("Expected an empty JSON array");
        }

        if (expression.Equals("<NotEmptyArray>", StringComparison.OrdinalIgnoreCase))
        {
            return actual is JsonArray nonEmptyArray && nonEmptyArray.Count > 0
                ? ValueMatchResult.Success()
                : ValueMatchResult.Failure("Expected a non-empty JSON array");
        }

        if (expression.Equals("<String>", StringComparison.OrdinalIgnoreCase))
        {
            return TryGetString(actual, out _)
                ? ValueMatchResult.Success()
                : ValueMatchResult.Failure("Expected a JSON string");
        }

        if (expression.Equals("<EmptyString>", StringComparison.OrdinalIgnoreCase))
        {
            return TryGetString(actual, out var emptyText) && emptyText.Length == 0
                ? ValueMatchResult.Success()
                : ValueMatchResult.Failure("Expected an empty JSON string");
        }

        if (expression.Equals("<NotEmptyString>", StringComparison.OrdinalIgnoreCase))
        {
            return TryGetString(actual, out var nonEmptyText) && nonEmptyText.Length > 0
                ? ValueMatchResult.Success()
                : ValueMatchResult.Failure("Expected a non-empty JSON string");
        }

        if (expression.Equals("<NotWhiteSpaceString>", StringComparison.OrdinalIgnoreCase))
        {
            return TryGetString(actual, out var meaningfulText) && !string.IsNullOrWhiteSpace(meaningfulText)
                ? ValueMatchResult.Success()
                : ValueMatchResult.Failure("Expected a non-whitespace JSON string");
        }

        if (expression.Equals("<WhiteSpaceString>", StringComparison.OrdinalIgnoreCase))
        {
            return TryGetString(actual, out var whiteSpaceText) && whiteSpaceText.Length > 0 &&
                   string.IsNullOrWhiteSpace(whiteSpaceText)
                ? ValueMatchResult.Success()
                : ValueMatchResult.Failure("Expected a whitespace-only JSON string");
        }

        if (expression.Equals("<Int>", StringComparison.OrdinalIgnoreCase))
        {
            return IsInt32(actual)
                ? ValueMatchResult.Success()
                : ValueMatchResult.Failure("Expected a 32-bit JSON integer");
        }

        if (expression.Equals("<Long>", StringComparison.OrdinalIgnoreCase))
        {
            return IsInt64(actual)
                ? ValueMatchResult.Success()
                : ValueMatchResult.Failure("Expected a 64-bit JSON integer");
        }

        if (expression.Equals("<Number>", StringComparison.OrdinalIgnoreCase))
        {
            return actual?.GetValueKind() == JsonValueKind.Number
                ? ValueMatchResult.Success()
                : ValueMatchResult.Failure("Expected a JSON number");
        }

        if (expression.Equals("<Decimal>", StringComparison.OrdinalIgnoreCase))
        {
            return TryGetDecimal(actual, out _)
                ? ValueMatchResult.Success()
                : ValueMatchResult.Failure("Expected a JSON number representable as Decimal");
        }

        if (expression.Equals("<Bool>", StringComparison.OrdinalIgnoreCase))
        {
            var kind = actual?.GetValueKind();
            return kind is JsonValueKind.True or JsonValueKind.False
                ? ValueMatchResult.Success()
                : ValueMatchResult.Failure("Expected a JSON boolean");
        }

        if (expression.Equals("<Guid>", StringComparison.OrdinalIgnoreCase))
        {
            return TryGetString(actual, out var guidText) && Guid.TryParse(guidText, out _)
                ? ValueMatchResult.Success()
                : ValueMatchResult.Failure("Expected a GUID string");
        }

        if (expression.Equals("<NotEmptyGuid>", StringComparison.OrdinalIgnoreCase))
        {
            return TryGetString(actual, out var nonEmptyGuidText) &&
                   Guid.TryParse(nonEmptyGuidText, out var guid) && guid != Guid.Empty
                ? ValueMatchResult.Success()
                : ValueMatchResult.Failure("Expected a non-empty GUID string");
        }

        if (expression.Equals("<DateTime>", StringComparison.OrdinalIgnoreCase))
        {
            return TryGetString(actual, out var dateTimeText) &&
                   DateTime.TryParse(dateTimeText, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out _)
                ? ValueMatchResult.Success()
                : ValueMatchResult.Failure("Expected a DateTime string");
        }

        if (expression.Equals("<DateTimeOffset>", StringComparison.OrdinalIgnoreCase))
        {
            return TryGetString(actual, out var dateTimeOffsetText) &&
                   DateTimeOffset.TryParse(dateTimeOffsetText, CultureInfo.InvariantCulture, DateTimeStyles.None, out _)
                ? ValueMatchResult.Success()
                : ValueMatchResult.Failure("Expected a DateTimeOffset string");
        }

        if (expression.Equals("<DateOnly>", StringComparison.OrdinalIgnoreCase))
        {
            return TryGetString(actual, out var dateOnlyText) &&
                   DateOnly.TryParse(dateOnlyText, CultureInfo.InvariantCulture, DateTimeStyles.None, out _)
                ? ValueMatchResult.Success()
                : ValueMatchResult.Failure("Expected a DateOnly string");
        }

        if (expression.Equals("<TimeOnly>", StringComparison.OrdinalIgnoreCase))
        {
            return TryGetString(actual, out var timeOnlyText) &&
                   TimeOnly.TryParse(timeOnlyText, CultureInfo.InvariantCulture, DateTimeStyles.None, out _)
                ? ValueMatchResult.Success()
                : ValueMatchResult.Failure("Expected a TimeOnly string");
        }

        if (expression.Equals("<Uri>", StringComparison.OrdinalIgnoreCase))
        {
            return TryGetString(actual, out var uriText) && Uri.TryCreate(uriText, UriKind.Absolute, out _)
                ? ValueMatchResult.Success()
                : ValueMatchResult.Failure("Expected an absolute URI string");
        }

        if (TryGetArgument(expression, "Regex", out var regexPattern))
        {
            return MatchRegex(regexPattern, actual);
        }

        if (TryGetArgument(expression, "OneOf", out var choices))
        {
            return MatchOneOf(choices, actual);
        }

        if (TryGetArgument(expression, "StartsWith", out var prefix))
        {
            return MatchStringPredicate(
                actual,
                value => value.StartsWith(prefix, StringComparison.Ordinal),
                $"Expected a string starting with '{prefix}'");
        }

        if (TryGetArgument(expression, "EndsWith", out var suffix))
        {
            return MatchStringPredicate(
                actual,
                value => value.EndsWith(suffix, StringComparison.Ordinal),
                $"Expected a string ending with '{suffix}'");
        }

        if (TryGetArgument(expression, "Contains", out var part))
        {
            return MatchStringPredicate(
                actual,
                value => value.Contains(part, StringComparison.Ordinal),
                $"Expected a string containing '{part}'");
        }

        if (TryGetArgument(expression, "Length", out var exactLength))
        {
            return MatchLength(actual, exactLength, value => value == 0, "length equal to");
        }

        if (TryGetArgument(expression, "MinLength", out var minimumLength))
        {
            return MatchLength(actual, minimumLength, value => value >= 0, "minimum length");
        }

        if (TryGetArgument(expression, "MaxLength", out var maximumLength))
        {
            return MatchLength(actual, maximumLength, value => value <= 0, "maximum length");
        }

        if (TryGetArgument(expression, "ArrayCount", out var arrayCount))
        {
            return MatchArrayCount(actual, arrayCount, value => value == 0, "count equal to");
        }

        if (TryGetArgument(expression, "ArrayMinCount", out var arrayMinCount))
        {
            return MatchArrayCount(actual, arrayMinCount, value => value >= 0, "minimum count");
        }

        if (TryGetArgument(expression, "ArrayMaxCount", out var arrayMaxCount))
        {
            return MatchArrayCount(actual, arrayMaxCount, value => value <= 0, "maximum count");
        }

        if (TryGetArgument(expression, "GreaterThan", out var greaterThan))
        {
            return MatchNumberComparison(actual, greaterThan, value => value > 0, ">");
        }

        if (TryGetArgument(expression, "GreaterThanOrEqual", out var greaterThanOrEqual))
        {
            return MatchNumberComparison(actual, greaterThanOrEqual, value => value >= 0, ">=");
        }

        if (TryGetArgument(expression, "LessThan", out var lessThan))
        {
            return MatchNumberComparison(actual, lessThan, value => value < 0, "<");
        }

        if (TryGetArgument(expression, "LessThanOrEqual", out var lessThanOrEqual))
        {
            return MatchNumberComparison(actual, lessThanOrEqual, value => value <= 0, "<=");
        }

        return ValueMatchResult.Failure($"Unsupported expression '{expression}'");
    }

    private static ValueMatchResult MatchRegex(string pattern, JsonNode? actual)
    {
        if (!TryGetString(actual, out var text))
        {
            return ValueMatchResult.Failure("Expected a JSON string");
        }

        try
        {
            return Regex.IsMatch(text, pattern, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1))
                ? ValueMatchResult.Success()
                : ValueMatchResult.Failure($"Expected a string matching regex '{pattern}'");
        }
        catch (ArgumentException exception)
        {
            return ValueMatchResult.Failure($"Invalid regex '{pattern}': {exception.Message}");
        }
        catch (RegexMatchTimeoutException)
        {
            return ValueMatchResult.Failure($"Regex '{pattern}' exceeded the one second match timeout");
        }
    }

    private static ValueMatchResult MatchOneOf(string choices, JsonNode? actual)
    {
        if (!TryGetString(actual, out var text))
        {
            return ValueMatchResult.Failure("Expected a JSON string");
        }

        var values = choices.Split('|', StringSplitOptions.None);
        return values.Contains(text, StringComparer.Ordinal)
            ? ValueMatchResult.Success()
            : ValueMatchResult.Failure($"Expected one of: {string.Join(", ", values)}");
    }

    private static ValueMatchResult MatchStringPredicate(
        JsonNode? actual,
        Func<string, bool> predicate,
        string failureMessage)
    {
        if (!TryGetString(actual, out var text))
        {
            return ValueMatchResult.Failure("Expected a JSON string");
        }

        return predicate(text)
            ? ValueMatchResult.Success()
            : ValueMatchResult.Failure(failureMessage);
    }

    private static ValueMatchResult MatchLength(
        JsonNode? actual,
        string expectedLengthText,
        Func<int, bool> comparison,
        string comparisonLabel)
    {
        if (!int.TryParse(expectedLengthText, NumberStyles.None, CultureInfo.InvariantCulture, out var expectedLength) ||
            expectedLength < 0)
        {
            return ValueMatchResult.Failure($"'{expectedLengthText}' is not a valid non-negative length");
        }

        if (!TryGetLength(actual, out var actualLength))
        {
            return ValueMatchResult.Failure("Expected a JSON string, array or object");
        }

        var compareResult = actualLength.CompareTo(expectedLength);
        return comparison(compareResult)
            ? ValueMatchResult.Success()
            : ValueMatchResult.Failure($"Expected {comparisonLabel} {expectedLengthText}");
    }

    private static ValueMatchResult MatchArrayCount(
        JsonNode? actual,
        string expectedCountText,
        Func<int, bool> comparison,
        string comparisonLabel)
    {
        if (!int.TryParse(expectedCountText, NumberStyles.None, CultureInfo.InvariantCulture, out var expectedCount) ||
            expectedCount < 0)
        {
            return ValueMatchResult.Failure($"'{expectedCountText}' is not a valid non-negative array count");
        }

        if (actual is not JsonArray array)
        {
            return ValueMatchResult.Failure("Expected a JSON array");
        }

        var compareResult = array.Count.CompareTo(expectedCount);
        return comparison(compareResult)
            ? ValueMatchResult.Success()
            : ValueMatchResult.Failure($"Expected array {comparisonLabel} {expectedCountText}");
    }

    private static ValueMatchResult MatchNumberComparison(
        JsonNode? actual,
        string thresholdText,
        Func<int, bool> comparison,
        string comparisonLabel)
    {
        if (!decimal.TryParse(thresholdText, NumberStyles.Float, CultureInfo.InvariantCulture, out var threshold))
        {
            return ValueMatchResult.Failure($"'{thresholdText}' is not a valid numeric threshold");
        }

        if (!TryGetDecimal(actual, out var actualNumber))
        {
            return ValueMatchResult.Failure("Expected a JSON number");
        }

        var compareResult = decimal.Compare(actualNumber, threshold);
        return comparison(compareResult)
            ? ValueMatchResult.Success()
            : ValueMatchResult.Failure($"Expected a number {comparisonLabel} {thresholdText}");
    }

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

    private static bool TryGetString(JsonNode? actual, out string value)
    {
        if (actual is JsonValue jsonValue && jsonValue.TryGetValue<string>(out var text) && text is not null)
        {
            value = text;
            return true;
        }

        value = string.Empty;
        return false;
    }

    private static bool TryGetLength(JsonNode? actual, out int length)
    {
        switch (actual)
        {
            case JsonValue value when value.TryGetValue<string>(out var text) && text is not null:
                length = text.Length;
                return true;
            case JsonArray array:
                length = array.Count;
                return true;
            case JsonObject jsonObject:
                length = jsonObject.Count;
                return true;
            default:
                length = default;
                return false;
        }
    }

    private static bool IsInt32(JsonNode? actual)
    {
        if (actual?.GetValueKind() != JsonValueKind.Number)
        {
            return false;
        }

        using var document = JsonDocument.Parse(actual.ToJsonString());
        return document.RootElement.TryGetInt32(out _);
    }

    private static bool IsInt64(JsonNode? actual)
    {
        if (actual?.GetValueKind() != JsonValueKind.Number)
        {
            return false;
        }

        using var document = JsonDocument.Parse(actual.ToJsonString());
        return document.RootElement.TryGetInt64(out _);
    }

    private static bool TryGetDecimal(JsonNode? actual, out decimal value)
    {
        value = default;
        if (actual?.GetValueKind() != JsonValueKind.Number)
        {
            return false;
        }

        using var document = JsonDocument.Parse(actual.ToJsonString());
        return document.RootElement.TryGetDecimal(out value);
    }
}
