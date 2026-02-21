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
        Assert.Equal("$.values[0]", result.Errors[0].Path);
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
        Assert.Contains(result.Errors, e => e.Path == "$.values[0]");
        Assert.Contains(result.Errors, e => e.Path == "$.values[1]");
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
        Assert.Contains(result.Errors, e => e.Code == "min_value" && e.Path == "$.values[0]");
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
        Assert.Equal("$.metadata.values[0]", result.Errors[0].Path);
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
        Assert.Equal("$.points.values[0]", result.Errors[0].Path);
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

    [Fact]
    public async Task Dictionary_UsingContext_CountRules_FailWithExpectedCodes()
    {
        var ctx = new ValidationContext<TestContext>(new TestContext(maxEntries: 10));

        var minResult = await Z.Dictionary<string, int>()
            .Using<TestContext>()
            .MinLength(2)
            .ValidateAsync(new Dictionary<string, int> { ["a"] = 1 }, ctx);
        Assert.False(minResult.IsSuccess);
        Assert.Contains(minResult.Errors, e => e.Code == "min_length");

        var maxResult = await Z.Dictionary<string, int>()
            .Using<TestContext>()
            .MaxLength(1)
            .ValidateAsync(new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 }, ctx);
        Assert.False(maxResult.IsSuccess);
        Assert.Contains(maxResult.Errors, e => e.Code == "max_length");

        var notEmptyResult = await Z.Dictionary<string, int>()
            .Using<TestContext>()
            .NotEmpty()
            .ValidateAsync(new Dictionary<string, int>(), ctx);
        Assert.False(notEmptyResult.IsSuccess);
        Assert.Contains(notEmptyResult.Errors, e => e.Code == "min_length");
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

    // ── complex value types ────────────────────────────────────────────────

    [Fact]
    public async Task Dictionary_ComplexValues_AllValid_ReturnsSuccess()
    {
        var itemSchema = Z.Object<OrderItem>()
            .Field(i => i.ProductId, Z.Guid())
            .Field(i => i.Quantity, Z.Int().Min(1));

        var schema = Z.Dictionary(Z.String(), itemSchema);

        var dict = new Dictionary<string, OrderItem>
        {
            ["order1"] = new OrderItem(Guid.NewGuid(), 5),
            ["order2"] = new OrderItem(Guid.NewGuid(), 10)
        };

        var result = await schema.ValidateAsync(dict);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Dictionary_ComplexValues_InvalidField_ReturnsFailureWithCorrectPath()
    {
        var itemSchema = Z.Object<OrderItem>()
            .Field(i => i.ProductId, Z.Guid())
            .Field(i => i.Quantity, Z.Int().Min(1));

        var schema = Z.Dictionary(Z.String(), itemSchema);

        var dict = new Dictionary<string, OrderItem>
        {
            ["order1"] = new OrderItem(Guid.NewGuid(), 0) // quantity too low
        };

        var result = await schema.ValidateAsync(dict);

        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Equal("$.values[0].quantity", result.Errors[0].Path);
        Assert.Equal("min_value", result.Errors[0].Code);
    }

    [Fact]
    public async Task Dictionary_ComplexValues_MultipleEntries_MultipleErrors_ReturnsAllErrors()
    {
        var itemSchema = Z.Object<OrderItem>()
            .Field(i => i.Quantity, Z.Int().Min(1).Max(100));

        var schema = Z.Dictionary(Z.String(), itemSchema);

        var dict = new Dictionary<string, OrderItem>
        {
            ["first"] = new OrderItem(Guid.NewGuid(), 0),   // too low
            ["second"] = new OrderItem(Guid.NewGuid(), 5),  // valid
            ["third"] = new OrderItem(Guid.NewGuid(), 150)  // too high
        };

        var result = await schema.ValidateAsync(dict);

        Assert.False(result.IsSuccess);
        Assert.Equal(2, result.Errors.Count);
        Assert.Contains(result.Errors, e => e.Path == "$.values[0].quantity");
        Assert.Contains(result.Errors, e => e.Path == "$.values[2].quantity");
    }

    [Fact]
    public async Task Dictionary_ComplexValues_MultipleInvalidFieldsInSameEntry_ReturnsAllErrors()
    {
        var itemSchema = Z.Object<OrderItem>()
            .Field(i => i.Quantity, Z.Int().Min(1).Max(100))
            .Field(i => i.Label, Z.String().MinLength(3));

        var schema = Z.Dictionary(Z.String(), itemSchema);

        var dict = new Dictionary<string, OrderItem>
        {
            ["widget"] = new OrderItem(Guid.NewGuid(), 0, "ab") // both fields invalid
        };

        var result = await schema.ValidateAsync(dict);

        Assert.False(result.IsSuccess);
        Assert.Equal(2, result.Errors.Count);
        Assert.Contains(result.Errors, e => e.Path == "$.values[0].quantity");
        Assert.Contains(result.Errors, e => e.Path == "$.values[0].label");
    }

    [Fact]
    public async Task Dictionary_ComplexValues_InlineBuilder_ValidatesCorrectly()
    {
        var schema = Z.Dictionary<string, OrderItem>()
            .EachValue(Z.Object<OrderItem>().Field(i => i.Quantity, Z.Int().Min(1)));

        var dict = new Dictionary<string, OrderItem>
        {
            ["order1"] = new OrderItem(Guid.NewGuid(), 5)
        };

        var result = await schema.ValidateAsync(dict);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Dictionary_ComplexValues_NestedInObject_PropagatesFullPath()
    {
        var itemSchema = Z.Object<OrderItem>()
            .Field(i => i.Quantity, Z.Int().Min(1).Max(100));

        var schema = Z.Object<Catalog>()
            .Field(c => c.Items, dict => dict.EachValue(itemSchema).NotEmpty());

        var catalog = new Catalog(new Dictionary<string, OrderItem>
        {
            ["gadget"] = new OrderItem(Guid.NewGuid(), 0) // quantity too low
        });

        var result = await schema.ValidateAsync(catalog);

        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Equal("$.items.values[0].quantity", result.Errors[0].Path);
        Assert.Equal("min_value", result.Errors[0].Code);
    }

    [Fact]
    public async Task Dictionary_ComplexValues_NestedInObject_PreBuiltSchema_PropagatesFullPath()
    {
        var itemSchema = Z.Object<OrderItem>()
            .Field(i => i.Quantity, Z.Int().Min(1).Max(100));

        var dictSchema = Z.Dictionary(Z.String(), itemSchema).NotEmpty();

        var schema = Z.Object<Catalog>()
            .Field(c => c.Items, dictSchema);

        var catalog = new Catalog(new Dictionary<string, OrderItem>
        {
            ["gadget"] = new OrderItem(Guid.NewGuid(), 5),
            ["widget"] = new OrderItem(Guid.NewGuid(), 200) // too high
        });

        var result = await schema.ValidateAsync(catalog);

        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Equal("$.items.values[1].quantity", result.Errors[0].Path);
        Assert.Equal("max_value", result.Errors[0].Code);
    }

    [Fact]
    public async Task Dictionary_ComplexValues_ChainedWithCountRules_BothFail_ReturnsAllErrors()
    {
        var itemSchema = Z.Object<OrderItem>()
            .Field(i => i.Quantity, Z.Int().Min(1));

        var schema = Z.Dictionary(Z.String(), itemSchema).MinLength(3);

        var dict = new Dictionary<string, OrderItem>
        {
            ["order1"] = new OrderItem(Guid.NewGuid(), 0) // quantity and count both fail
        };

        var result = await schema.ValidateAsync(dict);

        Assert.False(result.IsSuccess);
        Assert.Equal(2, result.Errors.Count);
        Assert.Contains(result.Errors, e => e.Code == "min_length" && e.Path == "$");
        Assert.Contains(result.Errors, e => e.Code == "min_value" && e.Path == "$.values[0].quantity");
    }

    // ── complex key types ──────────────────────────────────────────────────

    [Fact]
    public async Task Dictionary_ComplexKey_AllValid_ReturnsSuccess()
    {
        var keySchema = Z.Object<ProductKey>()
            .Field(k => k.Category, Z.String().MinLength(1))
            .Field(k => k.Code, Z.String().MinLength(3));

        var schema = Z.Dictionary(keySchema, Z.Int().Min(0));

        var dict = new Dictionary<ProductKey, int>
        {
            [new ProductKey("electronics", "abc")] = 10,
            [new ProductKey("furniture", "desk")] = 5
        };

        var result = await schema.ValidateAsync(dict);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Dictionary_ComplexKey_InvalidField_ReturnsFailureWithCorrectPath()
    {
        var keySchema = Z.Object<ProductKey>()
            .Field(k => k.Category, Z.String().MinLength(1))
            .Field(k => k.Code, Z.String().MinLength(5));

        var schema = Z.Dictionary(keySchema, Z.Int().Min(0));

        var dict = new Dictionary<ProductKey, int>
        {
            [new ProductKey("electronics", "ab")] = 10 // code too short
        };

        var result = await schema.ValidateAsync(dict);

        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Equal("$.keys[0].code", result.Errors[0].Path);
        Assert.Equal("min_length", result.Errors[0].Code);
    }

    [Fact]
    public async Task Dictionary_ComplexKey_MultipleInvalidKeys_ReturnsAllErrors()
    {
        var keySchema = Z.Object<ProductKey>()
            .Field(k => k.Code, Z.String().MinLength(5));

        var schema = Z.Dictionary(keySchema, Z.Int().Min(0));

        var dict = new Dictionary<ProductKey, int>
        {
            [new ProductKey("a", "ab")] = 1,  // code too short
            [new ProductKey("b", "cd")] = 2   // code too short
        };

        var result = await schema.ValidateAsync(dict);

        Assert.False(result.IsSuccess);
        Assert.Equal(2, result.Errors.Count);
        Assert.Contains(result.Errors, e => e.Path == "$.keys[0].code");
        Assert.Contains(result.Errors, e => e.Path == "$.keys[1].code");
    }

    [Fact]
    public async Task Dictionary_ComplexKey_MultipleInvalidFieldsInSameKey_ReturnsAllErrors()
    {
        var keySchema = Z.Object<ProductKey>()
            .Field(k => k.Category, Z.String().MinLength(3))
            .Field(k => k.Code, Z.String().MinLength(5));

        var schema = Z.Dictionary(keySchema, Z.Int().Min(0));

        var dict = new Dictionary<ProductKey, int>
        {
            [new ProductKey("ab", "cd")] = 1 // both fields invalid
        };

        var result = await schema.ValidateAsync(dict);

        Assert.False(result.IsSuccess);
        Assert.Equal(2, result.Errors.Count);
        Assert.Contains(result.Errors, e => e.Path == "$.keys[0].category");
        Assert.Contains(result.Errors, e => e.Path == "$.keys[0].code");
    }

    [Fact]
    public async Task Dictionary_ComplexKey_NestedInObject_PropagatesFullPath()
    {
        var keySchema = Z.Object<ProductKey>()
            .Field(k => k.Code, Z.String().MinLength(5));

        var schema = Z.Object<Inventory>()
            .Field(inv => inv.Stock, dict => dict.EachKey(keySchema));

        var inventory = new Inventory(new Dictionary<ProductKey, int>
        {
            [new ProductKey("electronics", "ab")] = 10 // code too short
        });

        var result = await schema.ValidateAsync(inventory);

        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Equal("$.stock.keys[0].code", result.Errors[0].Path);
    }

    [Fact]
    public async Task Dictionary_ComplexKey_AndComplexValue_BothInvalid_ReturnsAllErrors()
    {
        var keySchema = Z.Object<ProductKey>()
            .Field(k => k.Code, Z.String().MinLength(5));

        var valueSchema = Z.Object<OrderItem>()
            .Field(i => i.Quantity, Z.Int().Min(1));

        var schema = Z.Dictionary(keySchema, valueSchema);

        var key = new ProductKey("electronics", "ab"); // code too short
        var dict = new Dictionary<ProductKey, OrderItem>
        {
            [key] = new OrderItem(Guid.NewGuid(), 0, "widget") // quantity too low
        };

        var result = await schema.ValidateAsync(dict);

        Assert.False(result.IsSuccess);
        Assert.Equal(2, result.Errors.Count);
        Assert.Contains(result.Errors, e => e.Path == "$.keys[0].code");
        Assert.Contains(result.Errors, e => e.Code == "min_value" && e.Path == "$.values[0].quantity");
    }

    // ── RefineEachEntry (contextless) ──────────────────────────────────────

    [Fact]
    public async Task RefineEachEntry_AllPass_ReturnsSuccess()
    {
        var schema = Z.Dictionary<string, int>()
            .RefineEachEntry((k, v) => v >= 0, "Value must be non-negative");

        var result = await schema.ValidateAsync(new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 });
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task RefineEachEntry_OneFails_ProducesOneBracketPathError()
    {
        var schema = Z.Dictionary<string, int>()
            .RefineEachEntry((k, v) => v >= 0, "Value must be non-negative");

        var result = await schema.ValidateAsync(new Dictionary<string, int> { ["2024W15"] = -1 });

        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Equal("$[2024W15]", result.Errors[0].Path);
        Assert.Equal("entry_invalid", result.Errors[0].Code);
        Assert.Equal("Value must be non-negative", result.Errors[0].Message);
    }

    [Fact]
    public async Task RefineEachEntry_MultipleFail_ProducesOneErrorPerEntry()
    {
        var schema = Z.Dictionary<string, int>()
            .RefineEachEntry((k, v) => v >= 0, "Value must be non-negative");

        var result = await schema.ValidateAsync(new Dictionary<string, int>
        {
            ["alpha"] = -1,
            ["beta"] = 5,
            ["gamma"] = -3
        });

        Assert.False(result.IsSuccess);
        Assert.Equal(2, result.Errors.Count);
        Assert.Contains(result.Errors, e => e.Path == "$[alpha]");
        Assert.Contains(result.Errors, e => e.Path == "$[gamma]");
    }

    [Fact]
    public async Task RefineEachEntry_EmptyDictionary_ReturnsSuccess()
    {
        var schema = Z.Dictionary<string, int>()
            .RefineEachEntry((k, v) => false, "Always fails");

        var result = await schema.ValidateAsync(new Dictionary<string, int>());
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task RefineEachEntry_MultipleRefinements_BothApplyToSameEntry()
    {
        var schema = Z.Dictionary<string, int>()
            .RefineEachEntry((k, v) => v >= 0, "Must be non-negative", "non_negative")
            .RefineEachEntry((k, v) => v <= 100, "Must be at most 100", "max_value");

        var result = await schema.ValidateAsync(new Dictionary<string, int> { ["x"] = -5 });

        Assert.False(result.IsSuccess);
        // First refinement fails, second passes (v=-5 <= 100)
        Assert.Single(result.Errors);
        Assert.Equal("$[x]", result.Errors[0].Path);
        Assert.Equal("non_negative", result.Errors[0].Code);
    }

    [Fact]
    public async Task RefineEachEntry_MultipleRefinements_BothFailOnSameEntry()
    {
        var schema = Z.Dictionary<string, int>()
            .RefineEachEntry((k, v) => v <= 100, "Must be at most 100", "max100")
            .RefineEachEntry((k, v) => v <= 50, "Must be at most 50", "max50");

        // 200 > 100 and 200 > 50, so both refinements fail
        var result = await schema.ValidateAsync(new Dictionary<string, int> { ["x"] = 200 });

        Assert.False(result.IsSuccess);
        Assert.Equal(2, result.Errors.Count);
        Assert.All(result.Errors, e => Assert.Equal("$[x]", e.Path));
    }

    [Fact]
    public async Task RefineEachEntry_CombinedWithEachValue_AggregatesErrors()
    {
        var schema = Z.Dictionary<string, int>()
            .EachValue(v => v.Min(0))
            .RefineEachEntry((k, v) => v <= 100, "Must be at most 100", "max_entry");

        var result = await schema.ValidateAsync(new Dictionary<string, int> { ["x"] = -5 });

        Assert.False(result.IsSuccess);
        // EachValue reports at $.values[0], RefineEachEntry at $[x] — but -5 passes max_entry (<=100)
        Assert.Single(result.Errors);
        Assert.Equal("$.values[0]", result.Errors[0].Path);
    }

    [Fact]
    public async Task RefineEachEntryAsync_WithCancellationToken_Passes()
    {
        var schema = Z.Dictionary<string, int>()
            .RefineEachEntryAsync(async (k, v, ct) =>
            {
                await Task.Yield();
                return v >= 0;
            }, "Must be non-negative");

        var result = await schema.ValidateAsync(new Dictionary<string, int> { ["a"] = 1 });
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task RefineEachEntry_CustomCode_UsesProvidedCode()
    {
        var schema = Z.Dictionary<string, int>()
            .RefineEachEntry((k, v) => v >= 0, "bad value", code: "custom_code");

        var result = await schema.ValidateAsync(new Dictionary<string, int> { ["k"] = -1 });

        Assert.False(result.IsSuccess);
        Assert.Equal("custom_code", result.Errors[0].Code);
    }

    // ── RefineEachEntry (context-aware) ────────────────────────────────────

    [Fact]
    public async Task RefineEachEntry_ContextPredicate_UsesContextData()
    {
        var schema = Z.Dictionary<string, int>()
            .Using<RangeContext>()
            .RefineEachEntry((k, v, ctx) => v >= ctx.Min && v <= ctx.Max, "Out of range");

        var ctx = new ValidationContext<RangeContext>(new RangeContext(0, 10));
        var result = await schema.ValidateAsync(
            new Dictionary<string, int> { ["a"] = 5, ["b"] = 15 }, ctx);

        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Equal("$[b]", result.Errors[0].Path);
    }

    [Fact]
    public async Task RefineEachEntry_ValueOnlyOnContextSchema_Works()
    {
        var schema = Z.Dictionary<string, int>()
            .Using<RangeContext>()
            .RefineEachEntry((k, v) => v >= 0, "Must be non-negative");

        var ctx = new ValidationContext<RangeContext>(new RangeContext(0, 100));
        var result = await schema.ValidateAsync(
            new Dictionary<string, int> { ["ok"] = 5, ["bad"] = -1 }, ctx);

        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Equal("$[bad]", result.Errors[0].Path);
    }

    [Fact]
    public async Task RefineEachEntry_TransferredViaUsing_ContextPredicateRuns()
    {
        // Entry refinement added BEFORE .Using<>() is transferred as contextless → promoted to context-aware
        var schema = Z.Dictionary<string, int>()
            .RefineEachEntry((k, v) => v >= 0, "Must be non-negative", "non_neg")
            .Using<RangeContext>()
            .RefineEachEntry((k, v, ctx) => v <= ctx.Max, "Exceeds max", "too_high");

        var ctx = new ValidationContext<RangeContext>(new RangeContext(0, 10));
        var result = await schema.ValidateAsync(
            new Dictionary<string, int> { ["a"] = -1, ["b"] = 20 }, ctx);

        Assert.False(result.IsSuccess);
        // "a" fails non_neg, "b" fails too_high
        Assert.Equal(2, result.Errors.Count);
        Assert.Contains(result.Errors, e => e.Path == "$[a]" && e.Code == "non_neg");
        Assert.Contains(result.Errors, e => e.Path == "$[b]" && e.Code == "too_high");
    }

    [Fact]
    public async Task RefineEachEntryAsync_ContextAware_AsyncPredicate_Works()
    {
        var schema = Z.Dictionary<string, int>()
            .Using<RangeContext>()
            .RefineEachEntryAsync(async (k, v, ctx, ct) =>
            {
                await Task.Yield();
                return v <= ctx.Max;
            }, "Exceeds max");

        var ctx = new ValidationContext<RangeContext>(new RangeContext(0, 5));
        var result = await schema.ValidateAsync(
            new Dictionary<string, int> { ["x"] = 10 }, ctx);

        Assert.False(result.IsSuccess);
        Assert.Equal("$[x]", result.Errors[0].Path);
    }

    [Fact]
    public async Task RefineEachEntry_NestedInObjectField_PathIncludesFieldName()
    {
        var schema = Z.Object<Schedule>()
            .Field(s => s.Slots, dict => dict.RefineEachEntry((k, v) => v >= 0, "Must be non-negative"));

        var schedule = new Schedule(new Dictionary<string, int> { ["2024W15"] = -1 });
        var result = await schema.ValidateAsync(schedule);

        Assert.False(result.IsSuccess);
        Assert.Single(result.Errors);
        Assert.Equal("$.slots[2024W15]", result.Errors[0].Path);
    }

    record Request(IDictionary<string, string> Metadata);
    record Scores(IDictionary<string, int> Points);
    record OrderItem(Guid ProductId, int Quantity, string Label = "default");
    record Catalog(IDictionary<string, OrderItem> Items);
    record ProductKey(string Category, string Code);
    record Inventory(IDictionary<ProductKey, int> Stock);
    record Schedule(IDictionary<string, int> Slots);

    class TestContext
    {
        public int MaxEntries { get; }

        public TestContext(int maxEntries)
        {
            MaxEntries = maxEntries;
        }
    }

    class RangeContext
    {
        public int Min { get; }
        public int Max { get; }

        public RangeContext(int min, int max)
        {
            Min = min;
            Max = max;
        }
    }
}
