namespace Forge.Sync;

/// <summary>Represents whether one readiness requirement is satisfied, without using null as a status sentinel.</summary>
public readonly struct ReadinessRequirementResult<TReason>
    where TReason : notnull
{
    private readonly TReason? _reason;

    private ReadinessRequirementResult(bool isSatisfied, TReason? reason)
    {
        IsSatisfied = isSatisfied;
        _reason = reason;
    }

    /// <summary>Gets whether the requirement is satisfied.</summary>
    public bool IsSatisfied { get; }

    /// <summary>Gets the typed reason when the requirement is blocked.</summary>
    public TReason Reason => !IsSatisfied
        ? _reason!
        : throw new InvalidOperationException("A satisfied readiness requirement has no blocking reason.");

    /// <summary>Creates a satisfied result.</summary>
    public static ReadinessRequirementResult<TReason> Satisfied() => new(true, default);

    /// <summary>Creates a blocked result with a typed application reason.</summary>
    public static ReadinessRequirementResult<TReason> Blocked(TReason reason)
    {
        ArgumentNullException.ThrowIfNull(reason);
        return new ReadinessRequirementResult<TReason>(false, reason);
    }
}
