using System.Text;

namespace Forge.Parse;

/// <summary>
/// Represents the result of a JSON match operation.
/// </summary>
public sealed class JsonMatchResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JsonMatchResult"/> class.
    /// </summary>
    /// <param name="mismatches">Detected mismatches.</param>
    public JsonMatchResult(IReadOnlyList<JsonMismatch> mismatches)
    {
        ArgumentNullException.ThrowIfNull(mismatches);
        Mismatches = mismatches;
    }

    /// <summary>
    /// Gets a value indicating whether the expected and actual JSON matched.
    /// </summary>
    public bool IsMatch => Mismatches.Count == 0;

    /// <summary>
    /// Gets the detected mismatches.
    /// </summary>
    public IReadOnlyList<JsonMismatch> Mismatches { get; }

    /// <summary>
    /// Gets the first mismatch, or <see langword="null"/> when matching succeeded.
    /// </summary>
    public JsonMismatch? FirstMismatch => Mismatches.Count == 0 ? null : Mismatches[0];

    /// <inheritdoc />
    public override string ToString()
    {
        if (IsMatch)
        {
            return "Forge.Parse match succeeded.";
        }

        var builder = new StringBuilder();
        builder.Append("Forge.Parse match failed with ")
            .Append(Mismatches.Count)
            .Append(Mismatches.Count == 1 ? " mismatch." : " mismatches.")
            .AppendLine();

        for (var index = 0; index < Mismatches.Count; index++)
        {
            var mismatch = Mismatches[index];
            builder.AppendLine();
            builder.Append('[').Append(index + 1).Append("] ").AppendLine(mismatch.Path);
            builder.Append("  Expected: ").AppendLine(mismatch.Expected);
            builder.Append("  Actual:   ").AppendLine(mismatch.Actual);
            builder.Append("  Reason:   ").Append(mismatch.Message);
        }

        return builder.ToString();
    }
}
