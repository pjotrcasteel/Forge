namespace Forge.Sync.Tests;

[TestClass]
public sealed class FactEngineTests
{
    private static readonly FactKey<RequiresNewSubnet> RequiresNewSubnetFact = FactKey<RequiresNewSubnet>.Create();
    private static readonly FactKey<FramedIpv4> FramedIpv4Fact = FactKey<FramedIpv4>.Create();

    [TestMethod]
    public void Derive_ShouldPropagateTypedFactsUntilStableEvenWhenRulesAreReverseOrdered()
    {
        IFactRule<ChangeContext>[] rules =
        [
            new FramedIpv4Rule(),
            new RequiresNewSubnetRule()
        ];

        var result = FactEngine.Derive(new ChangeContext(true), FactSet.Empty, rules);

        Assert.IsTrue(result.Facts.Get(RequiresNewSubnetFact).Value);
        Assert.AreEqual("10.20.0.0/24", result.Facts.Get(FramedIpv4Fact).Value);
        Assert.IsTrue(result.Passes >= 2);
    }

    [TestMethod]
    public void FactKeys_ForDifferentValueTypes_ShouldNeverShareInternalIdentity()
    {
        var integerFact = FactKey<int>.Create();
        var textFact = FactKey<string>.Create();
        var builder = new FactSetBuilder(FactSet.Empty);

        Assert.IsTrue(builder.TryAdd(integerFact, 42));
        Assert.IsTrue(builder.TryAdd(textFact, "forty-two"));
        Assert.AreEqual(42, builder.Get(integerFact));
        Assert.AreEqual("forty-two", builder.Get(textFact));
    }

    [TestMethod]
    public void TryAdd_WhenSameTypedKeyGetsDifferentValue_ShouldRejectConflict()
    {
        var builder = new FactSetBuilder(FactSet.Empty);
        builder.TryAdd(RequiresNewSubnetFact, new RequiresNewSubnet(true));

        Assert.ThrowsExactly<FactConflictException>(() =>
            builder.TryAdd(RequiresNewSubnetFact, new RequiresNewSubnet(false)));
    }

    private sealed record ChangeContext(bool PrefixChanged);
    private readonly record struct RequiresNewSubnet(bool Value);
    private readonly record struct FramedIpv4(string Value);

    private sealed class RequiresNewSubnetRule() : FactRule<ChangeContext, RequiresNewSubnet>(RequiresNewSubnetFact)
    {
        protected override bool TryDerive(ChangeContext context, FactSetBuilder facts, out RequiresNewSubnet value)
        {
            value = new RequiresNewSubnet(context.PrefixChanged);
            return true;
        }
    }

    private sealed class FramedIpv4Rule() : FactRule<ChangeContext, FramedIpv4>(FramedIpv4Fact)
    {
        protected override bool TryDerive(ChangeContext context, FactSetBuilder facts, out FramedIpv4 value)
        {
            if (!facts.TryGet(RequiresNewSubnetFact, out var required) || !required.Value)
            {
                value = default;
                return false;
            }

            value = new FramedIpv4("10.20.0.0/24");
            return true;
        }
    }
}
