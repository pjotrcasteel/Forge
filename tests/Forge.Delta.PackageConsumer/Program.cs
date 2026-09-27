using Forge.Delta;

var before = new Customer("customer-1", "Before");
var after = new Customer("customer-1", "After");

if (CustomerDelta.AreEquivalent(before, after))
{
    throw new InvalidOperationException("Packed Forge.Delta did not generate semantic comparison code.");
}

var delta = CustomerDelta.Between(before, after);
if (!delta.NameChange.HasChanged || delta.Changes.Count != 1)
{
    throw new InvalidOperationException("Packed Forge.Delta did not generate the expected Delta API.");
}

Console.WriteLine("Forge.Delta package consumer validation passed.");

[GenerateDelta]
internal sealed record Customer(
    string Id,
    string Name);
