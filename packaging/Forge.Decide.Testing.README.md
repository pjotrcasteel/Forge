# Forge.Decide.Testing 1.20.0

> Explore strategy-space coverage, gaps and ambiguity without coupling to a test framework.

**Website:** https://pjotrcasteel.github.io/Forge/ · **NuGet:** https://www.nuget.org/packages/Forge.Decide.Testing · **Source:** https://github.com/pjotrcasteel/Forge

Forge.Decide.Testing is a thin, framework-neutral scenario layer over Forge.Decide. It evaluates named contexts in deterministic order and reports whether each context selects exactly one strategy, has no applicable strategy, or remains ambiguous under the configured policy.

## Install

```bash
dotnet add package Forge.Decide.Testing --version 1.20.0
```

## Explore a scenario matrix

```csharp
var scenarios = new[]
{
    new StrategyScenario<RequestContext>(
        "small-interactive",
        new RequestContext(Volume: 10, Interactive: true)),
    new StrategyScenario<RequestContext>(
        "large-batch",
        new RequestContext(Volume: 10_000, Interactive: false)),
};

var result = await StrategyScenarioMatrix.EvaluateAsync(
    space,
    scenarios,
    cancellationToken);

result.IsFullyResolved;
result.Uncovered;
result.Ambiguous;
result.Scenarios;
```

Every scenario retains its complete `StrategyComparison`, so a failed coverage assertion can explain which candidates were applicable or rejected and why.

## Why it is separate

`Forge.Decide` is production decision infrastructure. `Forge.Decide.Testing` is exploration tooling. Keeping them separate means applications that only need runtime decisions do not take on scenario APIs or a testing dependency.

The package itself depends on no test framework. Use the result with MSTest, xUnit, NUnit, Reqnroll, property-based generators, fuzzers, or your own tooling.

## Generated or exhaustive contexts

The matrix accepts any `IEnumerable<StrategyScenario<TContext>>`, so contexts may come from hand-written cases or generated combinatorial input. Evaluation is sequential and preserves input order; Forge does not introduce hidden concurrency or mutate the strategy space.

Typical checks include:

- every supported context selects exactly one strategy;
- forbidden combinations remain uncovered intentionally;
- no context is ambiguous under the configured policy;
- candidate explanations remain useful when a scenario fails;
- strategy coverage stays stable as new strategies are introduced.

.NET 10 · MIT.