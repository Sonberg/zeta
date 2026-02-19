using System.Collections.Generic;
using Zeta.Schemas;

namespace Zeta.Tests;

public class DictionarySchemaTests
{
    // ── standalone usage ───────────────────────────────────────────────────

    [Fact]
    public async Task Dictionary_ValidValues_ReturnsSuccess()
    {
        var schema = Z.Dictionary<string, int>().EachValue(v => v.Min(0));
        var result = await schema.ValidateAsync(new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 });
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Dictionary_InvalidValue_ReturnsFailure()
    {
        var schema = Z.Dictionary<string, int>().EachValue(v => v.Min(0));
        var result = await schema.ValidateAsync(new Dictionary<string, int> { ["city"] = -1 });

        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Equal("$.city", result.Errors[0].Path);
        Assert.Equal("min_value", result.Errors[0].Code);
    }

    [Fact]
    public async Task Dictionary_InvalidKey_ReturnsFailure()
    {
        var schema = Z.Dictionary<string, int>().EachKey(k => k.MinLength(2));
        var result = await schema.ValidateAsync(new Dictionary<string, int> { ["a"] = 1 });

        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Equal("$.keys[0]", result.Errors[0].Path);
        Assert.Equal("min_length", result.Errors[0].Code);
    }

    [Fact]
    public async Task Dictionary_MultipleInvalidValues_ReturnsAllErrors()
    {
        var schema = Z.Dictionary<string, int>().EachValue(v => v.Min(0));
        var result = await schema.ValidateAsync(new Dictionary<string, int>
        {
            ["a"] = -1,
            ["b"] = -2,
            ["c"] = 3
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(2, result.Errors.Count);
        Assert.Contains(result.Errors, e => e.Path == "$.a");
        Assert.Contains(result.Errors, e => e.Path == "$.b");
    }

    [Fact]
    public async Task Dictionary_MultipleInvalidKeys_ReturnsAllErrors()
    {
        var schema = Z.Dictionary<string, int>().EachKey(k => k.MinLength(3));
        var result = await schema.ValidateAsync(new Dictionary<string, int> { ["ab"] = 1, ["cd"] = 2 });

        Assert.False(result.IsSuccess);
        Assert.Equal(2, result.Errors.Count);
        Assert.Contains(result.Errors, e => e.Path == "$.keys[0]");
        Assert.Contains(result.Errors, e => e.Path == "$.keys[1]");
    }

    // ── count rules ────────────────────────────────────────────────────────

    [Fact]
    public async Task Dictionary_MinLength_Valid_ReturnsSuccess()
    {
        var schema = Z.Dictionary<string, int>().MinLength(2);
        var result = await schema.ValidateAsync(new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 });
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Dictionary_MinLength_Invalid_ReturnsFailure()
    {
        var schema = Z.Dictionary<string, int>().MinLength(2);
        var result = await schema.ValidateAsync(new Dictionary<string, int> { ["a"] = 1 });

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "min_length");
    }

    [Fact]
    public async Task Dictionary_MaxLength_Valid_ReturnsSuccess()
    {
        var schema = Z.Dictionary<string, int>().MaxLength(3);
        var result = await schema.ValidateAsync(new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 });
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Dictionary_MaxLength_Invalid_ReturnsFailure()
    {
        var schema = Z.Dictionary<string, int>().MaxLength(1);
        var result = await schema.ValidateAsync(new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 });

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "max_length");
    }

    [Fact]
    public async Task Dictionary_NotEmpty_Invalid_ReturnsFailure()
    {
        var schema = Z.Dictionary<string, int>().NotEmpty();
        var result = await schema.ValidateAsync(new Dictionary<string, int>());

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "min_length");
    }

    [Fact]
    public async Task Dictionary_NotEmpty_Valid_ReturnsSuccess()
    {
        var schema = Z.Dictionary<string, int>().NotEmpty();
        var result = await schema.ValidateAsync(new Dictionary<string, int> { ["x"] = 42 });
        Assert.True(result.IsSuccess);
    }

    // ── null handling ──────────────────────────────────────────────────────

    [Fact]
    public async Task Dictionary_Null_ReturnsFailure()
    {
        var schema = Z.Dictionary<string, int>();
        var result = await schema.ValidateAsync(null);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "null_value");
    }

    [Fact]
    public async Task Dictionary_Null_WithNullable_ReturnsSuccess()
    {
        var schema = Z.Dictionary<string, int>().Nullable();
        var result = await schema.ValidateAsync(null);
        Assert.True(result.IsSuccess);
    }

    // ── empty dictionary ───────────────────────────────────────────────────

    [Fact]
    public async Task Dictionary_Empty_WithEachValue_DoesNotValidateElements()
    {
        var schema = Z.Dictionary<string, int>().EachValue(v => v.Min(100));
        var result = await schema.ValidateAsync(new Dictionary<string, int>());
        Assert.True(result.IsSuccess);
    }

    // ── pre-built schemas ──────────────────────────────────────────────────

    [Fact]
    public async Task Dictionary_PreBuiltKeyValueSchema_ValidatesCorrectly()
    {
        var keySchema = Z.String().MinLength(2);
        var valueSchema = Z.Int().Min(0);
        var schema = Z.Dictionary(keySchema, valueSchema);

        var result = await schema.ValidateAsync(new Dictionary<string, int> { ["ab"] = 5 });
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Dictionary_PreBuiltKeyValueSchema_InvalidKey_ReturnsFailure()
    {
        var keySchema = Z.String().MinLength(3);
        var valueSchema = Z.Int().Min(0);
        var schema = Z.Dictionary(keySchema, valueSchema);

        var result = await schema.ValidateAsync(new Dictionary<string, int> { ["ab"] = 5 });

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "min_length" && e.Path == "$.keys[0]");
    }

    // ── chaining rules ─────────────────────────────────────────────────────

    [Fact]
    public async Task Dictionary_ChainedRules_BothFail_ReturnsAllErrors()
    {
        var schema = Z.Dictionary<string, int>()
            .EachValue(v => v.Min(0))
            .MinLength(3);

        var result = await schema.ValidateAsync(new Dictionary<string, int> { ["a"] = -1 });

        Assert.False(result.IsSuccess);
        Assert.Equal(2, result.Errors.Count);
        Assert.Contains(result.Errors, e => e.Code == "min_length" && e.Path == "$");
        Assert.Contains(result.Errors, e => e.Code == "min_value" && e.Path == "$.a");
    }

    // ── ObjectSchema field integration ─────────────────────────────────────

    [Fact]
    public async Task Dictionary_NestedInObject_ValidatesCorrectly()
    {
        var schema = Z.Object<Request>()
            .Field(r => r.Metadata, dict => dict.EachValue(v => v.MaxLength(50)));

        var request = new Request(new Dictionary<string, string> { ["city"] = "New York" });
        var result = await schema.ValidateAsync(request);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Dictionary_NestedInObject_InvalidValue_PropagatesPath()
    {
        var schema = Z.Object<Request>()
            .Field(r => r.Metadata, dict => dict.EachValue(v => v.MaxLength(3)));

        var request = new Request(new Dictionary<string, string> { ["city"] = "New York" });
        var result = await schema.ValidateAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Equal("$.metadata.city", result.Errors[0].Path);
        Assert.Equal("max_length", result.Errors[0].Code);
    }

    [Fact]
    public async Task Dictionary_NestedInObject_InvalidKey_PropagatesPath()
    {
        var schema = Z.Object<Request>()
            .Field(r => r.Metadata, dict => dict.EachKey(k => k.MinLength(5)));

        var request = new Request(new Dictionary<string, string> { ["city"] = "New York" });
        var result = await schema.ValidateAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Equal("$.metadata.keys[0]", result.Errors[0].Path);
    }

    [Fact]
    public async Task Dictionary_NestedInObject_PreBuiltSchema_ValidatesCorrectly()
    {
        var dictSchema = Z.Dictionary<string, int>()
            .EachValue(v => v.Min(0))
            .NotEmpty();

        var schema = Z.Object<Scores>()
            .Field(s => s.Points, dictSchema);

        var scores = new Scores(new Dictionary<string, int> { ["alice"] = 10, ["bob"] = 20 });
        var result = await schema.ValidateAsync(scores);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Dictionary_NestedInObject_PreBuiltSchema_InvalidValue_ReturnsFailure()
    {
        var dictSchema = Z.Dictionary<string, int>().EachValue(v => v.Min(0));

        var schema = Z.Object<Scores>()
            .Field(s => s.Points, dictSchema);

        var scores = new Scores(new Dictionary<string, int> { ["alice"] = -1 });
        var result = await schema.ValidateAsync(scores);

        Assert.False(result.IsSuccess);
        Assert.Equal("$.points.alice", result.Errors[0].Path);
    }

    // ── conditionals ───────────────────────────────────────────────────────

    [Fact]
    public async Task Dictionary_If_AppliesConditionalRule()
    {
        var schema = Z.Dictionary<string, int>()
            .If(d => d.Count > 0, s => s.EachValue(v => v.Min(0)));

        var result = await schema.ValidateAsync(new Dictionary<string, int> { ["x"] = -1 });
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "min_value");
    }

    [Fact]
    public async Task Dictionary_If_ConditionNotMet_SkipsRule()
    {
        var schema = Z.Dictionary<string, int>()
            .If(d => d.Count > 5, s => s.EachValue(v => v.Min(100)));

        var result = await schema.ValidateAsync(new Dictionary<string, int> { ["x"] = 1 });
        Assert.True(result.IsSuccess);
    }

    // ── context-aware ──────────────────────────────────────────────────────

    [Fact]
    public async Task Dictionary_UsingContext_ValidatesWithContext()
    {
        var schema = Z.Dictionary<string, int>()
            .EachValue(v => v.Min(0))
            .Using<TestContext>()
            .Refine((dict, ctx) => dict.Count <= ctx.MaxEntries, "Too many entries");

        var ctx = new ValidationContext<TestContext>(new TestContext(maxEntries: 2));
        var result = await schema.ValidateAsync(new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 }, ctx);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Dictionary_UsingContext_ContextRefineFails_ReturnsError()
    {
        var schema = Z.Dictionary<string, int>()
            .Using<TestContext>()
            .Refine((dict, ctx) => dict.Count <= ctx.MaxEntries, "Too many entries");

        var ctx = new ValidationContext<TestContext>(new TestContext(maxEntries: 1));
        var result = await schema.ValidateAsync(new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 }, ctx);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Message == "Too many entries");
    }

    [Fact]
    public async Task Dictionary_UsingContext_EachKeyAndValue_ValidatesCorrectly()
    {
        var schema = Z.Dictionary<string, int>()
            .Using<TestContext>()
            .EachKey(k => k.MinLength(2).Using<TestContext>())
            .EachValue(v => v.Min(0).Using<TestContext>());

        var ctx = new ValidationContext<TestContext>(new TestContext(maxEntries: 10));
        var result = await schema.ValidateAsync(
            new Dictionary<string, int> { ["ab"] = 5, ["cd"] = 10 }, ctx);
        Assert.True(result.IsSuccess);
    }

    // ── custom message ─────────────────────────────────────────────────────

    [Fact]
    public async Task Dictionary_MinLength_CustomMessage_ReturnsCustomMessage()
    {
        var schema = Z.Dictionary<string, int>().MinLength(2, "Need at least 2 entries");
        var result = await schema.ValidateAsync(new Dictionary<string, int> { ["a"] = 1 });

        Assert.False(result.IsSuccess);
        Assert.Equal("Need at least 2 entries", result.Errors[0].Message);
    }

    // ── IDictionary interface ──────────────────────────────────────────────

    [Fact]
    public async Task Dictionary_AcceptsIDictionaryInterface_ValidatesCorrectly()
    {
        IDictionary<string, int> dict = new Dictionary<string, int> { ["a"] = 1 };
        var schema = Z.Dictionary<string, int>().EachValue(v => v.Min(0));
        var result = await schema.ValidateAsync(dict);
        Assert.True(result.IsSuccess);
    }

    record Request(IDictionary<string, string> Metadata);
    record Scores(IDictionary<string, int> Points);

    class TestContext
    {
        public int MaxEntries { get; }

        public TestContext(int maxEntries)
        {
            MaxEntries = maxEntries;
        }
    }
}
