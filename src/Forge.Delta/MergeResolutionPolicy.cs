namespace Forge.Delta;

/// <summary>
/// Configures explicit conflict-resolution rules for semantic three-way merge analysis.
/// </summary>
public sealed class MergeResolutionPolicy
{
    private readonly Dictionary<string, Resolver> _resolvers = new(StringComparer.Ordinal);
    private Resolver? _defaultResolver;

    /// <summary>Resolves one conflict path in favor of the current branch.</summary>
    public MergeResolutionPolicy PreferCurrent(string path)
        => Set(path, MergeResolutionSource.Current, static conflict => conflict.Current);

    /// <summary>Resolves one conflict path in favor of the desired branch.</summary>
    public MergeResolutionPolicy PreferDesired(string path)
        => Set(path, MergeResolutionSource.Desired, static conflict => conflict.Desired);

    /// <summary>Resolves one conflict path back to the baseline value.</summary>
    public MergeResolutionPolicy PreferBaseline(string path)
        => Set(path, MergeResolutionSource.Baseline, static conflict => conflict.Baseline);

    /// <summary>Resolves one conflict path with application-owned logic.</summary>
    public MergeResolutionPolicy Resolve(string path, Func<MergeConflict, object?> resolver)
    {
        ArgumentNullException.ThrowIfNull(resolver);
        return Set(path, MergeResolutionSource.Custom, resolver);
    }

    /// <summary>Uses the current branch for conflicts without a path-specific rule.</summary>
    public MergeResolutionPolicy DefaultToCurrent()
        => SetDefault(MergeResolutionSource.Current, static conflict => conflict.Current);

    /// <summary>Uses the desired branch for conflicts without a path-specific rule.</summary>
    public MergeResolutionPolicy DefaultToDesired()
        => SetDefault(MergeResolutionSource.Desired, static conflict => conflict.Desired);

    /// <summary>Uses the baseline value for conflicts without a path-specific rule.</summary>
    public MergeResolutionPolicy DefaultToBaseline()
        => SetDefault(MergeResolutionSource.Baseline, static conflict => conflict.Baseline);

    internal bool TryResolve(MergeConflict conflict, out ResolvedMergeValue? value)
    {
        ArgumentNullException.ThrowIfNull(conflict);
        var resolver = _resolvers.TryGetValue(conflict.Path, out var configured)
            ? configured
            : _defaultResolver;
        if (resolver is null)
        {
            value = null;
            return false;
        }

        value = new ResolvedMergeValue(conflict.Path, resolver.Resolve(conflict), resolver.Source);
        return true;
    }

    private MergeResolutionPolicy Set(
        string path,
        MergeResolutionSource source,
        Func<MergeConflict, object?> resolver)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _resolvers[path] = new Resolver(source, resolver);
        return this;
    }

    private MergeResolutionPolicy SetDefault(
        MergeResolutionSource source,
        Func<MergeConflict, object?> resolver)
    {
        _defaultResolver = new Resolver(source, resolver);
        return this;
    }

    private sealed record Resolver(
        MergeResolutionSource Source,
        Func<MergeConflict, object?> Resolve);
}
