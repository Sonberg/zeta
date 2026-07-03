namespace Zeta.Tests;

public class ValidationPathTests
{
    private sealed record Item(int Quantity);
    private sealed record Container(List<Item> Items, Dictionary<string, Item> Stock);

    [Fact]
    public async Task ValidationError_ExposesStructuredPath()
    {
        var schema = Z.Schema<Container>()
            .Property(x => x.Items, s => s.Each(Z.Schema<Item>().Property(i => i.Quantity, q => q.Min(1))));

        var result = await schema.ValidateAsync(new Container([new Item(0)], []));

        Assert.True(result.IsFailure);
        Assert.IsType<ValidationPath>(result.Errors[0].Path);
        Assert.Equal("$.items[0].quantity", result.Errors[0].Path.ToPathString());
        Assert.Equal("$.items[0].quantity", result.Errors[0].PathString);
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
    public void ValidationPath_Equality_UsesNormalizedPath()
    {
        var a = ValidationPath.Parse("$.items[0].quantity");
        var b = ValidationPath.Parse("items[0].quantity");

        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }
}
