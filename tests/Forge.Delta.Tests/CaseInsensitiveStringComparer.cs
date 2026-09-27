namespace Forge.Delta.Tests;

internal readonly struct CaseInsensitiveStringComparer : IEqualityComparer<string>
{
    public bool Equals(string? x, string? y)
    {
        return string.Equals(x, y, StringComparison.OrdinalIgnoreCase);
    }

    public int GetHashCode(string obj)
    {
        return StringComparer.OrdinalIgnoreCase.GetHashCode(obj);
    }
}
