# Forge 1.0 release gates

Forge 1.0 is a capability release, not a relabelled preview. A public NuGet release should occur only after every executable CI gate below is green on .NET 10.

## Executable gates

1. `dotnet restore Forge.slnx`
2. `dotnet build Forge.slnx -c Release --no-restore`
3. `dotnet test Forge.slnx -c Release --no-build`
4. Native AOT publish of `Forge.Aot.Sample`
5. Pack `Forge.Delta` and `Forge.Sync` with SDK package validation enabled
6. Inspect `.nupkg` metadata/runtime/analyzer/XML-doc layout and `.snupkg` contents with `eng/verify_packages.py`
7. Restore/run a clean consumer that references only the packed `Forge.Delta`
8. Restore/run a separate clean consumer that references only the packed `Forge.Sync` and proves transitive Delta generation/runtime
9. Generate SHA-256 checksums and retain the exact `.nupkg` / `.snupkg` candidates as CI artifacts
10. Run the benchmark workflow and retain output as a release artifact; benchmarks inform claims but do not gate correctness

CI and the publish workflow encode the correctness gates above.

## Package compatibility after 1.0

The first stable package has no previous stable NuGet baseline. After `1.0.0` is published, every later stable release should use
SDK package validation against the latest previous stable package through `PackageValidationBaselineVersion`.

Breaking public API changes then require an explicit major-version decision rather than silently passing through packaging.

## Package ownership contract

- `Forge.Delta` has no NuGet package dependencies.
- `Forge.Sync` depends only on the matching `Forge.Delta` version.
- generators live under `analyzers/dotnet/cs`, never `lib`.
- runtime assemblies live under `lib/net10.0`, never `analyzers`.
- Roslyn implementation assemblies are not shipped.
- each package has its own focused NuGet README.
- both packages ship XML documentation and `.snupkg` symbols.
- official git builds publish repository metadata through Source Link support in the .NET SDK.

See `docs/NUGET.md` for the full package contract.

## Static gates performed while producing a source archive

- all project/solution references resolve;
- project/props/slnx XML parses;
- repository JSON and workflow YAML parse;
- public runtime API snapshots include the intended 1.0 surface;
- generated API contract source covers Delta merge/hash/invert and Sync manifest/compensation entry points;
- C# line-length and trailing-whitespace checks pass;
- public method/constructor signatures stay at six parameters or fewer;
- runtime source contains no IRMA/Fulfilment/TMF/SPIS domain leakage;
- package version is consistently `1.0.0`;
- package verification tooling itself parses/compiles;
- source archive integrity is checked after creation.

## Environment limitation

The artifact-generation environment used for this source archive does not contain the .NET SDK and cannot execute the .NET
gates locally. That limitation must never be converted into a claim that build/tests passed. The GitHub workflows are the
authoritative executable gate before publishing the packages.
