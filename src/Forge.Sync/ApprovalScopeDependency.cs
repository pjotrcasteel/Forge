namespace Forge.Sync;

/// <summary>Represents an approval-order dependency between two application-defined scopes.</summary>
public readonly record struct ApprovalScopeDependency<TScope>(TScope Predecessor, TScope Successor)
    where TScope : notnull;
