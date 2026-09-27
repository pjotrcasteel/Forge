namespace Forge.Sync;

/// <summary>
/// Immutable result of validating a plan against one or more constraints.
/// </summary>
public sealed class PlanConstraintValidation
{
    /// <summary>
    /// Creates a validation result.
    /// </summary>
    public PlanConstraintValidation(IReadOnlyList<PlanConstraintViolation> violations)
    {
        ArgumentNullException.ThrowIfNull(violations);
        Violations = Array.AsReadOnly(violations.ToArray());
    }

    public IReadOnlyList<PlanConstraintViolation> Violations { get; }
    public bool IsValid => Violations.Count == 0;
}
