namespace Forge.Sync;

internal static class FactKeyIdentity
{
    private static long s_nextId;

    public static long Next() => Interlocked.Increment(ref s_nextId);
}
