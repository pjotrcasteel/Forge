using System.Text.Json.Nodes;

namespace Forge.Parse;

internal sealed class MatchContext
{
    private readonly Dictionary<string, JsonNode?> _captures;

    public MatchContext()
        : this(new Dictionary<string, JsonNode?>(StringComparer.Ordinal))
    {
    }

    private MatchContext(Dictionary<string, JsonNode?> captures)
    {
        _captures = captures;
    }

    public MatchContext Clone() => new(new Dictionary<string, JsonNode?>(_captures, StringComparer.Ordinal));

    public void CopyFrom(MatchContext source)
    {
        _captures.Clear();
        foreach (var capture in source._captures)
        {
            _captures.Add(capture.Key, capture.Value);
        }
    }

    public bool TryGetCapture(string name, out JsonNode? value) => _captures.TryGetValue(name, out value);

    public void Capture(string name, JsonNode? value) => _captures.Add(name, value);
}
