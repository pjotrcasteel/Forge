using System.Security.Cryptography;
using System.Text;

namespace Forge.Decide;

/// <summary>
/// Computes deterministic digests for completed strategy decisions without imposing a serialization format on application-owned plans.
/// </summary>
public static class StrategyDecisionDigest
{
    /// <summary>
    /// Computes a SHA-256 digest over the selected strategy and complete candidate evidence.
    /// </summary>
    /// <typeparam name="TSpace">Consumer-defined strategy-space marker.</typeparam>
    /// <typeparam name="TPlan">Application-owned plan type.</typeparam>
    /// <param name="decision">Decision to fingerprint.</param>
    /// <param name="canonicalizePlan">Application-owned deterministic canonicalizer for proposed plans.</param>
    /// <returns>Uppercase hexadecimal SHA-256 digest.</returns>
    public static string ComputeSha256Hex<TSpace, TPlan>(StrategyDecision<TSpace, TPlan> decision, Func<TPlan, string> canonicalizePlan)
    {
        ArgumentNullException.ThrowIfNull(decision);
        ArgumentNullException.ThrowIfNull(canonicalizePlan);

        var canonical = new StringBuilder();
        Append(canonical, decision.SpaceId.Value);
        Append(canonical, decision.SelectedStrategyId.Value);

        foreach (var candidate in decision.Candidates)
        {
            Append(canonical, candidate.StrategyId.Value);

            switch (candidate)
            {
                case ApplicableStrategyCandidate<TPlan> applicable:
                    Append(canonical, "applicable");
                    Append(canonical, applicable.Reason);
                    Append(canonical, canonicalizePlan(applicable.Plan));
                    break;
                case RejectedStrategyCandidate<TPlan> rejected:
                    Append(canonical, "rejected");
                    Append(canonical, rejected.Reason);
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported candidate type '{candidate.GetType().FullName}'.");
            }
        }

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical.ToString())));
    }

    private static void Append(StringBuilder builder, string? value)
    {
        if (value is null)
        {
            builder.Append("-1:");
            return;
        }

        builder.Append(value.Length).Append(':').Append(value);
    }
}