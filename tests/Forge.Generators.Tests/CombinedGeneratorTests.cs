using Forge.Delta.Generators;
using Forge.Sync.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Forge.Generators.Tests;

[TestClass]
public sealed class CombinedGeneratorTests
{
    [TestMethod]
    public void Generators_WhenTypeHasBothAttributes_ProduceOneDeltaAndOneSync()
    {
        const string source = """
            using Forge.Delta;
            using Forge.Sync;

            namespace Demo;

            [GenerateDelta]
            [GenerateSync("Id")]
            public sealed record Item(int Id, string Name);
            """;

        var compilation = GeneratorTestHelper.CreateCompilation(source);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            new DeltaGenerator().AsSourceGenerator(),
            new SyncGenerator().AsSourceGenerator());

        driver = driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out var generatorDiagnostics);

        Assert.AreEqual(0, generatorDiagnostics.Count(d => d.Severity == DiagnosticSeverity.Error));
        Assert.AreEqual(0, GeneratorTestHelper.GetErrors(outputCompilation).Length);

        var generatedSources = driver.GetRunResult()
            .Results
            .SelectMany(result => result.GeneratedSources)
            .ToArray();

        Assert.AreEqual(2, generatedSources.Length);
        Assert.AreEqual(
            1,
            generatedSources.Count(sourceResult =>
                sourceResult.HintName.EndsWith(".Delta.g.cs", StringComparison.Ordinal)));
        Assert.AreEqual(
            1,
            generatedSources.Count(sourceResult =>
                sourceResult.HintName.EndsWith(".Sync.g.cs", StringComparison.Ordinal)));
    }
    [TestMethod]
    public void Generators_WhenSyncContainsDeltaOnlyNestedType_ProduceCompilableNestedDelta()
    {
        const string source = """
            using Forge.Delta;
            using Forge.Sync;

            namespace Demo;

            [GenerateDelta]
            public sealed record Configuration(string Mode);

            [GenerateSync("Id")]
            public sealed record Item(int Id, Configuration Configuration);
            """;

        var compilation = GeneratorTestHelper.CreateCompilation(source);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            new DeltaGenerator().AsSourceGenerator(),
            new SyncGenerator().AsSourceGenerator());

        driver = driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out var generatorDiagnostics);

        Assert.AreEqual(0, generatorDiagnostics.Count(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));
        Assert.AreEqual(0, GeneratorTestHelper.GetErrors(outputCompilation).Length);

        var generatedSources = driver.GetRunResult()
            .Results
            .SelectMany(result => result.GeneratedSources)
            .ToArray();

        Assert.AreEqual(3, generatedSources.Length);
        var itemDelta = generatedSources
            .Single(sourceResult => sourceResult.HintName.EndsWith("Item.Delta.g.cs", StringComparison.Ordinal))
            .SourceText
            .ToString();

        StringAssert.Contains(itemDelta, "global::Demo.ConfigurationDelta? ConfigurationDelta");
    }

}