using Zeta.Schemas;

namespace Zeta;

public static class SchemaExtensions
{
    /// <summary>
    /// Validates a value using a context-aware schema with context data.
    /// </summary>
    public static async ValueTask<Result<T>> ValidateAsync<T, TContext>(this ISchema<T, TContext> schema, T value, TContext data)
    {
        var result = await schema.ValidateAsync(value, new ValidationContext<TContext>(data));

        return result.IsSuccess
            ? Result<T>.Success(value)
            : Result<T>.Failure(result.Errors);
    }
    
    public static async ValueTask<Result<T>> ValidateAsync<T>(this ISchema<T> schema, T value)
    {
        return await schema.ValidateAsync(value, ValidationContext.Empty);
    }

    // ==================== String Schema ====================

    /// <summary>
    /// Creates a nullable version of this string schema that accepts null values.
    /// </summary>
    public static NullableContextSchema<string, TContext> Nullable<TContext>(this StringContextSchema<TContext> schema)
    {
        return new NullableContextSchema<string, TContext>(schema);
    }

    /// <summary>
    /// Creates a nullable version of this string schema that accepts null values.
    /// </summary>
    public static NullableContextlessSchema<string> Nullable(this StringContextlessSchema schema)
    {
        return new NullableContextlessSchema<string>(schema);
    }

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableContextSchema<string, TContext> Optional<TContext>(this StringContextSchema<TContext> schema)
        => schema.Nullable();

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableContextlessSchema<string> Optional(this StringContextlessSchema schema)
        => schema.Nullable();

    // ==================== Int Schema ====================

    /// <summary>
    /// Creates a nullable version of this int schema that accepts null values.
    /// </summary>
    public static NullableValueContextSchema<int, TContext> Nullable<TContext>(this IntContextSchema<TContext> schema)
    {
        return new NullableValueContextSchema<int, TContext>(schema);
    }

    /// <summary>
    /// Creates a nullable version of this int schema that accepts null values.
    /// </summary>
    public static NullableValueContextlessSchema<int> Nullable(this IntContextlessSchema schema)
    {
        return new NullableValueContextlessSchema<int>(schema);
    }

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableValueContextSchema<int, TContext> Optional<TContext>(this IntContextSchema<TContext> schema)
        => schema.Nullable();

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableValueContextlessSchema<int> Optional(this IntContextlessSchema schema)
        => schema.Nullable();

    // ==================== Double Schema ====================

    /// <summary>
    /// Creates a nullable version of this double schema that accepts null values.
    /// </summary>
    public static NullableValueContextSchema<double, TContext> Nullable<TContext>(this DoubleContextSchema<TContext> schema)
    {
        return new NullableValueContextSchema<double, TContext>(schema);
    }

    /// <summary>
    /// Creates a nullable version of this double schema that accepts null values.
    /// </summary>
    public static NullableValueContextlessSchema<double> Nullable(this DoubleContextlessSchema schema)
    {
        return new NullableValueContextlessSchema<double>(schema);
    }

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableValueContextSchema<double, TContext> Optional<TContext>(this DoubleContextSchema<TContext> schema)
        => schema.Nullable();

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableValueContextlessSchema<double> Optional(this DoubleContextlessSchema schema)
        => schema.Nullable();

    // ==================== Decimal Schema ====================

    /// <summary>
    /// Creates a nullable version of this decimal schema that accepts null values.
    /// </summary>
    public static NullableValueContextSchema<decimal, TContext> Nullable<TContext>(this DecimalContextSchema<TContext> schema)
    {
        return new NullableValueContextSchema<decimal, TContext>(schema);
    }

    /// <summary>
    /// Creates a nullable version of this decimal schema that accepts null values.
    /// </summary>
    public static NullableValueContextlessSchema<decimal> Nullable(this DecimalContextlessSchema schema)
    {
        return new NullableValueContextlessSchema<decimal>(schema);
    }

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableValueContextSchema<decimal, TContext> Optional<TContext>(this DecimalContextSchema<TContext> schema)
        => schema.Nullable();

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableValueContextlessSchema<decimal> Optional(this DecimalContextlessSchema schema)
        => schema.Nullable();

    // ==================== Object Schema ====================

    /// <summary>
    /// Creates a nullable version of this object schema that accepts null values.
    /// </summary>
    public static NullableContextSchema<T, TContext> Nullable<T, TContext>(this ObjectContextSchema<T, TContext> schema) where T : class
    {
        return new NullableContextSchema<T, TContext>(schema);
    }

    /// <summary>
    /// Creates a nullable version of this object schema that accepts null values.
    /// </summary>
    public static NullableContextlessSchema<T> Nullable<T>(this ObjectContextlessSchema<T> schema) where T : class
    {
        return new NullableContextlessSchema<T>(schema);
    }

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableContextSchema<T, TContext> Optional<T, TContext>(this ObjectContextSchema<T, TContext> schema) where T : class
        => schema.Nullable();

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableContextlessSchema<T> Optional<T>(this ObjectContextlessSchema<T> schema) where T : class
        => schema.Nullable();

    // ==================== Array Schema ====================

    /// <summary>
    /// Creates a nullable version of this array schema that accepts null values.
    /// </summary>
    public static NullableContextSchema<TElement[], TContext> Nullable<TElement, TContext>(this ArrayContextSchema<TElement, TContext> schema)
    {
        return new NullableContextSchema<TElement[], TContext>(schema);
    }

    /// <summary>
    /// Creates a nullable version of this array schema that accepts null values.
    /// </summary>
    public static NullableContextlessSchema<TElement[]> Nullable<TElement>(this ArrayContextlessSchema<TElement> schema)
    {
        return new NullableContextlessSchema<TElement[]>(schema);
    }

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableContextSchema<TElement[], TContext> Optional<TElement, TContext>(this ArrayContextSchema<TElement, TContext> schema)
        => schema.Nullable();

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableContextlessSchema<TElement[]> Optional<TElement>(this ArrayContextlessSchema<TElement> schema)
        => schema.Nullable();

    // ==================== List Schema ====================

    /// <summary>
    /// Creates a nullable version of this list schema that accepts null values.
    /// </summary>
    public static NullableContextSchema<List<TElement>, TContext> Nullable<TElement, TContext>(this ListContextSchema<TElement, TContext> schema)
    {
        return new NullableContextSchema<List<TElement>, TContext>(schema);
    }

    /// <summary>
    /// Creates a nullable version of this list schema that accepts null values.
    /// </summary>
    public static NullableContextlessSchema<List<TElement>> Nullable<TElement>(this ListContextlessSchema<TElement> schema)
    {
        return new NullableContextlessSchema<List<TElement>>(schema);
    }

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableContextSchema<List<TElement>, TContext> Optional<TElement, TContext>(this ListContextSchema<TElement, TContext> schema)
        => schema.Nullable();

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableContextlessSchema<List<TElement>> Optional<TElement>(this ListContextlessSchema<TElement> schema)
        => schema.Nullable();

    // ==================== DateTime Schema ====================

    /// <summary>
    /// Creates a nullable version of this DateTime schema that accepts null values.
    /// </summary>
    public static NullableValueContextSchema<DateTime, TContext> Nullable<TContext>(this DateTimeContextSchema<TContext> schema)
    {
        return new NullableValueContextSchema<DateTime, TContext>(schema);
    }

    /// <summary>
    /// Creates a nullable version of this DateTime schema that accepts null values.
    /// </summary>
    public static NullableValueContextlessSchema<DateTime> Nullable(this DateTimeContextlessSchema schema)
    {
        return new NullableValueContextlessSchema<DateTime>(schema);
    }

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableValueContextSchema<DateTime, TContext> Optional<TContext>(this DateTimeContextSchema<TContext> schema)
        => schema.Nullable();

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableValueContextlessSchema<DateTime> Optional(this DateTimeContextlessSchema schema)
        => schema.Nullable();

#if !NETSTANDARD2_0
    // ==================== DateOnly Schema ====================

    /// <summary>
    /// Creates a nullable version of this DateOnly schema that accepts null values.
    /// </summary>
    public static NullableValueContextSchema<DateOnly, TContext> Nullable<TContext>(this DateOnlyContextSchema<TContext> schema)
    {
        return new NullableValueContextSchema<DateOnly, TContext>(schema);
    }

    /// <summary>
    /// Creates a nullable version of this DateOnly schema that accepts null values.
    /// </summary>
    public static NullableValueContextlessSchema<DateOnly> Nullable(this DateOnlyContextlessSchema schema)
    {
        return new NullableValueContextlessSchema<DateOnly>(schema);
    }

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableValueContextSchema<DateOnly, TContext> Optional<TContext>(this DateOnlyContextSchema<TContext> schema)
        => schema.Nullable();

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableValueContextlessSchema<DateOnly> Optional(this DateOnlyContextlessSchema schema)
        => schema.Nullable();

    // ==================== TimeOnly Schema ====================

    /// <summary>
    /// Creates a nullable version of this TimeOnly schema that accepts null values.
    /// </summary>
    public static NullableValueContextSchema<TimeOnly, TContext> Nullable<TContext>(this TimeOnlyContextSchema<TContext> schema)
    {
        return new NullableValueContextSchema<TimeOnly, TContext>(schema);
    }

    /// <summary>
    /// Creates a nullable version of this TimeOnly schema that accepts null values.
    /// </summary>
    public static NullableValueContextlessSchema<TimeOnly> Nullable(this TimeOnlyContextlessSchema schema)
    {
        return new NullableValueContextlessSchema<TimeOnly>(schema);
    }

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableValueContextSchema<TimeOnly, TContext> Optional<TContext>(this TimeOnlyContextSchema<TContext> schema)
        => schema.Nullable();

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableValueContextlessSchema<TimeOnly> Optional(this TimeOnlyContextlessSchema schema)
        => schema.Nullable();
#endif

    // ==================== Guid Schema ====================

    /// <summary>
    /// Creates a nullable version of this Guid schema that accepts null values.
    /// </summary>
    public static NullableValueContextSchema<Guid, TContext> Nullable<TContext>(this GuidContextSchema<TContext> schema)
    {
        return new NullableValueContextSchema<Guid, TContext>(schema);
    }

    /// <summary>
    /// Creates a nullable version of this Guid schema that accepts null values.
    /// </summary>
    public static NullableValueContextlessSchema<Guid> Nullable(this GuidContextlessSchema schema)
    {
        return new NullableValueContextlessSchema<Guid>(schema);
    }

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableValueContextSchema<Guid, TContext> Optional<TContext>(this GuidContextSchema<TContext> schema)
        => schema.Nullable();

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableValueContextlessSchema<Guid> Optional(this GuidContextlessSchema schema)
        => schema.Nullable();

    // ==================== Bool Schema ====================

    /// <summary>
    /// Creates a nullable version of this bool schema that accepts null values.
    /// </summary>
    public static NullableValueContextSchema<bool, TContext> Nullable<TContext>(this BoolContextSchema<TContext> schema)
    {
        return new NullableValueContextSchema<bool, TContext>(schema);
    }

    /// <summary>
    /// Creates a nullable version of this bool schema that accepts null values.
    /// </summary>
    public static NullableValueContextlessSchema<bool> Nullable(this BoolContextlessSchema schema)
    {
        return new NullableValueContextlessSchema<bool>(schema);
    }

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableValueContextSchema<bool, TContext> Optional<TContext>(this BoolContextSchema<TContext> schema)
        => schema.Nullable();

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableValueContextlessSchema<bool> Optional(this BoolContextlessSchema schema)
        => schema.Nullable();

    // ==================== Implicit Promotion Extensions ====================

    /// <summary>
    /// Adds a field with a context-aware schema, automatically promoting the object schema to context-aware.
    /// </summary>
    public static ObjectContextSchema<T, TContext> Field<T, TProperty, TContext>(
        this ObjectContextlessSchema<T> schema,
        System.Linq.Expressions.Expression<Func<T, TProperty>> propertySelector,
        ISchema<TProperty, TContext> fieldSchema) where T : class
    {
        return schema.WithContext<TContext>().Field(propertySelector, fieldSchema);
    }
}