using Microsoft.CodeAnalysis;

namespace Forge.Sync.Generators;

internal static class SyncDiagnostics
{
    public static readonly DiagnosticDescriptor UnsupportedType = new(
        id: "FORGESYNC001",
        title: "Unsupported sync target",
        messageFormat: "Type '{0}' must be a top-level, non-generic, non-static and non-ref-like class, record, struct or record struct",
        category: "Forge.Sync",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MissingKey = new(
        id: "FORGESYNC002",
        title: "Sync key is required",
        messageFormat: "Type '{0}' must declare at least one sync key property",
        category: "Forge.Sync",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidKey = new(
        id: "FORGESYNC003",
        title: "Invalid sync key property",
        messageFormat: "Property '{0}' on type '{1}' must be a public readable instance property",
        category: "Forge.Sync",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor DuplicateKeyProperty = new(
        id: "FORGESYNC004",
        title: "Duplicate sync key property",
        messageFormat: "Property '{0}' is specified more than once as a sync key on type '{1}'",
        category: "Forge.Sync",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidDeltaComparer = new(
        id: "FORGESYNC005",
        title: "Invalid delta comparer",
        messageFormat: "Comparer '{0}' must implement IEqualityComparer<{1}> for property '{2}'",
        category: "Forge.Sync",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor DeltaComparerCannotBeCreated = new(
        id: "FORGESYNC006",
        title: "Delta comparer cannot be created",
        messageFormat: "Comparer '{0}' for property '{1}' must be a non-abstract class with a public parameterless constructor or a struct",
        category: "Forge.Sync",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
    public static readonly DiagnosticDescriptor UnsupportedDeltaPropertyType = new(
        id: "FORGESYNC007",
        title: "Unsupported delta property type",
        messageFormat: "Property '{0}' on type '{1}' uses unsupported ref-like or pointer type '{2}'",
        category: "Forge.Sync",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);


    public static readonly DiagnosticDescriptor InvalidKeyComparer = new(
        id: "FORGESYNC008",
        title: "Invalid sync key comparer",
        messageFormat: "Comparer '{0}' must implement IEqualityComparer<{1}> for sync key property '{2}'",
        category: "Forge.Sync",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor KeyComparerCannotBeCreated = new(
        id: "FORGESYNC009",
        title: "Sync key comparer cannot be created",
        messageFormat: "Comparer '{0}' for sync key property '{1}' must be a non-abstract class with a public parameterless constructor or a struct",
        category: "Forge.Sync",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor KeyComparerOnNonKeyProperty = new(
        id: "FORGESYNC010",
        title: "Sync key comparer has no effect",
        messageFormat: "Property '{0}' on type '{1}' has SyncKeyComparer but is not part of the sync key",
        category: "Forge.Sync",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor DeltaComparerOnKeyProperty = new(
        id: "FORGESYNC011",
        title: "Delta comparer has no effect on sync identity",
        messageFormat: "Property '{0}' on type '{1}' is a sync key and is excluded from the generated Delta; use SyncKeyComparer to customize identity equality",
        category: "Forge.Sync",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor UnsupportedKeyPropertyType = new(
        id: "FORGESYNC012",
        title: "Unsupported sync key property type",
        messageFormat: "Sync key property '{0}' on type '{1}' uses unsupported ref-like or pointer type '{2}'",
        category: "Forge.Sync",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidNestedSyncProperty = new(
        id: "FORGESYNC013",
        title: "Invalid nested sync property",
        messageFormat: "Property '{0}' on type '{1}' marked SyncNested must be a non-null readable collection implementing IReadOnlyList<T> where T has GenerateSync",
        category: "Forge.Sync",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor DeltaComparerOnNestedSyncProperty = new(
        id: "FORGESYNC014",
        title: "Delta comparer has no effect on nested sync property",
        messageFormat: "Property '{0}' on type '{1}' is reconciled through SyncNested and is excluded from the generated parent Delta",
        category: "Forge.Sync",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidNestedSyncMode = new(
        id: "FORGESYNC015",
        title: "Invalid nested sync mode",
        messageFormat: "Property '{0}' on type '{1}' configures unsupported SyncMode value '{2}'",
        category: "Forge.Sync",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor NestedSyncPropertyIsKey = new(
        id: "FORGESYNC016",
        title: "Nested sync property cannot be a key",
        messageFormat: "Property '{0}' on type '{1}' cannot be both a Sync key and a SyncNested collection",
        category: "Forge.Sync",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

}
