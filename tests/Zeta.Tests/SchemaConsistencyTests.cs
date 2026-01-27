using Zeta.Schemas;

namespace Zeta.Tests;

/// <summary>
/// Tests that verify schema types have consistent method signatures.
/// </summary>
public class SchemaConsistencyTests
{
    [Fact]
    public void WithContext_StringSchema_ReturnsTypedContextAwareSchema()
    {
        var contextless = Z.String();
        var contextAware = contextless.WithContext<object>();

        Assert.IsType<StringSchema<object>>(contextAware);
    }

    [Fact]
    public void WithContext_IntSchema_ReturnsTypedContextAwareSchema()
    {
        var contextless = Z.Int();
        var contextAware = contextless.WithContext<object>();

        Assert.IsType<IntSchema<object>>(contextAware);
    }

    [Fact]
    public void WithContext_DoubleSchema_ReturnsTypedContextAwareSchema()
    {
        var contextless = Z.Double();
        var contextAware = contextless.WithContext<object>();

        Assert.IsType<DoubleSchema<object>>(contextAware);
    }

    [Fact]
    public void WithContext_DecimalSchema_ReturnsTypedContextAwareSchema()
    {
        var contextless = Z.Decimal();
        var contextAware = contextless.WithContext<object>();

        Assert.IsType<DecimalSchema<object>>(contextAware);
    }

    [Fact]
    public void WithContext_BoolSchema_ReturnsTypedContextAwareSchema()
    {
        var contextless = Z.Bool();
        var contextAware = contextless.WithContext<object>();

        Assert.IsType<BoolSchema<object>>(contextAware);
    }

    [Fact]
    public void WithContext_GuidSchema_ReturnsTypedContextAwareSchema()
    {
        var contextless = Z.Guid();
        var contextAware = contextless.WithContext<object>();

        Assert.IsType<GuidSchema<object>>(contextAware);
    }

    [Fact]
    public void WithContext_DateTimeSchema_ReturnsTypedContextAwareSchema()
    {
        var contextless = Z.DateTime();
        var contextAware = contextless.WithContext<object>();

        Assert.IsType<DateTimeSchema<object>>(contextAware);
    }

    [Fact]
    public void WithContext_DateOnlySchema_ReturnsTypedContextAwareSchema()
    {
        var contextless = Z.DateOnly();
        var contextAware = contextless.WithContext<object>();

        Assert.IsType<DateOnlySchema<object>>(contextAware);
    }

    [Fact]
    public void WithContext_TimeOnlySchema_ReturnsTypedContextAwareSchema()
    {
        var contextless = Z.TimeOnly();
        var contextAware = contextless.WithContext<object>();

        Assert.IsType<TimeOnlySchema<object>>(contextAware);
    }

    [Fact]
    public void WithContext_ObjectSchema_ReturnsTypedContextAwareSchema()
    {
        var contextless = Z.Object<TestClass>();
        var contextAware = contextless.WithContext<object>();

        Assert.IsType<ObjectSchema<TestClass, object>>(contextAware);
    }

    [Fact]
    public void WithContext_ArraySchema_ReturnsTypedContextAwareSchema()
    {
        var contextless = Z.Array(Z.Int());
        var contextAware = contextless.WithContext<object>();

        Assert.IsType<ArraySchema<int, object>>(contextAware);
    }

    [Fact]
    public void WithContext_ListSchema_ReturnsTypedContextAwareSchema()
    {
        var contextless = Z.List(Z.String());
        var contextAware = contextless.WithContext<object>();

        Assert.IsType<ListSchema<string, object>>(contextAware);
    }

    private class TestClass { }
}
