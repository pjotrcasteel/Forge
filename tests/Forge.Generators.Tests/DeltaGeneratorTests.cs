using System.Collections.Immutable;
using Forge.Delta.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Forge.Generators.Tests;

[TestClass]
public sealed class DeltaGeneratorTests
{
    [TestMethod]
    public void Generator_ProducesCompilableStronglyTypedDelta()
    {
        const string source = """
            using Forge.Delta;

            namespace Demo;

            [GenerateDelta]
            public sealed record Customer(string Name, string? Email);
            """;

        var (driver, outputCompilation, diagnostics) = RunGenerator(source);

        Assert.AreEqual(0, diagnostics.Count(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));
        Assert.AreEqual(0, GeneratorTestHelper.GetErrors(outputCompilation).Length);

        var runResult = driver.GetRunResult();
        Assert.AreEqual(1, runResult.Results.Length);
        Assert.AreEqual(1, runResult.Results[0].GeneratedSources.Length);
        StringAssert.Contains(
            runResult.Results[0].GeneratedSources[0].SourceText.ToString(),
            "NameChange");
    }

    [TestMethod]
    public void Generator_WhenNestedDeltaExists_ProducesNestedDeltaAndFlattenedPath()
    {
        const string source = """
            using Forge.Delta;

            namespace Demo;

            [GenerateDelta]
            public sealed record Address(string City, string Country);

            [GenerateDelta]
            public sealed record Customer(string Name, Address Address);
            """;

        var (driver, outputCompilation, diagnostics) = RunGenerator(source);

        Assert.AreEqual(0, diagnostics.Count(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));
        Assert.AreEqual(0, GeneratorTestHelper.GetErrors(outputCompilation).Length);

        var customerSource = driver.GetRunResult()
            .Results[0]
            .GeneratedSources
            .Single(sourceResult => sourceResult.HintName.EndsWith("Customer.Delta.g.cs", StringComparison.Ordinal))
            .SourceText
            .ToString();

        StringAssert.Contains(customerSource, "global::Demo.AddressDelta? AddressDelta");
        StringAssert.Contains(customerSource, "\"Address.\" + nestedChange.Path");
        StringAssert.Contains(customerSource, "FromComparison");
    }

    [TestMethod]
    public void Generator_WhenCustomComparerIsValid_UsesStaticComparer()
    {
        const string source = """
            using System;
            using System.Collections.Generic;
            using Forge.Delta;

            namespace Demo;

            public readonly struct IgnoreCaseComparer : IEqualityComparer<string>
            {
                public bool Equals(string? x, string? y) =>
                    string.Equals(x, y, StringComparison.OrdinalIgnoreCase);

                public int GetHashCode(string obj) =>
                    StringComparer.OrdinalIgnoreCase.GetHashCode(obj);
            }

            [GenerateDelta]
            public sealed record Customer(
                [property: DeltaComparer(typeof(IgnoreCaseComparer))] string Name);
            """;

        var (driver, outputCompilation, diagnostics) = RunGenerator(source);

        Assert.AreEqual(0, diagnostics.Count(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));
        Assert.AreEqual(0, GeneratorTestHelper.GetErrors(outputCompilation).Length);

        var generatedSource = driver.GetRunResult().Results[0].GeneratedSources.Single().SourceText.ToString();
        StringAssert.Contains(generatedSource, "s_property0Comparer");
        StringAssert.Contains(generatedSource, "new global::Demo.IgnoreCaseComparer()");
    }

    [TestMethod]
    public void Generator_WhenComparerHasWrongType_ReportsDiagnostic()
    {
        const string source = """
            using System.Collections.Generic;
            using Forge.Delta;

            namespace Demo;

            public readonly struct IntComparer : IEqualityComparer<int>
            {
                public bool Equals(int x, int y) => x == y;
                public int GetHashCode(int obj) => obj;
            }

            [GenerateDelta]
            public sealed record Customer(
                [property: DeltaComparer(typeof(IntComparer))] string Name);
            """;

        var (_, _, diagnostics) = RunGenerator(source);

        Assert.IsTrue(diagnostics.Any(diagnostic => diagnostic.Id == "FORGEDELTA002"));
    }

    [TestMethod]
    public void Generator_WhenComparerCannotBeConstructed_ReportsDiagnostic()
    {
        const string source = """
            using System.Collections.Generic;
            using Forge.Delta;

            namespace Demo;

            public abstract class NameComparer : IEqualityComparer<string>
            {
                public abstract bool Equals(string? x, string? y);
                public abstract int GetHashCode(string obj);
            }

            [GenerateDelta]
            public sealed record Customer(
                [property: DeltaComparer(typeof(NameComparer))] string Name);
            """;

        var (_, _, diagnostics) = RunGenerator(source);

        Assert.IsTrue(diagnostics.Any(diagnostic => diagnostic.Id == "FORGEDELTA003"));
    }


    [TestMethod]
    public void Generator_WhenPropertyIsRefLike_ReportsDiagnostic()
    {
        const string source = """
            using System;
            using Forge.Delta;

            namespace Demo;

            [GenerateDelta]
            public sealed class BufferState
            {
                public Span<byte> Buffer => default;
            }
            """;

        var (driver, _, _) = RunGenerator(source);

        Assert.IsTrue(driver.GetRunResult().Diagnostics.Any(diagnostic => diagnostic.Id == "FORGEDELTA004"));
    }

    private static (
        GeneratorDriver Driver,
        Compilation OutputCompilation,
        ImmutableArray<Diagnostic> Diagnostics) RunGenerator(string source)
    {
        var compilation = GeneratorTestHelper.CreateCompilation(source);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new DeltaGenerator().AsSourceGenerator());

        driver = driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out var generatorDiagnostics);

        return (driver, outputCompilation, generatorDiagnostics);
    }
    [TestMethod]
    public void Generator_ProfileGeneratesDeltaForUnannotatedType()
    {
        const string source = """
            using Forge.Delta;

            namespace Demo;

            public sealed record ExternalCustomer(int Id, string Name, string? Email);

            [GenerateDeltaProfile(typeof(ExternalCustomer))]
            public sealed class ExternalCustomerProfile
            {
            }
            """;

        var compilation = GeneratorTestHelper.CreateCompilation(source);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new DeltaGenerator().AsSourceGenerator());

        driver = driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var outputCompilation,
            out var generatorDiagnostics);

        Assert.AreEqual(0, generatorDiagnostics.Count(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));
        Assert.AreEqual(0, GeneratorTestHelper.GetErrors(outputCompilation).Length);

        var generated = driver.GetRunResult()
            .Results[0]
            .GeneratedSources
            .Single(sourceResult => sourceResult.HintName.EndsWith(".ProfileDelta.g.cs", StringComparison.Ordinal))
            .SourceText
            .ToString();

        StringAssert.Contains(generated, "public sealed class ExternalCustomerProfileDelta");
        StringAssert.Contains(generated, "global::Forge.Delta.ValueChange<int> IdChange");
        StringAssert.Contains(generated, "global::Forge.Delta.ValueChange<string> NameChange");
        StringAssert.Contains(generated, "Between(global::Demo.ExternalCustomer before, global::Demo.ExternalCustomer after)");
    }


    [TestMethod]
    public void Generator_ProfileWithUnknownConfiguredProperty_ReportsDiagnostic()
    {
        const string source = """
            using Forge.Delta;

            namespace Demo;

            public sealed record ExternalCustomer(int Id, string Name);

            [GenerateDeltaProfile(typeof(ExternalCustomer))]
            [DeltaProfileIgnore("Missing")]
            public sealed class ExternalCustomerProfile
            {
            }
            """;

        var compilation = GeneratorTestHelper.CreateCompilation(source);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new DeltaGenerator().AsSourceGenerator());

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);

        var diagnostic = driver.GetRunResult().Diagnostics.Single(item => item.Id == "FORGEDELTA006");
        Assert.AreEqual(DiagnosticSeverity.Error, diagnostic.Severity);
    }


    [TestMethod]
    public void Generator_WhenAllPropertiesAreIgnored_ReportsEmptyDeltaWarning()
    {
        const string source = """
            using Forge.Delta;

            namespace Demo;

            [GenerateDelta]
            public sealed record Item(
                [property: DeltaIgnore] int Id);
            """;

        var compilation = GeneratorTestHelper.CreateCompilation(source);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new DeltaGenerator().AsSourceGenerator());

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);

        var diagnostic = driver.GetRunResult().Diagnostics.Single(item => item.Id == "FORGEDELTA011");
        Assert.AreEqual(DiagnosticSeverity.Warning, diagnostic.Severity);
    }


}
