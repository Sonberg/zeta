namespace Zeta.Tests;

public class PathFormattingOptionsTests
{
    private sealed record Person(string FirstName);

    [Fact]
    public async Task ObjectField_UsesCustomPropertyNameFormatter()
    {
        var schema = Z.Object<Person>()
            .Field(x => x.FirstName, s => s.MinLength(5));

        var context = new ValidationContext(pathFormattingOptions: new PathFormattingOptions
        {
            PropertyNameFormatter = static name => name switch
            {
                "FirstName" => "first-name",
                _ => name
            }
        });

        var result = await schema.ValidateAsync(new Person("abc"), context);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Path == "$.first-name");
    }

    [Fact]
    public async Task DictionaryEntryRefinement_UsesCustomDictionaryKeyFormatter()
    {
        var schema = Z.Dictionary<string, int>()
            .RefineEachEntry((_, value) => value > 0, "must be positive", "positive");

        var context = new ValidationContext(pathFormattingOptions: new PathFormattingOptions
        {
            DictionaryKeyFormatter = static key => $"<{key}>"
        });

        var result = await schema.ValidateAsync(new Dictionary<string, int> { ["Alpha"] = -1 }, context);

        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Path == "$[<Alpha>]");
    }
}
