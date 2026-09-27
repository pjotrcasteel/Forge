using Forge.Sync.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Forge.Generators.Tests;

[TestClass]
public sealed class SyncGeneratorTests
{
    [TestMethod]
    public void Generator_ProducesDeltaAndSyncWhenGenerateDeltaIsAbsent()
    {
        const string source = """
            using Forge.Sync;

            namespace Demo;

            [GenerateSync("Id")]
            public sealed record Item(int Id, string Name);
            """;

        var compilation = GeneratorTestHelper.CreateCompilation(source);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new SyncGenerator().AsSourceGenerator());

        driver = driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out var generatorDiagnostics);

        Assert.AreEqual(0, generatorDiagnostics.Count(d => d.Severity == DiagnosticSeverity.Error));
        Assert.AreEqual(0, GeneratorTestHelper.GetErrors(outputCompilation).Length);

        var generatedSources = driver.GetRunResult().Results[0].GeneratedSources;
        Assert.AreEqual(2, generatedSources.Length);
        Assert.IsTrue(generatedSources.Any(sourceResult => sourceResult.HintName.EndsWith(".Delta.g.cs", StringComparison.Ordinal)));
        Assert.IsTrue(generatedSources.Any(sourceResult => sourceResult.HintName.EndsWith(".Sync.g.cs", StringComparison.Ordinal)));
    }

    [TestMethod]
    public void Generator_WhenKeyDoesNotExist_ReportsDiagnostic()
    {
        const string source = """
            using Forge.Sync;

            namespace Demo;

            [GenerateSync("Missing")]
            public sealed record Item(int Id, string Name);
            """;

        var compilation = GeneratorTestHelper.CreateCompilation(source);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new SyncGenerator().AsSourceGenerator());

        driver = driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out _,
            out _);

        var diagnostic = driver.GetRunResult()
            .Diagnostics
            .Single(item => item.Id == "FORGESYNC003");

        Assert.AreEqual(DiagnosticSeverity.Error, diagnostic.Severity);
    }

    [TestMethod]
    public void Generator_WhenKeyIsConfiguredTwice_ReportsDiagnostic()
    {
        const string source = """
            using Forge.Sync;

            namespace Demo;

            [GenerateSync("Id", "Id")]
            public sealed record Item(int Id, string Name);
            """;

        var compilation = GeneratorTestHelper.CreateCompilation(source);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new SyncGenerator().AsSourceGenerator());

        driver = driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out _,
            out _);

        var diagnostic = driver.GetRunResult()
            .Diagnostics
            .Single(item => item.Id == "FORGESYNC004");

        Assert.AreEqual(DiagnosticSeverity.Error, diagnostic.Severity);
    }

    [TestMethod]
    public void Generator_WhenNestedSyncTypeExists_ProducesSemanticNestedDelta()
    {
        const string source = """
            using Forge.Sync;

            namespace Demo;

            [GenerateSync("Code")]
            public sealed record Configuration(string Code, string Value);

            [GenerateSync("Id")]
            public sealed record Item(int Id, Configuration Configuration);
            """;

        var compilation = GeneratorTestHelper.CreateCompilation(source);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new SyncGenerator().AsSourceGenerator());

        driver = driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out var generatorDiagnostics);

        Assert.AreEqual(0, generatorDiagnostics.Count(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));
        Assert.AreEqual(0, GeneratorTestHelper.GetErrors(outputCompilation).Length);

        var itemDelta = driver.GetRunResult()
            .Results[0]
            .GeneratedSources
            .Single(sourceResult => sourceResult.HintName.EndsWith("Item.Delta.g.cs", StringComparison.Ordinal))
            .SourceText
            .ToString();

        StringAssert.Contains(itemDelta, "global::Demo.ConfigurationDelta? ConfigurationDelta");
        StringAssert.Contains(itemDelta, "\"Configuration.\" + nestedChange.Path");
    }

    [TestMethod]
    public void Generator_WhenKeyIsInherited_UsesInheritedProperty()
    {
        const string source = """
            using Forge.Sync;

            namespace Demo;

            public abstract class Entity
            {
                protected Entity(int id) => Id = id;
                public int Id { get; }
            }

            [GenerateSync("Id")]
            public sealed class Item : Entity
            {
                public Item(int id, string name) : base(id) => Name = name;
                public string Name { get; }
            }
            """;

        var compilation = GeneratorTestHelper.CreateCompilation(source);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new SyncGenerator().AsSourceGenerator());

        driver = driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out var generatorDiagnostics);

        Assert.AreEqual(0, generatorDiagnostics.Count(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));
        Assert.AreEqual(0, GeneratorTestHelper.GetErrors(outputCompilation).Length);
    }

    [TestMethod]
    public void Generator_WithCustomKeyComparer_ProducesStronglyTypedKey()
    {
        const string source = """
            using System;
            using System.Collections.Generic;
            using Forge.Sync;

            namespace Demo;

            public sealed class IgnoreCaseComparer : IEqualityComparer<string>
            {
                public bool Equals(string? x, string? y) => StringComparer.OrdinalIgnoreCase.Equals(x, y);
                public int GetHashCode(string obj) => StringComparer.OrdinalIgnoreCase.GetHashCode(obj);
            }

            [GenerateSync("Id")]
            public sealed record Item(
                [property: SyncKeyComparer(typeof(IgnoreCaseComparer))] string Id,
                string Name);
            """;

        var compilation = GeneratorTestHelper.CreateCompilation(source);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new SyncGenerator().AsSourceGenerator());

        driver = driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out var generatorDiagnostics);

        Assert.AreEqual(0, generatorDiagnostics.Count(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));
        Assert.AreEqual(0, GeneratorTestHelper.GetErrors(outputCompilation).Length);

        var syncSource = driver.GetRunResult()
            .Results[0]
            .GeneratedSources
            .Single(sourceResult => sourceResult.HintName.EndsWith(".Sync.g.cs", StringComparison.Ordinal))
            .SourceText
            .ToString();

        StringAssert.Contains(syncSource, "public readonly struct Key : global::System.IEquatable<Key>");
        StringAssert.Contains(syncSource, "public static Key GetKey(global::Demo.Item item)");
        StringAssert.Contains(syncSource, "s_key0Comparer.Equals(Id, other.Id)");
    }

    [TestMethod]
    public void Generator_WhenKeyComparerDoesNotMatchPropertyType_ReportsDiagnostic()
    {
        const string source = """
            using System.Collections.Generic;
            using Forge.Sync;

            namespace Demo;

            public sealed class WrongComparer : IEqualityComparer<int>
            {
                public bool Equals(int x, int y) => x == y;
                public int GetHashCode(int obj) => obj;
            }

            [GenerateSync("Id")]
            public sealed record Item(
                [property: SyncKeyComparer(typeof(WrongComparer))] string Id,
                string Name);
            """;

        var compilation = GeneratorTestHelper.CreateCompilation(source);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new SyncGenerator().AsSourceGenerator());

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);

        var diagnostic = driver.GetRunResult()
            .Diagnostics
            .Single(item => item.Id == "FORGESYNC008");

        Assert.AreEqual(DiagnosticSeverity.Error, diagnostic.Severity);
    }

    [TestMethod]
    public void Generator_ExcludesSyncKeyPropertiesFromGeneratedDelta()
    {
        const string source = """
            using Forge.Sync;

            namespace Demo;

            [GenerateSync("Id")]
            public sealed record Item(int Id, string Name);
            """;

        var compilation = GeneratorTestHelper.CreateCompilation(source);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new SyncGenerator().AsSourceGenerator());

        driver = driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out var generatorDiagnostics);

        Assert.AreEqual(0, generatorDiagnostics.Count(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));
        Assert.AreEqual(0, GeneratorTestHelper.GetErrors(outputCompilation).Length);

        var deltaSource = driver.GetRunResult()
            .Results[0]
            .GeneratedSources
            .Single(sourceResult => sourceResult.HintName.EndsWith(".Delta.g.cs", StringComparison.Ordinal))
            .SourceText
            .ToString();

        Assert.IsFalse(deltaSource.Contains("IdChange", StringComparison.Ordinal));
        StringAssert.Contains(deltaSource, "NameChange");
    }

    [TestMethod]
    public void Generator_PlanUsesEquivalenceFastPathAndDesiredKeySet()
    {
        const string source = """
            using Forge.Sync;

            namespace Demo;

            [GenerateSync("Id")]
            public sealed record Item(int Id, string Name);
            """;

        var compilation = GeneratorTestHelper.CreateCompilation(source);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new SyncGenerator().AsSourceGenerator());

        driver = driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out var generatorDiagnostics);

        Assert.AreEqual(0, generatorDiagnostics.Count(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));
        Assert.AreEqual(0, GeneratorTestHelper.GetErrors(outputCompilation).Length);

        var syncSource = driver.GetRunResult()
            .Results[0]
            .GeneratedSources
            .Single(sourceResult => sourceResult.HintName.EndsWith(".Sync.g.cs", StringComparison.Ordinal))
            .SourceText
            .ToString();

        StringAssert.Contains(syncSource, "var desiredKeys = new global::System.Collections.Generic.HashSet<Key>");
        StringAssert.Contains(syncSource, "ItemDelta.AreEquivalent(currentItem, desiredItem)");
        Assert.IsFalse(syncSource.Contains("desiredByKey", StringComparison.Ordinal));
        Assert.AreEqual(
            1,
            syncSource.Split(
                "for (var index = 0; index < desired.Count; index++)",
                StringSplitOptions.None).Length - 1);
    }

    [TestMethod]
    public void Generator_WhenSyncKeyComparerIsOnNonKeyProperty_ReportsWarning()
    {
        const string source = """
            using System.Collections.Generic;
            using Forge.Sync;

            namespace Demo;

            public sealed class StringComparer : IEqualityComparer<string>
            {
                public bool Equals(string? x, string? y) => x == y;
                public int GetHashCode(string obj) => obj.GetHashCode();
            }

            [GenerateSync("Id")]
            public sealed record Item(
                string Id,
                [property: SyncKeyComparer(typeof(StringComparer))] string Name);
            """;

        var compilation = GeneratorTestHelper.CreateCompilation(source);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new SyncGenerator().AsSourceGenerator());

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);

        var diagnostic = driver.GetRunResult().Diagnostics.Single(item => item.Id == "FORGESYNC010");

        Assert.AreEqual(DiagnosticSeverity.Warning, diagnostic.Severity);
    }

    [TestMethod]
    public void Generator_WhenDeltaComparerIsOnSyncKey_ReportsWarning()
    {
        const string source = """
            using System.Collections.Generic;
            using Forge.Delta;
            using Forge.Sync;

            namespace Demo;

            public sealed class StringComparer : IEqualityComparer<string>
            {
                public bool Equals(string? x, string? y) => x == y;
                public int GetHashCode(string obj) => obj.GetHashCode();
            }

            [GenerateSync("Id")]
            public sealed record Item(
                [property: DeltaComparer(typeof(StringComparer))] string Id,
                string Name);
            """;

        var compilation = GeneratorTestHelper.CreateCompilation(source);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new SyncGenerator().AsSourceGenerator());

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);

        var diagnostic = driver.GetRunResult().Diagnostics.Single(item => item.Id == "FORGESYNC011");

        Assert.AreEqual(DiagnosticSeverity.Warning, diagnostic.Severity);
    }

    [TestMethod]
    public void Generator_WithNestedSyncCollection_ExcludesCollectionFromDeltaAndGeneratesChildPlanner()
    {
        const string source = """
            using System.Collections.Generic;
            using Forge.Sync;

            namespace Demo;

            [GenerateSync("Name")]
            public sealed record Characteristic(string Name, string? Value);

            [GenerateSync("Id")]
            public sealed record Service(
                string Id,
                [property: SyncNested] IReadOnlyList<Characteristic> Characteristics);
            """;

        var compilation = GeneratorTestHelper.CreateCompilation(source);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new SyncGenerator().AsSourceGenerator());

        driver = driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out var generatorDiagnostics);

        Assert.AreEqual(0, generatorDiagnostics.Count(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));
        Assert.AreEqual(0, GeneratorTestHelper.GetErrors(outputCompilation).Length);

        var sources = driver.GetRunResult().Results[0].GeneratedSources;
        var serviceDelta = sources
            .Single(sourceResult => sourceResult.HintName.EndsWith("Service.Delta.g.cs", StringComparison.Ordinal))
            .SourceText
            .ToString();
        var serviceSync = sources
            .Single(sourceResult => sourceResult.HintName.EndsWith("Service.Sync.g.cs", StringComparison.Ordinal))
            .SourceText
            .ToString();

        Assert.IsFalse(serviceDelta.Contains("CharacteristicsChange", StringComparison.Ordinal));
        StringAssert.Contains(serviceSync, "public static bool AreEquivalent(");
        StringAssert.Contains(serviceSync, "PlanCharacteristics(");
        StringAssert.Contains(serviceSync, "global::Demo.CharacteristicSync.AreEquivalent(currentItem.Characteristics");
        Assert.IsFalse(serviceSync.Contains("hasChanges |= global::Demo.CharacteristicSync.Plan(", StringComparison.Ordinal));
    }


    [TestMethod]
    public void Generator_WhenNestedCollectionIsAlsoAKey_ReportsDiagnostic()
    {
        const string source = """
            using System.Collections.Generic;
            using Forge.Sync;

            namespace Demo;

            [GenerateSync("Id")]
            public sealed record Child(string Id, string Value);

            [GenerateSync("Children")]
            public sealed record Parent(
                [property: SyncNested] IReadOnlyList<Child> Children);
            """;

        var compilation = GeneratorTestHelper.CreateCompilation(source);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new SyncGenerator().AsSourceGenerator());

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);

        var diagnostic = driver.GetRunResult().Diagnostics.Single(item => item.Id == "FORGESYNC016");
        Assert.AreEqual(DiagnosticSeverity.Error, diagnostic.Severity);
    }


}