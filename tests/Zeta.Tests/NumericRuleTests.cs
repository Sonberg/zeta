using System.Globalization;
using Zeta.Core;
using Zeta.Rules.Numeric;

namespace Zeta.Tests;

public class NumericRuleTests
{
    private static ValidationContext Context => new();

    // Int Min/Max Tests
    [Theory]
    [InlineData(10, 10, true)]
    [InlineData(11, 10, true)]
    [InlineData(9, 10, false)]
    [InlineData(int.MaxValue, 0, true)]
    [InlineData(int.MinValue, 0, false)]
    public async Task MinIntRule_ValidatesCorrectly(int value, int min, bool shouldPass)
    {
        var rule = new MinIntRule(min);
        var error = await rule.ValidateAsync(value, Context);

        if (shouldPass)
            Assert.Null(error);
        else
        {
            Assert.NotNull(error);
            Assert.Equal("min_value", error.Code);
            Assert.Contains($"at least {min}", error.Message);
        }
    }

    [Theory]
    [InlineData(10, 10, true)]
    [InlineData(9, 10, true)]
    [InlineData(11, 10, false)]
    public async Task MaxIntRule_ValidatesCorrectly(int value, int max, bool shouldPass)
    {
        var rule = new MaxIntRule(max);
        var error = await rule.ValidateAsync(value, Context);

        if (shouldPass)
            Assert.Null(error);
        else
        {
            Assert.NotNull(error);
            Assert.Equal("max_value", error.Code);
            Assert.Contains($"at most {max}", error.Message);
        }
    }

    // Double Min/Max Tests
    [Theory]
    [InlineData(10.5, 10.5, true)]
    [InlineData(10.6, 10.5, true)]
    [InlineData(10.4, 10.5, false)]
    public async Task MinDoubleRule_ValidatesCorrectly(double value, double min, bool shouldPass)
    {
        var rule = new MinDoubleRule(min);
        var error = await rule.ValidateAsync(value, Context);

        if (shouldPass)
            Assert.Null(error);
        else
        {
            Assert.NotNull(error);
            Assert.Equal("min_value", error.Code);
        }
    }

    [Theory]
    [InlineData(10.5, 10.5, true)]
    [InlineData(10.4, 10.5, true)]
    [InlineData(10.6, 10.5, false)]
    public async Task MaxDoubleRule_ValidatesCorrectly(double value, double max, bool shouldPass)
    {
        var rule = new MaxDoubleRule(max);
        var error = await rule.ValidateAsync(value, Context);

        if (shouldPass)
            Assert.Null(error);
        else
        {
            Assert.NotNull(error);
            Assert.Equal("max_value", error.Code);
        }
    }

    // Decimal Min/Max Tests
    [Theory]
    [InlineData("10.5", "10.5", true)]
    [InlineData("10.6", "10.5", true)]
    [InlineData("10.4", "10.5", false)]
    public async Task MinDecimalRule_ValidatesCorrectly(string valueStr, string minStr, bool shouldPass)
    {
        var value = decimal.Parse(valueStr, CultureInfo.InvariantCulture);
        var min = decimal.Parse(minStr, CultureInfo.InvariantCulture);
        var rule = new MinDecimalRule(min);
        var error = await rule.ValidateAsync(value, Context);

        if (shouldPass)
            Assert.Null(error);
        else
        {
            Assert.NotNull(error);
            Assert.Equal("min_value", error.Code);
        }
    }

    [Theory]
    [InlineData("10.5", "10.5", true)]
    [InlineData("10.4", "10.5", true)]
    [InlineData("10.6", "10.5", false)]
    public async Task MaxDecimalRule_ValidatesCorrectly(string valueStr, string maxStr, bool shouldPass)
    {
        var value = decimal.Parse(valueStr, CultureInfo.InvariantCulture);
        var max = decimal.Parse(maxStr, CultureInfo.InvariantCulture);
        var rule = new MaxDecimalRule(max);
        var error = await rule.ValidateAsync(value, Context);

        if (shouldPass)
            Assert.Null(error);
        else
        {
            Assert.NotNull(error);
            Assert.Equal("max_value", error.Code);
        }
    }

    // Positive Tests
    [Theory]
    [InlineData(0.001, true)]
    [InlineData(1.0, true)]
    [InlineData(0.0, false)]
    [InlineData(-0.001, false)]
    public async Task PositiveDoubleRule_ValidatesCorrectly(double value, bool shouldPass)
    {
        var rule = new PositiveDoubleRule();
        var error = await rule.ValidateAsync(value, Context);

        if (shouldPass)
            Assert.Null(error);
        else
        {
            Assert.NotNull(error);
            Assert.Equal("positive", error.Code);
            Assert.Contains("positive", error.Message);
        }
    }

    [Theory]
    [InlineData("0.01", true)]
    [InlineData("1.0", true)]
    [InlineData("0.0", false)]
    [InlineData("-0.01", false)]
    public async Task PositiveDecimalRule_ValidatesCorrectly(string valueStr, bool shouldPass)
    {
        var value = decimal.Parse(valueStr, CultureInfo.InvariantCulture);
        var rule = new PositiveDecimalRule();
        var error = await rule.ValidateAsync(value, Context);

        if (shouldPass)
            Assert.Null(error);
        else
        {
            Assert.NotNull(error);
            Assert.Equal("positive", error.Code);
        }
    }

    // Negative Tests
    [Theory]
    [InlineData(-0.001, true)]
    [InlineData(-1.0, true)]
    [InlineData(0.0, false)]
    [InlineData(0.001, false)]
    public async Task NegativeDoubleRule_ValidatesCorrectly(double value, bool shouldPass)
    {
        var rule = new NegativeDoubleRule();
        var error = await rule.ValidateAsync(value, Context);

        if (shouldPass)
            Assert.Null(error);
        else
        {
            Assert.NotNull(error);
            Assert.Equal("negative", error.Code);
            Assert.Contains("negative", error.Message);
        }
    }

    [Theory]
    [InlineData("-0.01", true)]
    [InlineData("-1.0", true)]
    [InlineData("0.0", false)]
    [InlineData("0.01", false)]
    public async Task NegativeDecimalRule_ValidatesCorrectly(string valueStr, bool shouldPass)
    {
        var value = decimal.Parse(valueStr, CultureInfo.InvariantCulture);
        var rule = new NegativeDecimalRule();
        var error = await rule.ValidateAsync(value, Context);

        if (shouldPass)
            Assert.Null(error);
        else
        {
            Assert.NotNull(error);
            Assert.Equal("negative", error.Code);
        }
    }

    // Finite Tests
    [Fact]
    public async Task FiniteRule_ValidNumber_ReturnsNull()
    {
        var rule = new FiniteRule();
        var error = await rule.ValidateAsync(123.456, Context);
        Assert.Null(error);
    }

    [Fact]
    public async Task FiniteRule_MaxValue_ReturnsNull()
    {
        var rule = new FiniteRule();
        var error = await rule.ValidateAsync(double.MaxValue, Context);
        Assert.Null(error);
    }

    [Fact]
    public async Task FiniteRule_PositiveInfinity_ReturnsError()
    {
        var rule = new FiniteRule();
        var error = await rule.ValidateAsync(double.PositiveInfinity, Context);

        Assert.NotNull(error);
        Assert.Equal("finite", error.Code);
        Assert.Contains("finite", error.Message);
    }

    [Fact]
    public async Task FiniteRule_NegativeInfinity_ReturnsError()
    {
        var rule = new FiniteRule();
        var error = await rule.ValidateAsync(double.NegativeInfinity, Context);

        Assert.NotNull(error);
        Assert.Equal("finite", error.Code);
    }

    [Fact]
    public async Task FiniteRule_NaN_ReturnsError()
    {
        var rule = new FiniteRule();
        var error = await rule.ValidateAsync(double.NaN, Context);

        Assert.NotNull(error);
        Assert.Equal("finite", error.Code);
    }

    // Precision Tests
    [Theory]
    [InlineData("10.55", 2, true)]
    [InlineData("10.5", 2, true)]
    [InlineData("10", 2, true)]
    [InlineData("10.555", 2, false)]
    [InlineData("10.1234", 3, false)]
    [InlineData("10.123", 3, true)]
    public async Task PrecisionRule_ValidatesCorrectly(string valueStr, int maxDecimals, bool shouldPass)
    {
        var value = decimal.Parse(valueStr, CultureInfo.InvariantCulture);
        var rule = new PrecisionRule(maxDecimals);
        var error = await rule.ValidateAsync(value, Context);

        if (shouldPass)
            Assert.Null(error);
        else
        {
            Assert.NotNull(error);
            Assert.Equal("precision", error.Code);
            Assert.Contains($"at most {maxDecimals}", error.Message);
        }
    }

    [Theory]
    [InlineData("0.00", 2, true)]
    [InlineData("0.1", 2, true)]
    [InlineData("0.12", 2, true)]
    public async Task PrecisionRule_TrailingZeros_ValidatesCorrectly(string valueStr, int maxDecimals, bool shouldPass)
    {
        // Note: Trailing zeros are lost when parsing decimals, so 0.000 becomes 0 with 0 decimal places
        var value = decimal.Parse(valueStr, CultureInfo.InvariantCulture);
        var rule = new PrecisionRule(maxDecimals);
        var error = await rule.ValidateAsync(value, Context);

        if (shouldPass)
            Assert.Null(error);
        else
            Assert.NotNull(error);
    }

    // MultipleOf Tests
    [Theory]
    [InlineData("1.75", "0.25", true)]
    [InlineData("2.00", "0.25", true)]
    [InlineData("1.30", "0.25", false)]
    [InlineData("10", "5", true)]
    [InlineData("11", "5", false)]
    public async Task MultipleOfRule_ValidatesCorrectly(string valueStr, string divisorStr, bool shouldPass)
    {
        var value = decimal.Parse(valueStr, CultureInfo.InvariantCulture);
        var divisor = decimal.Parse(divisorStr, CultureInfo.InvariantCulture);
        var rule = new MultipleOfRule(divisor);
        var error = await rule.ValidateAsync(value, Context);

        if (shouldPass)
            Assert.Null(error);
        else
        {
            Assert.NotNull(error);
            Assert.Equal("multiple_of", error.Code);
            Assert.Contains($"multiple of {divisor}", error.Message);
        }
    }

    [Fact]
    public async Task MultipleOfRule_Zero_IsMultipleOfAnything()
    {
        var rule = new MultipleOfRule(5m);
        var error = await rule.ValidateAsync(0m, Context);
        Assert.Null(error);
    }

    // Custom Message Tests
    [Fact]
    public async Task MinIntRule_WithCustomMessage_UsesCustomMessage()
    {
        var rule = new MinIntRule(10, "Custom minimum error");
        var error = await rule.ValidateAsync(5, Context);

        Assert.NotNull(error);
        Assert.Equal("Custom minimum error", error.Message);
    }

    [Fact]
    public async Task PrecisionRule_WithCustomMessage_UsesCustomMessage()
    {
        var rule = new PrecisionRule(2, "Too many decimals");
        var error = await rule.ValidateAsync(10.555m, Context);

        Assert.NotNull(error);
        Assert.Equal("Too many decimals", error.Message);
    }
}
