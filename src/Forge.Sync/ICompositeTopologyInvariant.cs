namespace Forge.Sync;

/// <summary>Validates an application-defined composite topology transition and returns strongly typed violations.</summary>
public interface ICompositeTopologyInvariant<in TContext, TViolation>
{
    /// <summary>Validates one cross-topology invariant without mutating the transition.</summary>
    IReadOnlyList<TViolation> Validate(TContext context);
}
