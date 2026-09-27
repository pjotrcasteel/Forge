using System.Text.Json;

namespace Forge.Sync;

/// <summary>Describes one stale-plan precondition failure.</summary>
public sealed record ManifestPreconditionFailure(
    string Key,
    ManifestPreconditionFailureReason Reason,
    JsonElement Expected,
    JsonElement Actual);
