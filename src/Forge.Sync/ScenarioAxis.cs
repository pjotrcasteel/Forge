namespace Forge.Sync;

/// <summary>Immutable typed scenario axis with unique application-owned case identities.</summary>
public sealed class ScenarioAxis<TCaseId, TValue>
    where TCaseId : notnull
{
    public ScenarioAxis(
        IReadOnlyList<ScenarioCase<TCaseId, TValue>> cases,
        IEqualityComparer<TCaseId>? comparer = null)
    {
        ArgumentNullException.ThrowIfNull(cases);
        var seen = new HashSet<TCaseId>(comparer ?? EqualityComparer<TCaseId>.Default);
        foreach (var scenarioCase in cases)
        {
            ArgumentNullException.ThrowIfNull(scenarioCase);
            if (!seen.Add(scenarioCase.Id))
            {
                throw new ArgumentException("Scenario case identities must be unique within an axis.", nameof(cases));
            }
        }

        Cases = Array.AsReadOnly(cases.ToArray());
    }

    public IReadOnlyList<ScenarioCase<TCaseId, TValue>> Cases { get; }
}
