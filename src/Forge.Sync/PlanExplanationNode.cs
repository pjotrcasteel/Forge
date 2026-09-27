namespace Forge.Sync;

/// <summary>
/// Represents one immutable explanation fact and optional child facts.
/// </summary>
[System.Diagnostics.DebuggerDisplay("{Code,nq}: {Summary,nq}")]
public sealed class PlanExplanationNode
{
    /// <summary>
    /// Initializes an explanation node.
    /// </summary>
    public PlanExplanationNode(
        string code,
        string summary,
        string? path = null,
        object? before = null,
        object? after = null,
        IReadOnlyList<PlanExplanationNode>? children = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(summary);
        Code = code;
        Summary = summary;
        Path = path;
        Before = before;
        After = after;
        Children = Array.AsReadOnly((children ?? []).ToArray());
    }

    /// <summary>Gets the stable machine-readable explanation code.</summary>
    public string Code { get; }

    /// <summary>Gets the human-readable explanation summary.</summary>
    public string Summary { get; }

    /// <summary>Gets the optional state path associated with this fact.</summary>
    public string? Path { get; }

    /// <summary>Gets the optional value before the transition.</summary>
    public object? Before { get; }

    /// <summary>Gets the optional value after the transition.</summary>
    public object? After { get; }

    /// <summary>Gets nested explanation facts.</summary>
    public IReadOnlyList<PlanExplanationNode> Children { get; }
}
