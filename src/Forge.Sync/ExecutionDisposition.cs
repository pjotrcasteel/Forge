namespace Forge.Sync;

/// <summary>Classifies application execution state for safe replanning decisions.</summary>
public enum ExecutionDisposition
{
    NotStarted,
    Running,
    Completed
}
