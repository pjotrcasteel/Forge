using Forge.Delta;

var id = Guid.NewGuid();
var before = new Customer(
    id,
    "Ada",
    "ada@old.test",
    new Address("Apeldoorn", "NL"));
var after = before with
{
    Email = null,
    Address = new Address("Amsterdam", "NL")
};

var delta = CustomerDelta.Between(before, after);

Console.WriteLine($"Has changes: {delta.HasChanges}");
foreach (var change in delta.Changes)
{
    Console.WriteLine($"{change.Path}: {change.Before} -> {change.After}");
}

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

[GenerateDelta]
internal sealed record Customer(
    Guid Id,
    string Name,
    string? Email,
    Address Address);
