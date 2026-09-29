namespace Forge.Parse;

/// <summary>
/// Describes a single mismatch between expected and actual JSON.
/// </summary>
/// <param name="Path">JSON path where the mismatch occurred.</param>
/// <param name="Expected">Expected representation.</param>
/// <param name="Actual">Actual representation.</param>
/// <param name="Message">Human-readable mismatch reason.</param>
public sealed record JsonMismatch(string Path, string Expected, string Actual, string Message);
