using Forge.Delta;

namespace Forge.Delta.Tests;

[GenerateDelta]
internal readonly record struct ValueObject(int Number, bool Enabled);
