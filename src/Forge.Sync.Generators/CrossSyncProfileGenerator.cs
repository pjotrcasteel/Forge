using System.Collections.Generic;
using System.Linq;
using System.Text;
using Forge.Generators.Shared;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Forge.Sync.Generators;

[Generator(LanguageNames.CSharp)]
public sealed class CrossSyncProfileGenerator : IIncrementalGenerator
{
    private const string ProfileAttributeName = "Forge.Sync.GenerateCrossSyncProfileAttribute";
    private const string IdentityAttributeName = "Forge.Sync.CrossSyncIdentityAttribute";
    private const string MapAttributeName = "Forge.Sync.CrossSyncMapAttribute";

    private static readonly DiagnosticDescriptor InvalidProfile = new(
        "FORGECROSS001",
        "Invalid cross-type profile",
        "CrossSync profile '{0}' must be a non-generic top-level partial class and declare at least one identity mapping",
        "Forge.Sync",
        DiagnosticSeverity.Error,
        true);

    private static readonly DiagnosticDescriptor InvalidProperty = new(
        "FORGECROSS002",
        "Invalid cross-type property mapping",
        "Property mapping '{0}' -> '{1}' on profile '{2}' is invalid or the property types differ",
        "Forge.Sync",
        DiagnosticSeverity.Error,
        true);

    private static readonly DiagnosticDescriptor IncompatibleIdentity = new(
        "FORGECROSS003",
        "Incompatible identity routes",
        "All identity routes on profile '{0}' must use the same property type",
        "Forge.Sync",
        DiagnosticSeverity.Error,
        true);

    private static readonly DiagnosticDescriptor InvalidComparer = new(
        "FORGECROSS004",
        "Invalid cross-type comparer",
        "Comparer '{0}' on profile '{1}' must implement IEqualityComparer<{2}> and have an accessible parameterless constructor",
        "Forge.Sync",
        DiagnosticSeverity.Error,
        true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var targets = context.SyntaxProvider.ForAttributeWithMetadataName(
            ProfileAttributeName,
            static (_, _) => true,
            static (generatorContext, _) => new ProfileTarget(
                (INamedTypeSymbol)generatorContext.TargetSymbol,
                generatorContext.Attributes[0].ConstructorArguments[0].Value as INamedTypeSymbol,
                generatorContext.Attributes[0].ConstructorArguments[1].Value as INamedTypeSymbol));

        context.RegisterSourceOutput(targets, static (productionContext, target) => Emit(productionContext, target));
    }

    private static void Emit(SourceProductionContext context, ProfileTarget target)
    {
        var profile = target.Profile;
        var currentType = target.CurrentType;
        var desiredType = target.DesiredType;
        var identities = ReadMappings(profile, IdentityAttributeName, true);
        var maps = ReadMappings(profile, MapAttributeName, false);

        if (currentType is null
            || desiredType is null
            || profile.Arity != 0
            || profile.ContainingType is not null
            || !IsPartial(profile)
            || identities.Count == 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(InvalidProfile, profile.Locations.FirstOrDefault(), profile.ToDisplayString()));
            return;
        }

        identities.Sort(static (left, right) => left.Order.CompareTo(right.Order));
        ITypeSymbol? identityType = null;
        foreach (var identity in identities)
        {
            var pair = ResolvePair(currentType, desiredType, identity);
            if (pair is null)
            {
                ReportInvalidProperty(context, profile, identity);
                return;
            }

            identity.Current = pair.Value.Current;
            identity.Desired = pair.Value.Desired;
            identityType ??= pair.Value.Current.Type;
            if (!SymbolEqualityComparer.Default.Equals(identityType, pair.Value.Current.Type))
            {
                context.ReportDiagnostic(Diagnostic.Create(IncompatibleIdentity, profile.Locations.FirstOrDefault(), profile.ToDisplayString()));
                return;
            }
        }

        foreach (var map in maps)
        {
            var pair = ResolvePair(currentType, desiredType, map);
            if (pair is null)
            {
                ReportInvalidProperty(context, profile, map);
                return;
            }

            map.Current = pair.Value.Current;
            map.Desired = pair.Value.Desired;
        }

        var identityComparer = identities.Select(static item => item.ComparerType).FirstOrDefault(static type => type is not null);
        if (identityComparer is not null && !ValidateComparer(identityComparer, identityType!, profile.ContainingAssembly))
        {
            ReportInvalidComparer(context, profile, identityComparer, identityType!);
            return;
        }

        if (identities.Any(item => item.ComparerType is not null && !SymbolEqualityComparer.Default.Equals(item.ComparerType, identityComparer)))
        {
            context.ReportDiagnostic(Diagnostic.Create(IncompatibleIdentity, profile.Locations.FirstOrDefault(), profile.ToDisplayString()));
            return;
        }

        foreach (var map in maps)
        {
            if (map.ComparerType is not null && !ValidateComparer(map.ComparerType, map.Current!.Type, profile.ContainingAssembly))
            {
                ReportInvalidComparer(context, profile, map.ComparerType, map.Current.Type);
                return;
            }
        }

        var source = Generate(target, identityType!, identities, maps, identityComparer);
        context.AddSource(
            SymbolFormatting.HintName(profile, ".CrossSyncProfile.g.cs"),
            Microsoft.CodeAnalysis.Text.SourceText.From(source, Encoding.UTF8));
    }

    private static string Generate(
        ProfileTarget target,
        ITypeSymbol identityType,
        IReadOnlyList<Mapping> identities,
        IReadOnlyList<Mapping> maps,
        INamedTypeSymbol? identityComparer)
    {
        var profile = target.Profile;
        var currentType = target.CurrentType!;
        var desiredType = target.DesiredType!;
        var builder = new StringBuilder();
        builder.AppendLine("// <auto-generated />");
        builder.AppendLine("#nullable enable");
        var ns = SymbolFormatting.Namespace(profile);
        if (!string.IsNullOrWhiteSpace(ns))
        {
            builder.Append("namespace ").Append(ns).AppendLine(";").AppendLine();
        }

        var accessibility = profile.DeclaredAccessibility == Accessibility.Public ? "public" : "internal";
        var profileName = SymbolFormatting.EscapeIdentifier(profile.Name);
        var currentName = SymbolFormatting.TypeName(currentType);
        var desiredName = SymbolFormatting.TypeName(desiredType);
        var keyName = SymbolFormatting.TypeName(identityType);
        builder.Append(accessibility).Append(" partial class ").Append(profileName).AppendLine();
        builder.AppendLine("{");

        if (identityComparer is not null)
        {
            builder.Append("    private static readonly global::System.Collections.Generic.IEqualityComparer<")
                .Append(keyName).Append("> s_identityComparer = new ")
                .Append(SymbolFormatting.TypeName(identityComparer)).AppendLine("();");
            builder.AppendLine();
        }

        for (var index = 0; index < maps.Count; index++)
        {
            if (maps[index].ComparerType is null)
            {
                continue;
            }

            builder.Append("    private static readonly global::System.Collections.Generic.IEqualityComparer<")
                .Append(SymbolFormatting.TypeName(maps[index].Current!.Type)).Append("> s_map")
                .Append(index).Append("Comparer = new ")
                .Append(SymbolFormatting.TypeName(maps[index].ComparerType!)).AppendLine("();");
        }

        if (maps.Any(static item => item.ComparerType is not null))
        {
            builder.AppendLine();
        }

        EmitPlan(builder, profileName, currentName, desiredName, keyName, identityComparer is not null);
        EmitIdentity(builder, "Current", currentName, keyName, identities, true);
        EmitIdentity(builder, "Desired", desiredName, keyName, identities, false);
        EmitEquivalent(builder, currentName, desiredName, maps);
        EmitBetween(builder, currentName, desiredName, maps);
        builder.AppendLine("}");
        return builder.ToString();
    }

    private static void EmitPlan(
        StringBuilder builder,
        string profileName,
        string currentName,
        string desiredName,
        string keyName,
        bool hasIdentityComparer)
    {
        builder.AppendLine("    /// <summary>Reconciles complete desired state using the generated profile.</summary>");
        builder.Append("    public static global::Forge.Sync.CrossSyncPlan<").Append(currentName).Append(", ")
            .Append(desiredName).Append(", ").Append(keyName).AppendLine(", global::Forge.Sync.CrossTypeDelta> Plan(");
        builder.Append("        global::System.Collections.Generic.IReadOnlyList<").Append(currentName).AppendLine("> current,");
        builder.Append("        global::System.Collections.Generic.IReadOnlyList<").Append(desiredName).AppendLine("> desired)");
        builder.AppendLine("        => Plan(current, desired, global::Forge.Sync.SyncMode.Replace);");
        builder.AppendLine();
        builder.AppendLine("    /// <summary>Reconciles complete or partial desired state using the generated profile.</summary>");
        builder.Append("    public static global::Forge.Sync.CrossSyncPlan<").Append(currentName).Append(", ")
            .Append(desiredName).Append(", ").Append(keyName).AppendLine(", global::Forge.Sync.CrossTypeDelta> Plan(");
        builder.Append("        global::System.Collections.Generic.IReadOnlyList<").Append(currentName).AppendLine("> current,");
        builder.Append("        global::System.Collections.Generic.IReadOnlyList<").Append(desiredName).AppendLine("> desired,");
        builder.AppendLine("        global::Forge.Sync.SyncMode mode)");
        builder.AppendLine("    {");
        builder.Append("        var definition = new global::Forge.Sync.CrossSyncMatchDefinition<").Append(currentName).Append(", ")
            .Append(desiredName).Append(", ").Append(keyName).AppendLine(", global::Forge.Sync.CrossTypeDelta>(");
        builder.AppendLine("            GetCurrentIdentity,");
        builder.AppendLine("            GetDesiredIdentity,");
        builder.AppendLine("            AreEquivalent,");
        builder.AppendLine("            Between,");
        builder.AppendLine("            mode,");
        builder.AppendLine(hasIdentityComparer ? "            s_identityComparer);" : "            null);");
        builder.AppendLine("        return global::Forge.Sync.CrossSync.Plan(current, desired, definition);");
        builder.AppendLine("    }");
        builder.AppendLine();
    }

    private static void EmitIdentity(
        StringBuilder builder,
        string side,
        string typeName,
        string keyName,
        IReadOnlyList<Mapping> identities,
        bool current)
    {
        builder.Append("    private static global::Forge.Sync.SyncIdentity<").Append(keyName).Append("> Get")
            .Append(side).Append("Identity(").Append(typeName).AppendLine(" item)");
        builder.AppendLine("    {");
        var canonical = PropertyExpression("item", current ? identities[0].Current! : identities[0].Desired!);
        if (identities.Count == 1)
        {
            builder.Append("        return new global::Forge.Sync.SyncIdentity<").Append(keyName).Append(">(")
                .Append(canonical).AppendLine(");");
        }
        else
        {
            builder.Append("        return new global::Forge.Sync.SyncIdentity<").Append(keyName).Append(">(")
                .Append(canonical).AppendLine(",");
            builder.AppendLine("        [");
            for (var index = 1; index < identities.Count; index++)
            {
                var expression = PropertyExpression("item", current ? identities[index].Current! : identities[index].Desired!);
                builder.Append("            ").Append(expression).AppendLine(index == identities.Count - 1 ? string.Empty : ",");
            }
            builder.AppendLine("        ]);");
        }
        builder.AppendLine("    }");
        builder.AppendLine();
    }

    private static void EmitEquivalent(
        StringBuilder builder,
        string currentName,
        string desiredName,
        IReadOnlyList<Mapping> maps)
    {
        builder.Append("    public static bool AreEquivalent(").Append(currentName).Append(" current, ")
            .Append(desiredName).AppendLine(" desired)");
        if (maps.Count == 0)
        {
            builder.AppendLine("        => true;");
            builder.AppendLine();
            return;
        }

        builder.AppendLine("    {");
        for (var index = 0; index < maps.Count; index++)
        {
            var map = maps[index];
            var current = PropertyExpression("current", map.Current!);
            var desired = PropertyExpression("desired", map.Desired!);
            var comparison = map.ComparerType is null
                ? "global::System.Collections.Generic.EqualityComparer<" + SymbolFormatting.TypeName(map.Current!.Type) + ">.Default.Equals(" + current + ", " + desired + ")"
                : "s_map" + index + "Comparer.Equals(" + current + ", " + desired + ")";
            builder.Append("        if (!(").Append(comparison).AppendLine(") )");
            builder.AppendLine("        {");
            builder.AppendLine("            return false;");
            builder.AppendLine("        }");
        }
        builder.AppendLine("        return true;");
        builder.AppendLine("    }");
        builder.AppendLine();
    }

    private static void EmitBetween(
        StringBuilder builder,
        string currentName,
        string desiredName,
        IReadOnlyList<Mapping> maps)
    {
        builder.Append("    public static global::Forge.Sync.CrossTypeDelta Between(").Append(currentName).Append(" current, ")
            .Append(desiredName).AppendLine(" desired)");
        builder.AppendLine("    {");
        builder.AppendLine("        var changes = new global::System.Collections.Generic.List<global::Forge.Delta.PropertyChange>();");
        for (var index = 0; index < maps.Count; index++)
        {
            var map = maps[index];
            var current = PropertyExpression("current", map.Current!);
            var desired = PropertyExpression("desired", map.Desired!);
            var comparison = map.ComparerType is null
                ? "global::System.Collections.Generic.EqualityComparer<" + SymbolFormatting.TypeName(map.Current!.Type) + ">.Default.Equals(" + current + ", " + desired + ")"
                : "s_map" + index + "Comparer.Equals(" + current + ", " + desired + ")";
            var path = Escape(map.Path ?? map.Desired!.Name);
            builder.Append("        if (!(").Append(comparison).AppendLine(") )");
            builder.AppendLine("        {");
            builder.Append("            changes.Add(new global::Forge.Delta.PropertyChange(\"").Append(path).Append("\", ")
                .Append(current).Append(", ").Append(desired).AppendLine("));");
            builder.AppendLine("        }");
        }
        builder.AppendLine("        return new global::Forge.Sync.CrossTypeDelta(changes);");
        builder.AppendLine("    }");
    }

    private static List<Mapping> ReadMappings(INamedTypeSymbol profile, string metadataName, bool identity)
    {
        var result = new List<Mapping>();
        foreach (var attribute in profile.GetAttributes().Where(item => item.AttributeClass?.ToDisplayString() == metadataName))
        {
            var current = attribute.ConstructorArguments[0].Value as string ?? string.Empty;
            var desired = attribute.ConstructorArguments[1].Value as string ?? string.Empty;
            var order = identity && attribute.ConstructorArguments.Length > 2 && attribute.ConstructorArguments[2].Value is int value ? value : 0;
            INamedTypeSymbol? comparer = null;
            string? path = null;
            foreach (var named in attribute.NamedArguments)
            {
                if (named.Key == "ComparerType") comparer = named.Value.Value as INamedTypeSymbol;
                if (named.Key == "Path") path = named.Value.Value as string;
            }
            result.Add(new Mapping(current, desired, order, path, comparer));
        }
        return result;
    }

    private static (IPropertySymbol Current, IPropertySymbol Desired)? ResolvePair(
        INamedTypeSymbol currentType,
        INamedTypeSymbol desiredType,
        Mapping mapping)
    {
        var current = FindReadableProperty(currentType, mapping.CurrentName);
        var desired = FindReadableProperty(desiredType, mapping.DesiredName);
        if (current is null || desired is null || !SymbolEqualityComparer.Default.Equals(current.Type, desired.Type))
        {
            return null;
        }
        return (current, desired);
    }

    private static IPropertySymbol? FindReadableProperty(INamedTypeSymbol type, string name)
    {
        for (INamedTypeSymbol? current = type; current is not null && current.SpecialType != SpecialType.System_Object; current = current.BaseType)
        {
            var property = current.GetMembers(name).OfType<IPropertySymbol>().FirstOrDefault(static item =>
                !item.IsStatic && !item.IsIndexer && item.GetMethod is not null && item.DeclaredAccessibility == Accessibility.Public);
            if (property is not null) return property;
        }
        return null;
    }

    private static bool IsPartial(INamedTypeSymbol type)
        => type.DeclaringSyntaxReferences
            .Select(reference => reference.GetSyntax())
            .OfType<TypeDeclarationSyntax>()
            .Any(declaration => declaration.Modifiers.Any(SyntaxKind.PartialKeyword));

    private static bool ValidateComparer(INamedTypeSymbol comparer, ITypeSymbol comparedType, IAssemblySymbol assembly)
    {
        var implements = comparer.AllInterfaces.Any(interfaceType =>
            interfaceType.OriginalDefinition.Name == "IEqualityComparer"
            && interfaceType.OriginalDefinition.Arity == 1
            && interfaceType.OriginalDefinition.ContainingNamespace.ToDisplayString() == "System.Collections.Generic"
            && SymbolEqualityComparer.Default.Equals(interfaceType.TypeArguments[0], comparedType));
        if (!implements) return false;

        return comparer.InstanceConstructors.Any(ctor =>
            ctor.Parameters.Length == 0
            && (ctor.DeclaredAccessibility == Accessibility.Public
                || (ctor.DeclaredAccessibility == Accessibility.Internal
                    && SymbolEqualityComparer.Default.Equals(comparer.ContainingAssembly, assembly))));
    }

    private static void ReportInvalidProperty(SourceProductionContext context, INamedTypeSymbol profile, Mapping mapping)
        => context.ReportDiagnostic(Diagnostic.Create(
            InvalidProperty,
            profile.Locations.FirstOrDefault(),
            mapping.CurrentName,
            mapping.DesiredName,
            profile.ToDisplayString()));

    private static void ReportInvalidComparer(
        SourceProductionContext context,
        INamedTypeSymbol profile,
        INamedTypeSymbol comparer,
        ITypeSymbol propertyType)
        => context.ReportDiagnostic(Diagnostic.Create(
            InvalidComparer,
            profile.Locations.FirstOrDefault(),
            comparer.ToDisplayString(),
            profile.ToDisplayString(),
            propertyType.ToDisplayString()));

    private static string PropertyExpression(string instance, IPropertySymbol property)
        => instance + "." + SymbolFormatting.EscapeIdentifier(property.Name);

    private static string Escape(string value)
        => value.Replace("\\", "\\\\").Replace("\"", "\\\"");

    private sealed class ProfileTarget
    {
        public ProfileTarget(
            INamedTypeSymbol profile,
            INamedTypeSymbol? currentType,
            INamedTypeSymbol? desiredType)
        {
            Profile = profile;
            CurrentType = currentType;
            DesiredType = desiredType;
        }

        public INamedTypeSymbol Profile { get; }
        public INamedTypeSymbol? CurrentType { get; }
        public INamedTypeSymbol? DesiredType { get; }
    }

    private sealed class Mapping
    {
        public Mapping(string currentName, string desiredName, int order, string? path, INamedTypeSymbol? comparerType)
        {
            CurrentName = currentName;
            DesiredName = desiredName;
            Order = order;
            Path = path;
            ComparerType = comparerType;
        }

        public string CurrentName { get; }
        public string DesiredName { get; }
        public int Order { get; }
        public string? Path { get; }
        public INamedTypeSymbol? ComparerType { get; }
        public IPropertySymbol? Current { get; set; }
        public IPropertySymbol? Desired { get; set; }
    }
}
