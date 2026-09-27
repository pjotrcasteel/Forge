using Forge.Delta;

namespace Forge.Sync.Tests;

[GenerateDelta]
internal sealed class ItemConfiguration
{
    public ItemConfiguration(string mode, bool enabled)
    {
        Mode = mode;
        Enabled = enabled;
    }

    public string Mode { get; }

    public bool Enabled { get; }
}
