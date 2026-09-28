using Forge.ServiceProvisioning.Sample;

namespace Forge.ServiceProvisioning.Sample.Tests;

[TestClass]
public sealed class ProvisioningScenarioTests
{
    [TestMethod]
    public void Run_ShouldProduceMeaningfulDeltaSyncAndDependencyWaves()
    {
        var result = ProvisioningScenario.Run();

        Assert.AreEqual(2, result.AddedCount);
        Assert.AreEqual(1, result.UpdatedCount);
        Assert.AreEqual(1, result.RemovedCount);
        Assert.AreEqual(1, result.UnchangedCount);
        CollectionAssert.Contains(result.RouterChangePaths.ToArray(), "Revision");
        CollectionAssert.Contains(result.RouterChangePaths.ToArray(), "Configuration");

        Assert.AreEqual(3, result.ExecutionWaves.Count);
        CollectionAssert.AreEqual(
            new[] { "Update:router", "Remove:legacy-vpn" },
            result.ExecutionWaves[0].Operations.ToArray());
        CollectionAssert.AreEqual(
            new[] { "Add:firewall" },
            result.ExecutionWaves[1].Operations.ToArray());
        CollectionAssert.AreEqual(
            new[] { "Add:monitoring" },
            result.ExecutionWaves[2].Operations.ToArray());
    }

    [TestMethod]
    public void Run_ShouldCreatePortableManifestAndRejectStaleCurrentState()
    {
        var result = ProvisioningScenario.Run();

        Assert.AreEqual(4, result.ManifestOperationCount);
        Assert.AreEqual(64, result.ManifestDigest.Length);
        Assert.IsTrue(result.ManifestJson.Contains("\"schemaVersion\"", StringComparison.Ordinal));
        Assert.IsTrue(result.OriginalStateAccepted);
        Assert.IsFalse(result.ChangedStateAccepted);
        CollectionAssert.AreEqual(new[] { "router" }, result.StaleKeys.ToArray());
    }

    [TestMethod]
    public void Run_ShouldRespectWorkThatAlreadyStartedDuringReplan()
    {
        var result = ProvisioningScenario.Run();

        Assert.AreEqual(1, result.Replan.CompletedKept);
        Assert.AreEqual(1, result.Replan.RunningReplacementConflicts);
        Assert.AreEqual(2, result.Replan.PendingCancelled);
        Assert.AreEqual(1, result.Replan.NewlyPlanned);
        Assert.IsFalse(result.Replan.CanProceedWithoutIntervention);
    }
}
