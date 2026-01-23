namespace Zeta.Tests;

public class GuidSchemaTests
{
    [Fact]
    public async Task NotEmpty_Valid_ReturnsSuccess()
    {
        var schema = Z.Guid().NotEmpty();
        var result = await schema.ValidateAsync(Guid.NewGuid());
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task NotEmpty_Invalid_ReturnsFailure()
    {
        var schema = Z.Guid().NotEmpty();
        var result = await schema.ValidateAsync(Guid.Empty);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "not_empty");
    }

    [Fact]
    public async Task Version_V4_Valid_ReturnsSuccess()
    {
        var schema = Z.Guid().Version(4);
        // UUID v4 has version nibble = 4 in the 7th byte
        var v4Guid = Guid.NewGuid(); // NewGuid generates v4
        var result = await schema.ValidateAsync(v4Guid);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Refine_Valid_ReturnsSuccess()
    {
        var allowedGuids = new HashSet<Guid>
        {
            Guid.Parse("550e8400-e29b-41d4-a716-446655440000"),
            Guid.Parse("6ba7b810-9dad-11d1-80b4-00c04fd430c8")
        };

        var schema = Z.Guid().Refine(g => allowedGuids.Contains(g), "GUID not in allowed list");
        var result = await schema.ValidateAsync(Guid.Parse("550e8400-e29b-41d4-a716-446655440000"));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Refine_Invalid_ReturnsFailure()
    {
        var allowedGuids = new HashSet<Guid>
        {
            Guid.Parse("550e8400-e29b-41d4-a716-446655440000")
        };

        var schema = Z.Guid().Refine(g => allowedGuids.Contains(g), "GUID not in allowed list");
        var result = await schema.ValidateAsync(Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Message == "GUID not in allowed list");
    }

    [Fact]
    public async Task MultipleRules_AllValid_ReturnsSuccess()
    {
        var schema = Z.Guid()
            .NotEmpty()
            .Refine(g => g.ToString().StartsWith("5"), "Must start with 5");

        var guid = Guid.Parse("550e8400-e29b-41d4-a716-446655440000");
        var result = await schema.ValidateAsync(guid);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task MultipleRules_OneInvalid_ReturnsFailure()
    {
        var schema = Z.Guid()
            .NotEmpty()
            .Refine(g => g.ToString().StartsWith("5"), "Must start with 5");

        var result = await schema.ValidateAsync(Guid.Empty);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "not_empty");
    }

    [Fact]
    public async Task Nullable_AllowsNull()
    {
        var schema = Z.Guid().NotEmpty().Nullable();
        var result = await schema.ValidateAsync(null);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Nullable_ValidatesNonNull()
    {
        var schema = Z.Guid().NotEmpty().Nullable();
        var result = await schema.ValidateAsync(Guid.Empty);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "not_empty");
    }
}
