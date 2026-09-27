using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Forge.Generators.Shared;

internal static class SymbolFormatting
{
    private static readonly SymbolDisplayFormat FullyQualifiedNullableFormat =
        SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(
            SymbolDisplayFormat.FullyQualifiedFormat.MiscellaneousOptions
            | SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    public static string TypeName(ITypeSymbol symbol)
    {
        return symbol.ToDisplayString(FullyQualifiedNullableFormat);
    }

    public static string EscapeIdentifier(string identifier)
    {
        return SyntaxFacts.GetKeywordKind(identifier) != SyntaxKind.None
            ? "@" + identifier
            : identifier;
    }

    public static string Namespace(INamedTypeSymbol symbol)
    {
        return symbol.ContainingNamespace.IsGlobalNamespace
            ? string.Empty
            : symbol.ContainingNamespace.ToDisplayString();
    }

    public static string HintName(INamedTypeSymbol symbol, string suffix)
    {
        var namespaceName = Namespace(symbol);
        return string.IsNullOrEmpty(namespaceName)
            ? symbol.Name + suffix
            : namespaceName + "." + symbol.Name + suffix;
    }
}