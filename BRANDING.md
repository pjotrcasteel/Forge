# Branding checkpoint

Forge 1.0 uses the package and namespace family:

- `Forge.Delta`
- `Forge.Sync`

The codebase treats these names as the stable 1.x public API identity.

Immediately before the first public NuGet push, verify that both exact package IDs are still available and perform the normal trademark/name collision check. Package availability can change independently of this repository.

Future Forge packages are not admitted merely because they fit the name. A new package must solve a recurring production-code primitive with a small, obvious API and must remain useful outside any one application domain.
