# Forge 1.20.0

> Strongly typed state transitions, structured test expectations, and explainable decision planning for .NET 10.
>
> **Website:** https://pjotrcasteel.github.io/Forge/ · **Forge.Delta:** https://www.nuget.org/packages/Forge.Delta · **Forge.Sync:** https://www.nuget.org/packages/Forge.Sync · **Forge.Parse:** https://www.nuget.org/packages/Forge.Parse · **Forge.Decide:** https://www.nuget.org/packages/Forge.Decide · **Source:** https://github.com/pjotrcasteel/Forge

Forge is an umbrella for focused .NET libraries that solve different state, structured-data, and decision-planning problems without becoming an application framework.

Use **Forge.Delta** when the question is _“what changed inside this object?”_  
Use **Forge.Sync** when the question is _“how does the state I have become the state I want?”_  
Use **Forge.Parse** when the question is _“does this dynamic structured output satisfy the expectation I care about?”_  
Use **Forge.Decide** when the question is _“which valid course of action should become the plan, and why?”_

Normal generated Delta/Sync hot paths use direct property access and dictionaries. Forge.Decide uses explicit typed candidate spaces. Forge packages avoid runtime reflection, dynamic proxies, hidden I/O, and mandatory dependency injection on their normal paths.

<a id="why"></a>

## When Forge is useful

Choose the subproject that matches the problem. Forge is a strong fit when application correctness depends on one or more of these problems:

- semantic object change detection;
- desired-state reconciliation;
- complete replacement versus partial upsert semantics;
- cross-type current/desired models;
- dependency-aware create/update/delete ordering;
- graph or topology transitions;
- plan validation before side effects begin;
- selecting between several legitimate application-owned plans;
- restricting which strategies are allowed to compete for a problem;
- explaining why a strategy was applicable or rejected;
- stale-plan detection after a delay or approval step;
- incremental or execution-aware replanning;
- portable, reviewable plan descriptions.

For a small CRUD update, a single obvious strategy, or one direct database write, ordinary application code is usually the simpler tool.

<a id="five-minute-start"></a>

## Five-minute start

Choose the smallest package that answers your first question.

For semantic object differences:

```bash
dotnet add package Forge.Delta --version 1.20.0
```

For desired-state reconciliation and planning:

```bash
dotnet add package Forge.Sync --version 1.20.0
```

Installing `Forge.Sync` also brings in `Forge.Delta`.

For structured JSON expectations in tests:

```bash
dotnet add package Forge.Parse --version 1.20.0
```

For Reqnroll DataTable integration, add `Forge.Parse.Reqnroll`.

For typed, explainable strategy decisions:

```bash
dotnet add package Forge.Decide --version 1.20.0
```

```csharp
using Forge.Decide;

var space = StrategySpace<InteractiveSpace>
    .Define<RequestContext, ExecutionPlan>("interactive")
    .Add(new FastPathStrategy())
    .Add(new BalancedStrategy())
    .Build();

var decision = await space.DecideAsync(
    context,
    cancellationToken);

Console.WriteLine(decision.SelectedStrategyId);
var plan = decision.Plan;
```

The consumer defines what `InteractiveSpace`, `RequestContext`, `ExecutionPlan`, and the strategies mean. Forge.Decide only owns the decision mechanics.

<a id="choose-capability"></a>

## Pick the capability by the planning problem

| Planning problem | Start with |
| --- | --- |
| What changed inside one object? | `Forge.Delta` |
| Which keyed items were added/updated/removed? | generated `Forge.Sync` |
| Does dynamic JSON satisfy a readable structured expectation? | `Forge.Parse` |
| Which of several valid approaches should become the plan? | `Forge.Decide` |
| Current and desired use different CLR types | cross-type reconciliation |
| More than one legitimate identity route exists | ordered fallback identity |
| Operations depend on other operations | `DependencyPlanner` |
| Nodes and relationships change together | `TopologySync` |
| A plan crosses an approval/outbox boundary | portable manifest + digest |
| Reality may change before execution | manifest preconditions |
| A previous plan is already partly executing | execution-aware replanning |
| Several plans form one release/change unit | reconciliation batch / composite topology |

The detailed Delta/Sync contracts live in **[docs/API_CONTRACT.md](docs/API_CONTRACT.md)**, **[docs/DESIGN.md](docs/DESIGN.md)**, and **[docs/RECONCILIATION.md](docs/RECONCILIATION.md)**. Forge.Decide's focused contract is documented in its package README.

<a id="boundary"></a>

## The family boundary

Forge packages calculate and describe results without taking ownership of your application. Delta compares, Sync reconciles, Parse matches structured expectations, and Decide selects an application-owned plan from explicitly admitted strategy proposals.

Forge does **not**:

- persist data;
- call HTTP or RPC endpoints;
- publish messages;
- perform authorization;
- scan assemblies for runtime plugins;
- dynamically compile external expressions;
- execute a plan.

That boundary keeps comparison, planning, and decision logic deterministic, testable, and infrastructure-independent.

<a id="design"></a>

## Design rules

1. .NET 10 first.
2. Source-generated normal Delta/Sync hot paths.
3. Native AOT and trimming-friendly runtime primitives.
4. No runtime reflection in generated comparison/reconciliation paths.
5. No mandatory dependency injection.
6. No persistence or transport assumptions.
7. No hidden I/O or mutation.
8. Strongly typed APIs and consumer-owned domain semantics.
9. Explicit state, identity, and candidate-space semantics.
10. Deterministic planning and portable plan descriptions.
11. Application-owned policy and execution.
12. New features must solve broad production primitives, not framework-specific convenience.

<a id="license"></a>

## License

MIT. See [LICENSE](LICENSE).

---

Forge is an open-source project by **[Pjotr Casteel](https://github.com/pjotrcasteel)**.