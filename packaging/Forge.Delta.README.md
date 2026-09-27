# Forge.Delta

**Compile-time, strongly typed state differences for .NET 10.**

Forge.Delta generates comparison code for your models at build time. Runtime comparison uses direct property access: no reflection, dynamic proxies, hidden I/O, or DI requirement.

## Install

```bash
dotnet add package Forge.Delta
```

## Quick start

```csharp
using Forge.Delta;

[GenerateDelta]
public sealed record Customer(
    Guid Id,
    string Name,
    string? Email);

var delta = CustomerDelta.Between(before, after);

if (delta.EmailChange.HasChanged)
{
    Console.WriteLine($"{delta.EmailChange.Before} -> {delta.EmailChange.After}");
}
```

The generated API also provides:

```csharp
CustomerDelta.AreEquivalent(before, after);
CustomerDelta.GetSemanticHashCode(after);
CustomerDelta.AnalyzeMerge(baseline, current, desired);
delta.Invert();
```

## Explicit semantics

Use `[DeltaIgnore]` for state that does not participate in comparison and `[DeltaComparer]` when domain equality differs from default equality.

Forge.Delta also supports nested generated deltas, external/unannotated models through `[GenerateDeltaProfile]`,
and explicit collection comparers for sequence, unordered, set, dictionary, and keyed-list semantics.

Forge does not recursively guess arbitrary object-graph or collection semantics.

## When to use Forge.Sync

Delta answers:

> What changed inside this object?

If you instead need to reconcile complete or partial keyed collections into
**Added / Updated / Removed / Unchanged**, use `Forge.Sync`. Every generated Sync update carries its generated Delta.

## Runtime characteristics

- .NET 10
- source-generated
- Native AOT and trimming friendly
- no runtime reflection in generated comparison paths
- no mandatory DI
- no infrastructure dependency

## Three-way resolution

`AnalyzeMerge` can be followed by explicit conflict resolution without mutating your domain object:

```csharp
var analysis = CustomerDelta.AnalyzeMerge(baseline, current, desired);
var resolved = MergeResolver.Resolve(
    analysis,
    new MergeResolutionPolicy()
        .PreferDesired("Email")
        .PreferCurrent("LastObservedAt"));
```

Non-conflicting branch changes are combined automatically. Conflicts without a configured policy remain explicit in `UnresolvedConflicts`.
