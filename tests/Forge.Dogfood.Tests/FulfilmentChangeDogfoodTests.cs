namespace Forge.Dogfood.Tests;

[TestClass]
public sealed class FulfilmentChangeDogfoodTests
{
    [TestMethod]
    public void Plan_RecreatedEquivalentCharacteristics_DoNotCreateFalseUpdate()
    {
        IReadOnlyList<ResourceFacingService> current =
        [
            Service("access", "Access", Characteristic("bandwidth", "100"))
        ];
        IReadOnlyList<ResourceFacingService> desired =
        [
            Service("access", "Access", Characteristic("bandwidth", "100"))
        ];

        var plan = ResourceFacingServiceSync.Plan(current, desired);

        Assert.AreEqual(0, plan.Updated.Count);
        Assert.AreEqual(1, plan.Unchanged.Count);
    }

    [TestMethod]
    public void Plan_ChangeTopology_ProducesAddUpdateRemoveAndUnchanged()
    {
        IReadOnlyList<ResourceFacingService> current =
        [
            Service("access", "Access", Characteristic("bandwidth", "100")),
            Service("subnet", "IpSubnet", Characteristic("ipSubnet", "10.1.0.0/24")),
            Service("router", "Router", Characteristic("model", "A"))
        ];
        IReadOnlyList<ResourceFacingService> desired =
        [
            Service("access", "Access", Characteristic("bandwidth", "100")),
            Service("subnet", "IpSubnet", Characteristic("ipSubnet", "10.2.0.0/24")),
            Service("firewall", "Firewall", Characteristic("profile", "standard"))
        ];

        var plan = ResourceFacingServiceSync.Plan(current, desired);

        Assert.AreEqual("firewall", plan.Added.Single().Key.LogicalId);
        Assert.AreEqual("router", plan.Removed.Single().Key.LogicalId);
        var subnetUpdate = plan.Updated.Single();
        Assert.AreEqual("subnet", subnetUpdate.Key.LogicalId);
        Assert.IsFalse(subnetUpdate.Delta.HasChanges);

        var characteristicPlan = ResourceFacingServiceSync.PlanCharacteristics(subnetUpdate);
        Assert.AreEqual("ipSubnet", characteristicPlan.Updated.Single().Key.Name);
        Assert.IsTrue(characteristicPlan.Updated.Single().Delta.ValueChange.HasChanged);
        Assert.AreEqual("10.1.0.0/24", characteristicPlan.Updated.Single().Delta.ValueChange.Before);
        Assert.AreEqual("10.2.0.0/24", characteristicPlan.Updated.Single().Delta.ValueChange.After);
        Assert.AreEqual("access", plan.Unchanged.Single().Key.LogicalId);
    }

    [TestMethod]
    public void Plan_FullDelete_ProducesOnlyRemovals()
    {
        IReadOnlyList<ResourceFacingService> current =
        [
            Service("access", "Access"),
            Service("subnet", "IpSubnet")
        ];

        var plan = ResourceFacingServiceSync.Plan(current, Array.Empty<ResourceFacingService>());

        Assert.AreEqual(2, plan.Removed.Count);
        Assert.AreEqual(0, plan.Added.Count);
        Assert.AreEqual(0, plan.Updated.Count);
        Assert.AreEqual(0, plan.Unchanged.Count);
    }

    [TestMethod]
    public void Plan_PartialDelete_RetainsDesiredLogicalServices()
    {
        IReadOnlyList<ResourceFacingService> current =
        [
            Service("access", "Access"),
            Service("router", "Router"),
            Service("subnet", "IpSubnet")
        ];
        IReadOnlyList<ResourceFacingService> desired =
        [
            Service("access", "Access"),
            Service("subnet", "IpSubnet")
        ];

        var plan = ResourceFacingServiceSync.Plan(current, desired);

        Assert.AreEqual("router", plan.Removed.Single().Key.LogicalId);
        Assert.AreEqual(2, plan.Unchanged.Count);
        Assert.AreEqual(0, plan.Updated.Count);
    }

    private static ResourceFacingService Service(
        string logicalId,
        string serviceType,
        params ServiceCharacteristic[] characteristics)
    {
        return new ResourceFacingService(logicalId, serviceType, characteristics);
    }

    private static ServiceCharacteristic Characteristic(string name, string? value)
    {
        return new ServiceCharacteristic(name, value);
    }
}
