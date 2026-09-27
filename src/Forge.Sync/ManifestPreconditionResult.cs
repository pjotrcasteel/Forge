namespace Forge.Sync;

/// <summary>Describes whether a portable plan still applies to a supplied current-state snapshot.</summary>
public sealed class ManifestPreconditionResult
{
    public ManifestPreconditionResult(IReadOnlyList<ManifestPreconditionFailure> failures)
    {
        ArgumentNullException.ThrowIfNull(failures);
        Failures = Array.AsReadOnly(failures.ToArray());
    }

    public IReadOnlyList<ManifestPreconditionFailure> Failures { get; }
    public bool IsSatisfied => Failures.Count == 0;
}
