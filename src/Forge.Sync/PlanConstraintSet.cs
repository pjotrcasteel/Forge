namespace Forge.Sync;

/// <summary>
/// Composable typed pre-execution constraints for any plan type.
/// </summary>
public sealed class PlanConstraintSet<TPlan>
{
    private readonly List<Constraint> _constraints = [];

    /// <summary>
    /// Requires a predicate to be true for the plan to be valid.
    /// </summary>
    public PlanConstraintSet<TPlan> Require(
        string code,
        string message,
        Func<TPlan, bool> predicate,
        string? path = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        ArgumentNullException.ThrowIfNull(predicate);
        _constraints.Add(new Constraint(code, message, path, predicate));
        return this;
    }

    /// <summary>
    /// Validates all constraints and returns every violation rather than failing fast.
    /// </summary>
    public PlanConstraintValidation Validate(TPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        var violations = new List<PlanConstraintViolation>();
        foreach (var constraint in _constraints)
        {
            if (!constraint.Predicate(plan))
            {
                violations.Add(new PlanConstraintViolation(constraint.Code, constraint.Message, constraint.Path));
            }
        }

        return new PlanConstraintValidation(violations);
    }

    private sealed record Constraint(
        string Code,
        string Message,
        string? Path,
        Func<TPlan, bool> Predicate);
}
