# Forge.Parse.Reqnroll 1.20.0

> Reqnroll DataTable integration for Forge.Parse.

**Website:** https://pjotrcasteel.github.io/Forge/ · **NuGet:** https://www.nuget.org/packages/Forge.Parse.Reqnroll · **Source:** https://github.com/pjotrcasteel/Forge

## Install

```bash
dotnet add package Forge.Parse.Reqnroll --version 1.20.0
```

The adapter is intentionally thin: Forge.Parse remains the matching engine.

```csharp
using Forge.Parse.Reqnroll;

var result = table.MatchJsonArray(actualJson);
```

Expanded columns can describe nested JSON:

```gherkin
| items[0].id | items[0].state |
| <Guid>      | active         |
```

```csharp
var expected = table.ToExpectedJsonObject(expandColumnPaths: true);
```

The package also exposes `MatchJsonObject`, `AssertMatchesJsonArray` and `AssertMatchesJsonObject`.

.NET 10 · MIT.
