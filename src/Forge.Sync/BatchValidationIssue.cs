namespace Forge.Sync;

/// <summary>Describes an application validation failure for a composed reconciliation batch.</summary>
public sealed record BatchValidationIssue(string Code, string Message);
