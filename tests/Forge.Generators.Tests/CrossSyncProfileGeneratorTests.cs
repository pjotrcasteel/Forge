using Forge.Sync.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Forge.Generators.Tests;

[TestClass]
public sealed class CrossSyncProfileGeneratorTests
{
    [TestMethod]
    public void Generator_WithProfile_ProducesExecutableCrossTypePlan()
    {
        const string source = """
            using Forge.Sync;

            namespace Demo;

            public sealed record Stored(string Id, string Name, int Quantity);
            public sealed record Requested(string Id, string DisplayName, int Quantity);

            [GenerateCrossSyncProfile(typeof(Stored), typeof(Requested))]
            [CrossSyncIdentity(nameof(Stored.Id), nameof(Requested.Id))]
            [CrossSyncMap(nameof(Stored.Name), nameof(Requested.DisplayName), Path = "Name")]
            [CrossSyncMap(nameof(Stored.Quantity), nameof(Requested.Quantity))]
            public partial class ItemProfile
            {
            }
            """;

        var compilation = GeneratorTestHelper.CreateCompilation(source);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new CrossSyncProfileGenerator().AsSourceGenerator());

        driver = driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out var diagnostics);

        Assert.AreEqual(0, diagnostics.Count(item => item.Severity == DiagnosticSeverity.Error));
        Assert.AreEqual(0, GeneratorTestHelper.GetErrors(outputCompilation).Length);
        var generated = driver.GetRunResult().Results[0].GeneratedSources.Single().SourceText.ToString();
        StringAssert.Contains(generated, "CrossSync.Plan(current, desired, definition)");
        StringAssert.Contains(generated, "new global::Forge.Delta.PropertyChange(\"Name\"");
    }

    [TestMethod]
    public void Generator_WhenIdentityTypesDiffer_ReportsDiagnostic()
    {
        const string source = """
            using Forge.Sync;

            namespace Demo;

            public sealed record Stored(int Id, string Name);
            public sealed record Requested(string Id, string Name);

            [GenerateCrossSyncProfile(typeof(Stored), typeof(Requested))]
            [CrossSyncIdentity(nameof(Stored.Id), nameof(Requested.Id))]
            public partial class ItemProfile
            {
            }
            """;

        var compilation = GeneratorTestHelper.CreateCompilation(source);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new CrossSyncProfileGenerator().AsSourceGenerator());
        driver = driver.RunGenerators(compilation);

        Assert.IsTrue(driver.GetRunResult().Diagnostics.Any(item => item.Id == "FORGECROSS002"));
    }
}
