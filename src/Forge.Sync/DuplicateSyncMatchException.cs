namespace Forge.Sync;

/// <summary>
/// Thrown when more than one desired item resolves to the same current item.
/// </summary>
public sealed class DuplicateSyncMatchException : InvalidOperationException
{
    /// <summary>
    /// Initializes the exception.
    /// </summary>
    public DuplicateSyncMatchException(Type currentType, Type desiredType, string currentKey)
        : base(
            $"More than one desired '{desiredType.FullName}' item resolves to current " +
            $"'{currentType.FullName}' item '{currentKey}'.")
    {
        CurrentType = currentType;
        DesiredType = desiredType;
        CurrentKey = currentKey;
    }

    /// <summary>Gets the current item type.</summary>
    public Type CurrentType { get; }

    /// <summary>Gets the desired item type.</summary>
    public Type DesiredType { get; }

    /// <summary>Gets the canonical key of the current item targeted more than once.</summary>
    public string CurrentKey { get; }
}
