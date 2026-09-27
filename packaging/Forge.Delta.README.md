# Forge.Delta 1.20.0

> Source-generated, strongly typed semantic differences for .NET 10.
>
> **Website:** https://pjotrcasteel.github.io/Forge/ · **NuGet:** https://www.nuget.org/packages/Forge.Delta · **Source:** https://github.com/pjotrcasteel/Forge

Forge.Delta answers one question:

> **What changed inside this object?**

It generates comparison code at build time. Normal runtime comparison uses direct property access: no runtime reflection, dynamic proxies, hidden I/O, or mandatory DI.

## Install

```bash
dotnet add package Forge.Delta --version 1.20.0
```

## Five-minute start

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
    Console.WriteLine(
        $"{delta.EmailChange.Before} -> {delta.EmailChange.After}");
}
```

The generated API also provides:

```csharp
CustomerDelta.AreEquivalent(before, after);
CustomerDelta.GetSemanticHashCode(after);
CustomerDelta.AnalyzeMerge(baseline, current, desired);
delta.Invert();
```

## When Forge.Delta is useful

Use it when you need semantic change detection with explicit domain equality, typed property changes, nested generated state, merge analysis, inversion, or deterministic flattened change paths.

For plain value equality where `Equals` is already the complete domain answer, you probably do not need Forge.Delta.

## Explicit semantics

Forge supports:

- `[DeltaIgnore]` for bookkeeping state;
- `[DeltaComparer]` for domain-specific equality;
- nested generated Delta state with paths such as `Address.City`;
- external/unannotated models through `[GenerateDeltaProfile]`;
- explicit sequence, unordered, set, dictionary, and keyed-list collection comparers.

Forge does not recursively guess arbitrary object-graph or collection semantics.

## Three-way analysis

```csharp
var analysis = CustomerDelta.AnalyzeMerge(
    baseline,
    current,
    desired);

var resolved = MergeResolver.Resolve(
    analysis,
    new MergeResolutionPolicy()
        .PreferDesired("Email")
        .PreferCurrent("LastObservedAt"));
```

Non-conflicting branch changes can be combined while unresolved conflicts remain explicit. Forge does not mutate your domain object for you.

## When to use Forge.Sync

If your question is instead:

> **How do keyed current and desired collections differ?**

use `Forge.Sync`. It classifies Add / Update / Remove / Unchanged / Preserved and every generated update carries its Forge.Delta result.

## Runtime characteristics

- .NET 10;
- source-generated normal hot path;
- Native AOT and trimming friendly;
- no runtime reflection in generated comparison paths;
- no mandatory DI;
- no infrastructure dependency.

Full documentation: https://pjotrcasteel.github.io/Forge/
