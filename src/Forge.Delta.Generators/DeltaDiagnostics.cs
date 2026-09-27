using Microsoft.CodeAnalysis;

namespace Forge.Delta.Generators;

internal static class DeltaDiagnostics
{
    public static readonly DiagnosticDescriptor UnsupportedType = new(
        id: "FORGEDELTA001",
        title: "Unsupported delta target",
        messageFormat: "Type '{0}' must be a top-level, non-generic, non-static and non-ref-like class, record, struct or record struct",
        category: "Forge.Delta",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidComparer = new(
        id: "FORGEDELTA002",
        title: "Invalid delta comparer",
        messageFormat: "Comparer '{0}' must implement IEqualityComparer<{1}> for property '{2}'",
        category: "Forge.Delta",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ComparerCannotBeCreated = new(
        id: "FORGEDELTA003",
        title: "Delta comparer cannot be created",
        messageFormat: "Comparer '{0}' for property '{1}' must be a non-abstract class with a public parameterless constructor or a struct",
        category: "Forge.Delta",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
    public static readonly DiagnosticDescriptor UnsupportedPropertyType = new(
        id: "FORGEDELTA004",
        title: "Unsupported delta property type",
        messageFormat: "Property '{0}' on type '{1}' uses unsupported ref-like or pointer type '{2}'",
        category: "Forge.Delta",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidProfileTarget = new(
        id: "FORGEDELTA005",
        title: "Unsupported delta profile target",
        messageFormat: "Profile '{0}' targets '{1}', which must be a top-level, non-generic, non-static and non-ref-like class, record, struct or record struct",
        category: "Forge.Delta",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidProfileProperty = new(
        id: "FORGEDELTA006",
        title: "Invalid delta profile property",
        messageFormat: "Profile '{0}' configures property '{1}', but target type '{2}' has no public readable property with that name",
        category: "Forge.Delta",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidProfileComparer = new(
        id: "FORGEDELTA007",
        title: "Invalid delta profile comparer",
        messageFormat: "Profile comparer '{0}' must implement IEqualityComparer<{1}> for target property '{2}'",
        category: "Forge.Delta",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ProfileComparerCannotBeCreated = new(
        id: "FORGEDELTA008",
        title: "Delta profile comparer cannot be created",
        messageFormat: "Profile comparer '{0}' for target property '{1}' must be constructible from the profile assembly",
        category: "Forge.Delta",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ConflictingProfileConfiguration = new(
        id: "FORGEDELTA009",
        title: "Conflicting delta profile configuration",
        messageFormat: "Profile '{0}' configures property '{1}' more than once or both ignores it and assigns a comparer",
        category: "Forge.Delta",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ComparerOnIgnoredProperty = new(
        id: "FORGEDELTA010",
        title: "Delta comparer has no effect",
        messageFormat: "Property '{0}' on type '{1}' has both DeltaIgnore and DeltaComparer; the comparer is ignored",
        category: "Forge.Delta",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor EmptyDelta = new(
        id: "FORGEDELTA011",
        title: "Delta has no participating properties",
        messageFormat: "Generated Delta '{0}' has no participating public readable properties and will always be empty",
        category: "Forge.Delta",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

}
