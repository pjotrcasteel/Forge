using System.Text.Json.Nodes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Forge.Parse.Tests;

[TestClass]
public sealed class JsonMatcherTests
{
    [TestMethod]
    public void Match_PartialObject_AllowsAdditionalProperties()
    {
        var result = new JsonMatcher().Match(
            """{ "id": "<Guid>", "state": "active" }""",
            """{ "id": "3a192561-aaa6-48e8-89fc-fcd8440fb813", "state": "active", "extra": true }""");

        Assert.IsTrue(result.IsMatch, result.ToString());
    }

    [TestMethod]
    public void Match_ExactObject_RejectsAdditionalProperties()
    {
        var result = new JsonMatcher().Match(
            """{ "id": 1 }""",
            """{ "id": 1, "extra": true }""",
            JsonMatchOptions.Exact);

        Assert.IsFalse(result.IsMatch);
        Assert.AreEqual("$.extra", result.FirstMismatch!.Path);
    }

    [TestMethod]
    public void Match_BuiltInPlaceholders_MatchDynamicValues()
    {
        var result = new JsonMatcher().Match(
            """
            {
              "id": "<NotEmptyGuid>",
              "state": "<OneOf:pending|active>",
              "reference": "<Regex:^ORD-[0-9]+$>",
              "count": "<GreaterThan:0>",
              "createdAt": "<DateTimeOffset>"
            }
            """,
            """
            {
              "id": "3a192561-aaa6-48e8-89fc-fcd8440fb813",
              "state": "active",
              "reference": "ORD-42",
              "count": 2,
              "createdAt": "2026-09-29T18:30:00+02:00"
            }
            """);

        Assert.IsTrue(result.IsMatch, result.ToString());
    }

    [TestMethod]
    public void Match_CaptureAndSame_MatchRepeatedDynamicValue()
    {
        var result = new JsonMatcher().Match(
            """{ "id": "<Capture:serviceId>", "dependency": { "id": "<Same:serviceId>" } }""",
            """{ "id": "service-42", "dependency": { "id": "service-42" } }""");

        Assert.IsTrue(result.IsMatch, result.ToString());
    }

    [TestMethod]
    public void Match_UnorderedSubset_MatchesExpectedRows()
    {
        var result = new JsonMatcher().Match(
            """[{ "id": 2 }, { "id": 1 }]""",
            """[{ "id": 1 }, { "id": 2 }, { "id": 3 }]""",
            JsonMatchOptions.UnorderedSubset);

        Assert.IsTrue(result.IsMatch, result.ToString());
    }

    [TestMethod]
    public void Match_IgnorePath_SkipsDynamicValue()
    {
        var options = JsonMatchOptions.Partial.Ignore("$.metadata.generatedAt");

        var result = new JsonMatcher().Match(
            """{ "metadata": { "generatedAt": "old", "source": "test" } }""",
            """{ "metadata": { "generatedAt": "new", "source": "test" } }""",
            options);

        Assert.IsTrue(result.IsMatch, result.ToString());
    }

    [TestMethod]
    public void Match_CustomMatcher_CanOverrideBuiltInMatcher()
    {
        var customMatcher = new DelegateValueMatcher(
            "<Guid>",
            actual => actual is JsonValue value &&
                      value.TryGetValue<string>(out var text) &&
                      text == "custom"
                ? ValueMatchResult.Success()
                : ValueMatchResult.Failure("Expected custom"));

        var result = new JsonMatcher([customMatcher]).Match(
            """{ "id": "<Guid>" }""",
            """{ "id": "custom" }""");

        Assert.IsTrue(result.IsMatch, result.ToString());
    }

    [TestMethod]
    public void Assert_WhenMismatch_ThrowsStructuredException()
    {
        var exception = Assert.ThrowsExactly<JsonMatchAssertionException>(
            () => JsonAssert.Matches("""{ "id": "<Guid>" }""", """{ "id": "invalid" }"""));

        Assert.IsNotNull(exception.Result);
        Assert.AreEqual("$.id", exception.Result!.FirstMismatch!.Path);
    }

    [TestMethod]
    public void ExpectedValueParser_ParsesCommonValues()
    {
        Assert.AreEqual(42L, ExpectedValueParser.Parse("42")!.GetValue<long>());
        Assert.IsTrue(ExpectedValueParser.Parse("true")!.GetValue<bool>());
        Assert.AreEqual("<Guid>", ExpectedValueParser.Parse("<Guid>")!.GetValue<string>());
        Assert.IsNull(ExpectedValueParser.Parse("null"));
    }
}
