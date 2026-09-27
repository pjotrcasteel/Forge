using Forge.Delta;

namespace Forge.Delta.Tests;

internal sealed record ExternalProfileCustomer(
    int Id,
    string Name,
    string? Email);

[GenerateDeltaProfile(typeof(ExternalProfileCustomer))]
[DeltaProfileIgnore(nameof(ExternalProfileCustomer.Id))]
[DeltaProfileComparer(nameof(ExternalProfileCustomer.Name), typeof(ProfileIgnoreCaseComparer))]
internal sealed class ExternalProfileCustomerProfile
{
}

internal sealed class ProfileIgnoreCaseComparer : IEqualityComparer<string>
{
    public bool Equals(string? x, string? y)
    {
        return StringComparer.OrdinalIgnoreCase.Equals(x, y);
    }

    public int GetHashCode(string obj)
    {
        return StringComparer.OrdinalIgnoreCase.GetHashCode(obj);
    }
}
