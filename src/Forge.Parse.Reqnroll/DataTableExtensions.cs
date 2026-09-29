using System.Text.Json.Nodes;
using Reqnroll;

namespace Forge.Parse.Reqnroll;

/// <summary>
/// Converts Reqnroll data tables into Forge.Parse expected JSON and matches them against actual JSON.
/// </summary>
public static class DataTableExtensions
{
    /// <summary>
    /// Converts all rows in a data table to a JSON array.
    /// </summary>
    public static JsonArray ToExpectedJsonArray(this DataTable table, bool expandColumnPaths = false)
    {
        ArgumentNullException.ThrowIfNull(table);

        var result = new JsonArray();
        foreach (var row in table.Rows)
        {
            result.Add(ToExpectedJsonObject(row, expandColumnPaths));
        }

        return result;
    }

    /// <summary>
    /// Converts the single row in a data table to a JSON object.
    /// </summary>
    public static JsonObject ToExpectedJsonObject(this DataTable table, bool expandColumnPaths = false)
    {
        ArgumentNullException.ThrowIfNull(table);

        if (table.RowCount != 1)
        {
            throw new InvalidOperationException($"Expected exactly one data row but found {table.RowCount}.");
        }

        return ToExpectedJsonObject(table.Rows[0], expandColumnPaths);
    }

    /// <summary>
    /// Matches a table interpreted as a JSON array against actual JSON text.
    /// </summary>
    public static JsonMatchResult MatchJsonArray(
        this DataTable table,
        string actualJson,
        bool expandColumnPaths = false,
        JsonMatchOptions? options = null,
        IEnumerable<IValueMatcher>? matchers = null)
    {
        ArgumentNullException.ThrowIfNull(table);
        var expected = table.ToExpectedJsonArray(expandColumnPaths);
        var actual = ParseActual(actualJson);
        return new JsonMatcher(matchers).Match(expected, actual, options);
    }

    /// <summary>
    /// Matches a single-row table interpreted as a JSON object against actual JSON text.
    /// </summary>
    public static JsonMatchResult MatchJsonObject(
        this DataTable table,
        string actualJson,
        bool expandColumnPaths = false,
        JsonMatchOptions? options = null,
        IEnumerable<IValueMatcher>? matchers = null)
    {
        ArgumentNullException.ThrowIfNull(table);
        var expected = table.ToExpectedJsonObject(expandColumnPaths);
        var actual = ParseActual(actualJson);
        return new JsonMatcher(matchers).Match(expected, actual, options);
    }

    /// <summary>
    /// Asserts that a table interpreted as a JSON array matches actual JSON text.
    /// </summary>
    public static void AssertMatchesJsonArray(
        this DataTable table,
        string actualJson,
        bool expandColumnPaths = false,
        JsonMatchOptions? options = null,
        IEnumerable<IValueMatcher>? matchers = null)
    {
        var result = table.MatchJsonArray(actualJson, expandColumnPaths, options, matchers);
        ThrowIfFailed(result);
    }

    /// <summary>
    /// Asserts that a single-row table interpreted as a JSON object matches actual JSON text.
    /// </summary>
    public static void AssertMatchesJsonObject(
        this DataTable table,
        string actualJson,
        bool expandColumnPaths = false,
        JsonMatchOptions? options = null,
        IEnumerable<IValueMatcher>? matchers = null)
    {
        var result = table.MatchJsonObject(actualJson, expandColumnPaths, options, matchers);
        ThrowIfFailed(result);
    }

    private static JsonObject ToExpectedJsonObject(DataTableRow row, bool expandColumnPaths)
    {
        var result = new JsonObject();
        foreach (var cell in row)
        {
            var value = ExpectedValueParser.Parse(cell.Value);
            if (expandColumnPaths)
            {
                SetPathValue(result, cell.Key, value);
            }
            else
            {
                result[cell.Key] = value;
            }
        }

        return result;
    }

    private static void SetPathValue(JsonObject root, string path, JsonNode? value)
    {
        var segments = ParseColumnPath(path);
        JsonNode current = root;

        for (var index = 0; index < segments.Count; index++)
        {
            var segment = segments[index];
            var isLast = index == segments.Count - 1;
            var nextIsArray = !isLast && segments[index + 1].Index is not null;

            if (segment.PropertyName is not null)
            {
                current = SetObjectSegment(current, segment.PropertyName, value, isLast, nextIsArray, path);
                continue;
            }

            current = SetArraySegment(current, segment.Index!.Value, value, isLast, nextIsArray, path);
        }
    }

    private static JsonNode SetObjectSegment(
        JsonNode current,
        string propertyName,
        JsonNode? value,
        bool isLast,
        bool nextIsArray,
        string fullPath)
    {
        if (current is not JsonObject currentObject)
        {
            throw new InvalidOperationException($"Column path '{fullPath}' conflicts with an existing non-object value.");
        }

        if (isLast)
        {
            if (currentObject.ContainsKey(propertyName))
            {
                throw new InvalidOperationException($"Column path '{fullPath}' is defined more than once.");
            }

            currentObject[propertyName] = value;
            return currentObject;
        }

        if (!currentObject.TryGetPropertyValue(propertyName, out var child) || child is null)
        {
            child = nextIsArray ? new JsonArray() : new JsonObject();
            currentObject[propertyName] = child;
        }

        ValidateContainer(child, nextIsArray, fullPath);
        return child;
    }

    private static JsonNode SetArraySegment(
        JsonNode current,
        int arrayIndex,
        JsonNode? value,
        bool isLast,
        bool nextIsArray,
        string fullPath)
    {
        if (current is not JsonArray array)
        {
            throw new InvalidOperationException($"Column path '{fullPath}' conflicts with an existing non-array value.");
        }

        while (array.Count <= arrayIndex)
        {
            array.Add(null);
        }

        if (isLast)
        {
            if (array[arrayIndex] is not null)
            {
                throw new InvalidOperationException($"Column path '{fullPath}' is defined more than once.");
            }

            array[arrayIndex] = value;
            return array;
        }

        var child = array[arrayIndex];
        if (child is null)
        {
            child = nextIsArray ? new JsonArray() : new JsonObject();
            array[arrayIndex] = child;
        }

        ValidateContainer(child, nextIsArray, fullPath);
        return child;
    }

    private static void ValidateContainer(JsonNode child, bool shouldBeArray, string fullPath)
    {
        if (shouldBeArray && child is not JsonArray)
        {
            throw new InvalidOperationException($"Column path '{fullPath}' conflicts with an existing non-array value.");
        }

        if (!shouldBeArray && child is not JsonObject)
        {
            throw new InvalidOperationException($"Column path '{fullPath}' conflicts with an existing non-object value.");
        }
    }

    private static IReadOnlyList<(string? PropertyName, int? Index)> ParseColumnPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new FormatException("Column path cannot be empty.");
        }

        var result = new List<(string? PropertyName, int? Index)>();
        var position = 0;

        while (position < path.Length)
        {
            var propertyStart = position;
            while (position < path.Length && path[position] is not '.' and not '[')
            {
                position++;
            }

            if (position > propertyStart)
            {
                var propertyName = path[propertyStart..position].Trim();
                if (propertyName.Length == 0)
                {
                    throw new FormatException($"'{path}' is not a valid column path.");
                }

                result.Add((propertyName, null));
            }
            else if (result.Count == 0 || path[position] != '[')
            {
                throw new FormatException($"'{path}' is not a valid column path.");
            }

            while (position < path.Length && path[position] == '[')
            {
                var closingBracket = path.IndexOf(']', position + 1);
                if (closingBracket < 0 ||
                    !int.TryParse(path[(position + 1)..closingBracket], out var arrayIndex) ||
                    arrayIndex < 0)
                {
                    throw new FormatException($"'{path}' contains an invalid array index.");
                }

                result.Add((null, arrayIndex));
                position = closingBracket + 1;
            }

            if (position >= path.Length)
            {
                break;
            }

            if (path[position] != '.')
            {
                throw new FormatException($"'{path}' is not a valid column path.");
            }

            position++;
            if (position >= path.Length || path[position] is '.' or ']')
            {
                throw new FormatException($"'{path}' is not a valid column path.");
            }
        }

        if (result.Count == 0 || result[0].PropertyName is null)
        {
            throw new FormatException($"'{path}' must start with a property name.");
        }

        return result;
    }

    private static JsonNode? ParseActual(string actualJson)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(actualJson);

        try
        {
            return JsonNode.Parse(actualJson);
        }
        catch (System.Text.Json.JsonException exception)
        {
            throw new FormatException("Unable to parse actual JSON.", exception);
        }
    }

    private static void ThrowIfFailed(JsonMatchResult result)
    {
        if (!result.IsMatch)
        {
            throw new JsonMatchAssertionException(result);
        }
    }
}
