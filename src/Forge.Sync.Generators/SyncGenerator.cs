using System.Collections.Generic;
using System.Linq;
using Forge.Generators.Shared;
using Microsoft.CodeAnalysis;

namespace Forge.Sync.Generators;

[Generator(LanguageNames.CSharp)]
public sealed class SyncGenerator : IIncrementalGenerator
{
    private const string GenerateSyncAttributeName = "Forge.Sync.GenerateSyncAttribute";
    private const string SyncKeyComparerAttributeName = "Forge.Sync.SyncKeyComparerAttribute";
    private const string SyncNestedAttributeName = "Forge.Sync.SyncNestedAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var targets = context.SyntaxProvider.ForAttributeWithMetadataName(
            GenerateSyncAttributeName,
            static (_, _) => true,
            static (generatorContext, _) => new SyncTarget(
                (INamedTypeSymbol)generatorContext.TargetSymbol,
                generatorContext.Attributes[0]));

        context.RegisterSourceOutput(targets, static (productionContext, target) =>
        {
            var type = target.Type;
            if (type.Arity != 0 || type.ContainingType is not null || type.IsStatic || type.IsRefLikeType)
            {
                productionContext.ReportDiagnostic(
                    Diagnostic.Create(
                        SyncDiagnostics.UnsupportedType,
                        type.Locations.Length == 0 ? null : type.Locations[0],
                        type.ToDisplayString()));
                return;
            }

            var keyNames = ReadKeyNames(target.Attribute);
            if (keyNames.Count == 0)
            {
                productionContext.ReportDiagnostic(
                    Diagnostic.Create(
                        SyncDiagnostics.MissingKey,
                        type.Locations.Length == 0 ? null : type.Locations[0],
                        type.ToDisplayString()));
                return;
            }

            var duplicateKeyName = keyNames
                .GroupBy(static keyName => keyName, System.StringComparer.Ordinal)
                .FirstOrDefault(static group => group.Count() > 1)
                ?.Key;

            if (duplicateKeyName is not null)
            {
                productionContext.ReportDiagnostic(
                    Diagnostic.Create(
                        SyncDiagnostics.DuplicateKeyProperty,
                        type.Locations.Length == 0 ? null : type.Locations[0],
                        duplicateKeyName,
                        type.ToDisplayString()));
                return;
            }

            var keyProperties = new List<IPropertySymbol>(keyNames.Count);
            foreach (var keyName in keyNames)
            {
                var property = FindPublicReadableProperty(type, keyName);
                if (property is null)
                {
                    productionContext.ReportDiagnostic(
                        Diagnostic.Create(
                            SyncDiagnostics.InvalidKey,
                            type.Locations.Length == 0 ? null : type.Locations[0],
                            keyName,
                            type.ToDisplayString()));
                    return;
                }

                keyProperties.Add(property);
            }

            var keyComparerTypes = keyProperties
                .Select(GetKeyComparerType)
                .ToArray();
            var nestedProperties = GetNestedSyncProperties(productionContext, type);
            if (nestedProperties is null
                || !ValidateNestedKeyOverlap(productionContext, type, keyProperties, nestedProperties))
            {
                return;
            }

            ReportIneffectiveConfiguration(productionContext, type, keyProperties, nestedProperties);

            var deltaExcludedProperties = keyProperties
                .Concat(nestedProperties.Select(static nested => nested.Property))
                .ToArray();
            if (!ValidateDeltaPropertyTypes(productionContext, type, deltaExcludedProperties)
                || !ValidateKeyPropertyTypes(productionContext, type, keyProperties)
                || !ValidateDeltaComparers(productionContext, type, deltaExcludedProperties)
                || !ValidateKeyComparers(productionContext, keyProperties, keyComparerTypes))
            {
                return;
            }

            productionContext.AddSource(
                SymbolFormatting.HintName(type, ".Delta.g.cs"),
                Microsoft.CodeAnalysis.Text.SourceText.From(
                    DeltaEmitter.Emit(type, deltaExcludedProperties),
                    System.Text.Encoding.UTF8));

            productionContext.AddSource(
                SymbolFormatting.HintName(type, ".Sync.g.cs"),
                Microsoft.CodeAnalysis.Text.SourceText.From(
                    SyncEmitter.Emit(type, keyProperties, keyComparerTypes, nestedProperties),
                    System.Text.Encoding.UTF8));
        });
    }

    private static IPropertySymbol? FindPublicReadableProperty(INamedTypeSymbol type, string propertyName)
    {
        for (INamedTypeSymbol? current = type;
             current is not null && current.SpecialType != SpecialType.System_Object;
             current = current.BaseType)
        {
            var property = current.GetMembers(propertyName)
                .OfType<IPropertySymbol>()
                .FirstOrDefault(static candidate =>
                    !candidate.IsStatic
                    && !candidate.IsIndexer
                    && candidate.DeclaredAccessibility == Accessibility.Public
                    && candidate.GetMethod is not null);

            if (property is not null)
            {
                return property;
            }
        }

        return null;
    }

    private static bool ValidateNestedKeyOverlap(
        SourceProductionContext context,
        INamedTypeSymbol type,
        IReadOnlyList<IPropertySymbol> keyProperties,
        IReadOnlyList<NestedSyncProperty> nestedProperties)
    {
        var keyNames = new HashSet<string>(
            keyProperties.Select(static property => property.Name),
            System.StringComparer.Ordinal);
        var valid = true;
        foreach (var nested in nestedProperties)
        {
            if (!keyNames.Contains(nested.Property.Name))
            {
                continue;
            }

            context.ReportDiagnostic(
                Diagnostic.Create(
                    SyncDiagnostics.NestedSyncPropertyIsKey,
                    nested.Property.Locations.FirstOrDefault(),
                    nested.Property.Name,
                    type.ToDisplayString()));
            valid = false;
        }

        return valid;
    }

    private static void ReportIneffectiveConfiguration(
        SourceProductionContext context,
        INamedTypeSymbol type,
        IReadOnlyList<IPropertySymbol> keyProperties,
        IReadOnlyList<NestedSyncProperty> nestedProperties)
    {
        var keyNames = new HashSet<string>(
            keyProperties.Select(static property => property.Name),
            System.StringComparer.Ordinal);
        var nestedNames = new HashSet<string>(
            nestedProperties.Select(static nested => nested.Property.Name),
            System.StringComparer.Ordinal);

        foreach (var property in GetPublicReadableProperties(type))
        {
            var isKey = keyNames.Contains(property.Name);
            if (!isKey && FindAttribute(property, SyncKeyComparerAttributeName) is not null)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        SyncDiagnostics.KeyComparerOnNonKeyProperty,
                        property.Locations.FirstOrDefault(),
                        property.Name,
                        type.ToDisplayString()));
            }

            if (isKey && DeltaEmitter.GetComparerType(property) is not null)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        SyncDiagnostics.DeltaComparerOnKeyProperty,
                        property.Locations.FirstOrDefault(),
                        property.Name,
                        type.ToDisplayString()));
            }

            if (nestedNames.Contains(property.Name) && DeltaEmitter.GetComparerType(property) is not null)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        SyncDiagnostics.DeltaComparerOnNestedSyncProperty,
                        property.Locations.FirstOrDefault(),
                        property.Name,
                        type.ToDisplayString()));
            }
        }
    }

    private static IReadOnlyList<IPropertySymbol> GetPublicReadableProperties(INamedTypeSymbol type)
    {
        var result = new Dictionary<string, IPropertySymbol>(System.StringComparer.Ordinal);
        var hierarchy = new Stack<INamedTypeSymbol>();
        for (INamedTypeSymbol? current = type;
             current is not null && current.SpecialType != SpecialType.System_Object;
             current = current.BaseType)
        {
            hierarchy.Push(current);
        }

        while (hierarchy.Count != 0)
        {
            var current = hierarchy.Pop();
            foreach (var property in current.GetMembers().OfType<IPropertySymbol>())
            {
                if (property.IsStatic
                    || property.IsIndexer
                    || property.DeclaredAccessibility != Accessibility.Public
                    || property.GetMethod is null)
                {
                    continue;
                }

                result[property.Name] = property;
            }
        }

        return result.Values.ToArray();
    }

    private static IReadOnlyList<NestedSyncProperty>? GetNestedSyncProperties(
        SourceProductionContext context,
        INamedTypeSymbol type)
    {
        var result = new List<NestedSyncProperty>();
        var valid = true;
        foreach (var property in GetPublicReadableProperties(type))
        {
            var attribute = FindAttribute(property, SyncNestedAttributeName);
            if (attribute is null)
            {
                continue;
            }

            var mode = attribute.ConstructorArguments.Length == 0
                ? 0
                : (int)(attribute.ConstructorArguments[0].Value ?? 0);
            if (mode != 0 && mode != 1)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        SyncDiagnostics.InvalidNestedSyncMode,
                        property.Locations.FirstOrDefault(),
                        property.Name,
                        type.ToDisplayString(),
                        mode));
                valid = false;
                continue;
            }

            var itemType = GetReadOnlyListItemType(property.Type);
            if (itemType is null
                || property.NullableAnnotation == NullableAnnotation.Annotated
                || !HasAttribute(itemType, GenerateSyncAttributeName))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        SyncDiagnostics.InvalidNestedSyncProperty,
                        property.Locations.FirstOrDefault(),
                        property.Name,
                        type.ToDisplayString()));
                valid = false;
                continue;
            }

            result.Add(new NestedSyncProperty(property, itemType, mode));
        }

        return valid ? result : null;
    }

    private static INamedTypeSymbol? GetReadOnlyListItemType(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol namedType)
        {
            return null;
        }

        if (IsReadOnlyList(namedType))
        {
            return namedType.TypeArguments[0] as INamedTypeSymbol;
        }

        foreach (var @interface in namedType.AllInterfaces)
        {
            if (IsReadOnlyList(@interface))
            {
                return @interface.TypeArguments[0] as INamedTypeSymbol;
            }
        }

        return null;
    }

    private static bool IsReadOnlyList(INamedTypeSymbol type)
    {
        return type.OriginalDefinition.MetadataName == "IReadOnlyList`1"
               && type.OriginalDefinition.ContainingNamespace.ToDisplayString() == "System.Collections.Generic";
    }

    private static bool ValidateDeltaPropertyTypes(
        SourceProductionContext context,
        INamedTypeSymbol type,
        IReadOnlyList<IPropertySymbol> excludedProperties)
    {
        var excludedNames = new HashSet<string>(
            excludedProperties.Select(static property => property.Name),
            System.StringComparer.Ordinal);
        var valid = true;
        foreach (var property in DeltaEmitter.GetProperties(type))
        {
            if (excludedNames.Contains(property.Name) || !IsUnsupportedPropertyType(property.Type))
            {
                continue;
            }

            context.ReportDiagnostic(
                Diagnostic.Create(
                    SyncDiagnostics.UnsupportedDeltaPropertyType,
                    property.Locations.FirstOrDefault(),
                    property.Name,
                    type.ToDisplayString(),
                    property.Type.ToDisplayString()));
            valid = false;
        }

        return valid;
    }

    private static bool ValidateKeyPropertyTypes(
        SourceProductionContext context,
        INamedTypeSymbol type,
        IReadOnlyList<IPropertySymbol> keyProperties)
    {
        var valid = true;
        foreach (var property in keyProperties)
        {
            if (!IsUnsupportedPropertyType(property.Type))
            {
                continue;
            }

            context.ReportDiagnostic(
                Diagnostic.Create(
                    SyncDiagnostics.UnsupportedKeyPropertyType,
                    property.Locations.FirstOrDefault(),
                    property.Name,
                    type.ToDisplayString(),
                    property.Type.ToDisplayString()));
            valid = false;
        }

        return valid;
    }

    private static bool IsUnsupportedPropertyType(ITypeSymbol type)
    {
        return type.TypeKind is TypeKind.Pointer or TypeKind.FunctionPointer
               || type is INamedTypeSymbol { IsRefLikeType: true };
    }

    private static bool ValidateDeltaComparers(
        SourceProductionContext context,
        INamedTypeSymbol type,
        IReadOnlyList<IPropertySymbol> excludedProperties)
    {
        var excludedNames = new HashSet<string>(
            excludedProperties.Select(static property => property.Name),
            System.StringComparer.Ordinal);
        var valid = true;
        foreach (var property in DeltaEmitter.GetProperties(type))
        {
            if (excludedNames.Contains(property.Name))
            {
                continue;
            }

            var comparerType = DeltaEmitter.GetComparerType(property);
            if (comparerType is null)
            {
                continue;
            }

            if (!ImplementsEqualityComparer(comparerType, property.Type))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        SyncDiagnostics.InvalidDeltaComparer,
                        property.Locations.FirstOrDefault(),
                        comparerType.ToDisplayString(),
                        property.Type.ToDisplayString(),
                        property.Name));
                valid = false;
                continue;
            }

            if (!CanCreateComparer(comparerType, property))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        SyncDiagnostics.DeltaComparerCannotBeCreated,
                        property.Locations.FirstOrDefault(),
                        comparerType.ToDisplayString(),
                        property.Name));
                valid = false;
            }
        }

        return valid;
    }

    private static bool ValidateKeyComparers(
        SourceProductionContext context,
        IReadOnlyList<IPropertySymbol> keyProperties,
        IReadOnlyList<INamedTypeSymbol?> comparerTypes)
    {
        var valid = true;
        for (var index = 0; index < keyProperties.Count; index++)
        {
            var comparerType = comparerTypes[index];
            if (comparerType is null)
            {
                continue;
            }

            var property = keyProperties[index];
            if (!ImplementsEqualityComparer(comparerType, property.Type))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        SyncDiagnostics.InvalidKeyComparer,
                        property.Locations.FirstOrDefault(),
                        comparerType.ToDisplayString(),
                        property.Type.ToDisplayString(),
                        property.Name));
                valid = false;
                continue;
            }

            if (!CanCreateComparer(comparerType, property))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        SyncDiagnostics.KeyComparerCannotBeCreated,
                        property.Locations.FirstOrDefault(),
                        comparerType.ToDisplayString(),
                        property.Name));
                valid = false;
            }
        }

        return valid;
    }

    private static INamedTypeSymbol? GetKeyComparerType(IPropertySymbol property)
    {
        var attribute = FindAttribute(property, SyncKeyComparerAttributeName);
        if (attribute is null || attribute.ConstructorArguments.Length == 0)
        {
            return null;
        }

        return attribute.ConstructorArguments[0].Value as INamedTypeSymbol;
    }

    private static bool HasAttribute(ISymbol symbol, string metadataName)
    {
        return FindAttribute(symbol, metadataName) is not null;
    }

    private static AttributeData? FindAttribute(ISymbol symbol, string metadataName)
    {
        for (ISymbol? current = symbol; current is not null; current = GetOverriddenSymbol(current))
        {
            var attribute = current.GetAttributes().FirstOrDefault(candidate =>
                string.Equals(
                    candidate.AttributeClass?.ToDisplayString(),
                    metadataName,
                    System.StringComparison.Ordinal));

            if (attribute is not null)
            {
                return attribute;
            }
        }

        return null;
    }

    private static ISymbol? GetOverriddenSymbol(ISymbol symbol)
    {
        return symbol is IPropertySymbol property ? property.OverriddenProperty : null;
    }

    private static bool ImplementsEqualityComparer(INamedTypeSymbol comparerType, ITypeSymbol propertyType)
    {
        return comparerType.AllInterfaces.Any(@interface =>
            @interface.OriginalDefinition.MetadataName == "IEqualityComparer`1"
            && @interface.OriginalDefinition.ContainingNamespace.ToDisplayString() == "System.Collections.Generic"
            && @interface.TypeArguments.Length == 1
            && SymbolEqualityComparer.Default.Equals(@interface.TypeArguments[0], propertyType));
    }

    private static bool CanCreateComparer(INamedTypeSymbol comparerType, IPropertySymbol property)
    {
        if (comparerType.TypeKind == TypeKind.Struct)
        {
            return true;
        }

        if (comparerType.TypeKind != TypeKind.Class || comparerType.IsAbstract || comparerType.IsStatic)
        {
            return false;
        }

        var sameAssembly = SymbolEqualityComparer.Default.Equals(
            comparerType.ContainingAssembly,
            property.ContainingAssembly);

        return comparerType.InstanceConstructors.Any(constructor =>
            constructor.Parameters.Length == 0
            && (constructor.DeclaredAccessibility == Accessibility.Public
                || (sameAssembly
                    && constructor.DeclaredAccessibility is Accessibility.Internal
                        or Accessibility.ProtectedOrInternal)));
    }

    private static IReadOnlyList<string> ReadKeyNames(AttributeData attribute)
    {
        if (attribute.ConstructorArguments.Length == 0)
        {
            return System.Array.Empty<string>();
        }

        var argument = attribute.ConstructorArguments[0];
        if (argument.Kind != TypedConstantKind.Array)
        {
            return System.Array.Empty<string>();
        }

        return argument.Values
            .Where(static value => value.Value is string)
            .Select(static value => (string)value.Value!)
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .ToArray();
    }

    private sealed class SyncTarget
    {
        public SyncTarget(INamedTypeSymbol type, AttributeData attribute)
        {
            Type = type;
            Attribute = attribute;
        }

        public INamedTypeSymbol Type { get; }

        public AttributeData Attribute { get; }
    }
}
