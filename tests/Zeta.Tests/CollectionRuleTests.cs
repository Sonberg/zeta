using System.Collections.Generic;
using Zeta.Core;
using Zeta.Rules.Collection;

namespace Zeta.Tests;

public class CollectionRuleTests
{
    private static ValidationContext Context => new();

    [Theory]
    [InlineData(new[] { 1, 2, 3 }, 3, true)]
    [InlineData(new[] { 1, 2, 3, 4 }, 3, true)]
    [InlineData(new[] { 1, 2 }, 3, false)]
    [InlineData(new int[] { }, 1, false)]
    public async Task MinLengthRule_ValidatesCorrectly(int[] items, int min, bool shouldPass)
    {
        var rule = new MinLengthRule<int>(min);
        var error = await rule.ValidateAsync(items, Context);

        if (shouldPass)
            Assert.Null(error);
        else
        {
            Assert.NotNull(error);
            Assert.Equal("min_length", error.Code);
            Assert.Contains($"at least {min}", error.Message);
        }
    }

    [Theory]
    [InlineData(new[] { 1, 2, 3 }, 3, true)]
    [InlineData(new[] { 1, 2 }, 3, true)]
    [InlineData(new[] { 1, 2, 3, 4 }, 3, false)]
    public async Task MaxLengthRule_ValidatesCorrectly(int[] items, int max, bool shouldPass)
    {
        var rule = new MaxLengthRule<int>(max);
        var error = await rule.ValidateAsync(items, Context);

        if (shouldPass)
            Assert.Null(error);
        else
        {
            Assert.NotNull(error);
            Assert.Equal("max_length", error.Code);
            Assert.Contains($"at most {max}", error.Message);
        }
    }

    [Theory]
    [InlineData(new[] { 1, 2, 3 }, 3, true)]
    [InlineData(new[] { 1, 2 }, 3, false)]
    [InlineData(new[] { 1, 2, 3, 4 }, 3, false)]
    public async Task LengthRule_ValidatesCorrectly(int[] items, int exact, bool shouldPass)
    {
        var rule = new LengthRule<int>(exact);
        var error = await rule.ValidateAsync(items, Context);

        if (shouldPass)
            Assert.Null(error);
        else
        {
            Assert.NotNull(error);
            Assert.Equal("length", error.Code);
            Assert.Contains($"exactly {exact}", error.Message);
        }
    }

    [Theory]
    [InlineData(new[] { 1 }, true)]
    [InlineData(new[] { 1, 2, 3 }, true)]
    [InlineData(new int[] { }, false)]
    public async Task NotEmptyRule_ValidatesCorrectly(int[] items, bool shouldPass)
    {
        var rule = new NotEmptyRule<int>();
        var error = await rule.ValidateAsync(items, Context);

        if (shouldPass)
            Assert.Null(error);
        else
        {
            Assert.NotNull(error);
            Assert.Equal("min_length", error.Code);
            Assert.Contains("not be empty", error.Message);
        }
    }

    [Fact]
    public async Task MinLengthRule_WithList_ValidatesCorrectly()
    {
        var rule = new MinLengthRule<string>(2);
        var list = new List<string> { "a", "b", "c" };
        var error = await rule.ValidateAsync(list, Context);

        Assert.Null(error);
    }

    [Fact]
    public async Task MaxLengthRule_WithList_InvalidList_ReturnsError()
    {
        var rule = new MaxLengthRule<string>(2);
        var list = new List<string> { "a", "b", "c" };
        var error = await rule.ValidateAsync(list, Context);

        Assert.NotNull(error);
        Assert.Equal("max_length", error.Code);
    }

    [Fact]
    public async Task NotEmptyRule_WithList_EmptyList_ReturnsError()
    {
        var rule = new NotEmptyRule<string>();
        var list = new List<string>();
        var error = await rule.ValidateAsync(list, Context);

        Assert.NotNull(error);
        Assert.Equal("min_length", error.Code);
    }

    [Fact]
    public async Task LengthRule_WithZeroLength_ValidatesEmptyCollection()
    {
        var rule = new LengthRule<int>(0);
        var error = await rule.ValidateAsync(Array.Empty<int>(), Context);

        Assert.Null(error);
    }

    [Fact]
    public async Task MinLengthRule_WithCustomMessage_UsesCustomMessage()
    {
        var rule = new MinLengthRule<int>(3, "Collection too small");
        var error = await rule.ValidateAsync(new[] { 1, 2 }, Context);

        Assert.NotNull(error);
        Assert.Equal("Collection too small", error.Message);
    }

    [Fact]
    public async Task MaxLengthRule_WithCustomMessage_UsesCustomMessage()
    {
        var rule = new MaxLengthRule<int>(2, "Collection too large");
        var error = await rule.ValidateAsync(new[] { 1, 2, 3 }, Context);

        Assert.NotNull(error);
        Assert.Equal("Collection too large", error.Message);
    }

    [Fact]
    public async Task LengthRule_WithCustomMessage_UsesCustomMessage()
    {
        var rule = new LengthRule<int>(5, "Wrong size");
        var error = await rule.ValidateAsync(new[] { 1, 2, 3 }, Context);

        Assert.NotNull(error);
        Assert.Equal("Wrong size", error.Message);
    }

    [Fact]
    public async Task NotEmptyRule_WithCustomMessage_UsesCustomMessage()
    {
        var rule = new NotEmptyRule<int>("Cannot be empty");
        var error = await rule.ValidateAsync(Array.Empty<int>(), Context);

        Assert.NotNull(error);
        Assert.Equal("Cannot be empty", error.Message);
    }

    // Test with different collection types
    [Fact]
    public async Task MinLengthRule_WithHashSet_ValidatesCorrectly()
    {
        var rule = new MinLengthRule<string>(2);
        var hashSet = new HashSet<string> { "a", "b", "c" };
        var error = await rule.ValidateAsync(hashSet, Context);

        Assert.Null(error);
    }

    [Fact]
    public async Task MaxLengthRule_WithHashSet_InvalidSet_ReturnsError()
    {
        var rule = new MaxLengthRule<string>(2);
        var hashSet = new HashSet<string> { "a", "b", "c" };
        var error = await rule.ValidateAsync(hashSet, Context);

        Assert.NotNull(error);
        Assert.Equal("max_length", error.Code);
    }

    // Edge cases
    [Fact]
    public async Task MinLengthRule_WithVeryLargeMinimum_ValidatesCorrectly()
    {
        var rule = new MinLengthRule<int>(int.MaxValue);
        var error = await rule.ValidateAsync(new[] { 1, 2, 3 }, Context);

        Assert.NotNull(error);
        Assert.Equal("min_length", error.Code);
    }

    [Fact]
    public async Task MaxLengthRule_WithZeroMaximum_OnlyAllowsEmpty()
    {
        var rule = new MaxLengthRule<int>(0);

        var errorEmpty = await rule.ValidateAsync(Array.Empty<int>(), Context);
        Assert.Null(errorEmpty);

        var errorNonEmpty = await rule.ValidateAsync(new[] { 1 }, Context);
        Assert.NotNull(errorNonEmpty);
    }
}
