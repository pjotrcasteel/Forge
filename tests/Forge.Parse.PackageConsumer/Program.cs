using Forge.Parse;

var result = new JsonMatcher().Match(
    """{ "id": "<Guid>", "state": "<OneOf:pending|active>" }""",
    """{ "id": "3a192561-aaa6-48e8-89fc-fcd8440fb813", "state": "active", "extra": true }""");

if (!result.IsMatch)
{
    throw new InvalidOperationException(result.ToString());
}

Console.WriteLine("Forge.Parse package consumer validation passed.");
