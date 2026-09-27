namespace Forge.Sync;

/// <summary>Builds lazy strongly typed scenario matrices without pre-materializing the Cartesian product.</summary>
public static class ScenarioMatrix
{
    public static IEnumerable<ScenarioCombination<TLeftId, TRightId, TLeft, TRight>> Cross<TLeftId, TRightId, TLeft, TRight>(
        ScenarioAxis<TLeftId, TLeft> left,
        ScenarioAxis<TRightId, TRight> right)
        where TLeftId : notnull
        where TRightId : notnull
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        return Enumerate(left, right);
    }

    private static IEnumerable<ScenarioCombination<TLeftId, TRightId, TLeft, TRight>> Enumerate<TLeftId, TRightId, TLeft, TRight>(
        ScenarioAxis<TLeftId, TLeft> left,
        ScenarioAxis<TRightId, TRight> right)
        where TLeftId : notnull
        where TRightId : notnull
    {
        foreach (var leftCase in left.Cases)
        {
            foreach (var rightCase in right.Cases)
            {
                yield return new ScenarioCombination<TLeftId, TRightId, TLeft, TRight>(leftCase, rightCase);
            }
        }
    }
}
