# Forge.Decide sample

This sample shows the complete Forge.Decide decision lifecycle without application-framework or domain-specific concepts.

It models a generic workload-routing problem and demonstrates:

1. **Strategy-space boundaries** — `InteractiveWorkload` admits `fast-lane` and `balanced`; the globally available `economy` strategy is not evaluated at all.
2. **Proposal evaluation** — strategies return either a plan plus explanation or an explicit rejection reason.
3. **Frozen comparison** — proposals are evaluated once before selection.
4. **Production + shadow selection** — two policies select from the exact same comparison without re-running strategies.
5. **Explanation** — the production decision reports selected, applicable and rejected candidates.
6. **Deterministic digest** — the application supplies a canonical representation of its own plan type.
7. **Replay/regression diff** — a changed context is re-evaluated and compared with the earlier decision.
8. **Different candidate spaces** — a batch space admits `balanced` and `economy` instead.

Run it with:

```bash
dotnet run --project samples/Forge.Decide.Sample/Forge.Decide.Sample.csproj
```

The important boundary is that Forge.Decide only calculates and describes the decision. It never executes the returned `WorkloadPlan`.