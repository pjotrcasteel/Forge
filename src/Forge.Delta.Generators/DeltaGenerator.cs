using System.Linq;
using Forge.Generators.Shared;
using Microsoft.CodeAnalysis;

namespace Forge.Delta.Generators;

[Generator(LanguageNames.CSharp)]
public sealed class DeltaGenerator : IIncrementalGenerator
{
    private const string GenerateDeltaAttributeName = "Forge.Delta.GenerateDeltaAttribute";
    private const string GenerateDeltaProfileAttributeName = "Forge.Delta.GenerateDeltaProfileAttribute";
    private const string DeltaProfileIgnoreAttributeName = "Forge.Delta.DeltaProfileIgnoreAttribute";
    private const string DeltaProfileComparerAttributeName = "Forge.Delta.DeltaProfileComparerAttribute";
    private const string GenerateSyncAttributeName = "Forge.Sync.GenerateSyncAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var targets = context.SyntaxProvider.ForAttributeWithMetadataName(
            GenerateDeltaAttributeName,
            static (_, _) => true,
            static (generatorContext, _) => (INamedTypeSymbol)generatorContext.TargetSymbol);

        context.RegisterSourceOutput(targets, static (productionContext, type) =>
        {
            if (HasAttribute(type, GenerateSyncAttributeName))
            {
                return;
            }

            if (type.Arity != 0 || type.ContainingType is not null || type.IsStatic || type.IsRefLikeType)
            {
                productionContext.ReportDiagnostic(
                    Diagnostic.Create(
                        DeltaDiagnostics.UnsupportedType,
                        type.Locations.Length == 0 ? null : type.Locations[0],
                        type.ToDisplayString()));
                return;
            }

            ReportDeltaConfiguration(productionContext, type, type.Name + "Delta", System.Array.Empty<IPropertySymbol>());
            if (!ValidatePropertyTypes(productionContext, type)
                || !ValidateComparers(productionContext, type, type.ContainingAssembly))
            {
                return;
            }

            productionContext.AddSource(
                SymbolFormatting.HintName(type, ".Delta.g.cs"),
                Microsoft.CodeAnalysis.Text.SourceText.From(
                    DeltaEmitter.Emit(type),
                    System.Text.Encoding.UTF8));
        });

        var profiles = context.SyntaxProvider.ForAttributeWithMetadataName(
            GenerateDeltaProfileAttributeName,
            static (_, _) => true,
            static (generatorContext, _) => new DeltaProfileTarget(
                (INamedTypeSymbol)generatorContext.TargetSymbol,
                generatorContext.Attributes[0].ConstructorArguments[0].Value as INamedTypeSymbol));

        context.RegisterSourceOutput(profiles, static (productionContext, profile) =>
        {
            var target = profile.TargetType;
            if (target is null
                || target.Arity != 0
                || target.ContainingType is not null
                || target.IsStatic
                || target.IsRefLikeType)
            {
                productionContext.ReportDiagnostic(
                    Diagnostic.Create(
                        DeltaDiagnostics.InvalidProfileTarget,
                        profile.ProfileType.Locations.FirstOrDefault(),
                        profile.ProfileType.ToDisplayString(),
                        target?.ToDisplayString() ?? "<unknown>"));
                return;
            }

            var configuration = ReadProfileConfiguration(productionContext, profile.ProfileType, target);
            if (configuration is null
                || !ValidatePropertyTypes(productionContext, target)
                || !ValidateComparers(productionContext, target, profile.ProfileType.ContainingAssembly))
            {
                return;
            }

            ReportDeltaConfiguration(
                productionContext,
                target,
                profile.ProfileType.Name + "Delta",
                configuration.IgnoredProperties);

            var accessibility = profile.ProfileType.DeclaredAccessibility == Accessibility.Public
                ? "public"
                : "internal";
            productionContext.AddSource(
                SymbolFormatting.HintName(profile.ProfileType, ".ProfileDelta.g.cs"),
                Microsoft.CodeAnalysis.Text.SourceText.From(
                    DeltaEmitter.Emit(
                        target,
                        configuration.IgnoredProperties,
                        profile.ProfileType.Name + "Delta",
                        SymbolFormatting.Namespace(profile.ProfileType),
                        accessibility,
                        configuration.ComparerOverrides),
                    System.Text.Encoding.UTF8));
        });
    }

    private sealed class DeltaProfileTarget
    {
        public DeltaProfileTarget(INamedTypeSymbol profileType, INamedTypeSymbol? targetType)
        {
            ProfileType = profileType;
            TargetType = targetType;
        }

        public INamedTypeSymbol ProfileType { get; }

        public INamedTypeSymbol? TargetType { get; }
    }


    private static void ReportDeltaConfiguration(
        SourceProductionContext context,
        INamedTypeSymbol targetType,
        string deltaName,
        IReadOnlyCollection<IPropertySymbol> additionallyIgnored)
    {
        var additionallyIgnoredNames = new HashSet<string>(
            additionallyIgnored.Select(static property => property.Name),
            System.StringComparer.Ordinal);
        var participatingCount = 0;
        foreach (var property in DeltaEmitter.GetAllProperties(targetType))
        {
            var ignored = DeltaEmitter.IsIgnored(property) || additionallyIgnoredNames.Contains(property.Name);
            if (!ignored)
            {
                participatingCount++;
                continue;
            }

            if (DeltaEmitter.IsIgnored(property) && DeltaEmitter.GetComparerType(property) is not null)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DeltaDiagnostics.ComparerOnIgnoredProperty,
                        property.Locations.FirstOrDefault(),
                        property.Name,
                        targetType.ToDisplayString()));
            }
        }

        if (participatingCount == 0)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DeltaDiagnostics.EmptyDelta,
                    targetType.Locations.FirstOrDefault(),
                    deltaName));
        }
    }

    private static ProfileConfiguration? ReadProfileConfiguration(
        SourceProductionContext context,
        INamedTypeSymbol profileType,
        INamedTypeSymbol targetType)
    {
        var properties = DeltaEmitter.GetProperties(targetType)
            .ToDictionary(static property => property.Name, System.StringComparer.Ordinal);
        var ignored = new Dictionary<string, IPropertySymbol>(System.StringComparer.Ordinal);
        var comparers = new Dictionary<string, INamedTypeSymbol>(System.StringComparer.Ordinal);
        var valid = true;

        foreach (var attribute in profileType.GetAttributes())
        {
            var metadataName = attribute.AttributeClass?.ToDisplayString();
            if (metadataName == DeltaProfileIgnoreAttributeName)
            {
                var propertyName = attribute.ConstructorArguments[0].Value as string;
                if (propertyName is null || !properties.TryGetValue(propertyName, out var property))
                {
                    ReportInvalidProfileProperty(context, profileType, targetType, propertyName);
                    valid = false;
                    continue;
                }

                if (ignored.ContainsKey(propertyName) || comparers.ContainsKey(propertyName))
                {
                    ReportConflictingProfileConfiguration(context, profileType, propertyName);
                    valid = false;
                    continue;
                }

                ignored.Add(propertyName, property);
                continue;
            }

            if (metadataName != DeltaProfileComparerAttributeName)
            {
                continue;
            }

            var comparerPropertyName = attribute.ConstructorArguments[0].Value as string;
            var comparerType = attribute.ConstructorArguments[1].Value as INamedTypeSymbol;
            if (comparerPropertyName is null || !properties.TryGetValue(comparerPropertyName, out var comparerProperty))
            {
                ReportInvalidProfileProperty(context, profileType, targetType, comparerPropertyName);
                valid = false;
                continue;
            }

            if (comparerType is null
                || !ImplementsEqualityComparer(comparerType, comparerProperty.Type))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DeltaDiagnostics.InvalidProfileComparer,
                        profileType.Locations.FirstOrDefault(),
                        comparerType?.ToDisplayString() ?? "<unknown>",
                        comparerProperty.Type.ToDisplayString(),
                        comparerPropertyName));
                valid = false;
                continue;
            }

            if (!CanCreateComparer(comparerType, profileType.ContainingAssembly))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DeltaDiagnostics.ProfileComparerCannotBeCreated,
                        profileType.Locations.FirstOrDefault(),
                        comparerType.ToDisplayString(),
                        comparerPropertyName));
                valid = false;
                continue;
            }

            if (ignored.ContainsKey(comparerPropertyName) || comparers.ContainsKey(comparerPropertyName))
            {
                ReportConflictingProfileConfiguration(context, profileType, comparerPropertyName);
                valid = false;
                continue;
            }

            comparers.Add(comparerPropertyName, comparerType);
        }

        return valid
            ? new ProfileConfiguration(ignored.Values.ToArray(), comparers)
            : null;
    }

    private static void ReportInvalidProfileProperty(
        SourceProductionContext context,
        INamedTypeSymbol profileType,
        INamedTypeSymbol targetType,
        string? propertyName)
    {
        context.ReportDiagnostic(
            Diagnostic.Create(
                DeltaDiagnostics.InvalidProfileProperty,
                profileType.Locations.FirstOrDefault(),
                profileType.ToDisplayString(),
                propertyName ?? "<unknown>",
                targetType.ToDisplayString()));
    }

    private static void ReportConflictingProfileConfiguration(
        SourceProductionContext context,
        INamedTypeSymbol profileType,
        string propertyName)
    {
        context.ReportDiagnostic(
            Diagnostic.Create(
                DeltaDiagnostics.ConflictingProfileConfiguration,
                profileType.Locations.FirstOrDefault(),
                profileType.ToDisplayString(),
                propertyName));
    }

    private sealed class ProfileConfiguration
    {
        public ProfileConfiguration(
            IReadOnlyCollection<IPropertySymbol> ignoredProperties,
            IReadOnlyDictionary<string, INamedTypeSymbol> comparerOverrides)
        {
            IgnoredProperties = ignoredProperties;
            ComparerOverrides = comparerOverrides;
        }

        public IReadOnlyCollection<IPropertySymbol> IgnoredProperties { get; }

        public IReadOnlyDictionary<string, INamedTypeSymbol> ComparerOverrides { get; }
    }

    private static bool ValidatePropertyTypes(SourceProductionContext context, INamedTypeSymbol type)
    {
        var valid = true;
        foreach (var property in DeltaEmitter.GetProperties(type))
        {
            if (!IsUnsupportedPropertyType(property.Type))
            {
                continue;
            }

            context.ReportDiagnostic(
                Diagnostic.Create(
                    DeltaDiagnostics.UnsupportedPropertyType,
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

    private static bool ValidateComparers(
        SourceProductionContext context,
        INamedTypeSymbol type,
        IAssemblySymbol generatedAssembly)
    {
        var valid = true;
        foreach (var property in DeltaEmitter.GetProperties(type))
        {
            var comparerType = DeltaEmitter.GetComparerType(property);
            if (comparerType is null)
            {
                continue;
            }

            if (!ImplementsEqualityComparer(comparerType, property.Type))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DeltaDiagnostics.InvalidComparer,
                        property.Locations.FirstOrDefault(),
                        comparerType.ToDisplayString(),
                        property.Type.ToDisplayString(),
                        property.Name));
                valid = false;
                continue;
            }

            if (!CanCreateComparer(comparerType, generatedAssembly))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DeltaDiagnostics.ComparerCannotBeCreated,
                        property.Locations.FirstOrDefault(),
                        comparerType.ToDisplayString(),
                        property.Name));
                valid = false;
            }
        }

        return valid;
    }

    private static bool ImplementsEqualityComparer(INamedTypeSymbol comparerType, ITypeSymbol propertyType)
    {
        return comparerType.AllInterfaces.Any(@interface =>
            @interface.OriginalDefinition.MetadataName == "IEqualityComparer`1"
            && @interface.OriginalDefinition.ContainingNamespace.ToDisplayString() == "System.Collections.Generic"
            && @interface.TypeArguments.Length == 1
            && SymbolEqualityComparer.Default.Equals(@interface.TypeArguments[0], propertyType));
    }

    private static bool CanCreateComparer(INamedTypeSymbol comparerType, IAssemblySymbol generatedAssembly)
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
            generatedAssembly);

        return comparerType.InstanceConstructors.Any(constructor =>
            constructor.Parameters.Length == 0
            && (constructor.DeclaredAccessibility == Accessibility.Public
                || (sameAssembly
                    && constructor.DeclaredAccessibility is Accessibility.Internal
                        or Accessibility.ProtectedOrInternal)));
    }

    private static bool HasAttribute(ISymbol symbol, string metadataName)
    {
        foreach (var attribute in symbol.GetAttributes())
        {
            if (string.Equals(
                    attribute.AttributeClass?.ToDisplayString(),
                    metadataName,
                    System.StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
