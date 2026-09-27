namespace Forge.Delta.Tests;

[TestClass]
public sealed class ValueObjectDeltaTests
{
    [TestMethod]
    public void Between_ForStruct_ReturnsChanges()
    {
        var result = ValueObjectDelta.Between(
            new ValueObject(1, true),
            new ValueObject(2, true));

        Assert.IsTrue(result.HasChanges);
        Assert.IsTrue(result.NumberChange.HasChanged);
        Assert.IsFalse(result.EnabledChange.HasChanged);
    }
}
