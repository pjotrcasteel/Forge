# Performance

Forge is source-generator-first, but "generated" is not treated as a performance claim by itself. Performance work is measured against explicit workloads and should not change reconciliation semantics merely to improve a benchmark.

## Hot-path design

### Delta

Generated Delta types expose `AreEquivalent(before, after)` for callers that only need to know whether state changed. It performs generated comparisons without constructing a Delta or its detailed change collection. `Between` remains the detailed path when the actual changes are required.

### Sync

Generated Sync types expose:

```csharp
ItemSync.AreEquivalent(current, desired);
ItemSync.AreEquivalent(current, desired, SyncMode.Upsert);
```

The equivalence path validates logical-key uniqueness and compares matched items without constructing `SyncAddition`, `SyncUpdate`, `SyncRemoval`, `SyncUnchanged`, `SyncPreserved` or `SyncPlan` instances.

Explicit nested Sync boundaries recursively use the child `AreEquivalent` path. A parent therefore does not allocate a complete child plan merely to determine whether the parent belongs in `Updated`. A detailed nested plan is created only when application code asks for it through the generated `Plan<Property>` helper.

### Collection comparers

The reusable collection comparers use algorithms appropriate to their declared semantics:

- sequence equality: linear positional scan;
- unordered equality with multiplicity: frequency dictionary;
- set equality: set semantics;
- dictionary equality: indexed key/value comparison;
- keyed-list equality: logical-key indexes.

These helpers deliberately do not infer collection semantics. The consumer still chooses the comparer explicitly.

## Benchmarks

The BenchmarkDotNet project covers:

- manual vs generated Delta equivalence;
- detailed Delta creation;
- mostly-unchanged and mixed Sync plans;
- Sync equivalence without plan materialization;
- nested Sync equivalence and changed plans;
- unordered-list and dictionary comparer workloads.

Run locally with:

```bash
dotnet run --project benchmarks/Forge.Benchmarks/Forge.Benchmarks.csproj -c Release
```

The repository also contains a manually triggered `Benchmarks` GitHub Actions workflow. It runs on .NET 10 and uploads BenchmarkDotNet JSON and Markdown result artifacts.

## Benchmark claim policy

Forge does not publish timing, allocation, throughput or relative-performance claims until those numbers have been produced by a real .NET 10 runner using the checked-in benchmark suite. Results should include runtime, architecture and benchmark parameters.

The current development environment used for milestone 0.17 does not contain a .NET SDK and cannot download one, so no benchmark numbers are claimed for this milestone.
