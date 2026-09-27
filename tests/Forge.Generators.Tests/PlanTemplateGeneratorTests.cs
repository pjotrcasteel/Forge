using Forge.Sync.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Forge.Generators.Tests;

[TestClass]
public sealed class PlanTemplateGeneratorTests
{
    [TestMethod]
    public void Generator_WithTypedTemplate_ProducesInstantiateEntryPoint()
    {
        const string source = """
            using Forge.Sync;

            namespace Demo;

            public sealed record Context(bool Replace);
            public sealed record Operation(int Value);
            public readonly record struct OperationId(int Value);

            [GeneratePlanTemplate(typeof(Context), typeof(Operation), typeof(OperationId))]
            public partial class ChangeTemplate
            {
                private static partial void Build(Context context, PlanTemplateBuilder<Operation, OperationId> builder)
                {
                    builder.AddOperation(new OperationId(1), new Operation(context.Replace ? 2 : 1));
                }
            }
            """;

        var compilation = GeneratorTestHelper.CreateCompilation(source);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new PlanTemplateGenerator().AsSourceGenerator());
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        Assert.AreEqual(0, diagnostics.Count(item => item.Severity == DiagnosticSeverity.Error));
        Assert.AreEqual(0, GeneratorTestHelper.GetErrors(outputCompilation).Length);
        var generated = driver.GetRunResult().Results[0].GeneratedSources.Single().SourceText.ToString();
        StringAssert.Contains(generated, "PlanTemplateInstance");
        StringAssert.Contains(generated, "Instantiate(");
        StringAssert.Contains(generated, "Build(context, builder)");
    }

    [TestMethod]
    public void Generator_WithStaticTemplate_PreservesStaticPartialModifier()
    {
        const string source = """
            using Forge.Sync;

            public sealed record Context(int Value);
            public sealed record Operation(int Value);
            public readonly record struct OperationId(int Value);

            [GeneratePlanTemplate(typeof(Context), typeof(Operation), typeof(OperationId))]
            public static partial class StaticTemplate
            {
                private static partial void Build(Context context, PlanTemplateBuilder<Operation, OperationId> builder)
                {
                    builder.AddOperation(new OperationId(1), new Operation(context.Value));
                }
            }
            """;

        var compilation = GeneratorTestHelper.CreateCompilation(source);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new PlanTemplateGenerator().AsSourceGenerator());
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

        Assert.AreEqual(0, diagnostics.Count(item => item.Severity == DiagnosticSeverity.Error));
        Assert.AreEqual(0, GeneratorTestHelper.GetErrors(outputCompilation).Length);
        var generated = driver.GetRunResult().Results[0].GeneratedSources.Single().SourceText.ToString();
        StringAssert.Contains(generated, "public static partial class StaticTemplate");
    }

    [TestMethod]
    public void Generator_WhenTemplateIsNotPartial_ReportsDiagnostic()
    {
        const string source = """
            using Forge.Sync;

            [GeneratePlanTemplate(typeof(int), typeof(int), typeof(int))]
            public class InvalidTemplate
            {
            }
            """;

        var compilation = GeneratorTestHelper.CreateCompilation(source);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new PlanTemplateGenerator().AsSourceGenerator());
        driver = driver.RunGenerators(compilation);

        Assert.IsTrue(driver.GetRunResult().Diagnostics.Any(item => item.Id == "FORGETEMPLATE001"));
    }
}
