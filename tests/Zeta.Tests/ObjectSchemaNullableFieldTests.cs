using Xunit;
using Zeta.Schemas;

namespace Zeta.Tests;

/// <summary>
/// Tests for automatic nullable value type wrapping in ObjectSchema fields.
/// </summary>
public class ObjectSchemaNullableFieldTests
{
    private record TestModel(
        int RequiredInt,
        int? NullableInt,
        double? NullableDouble,
        decimal? NullableDecimal,
        bool? NullableBool,
        Guid? NullableGuid,
        DateTime? NullableDateTime
#if !NETSTANDARD2_0
        ,
        DateOnly? NullableDateOnly,
        TimeOnly? NullableTimeOnly
#endif
    );

    #region Int Tests

    [Fact]
    public async Task Field_NullableInt_InlineBuilder_NullValue_Succeeds()
    {
        var schema = Z.Object<TestModel>()
            .Field(x => x.NullableInt, s => s.Min(10).Max(100));

        var result = await schema.ValidateAsync(new TestModel(5, null, null, null, null, null, null
#if !NETSTANDARD2_0
            , null, null
#endif
        ));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Field_NullableInt_InlineBuilder_ValidValue_Succeeds()
    {
        var schema = Z.Object<TestModel>()
            .Field(x => x.NullableInt, s => s.Min(10).Max(100));

        var result = await schema.ValidateAsync(new TestModel(5, 50, null, null, null, null, null
#if !NETSTANDARD2_0
            , null, null
#endif
        ));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Field_NullableInt_InlineBuilder_InvalidValue_Fails()
    {
        var schema = Z.Object<TestModel>()
            .Field(x => x.NullableInt, s => s.Min(10).Max(100));

        var result = await schema.ValidateAsync(new TestModel(5, 5, null, null, null, null, null
#if !NETSTANDARD2_0
            , null, null
#endif
        ));
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Path == "nullableInt" && e.Code == "min_value");
    }

    [Fact]
    public async Task Field_NullableInt_PrebuiltSchema_NullValue_Succeeds()
    {
        var intSchema = Z.Int().Min(10).Max(100);
        var schema = Z.Object<TestModel>()
            .Field(x => x.NullableInt, intSchema);

        var result = await schema.ValidateAsync(new TestModel(5, null, null, null, null, null, null
#if !NETSTANDARD2_0
            , null, null
#endif
        ));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Field_NullableInt_PrebuiltSchema_ValidValue_Succeeds()
    {
        var intSchema = Z.Int().Min(10).Max(100);
        var schema = Z.Object<TestModel>()
            .Field(x => x.NullableInt, intSchema);

        var result = await schema.ValidateAsync(new TestModel(5, 50, null, null, null, null, null
#if !NETSTANDARD2_0
            , null, null
#endif
        ));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Field_NullableInt_PrebuiltSchema_InvalidValue_Fails()
    {
        var intSchema = Z.Int().Min(10).Max(100);
        var schema = Z.Object<TestModel>()
            .Field(x => x.NullableInt, intSchema);

        var result = await schema.ValidateAsync(new TestModel(5, 5, null, null, null, null, null
#if !NETSTANDARD2_0
            , null, null
#endif
        ));
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Path == "nullableInt" && e.Code == "min_value");
    }

    #endregion

    #region Double Tests

    [Fact]
    public async Task Field_NullableDouble_InlineBuilder_NullValue_Succeeds()
    {
        var schema = Z.Object<TestModel>()
            .Field(x => x.NullableDouble, s => s.Min(1.5).Max(10.5));

        var result = await schema.ValidateAsync(new TestModel(5, null, null, null, null, null, null
#if !NETSTANDARD2_0
            , null, null
#endif
        ));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Field_NullableDouble_InlineBuilder_ValidValue_Succeeds()
    {
        var schema = Z.Object<TestModel>()
            .Field(x => x.NullableDouble, s => s.Min(1.5).Max(10.5));

        var result = await schema.ValidateAsync(new TestModel(5, null, 5.0, null, null, null, null
#if !NETSTANDARD2_0
            , null, null
#endif
        ));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Field_NullableDouble_InlineBuilder_InvalidValue_Fails()
    {
        var schema = Z.Object<TestModel>()
            .Field(x => x.NullableDouble, s => s.Min(1.5).Max(10.5));

        var result = await schema.ValidateAsync(new TestModel(5, null, 0.5, null, null, null, null
#if !NETSTANDARD2_0
            , null, null
#endif
        ));
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Path == "nullableDouble" && e.Code == "min_value");
    }

    #endregion

    #region Decimal Tests

    [Fact]
    public async Task Field_NullableDecimal_InlineBuilder_NullValue_Succeeds()
    {
        var schema = Z.Object<TestModel>()
            .Field(x => x.NullableDecimal, s => s.Positive().Precision(2));

        var result = await schema.ValidateAsync(new TestModel(5, null, null, null, null, null, null
#if !NETSTANDARD2_0
            , null, null
#endif
        ));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Field_NullableDecimal_InlineBuilder_ValidValue_Succeeds()
    {
        var schema = Z.Object<TestModel>()
            .Field(x => x.NullableDecimal, s => s.Positive().Precision(2));

        var result = await schema.ValidateAsync(new TestModel(5, null, null, 10.5m, null, null, null
#if !NETSTANDARD2_0
            , null, null
#endif
        ));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Field_NullableDecimal_InlineBuilder_InvalidValue_Fails()
    {
        var schema = Z.Object<TestModel>()
            .Field(x => x.NullableDecimal, s => s.Positive().Precision(2));

        var result = await schema.ValidateAsync(new TestModel(5, null, null, -5.0m, null, null, null
#if !NETSTANDARD2_0
            , null, null
#endif
        ));
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Path == "nullableDecimal" && e.Code == "positive");
    }

    #endregion

    #region Bool Tests

    [Fact]
    public async Task Field_NullableBool_InlineBuilder_NullValue_Succeeds()
    {
        var schema = Z.Object<TestModel>()
            .Field(x => x.NullableBool, s => s.Refine(b => b == true, "Must be true"));

        var result = await schema.ValidateAsync(new TestModel(5, null, null, null, null, null, null
#if !NETSTANDARD2_0
            , null, null
#endif
        ));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Field_NullableBool_InlineBuilder_ValidValue_Succeeds()
    {
        var schema = Z.Object<TestModel>()
            .Field(x => x.NullableBool, s => s.Refine(b => b == true, "Must be true"));

        var result = await schema.ValidateAsync(new TestModel(5, null, null, null, true, null, null
#if !NETSTANDARD2_0
            , null, null
#endif
        ));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Field_NullableBool_InlineBuilder_InvalidValue_Fails()
    {
        var schema = Z.Object<TestModel>()
            .Field(x => x.NullableBool, s => s.Refine(b => b == true, "Must be true"));

        var result = await schema.ValidateAsync(new TestModel(5, null, null, null, false, null, null
#if !NETSTANDARD2_0
            , null, null
#endif
        ));
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Path == "nullableBool" && e.Code == "custom_error");
    }

    #endregion

    #region Guid Tests

    [Fact]
    public async Task Field_NullableGuid_InlineBuilder_NullValue_Succeeds()
    {
        var schema = Z.Object<TestModel>()
            .Field(x => x.NullableGuid, s => s.Refine(g => g != Guid.Empty, "Must not be empty"));

        var result = await schema.ValidateAsync(new TestModel(5, null, null, null, null, null, null
#if !NETSTANDARD2_0
            , null, null
#endif
        ));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Field_NullableGuid_InlineBuilder_ValidValue_Succeeds()
    {
        var validGuid = Guid.NewGuid();
        var schema = Z.Object<TestModel>()
            .Field(x => x.NullableGuid, s => s.Refine(g => g != Guid.Empty, "Must not be empty"));

        var result = await schema.ValidateAsync(new TestModel(5, null, null, null, null, validGuid, null
#if !NETSTANDARD2_0
            , null, null
#endif
        ));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Field_NullableGuid_InlineBuilder_InvalidValue_Fails()
    {
        var schema = Z.Object<TestModel>()
            .Field(x => x.NullableGuid, s => s.Refine(g => g != Guid.Empty, "Must not be empty"));

        var result = await schema.ValidateAsync(new TestModel(5, null, null, null, null, Guid.Empty, null
#if !NETSTANDARD2_0
            , null, null
#endif
        ));
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Path == "nullableGuid" && e.Code == "custom_error");
    }

    #endregion

    #region DateTime Tests

    [Fact]
    public async Task Field_NullableDateTime_InlineBuilder_NullValue_Succeeds()
    {
        var schema = Z.Object<TestModel>()
            .Field(x => x.NullableDateTime, s => s.Min(new DateTime(2020, 1, 1)));

        var result = await schema.ValidateAsync(new TestModel(5, null, null, null, null, null, null
#if !NETSTANDARD2_0
            , null, null
#endif
        ));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Field_NullableDateTime_InlineBuilder_ValidValue_Succeeds()
    {
        var schema = Z.Object<TestModel>()
            .Field(x => x.NullableDateTime, s => s.Min(new DateTime(2020, 1, 1)));

        var result = await schema.ValidateAsync(new TestModel(5, null, null, null, null, null, new DateTime(2023, 1, 1)
#if !NETSTANDARD2_0
            , null, null
#endif
        ));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Field_NullableDateTime_InlineBuilder_InvalidValue_Fails()
    {
        var schema = Z.Object<TestModel>()
            .Field(x => x.NullableDateTime, s => s.Min(new DateTime(2020, 1, 1)));

        var result = await schema.ValidateAsync(new TestModel(5, null, null, null, null, null, new DateTime(2019, 1, 1)
#if !NETSTANDARD2_0
            , null, null
#endif
        ));
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Path == "nullableDateTime" && e.Code == "min_date");
    }

    #endregion

#if !NETSTANDARD2_0
    #region DateOnly Tests

    [Fact]
    public async Task Field_NullableDateOnly_InlineBuilder_NullValue_Succeeds()
    {
        var schema = Z.Object<TestModel>()
            .Field(x => x.NullableDateOnly, s => s.Min(new DateOnly(2020, 1, 1)));

        var result = await schema.ValidateAsync(new TestModel(5, null, null, null, null, null, null, null, null));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Field_NullableDateOnly_InlineBuilder_ValidValue_Succeeds()
    {
        var schema = Z.Object<TestModel>()
            .Field(x => x.NullableDateOnly, s => s.Min(new DateOnly(2020, 1, 1)));

        var result = await schema.ValidateAsync(new TestModel(5, null, null, null, null, null, null, new DateOnly(2023, 1, 1), null));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Field_NullableDateOnly_InlineBuilder_InvalidValue_Fails()
    {
        var schema = Z.Object<TestModel>()
            .Field(x => x.NullableDateOnly, s => s.Min(new DateOnly(2020, 1, 1)));

        var result = await schema.ValidateAsync(new TestModel(5, null, null, null, null, null, null, new DateOnly(2019, 1, 1), null));
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Path == "nullableDateOnly" && e.Code == "min_date");
    }

    #endregion

    #region TimeOnly Tests

    [Fact]
    public async Task Field_NullableTimeOnly_InlineBuilder_NullValue_Succeeds()
    {
        var schema = Z.Object<TestModel>()
            .Field(x => x.NullableTimeOnly, s => s.Min(new TimeOnly(9, 0)));

        var result = await schema.ValidateAsync(new TestModel(5, null, null, null, null, null, null, null, null));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Field_NullableTimeOnly_InlineBuilder_ValidValue_Succeeds()
    {
        var schema = Z.Object<TestModel>()
            .Field(x => x.NullableTimeOnly, s => s.Min(new TimeOnly(9, 0)));

        var result = await schema.ValidateAsync(new TestModel(5, null, null, null, null, null, null, null, new TimeOnly(10, 0)));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Field_NullableTimeOnly_InlineBuilder_InvalidValue_Fails()
    {
        var schema = Z.Object<TestModel>()
            .Field(x => x.NullableTimeOnly, s => s.Min(new TimeOnly(9, 0)));

        var result = await schema.ValidateAsync(new TestModel(5, null, null, null, null, null, null, null, new TimeOnly(8, 0)));
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Path == "nullableTimeOnly" && e.Code == "min_time");
    }

    #endregion
#endif

    #region Multiple Nullable Fields

    [Fact]
    public async Task Field_MultipleNullableFields_AllNull_Succeeds()
    {
        var schema = Z.Object<TestModel>()
            .Field(x => x.NullableInt, s => s.Min(10))
            .Field(x => x.NullableDouble, s => s.Min(1.5))
            .Field(x => x.NullableDecimal, s => s.Positive());

        var result = await schema.ValidateAsync(new TestModel(5, null, null, null, null, null, null
#if !NETSTANDARD2_0
            , null, null
#endif
        ));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Field_MultipleNullableFields_AllValid_Succeeds()
    {
        var schema = Z.Object<TestModel>()
            .Field(x => x.NullableInt, s => s.Min(10))
            .Field(x => x.NullableDouble, s => s.Min(1.5))
            .Field(x => x.NullableDecimal, s => s.Positive());

        var result = await schema.ValidateAsync(new TestModel(5, 20, 5.0, 10.5m, null, null, null
#if !NETSTANDARD2_0
            , null, null
#endif
        ));
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Field_MultipleNullableFields_MultipleInvalid_ReturnsAllErrors()
    {
        var schema = Z.Object<TestModel>()
            .Field(x => x.NullableInt, s => s.Min(10))
            .Field(x => x.NullableDouble, s => s.Min(1.5))
            .Field(x => x.NullableDecimal, s => s.Positive());

        var result = await schema.ValidateAsync(new TestModel(5, 5, 0.5, -10.5m, null, null, null
#if !NETSTANDARD2_0
            , null, null
#endif
        ));
        Assert.False(result.IsSuccess);
        Assert.Equal(3, result.Errors.Count);
        Assert.Contains(result.Errors, e => e.Path == "nullableInt" && e.Code == "min_value");
        Assert.Contains(result.Errors, e => e.Path == "nullableDouble" && e.Code == "min_value");
        Assert.Contains(result.Errors, e => e.Path == "nullableDecimal" && e.Code == "positive");
    }

    #endregion

    #region Context-Aware Tests

    private record UserContext(string BannedEmail);

    [Fact]
    public async Task Field_NullableInt_ContextAware_NullValue_Succeeds()
    {
        var schema = Z.Object<TestModel>()
            .Field(x => x.NullableInt, s => s.Min(10).Max(100))
            .WithContext<UserContext>();

        var context = Z.Context(new UserContext("banned@test.com"));
        var result = await schema.ValidateAsync(new TestModel(5, null, null, null, null, null, null
#if !NETSTANDARD2_0
            , null, null
#endif
        ), context);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Field_NullableInt_ContextAware_ValidValue_Succeeds()
    {
        var schema = Z.Object<TestModel>()
            .Field(x => x.NullableInt, s => s.Min(10).Max(100))
            .WithContext<UserContext>();

        var context = Z.Context(new UserContext("banned@test.com"));
        var result = await schema.ValidateAsync(new TestModel(5, 50, null, null, null, null, null
#if !NETSTANDARD2_0
            , null, null
#endif
        ), context);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Field_NullableInt_ContextAware_InvalidValue_Fails()
    {
        var schema = Z.Object<TestModel>()
            .Field(x => x.NullableInt, s => s.Min(10).Max(100))
            .WithContext<UserContext>();

        var context = Z.Context(new UserContext("banned@test.com"));
        var result = await schema.ValidateAsync(new TestModel(5, 5, null, null, null, null, null
#if !NETSTANDARD2_0
            , null, null
#endif
        ), context);
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Path == "nullableInt" && e.Code == "min_value");
    }

    #endregion
}
