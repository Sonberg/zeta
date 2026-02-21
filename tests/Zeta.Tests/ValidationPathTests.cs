using System.Collections;

namespace Zeta.Tests;

public class ValidationPathTests
{
    private sealed record Item(int Quantity);
    private sealed record Container(List<Item> Items, Dictionary<string, Item> Stock);
    private sealed class ArrayContainer
    {
        public Item[] Items { get; init; } = [];
    }

    [Fact]
    public async Task ValidationError_ExposesStructuredPath()
    {
        var schema = Z.Object<Container>()
            .Field(x => x.Items, s => s.Each(Z.Object<Item>().Field(i => i.Quantity, q => q.Min(1))));

        var result = await schema.ValidateAsync(new Container([new Item(0)], []));

        Assert.True(result.IsFailure);
        Assert.IsType<ValidationPath>(result.Errors[0].Path);
        Assert.Equal("$.items[0].quantity", result.Errors[0].Path.ToPathString());
        Assert.Equal("$.items[0].quantity", result.Errors[0].PathString);
    }

    [Fact]
    public void ValidationPath_CanResolveValueFromObjectGraph()
    {
        var path = ValidationPath.Parse("$.stock[alpha].quantity");
        var model = new Container([], new Dictionary<string, Item> { ["alpha"] = new(42) });

        var found = path.TryGetValue(model, out var value);

        Assert.True(found);
        Assert.Equal(42, Assert.IsType<int>(value));
    }

    [Fact]
    public void ValidationPath_CanUseCustomFormatter()
    {
        var path = ValidationPath.Parse("$.items[1].quantity");

        var custom = path.ToPathString(segment => segment.Kind switch
        {
            ValidationPathSegmentKind.Property => "/" + segment.PropertyName,
            ValidationPathSegmentKind.Index => $"/#{segment.Index}",
            ValidationPathSegmentKind.DictionaryKey => $"/@{segment.DictionaryKey}",
            _ => string.Empty
        });

        Assert.Equal("$/items/#1/quantity", custom);
    }

    [Fact]
    public void ValidationPath_Parse_HandlesRootAndBareProperty()
    {
        Assert.Equal("$", ValidationPath.Parse(null).ToPathString());
        Assert.Equal("$", ValidationPath.Parse(string.Empty).ToPathString());
        Assert.Equal("$", ValidationPath.Parse("$").ToPathString());
        Assert.Equal("$.name", ValidationPath.Parse("name").ToPathString());
    }

    [Fact]
    public void ValidationPath_ToPathString_UsesProvidedFormattingOptions()
    {
        var path = ValidationPath.Parse("$.firstName[alpha]");
        var options = new PathFormattingOptions
        {
            PropertyNameFormatter = static name => name.ToUpperInvariant(),
            DictionaryKeyFormatter = static key => $"<{key}>"
        };

        Assert.Equal("$.FIRSTNAME[<alpha>]", path.ToPathString(options));
        Assert.Equal("$.FIRSTNAME[<alpha>]", path.ToPathString(options));
    }

    [Fact]
    public void ValidationPath_TryGetValue_ResolvesFromArray()
    {
        var path = ValidationPath.Parse("$.items[0].quantity");
        var model = new ArrayContainer { Items = [new Item(7)] };

        var found = path.TryGetValue(model, out var value);

        Assert.True(found);
        Assert.Equal(7, Assert.IsType<int>(value));
    }

    [Fact]
    public void ValidationPath_TryGetValue_DictionaryFallbackMatchesByToString()
    {
        var path = ValidationPath.Parse("$[k42]");
        IDictionary dictionary = new Hashtable { [new StringKey("k42")] = "answer" };

        var found = path.TryGetValue(dictionary, out var value);

        Assert.True(found);
        Assert.Equal("answer", value);
    }

    private sealed class StringKey(string value)
    {
        public override string ToString() => value;
    }

    [Fact]
    public void ValidationPath_TryGetValue_ReturnsFalseForMissingSegments()
    {
        var missingProperty = ValidationPath.Parse("$.missing");
        var missingIndex = ValidationPath.Parse("$.items[2]");
        var model = new Container([new Item(1)], []);

        Assert.False(missingProperty.TryGetValue(model, out _));
        Assert.False(missingIndex.TryGetValue(model, out _));
        Assert.False(ValidationPath.Parse("$.items[0]").TryGetValue(null, out _));
    }

    [Fact]
    public void ValidationPath_GetValue_ThrowsForMissingValue()
    {
        var path = ValidationPath.Parse("$.unknown");
        var model = new Container([], []);

        Assert.Throws<InvalidOperationException>(() => path.GetValue(model));
    }

    [Fact]
    public void ValidationPath_Equality_UsesNormalizedPath()
    {
        var a = ValidationPath.Parse("$.items[0].quantity");
        var b = ValidationPath.Parse("items[0].quantity");

        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }
}
