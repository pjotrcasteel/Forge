namespace Forge.Delta;

/// <summary>
/// Selects a stable logical key for collection comparison.
/// </summary>
/// <typeparam name="T">The collection item type.</typeparam>
/// <typeparam name="TKey">The logical key type.</typeparam>
public interface IKeySelector<in T, out TKey>
{
    /// <summary>
    /// Gets the logical key for an item.
    /// </summary>
    TKey GetKey(T item);
}
