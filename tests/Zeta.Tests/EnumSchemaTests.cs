namespace Zeta.Tests;

public class EnumSchemaTests
{
    private enum Channel
    {
        Unknown = 0,
        Online = 1,
        Store = 2
    }

    private record Query(Channel Channel, Channel? OptionalChannel);

    [Fact]
    public async Task Defined_ValidValue_ReturnsSuccess()
    {
        var schema = Z.Enum<Channel>().Defined();

        var result = await schema.ValidateAsync(Channel.Online);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Defined_InvalidValue_ReturnsFailure()
    {
        var schema = Z.Enum<Channel>().Defined();

        var result = await schema.ValidateAsync((Channel)99);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "enum_defined");
    }

    [Fact]
    public async Task OneOf_InvalidValue_ReturnsFailure()
    {
        var schema = Z.Enum<Channel>().OneOf(Channel.Online, Channel.Store);

        var result = await schema.ValidateAsync(Channel.Unknown);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "enum_one_of");
    }

    [Fact]
    public async Task ContextAware_OneOf_UsesContextPromotion()
    {
        var schema = Z.Enum<Channel>()
            .Using<FeatureContext>()
            .Refine((value, ctx) => !ctx.ForbidStore || value != Channel.Store, "Store is disabled")
            .OneOf(Channel.Online, Channel.Store);

        var result = await schema.ValidateAsync(Channel.Store, new FeatureContext(true));

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Message == "Store is disabled");
    }

    [Fact]
    public async Task ObjectProperty_EnumBuilder_ReportsPropertyPath()
    {
        var schema = Z.Schema<Query>()
            .Property(x => x.Channel, s => s.OneOf(Channel.Online, Channel.Store));

        var result = await schema.ValidateAsync(new Query(Channel.Unknown, null));

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Path == "$.channel" && e.Code == "enum_one_of");
    }

    [Fact]
    public async Task ObjectProperty_NullableEnum_SkipsWhenNull()
    {
        var schema = Z.Schema<Query>()
            .Property(x => x.OptionalChannel, s => s.Nullable().Defined());

        var result = await schema.ValidateAsync(new Query(Channel.Online, null));

        Assert.True(result.IsSuccess);
    }

    private record FeatureContext(bool ForbidStore);
}
