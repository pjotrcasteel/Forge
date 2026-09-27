using Forge.Delta;

namespace Forge.Delta.Tests;

[GenerateDelta]
internal sealed record CustomerProfile(Address? Address);

[GenerateDelta]
internal readonly record struct Coordinates(int X, int Y);

[GenerateDelta]
internal sealed record Location(Coordinates? Coordinates);
