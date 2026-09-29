using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Forge.Parse;

/// <summary>
/// Parses compact expected-value text into JSON values for use in data-driven tests.
/// </summary>
public static class ExpectedValueParser
{
    /// <summary>
    /// Parses a value into the most natural JSON representation.
    /// </summary>
    /// <param name="value">Text value to parse.</param>
    public static JsonNode? Parse(string? value)
    {
        if (value is null)
        {
            return null;
        }

        var trimmed = value.Trim();

        if (trimmed.Equals("null", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (IsPlaceholderLike(trimmed))
        {
            return JsonValue.Create(trimmed);
        }

        if (trimmed.StartsWith('{') || trimmed.StartsWith('[') || trimmed.StartsWith('"'))
        {
            try
            {
                return JsonNode.Parse(trimmed);
            }
            catch (JsonException exception)
            {
                throw new FormatException($"Unable to parse '{value}' as JSON.", exception);
            }
        }

        if (bool.TryParse(trimmed, out var boolean))
        {
            return JsonValue.Create(boolean);
        }

        if (long.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out var integer))
        {
            return JsonValue.Create(integer);
        }

        if (decimal.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
        {
            return JsonValue.Create(number);
        }

        return JsonValue.Create(value);
    }

    private static bool IsPlaceholderLike(string value) =>
        value.Length >= 3 && value[0] == '<' && value[^1] == '>';
}
