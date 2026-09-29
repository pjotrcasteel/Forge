namespace Forge.Parse;

/// <summary>
/// Controls structural JSON matching behavior.
/// </summary>
public sealed record JsonMatchOptions
{
    /// <summary>
    /// Gets the default options. Objects are matched partially and arrays positionally.
    /// </summary>
    public static JsonMatchOptions Partial { get; } = new();

    /// <summary>
    /// Gets exact object and array matching options.
    /// </summary>
    public static JsonMatchOptions Exact { get; } = new()
    {
        AllowAdditionalProperties = false,
        AllowAdditionalArrayItems = false
    };

    /// <summary>
    /// Gets options for exact-length arrays where item order is ignored.
    /// </summary>
    public static JsonMatchOptions Unordered { get; } = new()
    {
        IgnoreArrayOrder = true
    };

    /// <summary>
    /// Gets options for matching expected array items against any subset of the actual array.
    /// </summary>
    public static JsonMatchOptions UnorderedSubset { get; } = new()
    {
        IgnoreArrayOrder = true,
        AllowAdditionalArrayItems = true
    };

    /// <summary>
    /// Gets a value indicating whether actual objects may contain properties not present in the expected object.
    /// </summary>
    public bool AllowAdditionalProperties { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether actual arrays may contain additional items.
    /// </summary>
    public bool AllowAdditionalArrayItems { get; init; }

    /// <summary>
    /// Gets a value indicating whether array items may match in any order.
    /// </summary>
    public bool IgnoreArrayOrder { get; init; }

    /// <summary>
    /// Gets the comparison used for JSON property names.
    /// </summary>
    public StringComparison PropertyNameComparison { get; init; } = StringComparison.Ordinal;

    /// <summary>
    /// Gets exact JSON paths that are ignored during matching.
    /// </summary>
    public IReadOnlyList<string> IgnoredPaths { get; init; } = [];

    /// <summary>
    /// Gets exact JSON paths where array order is ignored.
    /// </summary>
    public IReadOnlyList<string> UnorderedArrayPaths { get; init; } = [];

    /// <summary>
    /// Returns a copy that ignores the supplied exact JSON paths.
    /// </summary>
    public JsonMatchOptions Ignore(params string[] paths) =>
        this with { IgnoredPaths = AppendPaths(IgnoredPaths, paths) };

    /// <summary>
    /// Returns a copy that matches arrays at the supplied exact JSON paths without considering item order.
    /// </summary>
    public JsonMatchOptions UseUnorderedArrayMatching(params string[] paths) =>
        this with { UnorderedArrayPaths = AppendPaths(UnorderedArrayPaths, paths) };

    internal bool IsIgnored(string path) => IgnoredPaths.Contains(path, StringComparer.Ordinal);

    internal bool IsUnorderedArray(string path) =>
        IgnoreArrayOrder || UnorderedArrayPaths.Contains(path, StringComparer.Ordinal);

    private static IReadOnlyList<string> AppendPaths(IReadOnlyList<string> existing, string[] paths)
    {
        ArgumentNullException.ThrowIfNull(paths);

        if (paths.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException("JSON paths cannot be null or whitespace.", nameof(paths));
        }

        return existing
            .Concat(paths)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }
}
