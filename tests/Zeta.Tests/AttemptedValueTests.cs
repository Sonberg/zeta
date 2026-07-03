namespace Zeta.Tests;

public class AttemptedValueTests
{
    private sealed record Item(int Quantity);
    private sealed record Container(List<Item> Items);

    [Fact]
    public async Task Error_CapturesTheOffendingLeafValue()
    {
        var schema = Z.Schema<Container>()
            .Property(x => x.Items, s => s.Each(Z.Schema<Item>().Property(i => i.Quantity, q => q.Min(1))));

        var result = await schema.ValidateAsync(new Container([new Item(0)]));

        Assert.True(result.IsFailure);
        var error = result.Errors[0];
        Assert.Equal("$.items[0].quantity", error.PathString);
        Assert.True(error.HasAttemptedValue);
        Assert.True(error.TryGetAttemptedValue<int>(out var value));
        Assert.Equal(0, value);
    }

    [Fact]
    public async Task Error_CaptureIsExact_ForCamelCasedPaths()
    {
        // The path renders "firstName" but the captured value is the actual string — no reflection round-trip.
        var schema = Z.String().MinLength(5);

        var result = await schema.ValidateAsync("hi");

        Assert.True(result.IsFailure);
        Assert.True(result.Errors[0].TryGetAttemptedValue<string>(out var value));
        Assert.Equal("hi", value);
    }

    [Fact]
    public void ManuallyConstructedError_HasNoAttemptedValue()
    {
        var error = new ValidationError("$.x", "code", "message");

        Assert.False(error.HasAttemptedValue);
        Assert.False(error.TryGetAttemptedValue<int>(out _));
    }
}
