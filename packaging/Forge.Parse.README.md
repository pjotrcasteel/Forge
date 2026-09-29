# Forge.Parse 1.20.0

> Structured JSON expectation matching for dynamic .NET tests.

**Website:** https://pjotrcasteel.github.io/Forge/ · **NuGet:** https://www.nuget.org/packages/Forge.Parse · **Source:** https://github.com/pjotrcasteel/Forge

Forge.Parse makes JSON assertions readable when IDs, timestamps and relationships are created at runtime.

## Install

```bash
dotnet add package Forge.Parse --version 1.20.0
```

## Five-minute start

```csharp
using Forge.Parse;

JsonAssert.Matches(
    """
    {
      "id": "<Guid>",
      "state": "<OneOf:pending|active>",
      "reference": "<Regex:^ORD-[0-9]+$>",
      "createdAt": "<DateTimeOffset>"
    }
    """,
    actualJson);
```

Objects are partial by default: extra properties in the actual JSON do not create noise.

## Dynamic relationships

```json
{
  "id": "<Capture:serviceId>",
  "dependency": {
    "serviceId": "<Same:serviceId>"
  }
}
```

Use `<Present>` and `<Missing>` for property presence, and `<<Guid>>` when the literal string `<Guid>` is expected.

## Matching modes

```csharp
JsonMatchOptions.Partial;
JsonMatchOptions.Exact;
JsonMatchOptions.Unordered;
JsonMatchOptions.UnorderedSubset;
```

Target specific paths when needed:

```csharp
var options = JsonMatchOptions.Partial
    .Ignore("$.metadata.generatedAt")
    .UseUnorderedArrayMatching("$.items");
```

## Built-in expectations

Forge.Parse includes type, shape, string, numeric, array-count and relationship matchers such as:

`<Guid>`, `<NotEmptyGuid>`, `<String>`, `<Int>`, `<Number>`, `<Bool>`, `<DateTimeOffset>`, `<Uri>`, `<Regex:...>`, `<OneOf:a|b>`, `<GreaterThan:10>`, `<ArrayMinCount:1>`, `<Capture:name>`, `<Same:name>`.

Custom expectations can be implemented with `IValueMatcher` or `DelegateValueMatcher`.

## Forge family

- **Forge.Delta** — what changed inside this object?
- **Forge.Sync** — how does current state become desired state?
- **Forge.Parse** — does structured dynamic output satisfy this expectation?

Forge.Parse is test-oriented and does not own application behavior, persistence or execution.

.NET 10 · MIT.
