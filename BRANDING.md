# Forge branding checkpoint

Forge follows the shared [PjotrCasteel Open Source Design DNA](docs/CREATOR_DESIGN_DNA.md).

This file defines only the Forge-specific identity and package-family rules. The creator-level principles, documentation philosophy, accessibility baseline, signature treatment and rules against shared-theme cloning live in the Design DNA.

## Forge family

Forge 1.x currently ships the package and namespace family:

- `Forge.Delta`
- `Forge.Sync`

`Forge.Parse` is reserved as a future Forge-family product name. It belongs inside the Forge identity and must not be presented as a separate standalone brand named ParseForge.

Forge family members share the Forge parent mark and blueprint / fabrication design language. Individual packages may use a restrained secondary symbol, accent or interaction motif, but they should remain immediately recognizable as Forge.

## Public API identity

The codebase treats the published Forge package names as stable 1.x public API identity.

Immediately before the first public NuGet push of any new Forge-family package, verify that the exact package ID is still available and perform the normal trademark/name collision check. Package availability can change independently of this repository.

Future Forge packages are not admitted merely because they fit the name. A new package must solve a recurring production-code primitive with a small, obvious API and must remain useful outside any one application domain.

## Forge-specific visual character

Forge should retain:

- blueprint / fabrication / explicit-transition metaphors;
- forged orange with steel/blue supporting accents;
- structural grids and technical drawing cues;
- interactions that expose a typed transition, transformation or planning result;
- the boundary that Forge calculates or interprets while application-owned code retains execution authority.

Forge should not borrow Causalia's failure-lab / causal-trace personality or a future GORM graph-visualization identity.
