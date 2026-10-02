using System.Diagnostics;

namespace Forge.Decide.OpenTelemetry;

/// <summary>
/// Defines the ActivitySource used by Forge.Decide OpenTelemetry instrumentation.
/// </summary>
public static class ForgeDecideTelemetry
{
    /// <summary>
    /// ActivitySource name to register with an OpenTelemetry tracer provider.
    /// </summary>
    public const string ActivitySourceName = "Forge.Decide";

    internal static ActivitySource Source { get; } = new(ActivitySourceName);
}