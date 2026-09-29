namespace Forge.ServiceProvisioning.Sample;

public static class Program
{
    public static void Main()
    {
        var result = ProvisioningScenario.Run();

        Console.WriteLine("FORGE DISTRIBUTED ROLLOUT SAMPLE");
        Console.WriteLine("=================================");
        Console.WriteLine();
        Console.WriteLine("1. DELTA + SYNC");
        Console.WriteLine($"Added: {result.AddedCount}");
        Console.WriteLine($"Updated: {result.UpdatedCount}");
        Console.WriteLine($"Removed: {result.RemovedCount}");
        Console.WriteLine($"Unchanged: {result.UnchangedCount}");
        Console.WriteLine($"API changes: {string.Join(", ", result.ApiChangePaths)}");
        Console.WriteLine();

        Console.WriteLine("2. DEPENDENCY WAVES");
        foreach (var wave in result.ExecutionWaves)
        {
            Console.WriteLine($"Wave {wave.Number}: {string.Join(", ", wave.Operations)}");
        }

        Console.WriteLine();
        Console.WriteLine("3. PORTABLE MANIFEST");
        Console.WriteLine($"Operations: {result.ManifestOperationCount}");
        Console.WriteLine($"SHA-256: {result.ManifestDigest}");
        Console.WriteLine(result.ManifestJson);
        Console.WriteLine();

        Console.WriteLine("4. STALE-PLAN PROTECTION");
        Console.WriteLine($"Original state accepted: {result.OriginalStateAccepted}");
        Console.WriteLine($"Changed state accepted: {result.ChangedStateAccepted}");
        Console.WriteLine($"Stale keys: {string.Join(", ", result.StaleKeys)}");
        Console.WriteLine();

        Console.WriteLine("5. EXECUTION-AWARE REPLAN");
        Console.WriteLine($"Completed work kept: {result.Replan.CompletedKept}");
        Console.WriteLine($"Running replacements requiring intervention: {result.Replan.RunningReplacementConflicts}");
        Console.WriteLine($"Pending operations cancelled safely: {result.Replan.PendingCancelled}");
        Console.WriteLine($"New operations planned: {result.Replan.NewlyPlanned}");
        Console.WriteLine($"Can proceed without intervention: {result.Replan.CanProceedWithoutIntervention}");
    }
}
