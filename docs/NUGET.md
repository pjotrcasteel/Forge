# NuGet packaging contract

Forge ships as two public packages:

```text
Forge.Delta
Forge.Sync -> Forge.Delta
```

The package boundary is intentional.

## Forge.Delta

`Forge.Delta` is independently consumable and must have no NuGet package dependencies.

Its package contains:

```text
lib/net10.0/Forge.Delta.dll
lib/net10.0/Forge.Delta.xml
analyzers/dotnet/cs/Forge.Delta.Generators.dll
README.md
CHANGELOG.md
LICENSE
```

The Roslyn generator is an analyzer asset, not a runtime library, and Roslyn assemblies must never be embedded in the package.

## Forge.Sync

`Forge.Sync` depends only on the matching `Forge.Delta` package version.

Its package contains:

```text
lib/net10.0/Forge.Sync.dll
lib/net10.0/Forge.Sync.xml
analyzers/dotnet/cs/Forge.Sync.Generators.dll
README.md
CHANGELOG.md
LICENSE
```

Installing only `Forge.Sync` must make both generated Sync and generated Delta APIs available to the consumer through normal transitive package restore.

## Symbols and Source Link

Both packages produce `.snupkg` symbol packages. The .NET 10 SDK supplies Source Link support for common git providers.
Package projects set `PublishRepositoryUrl` and `EmbedUntrackedSources` so official git builds carry repository/commit
information and sources that cannot be linked remain debuggable.

`eng/verify_packages.py` requires the runtime PDB in each symbol package.

## Package validation

Both runtime package projects enable SDK package validation during `dotnet pack`.

For the first `1.0.0` release there is no previous public stable package to use as a baseline. After `1.0.0` is published, subsequent releases should set:

```xml
<PackageValidationBaselineVersion>1.0.0</PackageValidationBaselineVersion>
```

or the latest previous stable release, so binary/API breaks are detected during packing.

## Packed consumer tests

Repository project references are not sufficient proof that the packages are correct.

CI packs the NuGets first and then runs two isolated consumers using only `artifacts/packages` as their package source:

```text
Forge.Delta.PackageConsumer
    installs only Forge.Delta
    proves the Delta generator/runtime work from the .nupkg

Forge.PackageConsumer
    installs only Forge.Sync
    proves Sync generation
    proves Forge.Delta runtime + generator arrive transitively
```

Each consumer uses its own empty `NUGET_PACKAGES` directory so a developer/global cache cannot hide a broken package dependency or analyzer layout.

## Package inspection gate

`eng/verify_packages.py` validates:

- package ID and version;
- MIT license expression;
- package-specific README;
- runtime DLL + XML documentation;
- generator placement under `analyzers/dotnet/cs`;
- absence of Roslyn DLLs and source files;
- no generator DLL leaked into `lib`;
- `Forge.Delta` has no package dependencies;
- `Forge.Sync` depends only on the matching `Forge.Delta` version;
- `.snupkg` exists and contains the runtime portable PDB.

CI retains the produced `.nupkg`, `.snupkg`, and SHA-256 checksum file as an artifact so the exact candidate can be manually dogfooded before publication.
