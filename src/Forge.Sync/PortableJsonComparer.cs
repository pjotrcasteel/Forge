using System.Text.Json;

namespace Forge.Sync;

/// <summary>Performs deterministic structural JSON equality for portable plan preconditions.</summary>
public static class PortableJsonComparer
{
    public static bool AreEquivalent(JsonElement left, JsonElement right)
    {
        if (left.ValueKind != right.ValueKind)
        {
            return false;
        }

        return left.ValueKind switch
        {
            JsonValueKind.Object => ObjectsEqual(left, right),
            JsonValueKind.Array => ArraysEqual(left, right),
            JsonValueKind.String => left.GetString() == right.GetString(),
            JsonValueKind.Number => left.GetRawText() == right.GetRawText(),
            JsonValueKind.True or JsonValueKind.False => left.GetBoolean() == right.GetBoolean(),
            JsonValueKind.Null or JsonValueKind.Undefined => true,
            _ => left.GetRawText() == right.GetRawText()
        };
    }

    private static bool ObjectsEqual(JsonElement left, JsonElement right)
    {
        var leftProperties = left.EnumerateObject()
            .ToDictionary(static property => property.Name, static property => property.Value, StringComparer.Ordinal);
        var rightProperties = right.EnumerateObject()
            .ToDictionary(static property => property.Name, static property => property.Value, StringComparer.Ordinal);
        if (leftProperties.Count != rightProperties.Count)
        {
            return false;
        }

        foreach (var pair in leftProperties)
        {
            if (!rightProperties.TryGetValue(pair.Key, out var rightValue)
                || !AreEquivalent(pair.Value, rightValue))
            {
                return false;
            }
        }

        return true;
    }

    private static bool ArraysEqual(JsonElement left, JsonElement right)
    {
        var leftItems = left.EnumerateArray().ToArray();
        var rightItems = right.EnumerateArray().ToArray();
        if (leftItems.Length != rightItems.Length)
        {
            return false;
        }

        for (var index = 0; index < leftItems.Length; index++)
        {
            if (!AreEquivalent(leftItems[index], rightItems[index]))
            {
                return false;
            }
        }

        return true;
    }
}
