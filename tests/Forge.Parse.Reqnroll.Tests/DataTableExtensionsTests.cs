using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reqnroll;

namespace Forge.Parse.Reqnroll.Tests;

[TestClass]
public sealed class DataTableExtensionsTests
{
    [TestMethod]
    public void ToExpectedJsonObject_WithExpandedPaths_CreatesNestedObjectsAndArrays()
    {
        var table = new DataTable("service.id", "items[0].state", "quantity");
        table.AddRow("<Guid>", "active", "2");

        var result = table.ToExpectedJsonObject(expandColumnPaths: true);

        Assert.AreEqual("<Guid>", result["service"]!["id"]!.GetValue<string>());
        Assert.AreEqual("active", result["items"]![0]!["state"]!.GetValue<string>());
        Assert.AreEqual(2L, result["quantity"]!.GetValue<long>());
    }

    [TestMethod]
    public void MatchJsonObject_UsesCoreForgeParseMatchers()
    {
        var table = new DataTable("id", "state");
        table.AddRow("<Guid>", "active");

        var result = table.MatchJsonObject(
            """{ "id": "46d490c3-07d3-4fa7-a085-f898417bb280", "state": "active", "extra": true }""");

        Assert.IsTrue(result.IsMatch, result.ToString());
    }

    [TestMethod]
    public void AssertMatchesJsonObject_WhenMismatch_ThrowsStructuredException()
    {
        var table = new DataTable("id");
        table.AddRow("<Guid>");

        var exception = Assert.ThrowsExactly<JsonMatchAssertionException>(
            () => table.AssertMatchesJsonObject("""{ "id": "invalid" }"""));

        Assert.IsNotNull(exception.Result);
        Assert.AreEqual("$.id", exception.Result!.FirstMismatch!.Path);
    }
}
