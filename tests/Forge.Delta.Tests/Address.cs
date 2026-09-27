using Forge.Delta;

namespace Forge.Delta.Tests;

[GenerateDelta]
internal sealed class Address
{
    public Address(string city, string country)
    {
        City = city;
        Country = country;
    }

    public string City { get; }

    public string Country { get; }
}
