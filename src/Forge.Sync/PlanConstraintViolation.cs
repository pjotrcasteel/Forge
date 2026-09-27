namespace Forge.Sync;

/// <summary>
/// Describes one failed pre-execution plan constraint.
/// </summary>
public sealed record PlanConstraintViolation(
    string Code,
    string Message,
    string? Path = null);
