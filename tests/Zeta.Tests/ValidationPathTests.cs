namespace Zeta.Tests;

public class ValidationPathTests
{
    private sealed record Item(int Quantity);
    private sealed record Container(List<Item> Items, Dictionary<string, Item> Stock);

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
}
