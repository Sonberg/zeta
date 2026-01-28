using Zeta.Core;
using Zeta.Schemas;

namespace Zeta.Tests;

public class SchemaFactoryTests
{
    [Fact]
    public void Create_String_ReturnsStringSchema()
    {
        var schema = SchemaFactory.Create<string>();

        Assert.NotNull(schema);
        Assert.IsType<StringContextlessSchema>(schema);
    }

    [Fact]
    public async Task Create_String_CreatesWorkingSchema()
    {
        var schema = SchemaFactory.Create<string>();
        var result = await schema.ValidateAsync("test");

        Assert.True(result.IsSuccess);
        Assert.Equal("test", result.Value);
    }

    [Fact]
    public void Create_Int_ReturnsIntSchema()
    {
        var schema = SchemaFactory.Create<int>();

        Assert.NotNull(schema);
        Assert.IsType<IntContextlessSchema>(schema);
    }

    [Fact]
    public async Task Create_Int_CreatesWorkingSchema()
    {
        var schema = SchemaFactory.Create<int>();
        var result = await schema.ValidateAsync(42);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Create_Double_ReturnsDoubleSchema()
    {
        var schema = SchemaFactory.Create<double>();

        Assert.NotNull(schema);
        Assert.IsType<DoubleContextlessSchema>(schema);
    }

    [Fact]
    public async Task Create_Double_CreatesWorkingSchema()
    {
        var schema = SchemaFactory.Create<double>();
        var result = await schema.ValidateAsync(3.14);

        Assert.True(result.IsSuccess);
        Assert.Equal(3.14, result.Value);
    }

    [Fact]
    public void Create_Decimal_ReturnsDecimalSchema()
    {
        var schema = SchemaFactory.Create<decimal>();

        Assert.NotNull(schema);
        Assert.IsType<DecimalContextlessSchema>(schema);
    }

    [Fact]
    public async Task Create_Decimal_CreatesWorkingSchema()
    {
        var schema = SchemaFactory.Create<decimal>();
        var result = await schema.ValidateAsync(99.99m);

        Assert.True(result.IsSuccess);
        Assert.Equal(99.99m, result.Value);
    }

    [Fact]
    public void Create_Bool_ReturnsBoolSchema()
    {
        var schema = SchemaFactory.Create<bool>();

        Assert.NotNull(schema);
        Assert.IsType<BoolContextlessSchema>(schema);
    }

    [Fact]
    public async Task Create_Bool_CreatesWorkingSchema()
    {
        var schema = SchemaFactory.Create<bool>();
        var result = await schema.ValidateAsync(true);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
    }

    [Fact]
    public void Create_Guid_ReturnsGuidSchema()
    {
        var schema = SchemaFactory.Create<Guid>();

        Assert.NotNull(schema);
        Assert.IsType<GuidContextlessSchema>(schema);
    }

    [Fact]
    public async Task Create_Guid_CreatesWorkingSchema()
    {
        var schema = SchemaFactory.Create<Guid>();
        var testGuid = Guid.NewGuid();
        var result = await schema.ValidateAsync(testGuid);

        Assert.True(result.IsSuccess);
        Assert.Equal(testGuid, result.Value);
    }

    [Fact]
    public void Create_DateTime_ReturnsDateTimeSchema()
    {
        var schema = SchemaFactory.Create<DateTime>();

        Assert.NotNull(schema);
        Assert.IsType<DateTimeContextlessSchema>(schema);
    }

    [Fact]
    public async Task Create_DateTime_CreatesWorkingSchema()
    {
        var schema = SchemaFactory.Create<DateTime>();
        var testDate = DateTime.UtcNow;
        var result = await schema.ValidateAsync(testDate);

        Assert.True(result.IsSuccess);
        Assert.Equal(testDate, result.Value);
    }

#if !NETSTANDARD2_0
    [Fact]
    public void Create_DateOnly_ReturnsDateOnlySchema()
    {
        var schema = SchemaFactory.Create<DateOnly>();

        Assert.NotNull(schema);
        Assert.IsType<DateOnlyContextlessSchema>(schema);
    }

    [Fact]
    public async Task Create_DateOnly_CreatesWorkingSchema()
    {
        var schema = SchemaFactory.Create<DateOnly>();
        var testDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var result = await schema.ValidateAsync(testDate);

        Assert.True(result.IsSuccess);
        Assert.Equal(testDate, result.Value);
    }

    [Fact]
    public void Create_TimeOnly_ReturnsTimeOnlySchema()
    {
        var schema = SchemaFactory.Create<TimeOnly>();

        Assert.NotNull(schema);
        Assert.IsType<TimeOnlyContextlessSchema>(schema);
    }

    [Fact]
    public async Task Create_TimeOnly_CreatesWorkingSchema()
    {
        var schema = SchemaFactory.Create<TimeOnly>();
        var testTime = TimeOnly.FromDateTime(DateTime.UtcNow);
        var result = await schema.ValidateAsync(testTime);

        Assert.True(result.IsSuccess);
        Assert.Equal(testTime, result.Value);
    }
#endif

    [Fact]
    public void Create_UnregisteredType_ThrowsNotSupportedException()
    {
        var exception = Assert.Throws<NotSupportedException>(() =>
            SchemaFactory.Create<UnregisteredTestType>());

        Assert.Contains("TProperty", exception.Message);
    }

    [Fact]
    public void Create_UnregisteredPrimitiveType_ThrowsNotSupportedException()
    {
        var exception = Assert.Throws<NotSupportedException>(() =>
            SchemaFactory.Create<long>());

        Assert.Contains("TProperty", exception.Message);
    }

    [Fact]
    public void Create_MultipleCallsForSameType_ReturnsSeparateInstances()
    {
        var schema1 = SchemaFactory.Create<string>();
        var schema2 = SchemaFactory.Create<string>();

        Assert.NotSame(schema1, schema2);
    }

    private class UnregisteredTestType { }
}
