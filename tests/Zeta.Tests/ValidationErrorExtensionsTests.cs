namespace Zeta.Tests;

public class ValidationErrorExtensionsTests
{
    [Fact]
    public void ToPathDictionary_MultipleErrorsSamePath_GroupsMessagesTogether()
    {
        var errors = new[]
        {
            new ValidationError("email", "email", "Invalid email format"),
            new ValidationError("email", "min_length", "Email is too short"),
        };

        var dictionary = errors.ToPathDictionary();

        Assert.Single(dictionary);
        Assert.Equal(["Invalid email format", "Email is too short"], dictionary["$.email"]);
    }

    [Fact]
    public void ToPathDictionary_ErrorsOnDifferentPaths_KeepsSeparateEntries()
    {
        var errors = new[]
        {
            new ValidationError("name", "required", "Name is required"),
            new ValidationError("email", "email", "Invalid email"),
        };

        var dictionary = errors.ToPathDictionary();

        Assert.Equal(2, dictionary.Count);
        Assert.Equal(["Name is required"], dictionary["$.name"]);
        Assert.Equal(["Invalid email"], dictionary["$.email"]);
    }

    [Fact]
    public void ToPathDictionary_EmptyInput_ReturnsEmptyDictionary()
    {
        var dictionary = Array.Empty<ValidationError>().ToPathDictionary();

        Assert.Empty(dictionary);
    }
}
