using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Forge.Generators.Tests;

internal static class GeneratorTestHelper
{
    public static CSharpCompilation CreateCompilation(string source)
    {
        var references = GetPlatformReferences()
            .Concat(
            [
                MetadataReference.CreateFromFile(typeof(Forge.Delta.GenerateDeltaAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Forge.Sync.GenerateSyncAttribute).Assembly.Location)
            ])
            .GroupBy(reference => reference.Display, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First());

        return CSharpCompilation.Create(
            assemblyName: "GeneratorTests",
            syntaxTrees: [CSharpSyntaxTree.ParseText(source)],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    public static ImmutableArray<Diagnostic> GetErrors(Compilation compilation)
    {
        return compilation.GetDiagnostics()
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToImmutableArray();
    }

    private static IEnumerable<MetadataReference> GetPlatformReferences()
    {
        var trustedPlatformAssemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
        if (string.IsNullOrWhiteSpace(trustedPlatformAssemblies))
        {
            throw new InvalidOperationException("Trusted platform assemblies are unavailable.");
        }

        return trustedPlatformAssemblies
            .Split(Path.PathSeparator)
            .Select(static path => MetadataReference.CreateFromFile(path));
    }
}