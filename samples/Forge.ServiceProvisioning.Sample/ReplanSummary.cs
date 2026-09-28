namespace Forge.ServiceProvisioning.Sample;

public sealed record ReplanSummary(
    int CompletedKept,
    int RunningReplacementConflicts,
    int PendingCancelled,
    int NewlyPlanned,
    bool CanProceedWithoutIntervention);
