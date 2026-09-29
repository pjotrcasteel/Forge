using System.Text;

namespace Forge.Parse;

internal static class JsonPathFormatter
{
    public static string AppendProperty(string path, string propertyName)
    {
        if (IsSimpleIdentifier(propertyName))
        {
            return $"{path}.{propertyName}";
        }

        return $"{path}['{Escape(propertyName)}']";
    }

    public static string AppendIndex(string path, int index) => $"{path}[{index}]";

    private static bool IsSimpleIdentifier(string value)
    {
        if (string.IsNullOrEmpty(value) || (!char.IsLetter(value[0]) && value[0] != '_'))
        {
            return false;
        }

        for (var index = 1; index < value.Length; index++)
        {
            if (!char.IsLetterOrDigit(value[index]) && value[index] != '_')
            {
                return false;
            }
        }

        return true;
    }

    private static string Escape(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var character in value)
        {
            if (character is '\\' or '\'')
            {
                builder.Append('\\');
            }

            builder.Append(character);
        }

        return builder.ToString();
    }
}
