namespace Forge.Sync;

/// <summary>Defines application-owned preference ordering for plan metrics.</summary>
public interface IPlanPreference<in TMetrics>
{
    /// <summary>Returns a negative value when left is preferred, positive when right is preferred, and zero for equal preference.</summary>
    int Compare(TMetrics left, TMetrics right);
}
