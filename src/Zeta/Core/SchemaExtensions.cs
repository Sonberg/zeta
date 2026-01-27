using Zeta.Core;
using Zeta.Schemas;

namespace Zeta;

public static class SchemaExtensions
{
    /// <summary>
    /// Validates a value using a context-aware schema with context data.
    /// </summary>
    public static async ValueTask<Result<T>> ValidateAsync<T, TContext>(this ISchema<T, TContext> schema, T value, TContext data)
    {
        var result = await schema.ValidateAsync(value, new ValidationContext<TContext>(data, ValidationExecutionContext.Empty));

        return result.IsSuccess
            ? Result<T>.Success(value)
            : Result<T>.Failure(result.Errors);
    }

    // ==================== String Schema ====================

    /// <summary>
    /// Creates a nullable version of this string schema that accepts null values.
    /// </summary>
    public static NullableSchema<string, TContext> Nullable<TContext>(this StringSchema<TContext> schema)
    {
        return new NullableSchema<string, TContext>(schema);
    }

    /// <summary>
    /// Creates a nullable version of this string schema that accepts null values.
    /// </summary>
    public static NullableSchema<string> Nullable(this StringSchema schema)
    {
        return new NullableSchema<string>(schema);
    }

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableSchema<string, TContext> Optional<TContext>(this StringSchema<TContext> schema)
        => schema.Nullable();

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableSchema<string> Optional(this StringSchema schema)
        => schema.Nullable();

    // ==================== Int Schema ====================

    /// <summary>
    /// Creates a nullable version of this int schema that accepts null values.
    /// </summary>
    public static NullableValueSchema<int, TContext> Nullable<TContext>(this IntSchema<TContext> schema)
    {
        return new NullableValueSchema<int, TContext>(schema);
    }

    /// <summary>
    /// Creates a nullable version of this int schema that accepts null values.
    /// </summary>
    public static NullableValueSchema<int> Nullable(this IntSchema schema)
    {
        return new NullableValueSchema<int>(schema);
    }

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableValueSchema<int, TContext> Optional<TContext>(this IntSchema<TContext> schema)
        => schema.Nullable();

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableValueSchema<int> Optional(this IntSchema schema)
        => schema.Nullable();

    // ==================== Double Schema ====================

    /// <summary>
    /// Creates a nullable version of this double schema that accepts null values.
    /// </summary>
    public static NullableValueSchema<double, TContext> Nullable<TContext>(this DoubleSchema<TContext> schema)
    {
        return new NullableValueSchema<double, TContext>(schema);
    }

    /// <summary>
    /// Creates a nullable version of this double schema that accepts null values.
    /// </summary>
    public static NullableValueSchema<double> Nullable(this DoubleSchema schema)
    {
        return new NullableValueSchema<double>(schema);
    }

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableValueSchema<double, TContext> Optional<TContext>(this DoubleSchema<TContext> schema)
        => schema.Nullable();

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableValueSchema<double> Optional(this DoubleSchema schema)
        => schema.Nullable();

    // ==================== Decimal Schema ====================

    /// <summary>
    /// Creates a nullable version of this decimal schema that accepts null values.
    /// </summary>
    public static NullableValueSchema<decimal, TContext> Nullable<TContext>(this DecimalSchema<TContext> schema)
    {
        return new NullableValueSchema<decimal, TContext>(schema);
    }

    /// <summary>
    /// Creates a nullable version of this decimal schema that accepts null values.
    /// </summary>
    public static NullableValueSchema<decimal> Nullable(this DecimalSchema schema)
    {
        return new NullableValueSchema<decimal>(schema);
    }

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableValueSchema<decimal, TContext> Optional<TContext>(this DecimalSchema<TContext> schema)
        => schema.Nullable();

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableValueSchema<decimal> Optional(this DecimalSchema schema)
        => schema.Nullable();

    // ==================== Object Schema ====================

    /// <summary>
    /// Creates a nullable version of this object schema that accepts null values.
    /// </summary>
    public static NullableSchema<T, TContext> Nullable<T, TContext>(this ObjectSchema<T, TContext> schema) where T : class
    {
        return new NullableSchema<T, TContext>(schema);
    }

    /// <summary>
    /// Creates a nullable version of this object schema that accepts null values.
    /// </summary>
    public static NullableSchema<T> Nullable<T>(this ObjectSchema<T> schema) where T : class
    {
        return new NullableSchema<T>(schema);
    }

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableSchema<T, TContext> Optional<T, TContext>(this ObjectSchema<T, TContext> schema) where T : class
        => schema.Nullable();

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableSchema<T> Optional<T>(this ObjectSchema<T> schema) where T : class
        => schema.Nullable();

    // ==================== Array Schema ====================

    /// <summary>
    /// Creates a nullable version of this array schema that accepts null values.
    /// </summary>
    public static NullableSchema<TElement[], TContext> Nullable<TElement, TContext>(this ArraySchema<TElement, TContext> schema)
    {
        return new NullableSchema<TElement[], TContext>(schema);
    }

    /// <summary>
    /// Creates a nullable version of this array schema that accepts null values.
    /// </summary>
    public static NullableSchema<TElement[]> Nullable<TElement>(this ArraySchema<TElement> schema)
    {
        return new NullableSchema<TElement[]>(schema);
    }

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableSchema<TElement[], TContext> Optional<TElement, TContext>(this ArraySchema<TElement, TContext> schema)
        => schema.Nullable();

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableSchema<TElement[]> Optional<TElement>(this ArraySchema<TElement> schema)
        => schema.Nullable();

    // ==================== List Schema ====================

    /// <summary>
    /// Creates a nullable version of this list schema that accepts null values.
    /// </summary>
    public static NullableSchema<List<TElement>, TContext> Nullable<TElement, TContext>(this ListSchema<TElement, TContext> schema)
    {
        return new NullableSchema<List<TElement>, TContext>(schema);
    }

    /// <summary>
    /// Creates a nullable version of this list schema that accepts null values.
    /// </summary>
    public static NullableSchema<List<TElement>> Nullable<TElement>(this ListSchema<TElement> schema)
    {
        return new NullableSchema<List<TElement>>(schema);
    }

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableSchema<List<TElement>, TContext> Optional<TElement, TContext>(this ListSchema<TElement, TContext> schema)
        => schema.Nullable();

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableSchema<List<TElement>> Optional<TElement>(this ListSchema<TElement> schema)
        => schema.Nullable();

    // ==================== DateTime Schema ====================

    /// <summary>
    /// Creates a nullable version of this DateTime schema that accepts null values.
    /// </summary>
    public static NullableValueSchema<DateTime, TContext> Nullable<TContext>(this DateTimeSchema<TContext> schema)
    {
        return new NullableValueSchema<DateTime, TContext>(schema);
    }

    /// <summary>
    /// Creates a nullable version of this DateTime schema that accepts null values.
    /// </summary>
    public static NullableValueSchema<DateTime> Nullable(this DateTimeSchema schema)
    {
        return new NullableValueSchema<DateTime>(schema);
    }

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableValueSchema<DateTime, TContext> Optional<TContext>(this DateTimeSchema<TContext> schema)
        => schema.Nullable();

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableValueSchema<DateTime> Optional(this DateTimeSchema schema)
        => schema.Nullable();

#if !NETSTANDARD2_0
    // ==================== DateOnly Schema ====================

    /// <summary>
    /// Creates a nullable version of this DateOnly schema that accepts null values.
    /// </summary>
    public static NullableValueSchema<DateOnly, TContext> Nullable<TContext>(this DateOnlySchema<TContext> schema)
    {
        return new NullableValueSchema<DateOnly, TContext>(schema);
    }

    /// <summary>
    /// Creates a nullable version of this DateOnly schema that accepts null values.
    /// </summary>
    public static NullableValueSchema<DateOnly> Nullable(this DateOnlySchema schema)
    {
        return new NullableValueSchema<DateOnly>(schema);
    }

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableValueSchema<DateOnly, TContext> Optional<TContext>(this DateOnlySchema<TContext> schema)
        => schema.Nullable();

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableValueSchema<DateOnly> Optional(this DateOnlySchema schema)
        => schema.Nullable();

    // ==================== TimeOnly Schema ====================

    /// <summary>
    /// Creates a nullable version of this TimeOnly schema that accepts null values.
    /// </summary>
    public static NullableValueSchema<TimeOnly, TContext> Nullable<TContext>(this TimeOnlySchema<TContext> schema)
    {
        return new NullableValueSchema<TimeOnly, TContext>(schema);
    }

    /// <summary>
    /// Creates a nullable version of this TimeOnly schema that accepts null values.
    /// </summary>
    public static NullableValueSchema<TimeOnly> Nullable(this TimeOnlySchema schema)
    {
        return new NullableValueSchema<TimeOnly>(schema);
    }

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableValueSchema<TimeOnly, TContext> Optional<TContext>(this TimeOnlySchema<TContext> schema)
        => schema.Nullable();

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableValueSchema<TimeOnly> Optional(this TimeOnlySchema schema)
        => schema.Nullable();
#endif

    // ==================== Guid Schema ====================

    /// <summary>
    /// Creates a nullable version of this Guid schema that accepts null values.
    /// </summary>
    public static NullableValueSchema<Guid, TContext> Nullable<TContext>(this GuidSchema<TContext> schema)
    {
        return new NullableValueSchema<Guid, TContext>(schema);
    }

    /// <summary>
    /// Creates a nullable version of this Guid schema that accepts null values.
    /// </summary>
    public static NullableValueSchema<Guid> Nullable(this GuidSchema schema)
    {
        return new NullableValueSchema<Guid>(schema);
    }

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableValueSchema<Guid, TContext> Optional<TContext>(this GuidSchema<TContext> schema)
        => schema.Nullable();

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableValueSchema<Guid> Optional(this GuidSchema schema)
        => schema.Nullable();

    // ==================== Bool Schema ====================

    /// <summary>
    /// Creates a nullable version of this bool schema that accepts null values.
    /// </summary>
    public static NullableValueSchema<bool, TContext> Nullable<TContext>(this BoolSchema<TContext> schema)
    {
        return new NullableValueSchema<bool, TContext>(schema);
    }

    /// <summary>
    /// Creates a nullable version of this bool schema that accepts null values.
    /// </summary>
    public static NullableValueSchema<bool> Nullable(this BoolSchema schema)
    {
        return new NullableValueSchema<bool>(schema);
    }

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableValueSchema<bool, TContext> Optional<TContext>(this BoolSchema<TContext> schema)
        => schema.Nullable();

    /// <summary>
    /// Creates an optional version of this schema that skips validation when null.
    /// </summary>
    public static NullableValueSchema<bool> Optional(this BoolSchema schema)
        => schema.Nullable();

    // ==================== Implicit Promotion Extensions ====================

    /// <summary>
    /// Adds a field with a context-aware schema, automatically promoting the object schema to context-aware.
    /// </summary>
    public static ObjectSchema<T, TContext> Field<T, TProperty, TContext>(
        this ObjectSchema<T> schema,
        System.Linq.Expressions.Expression<Func<T, TProperty>> propertySelector,
        ISchema<TProperty, TContext> fieldSchema) where T : class
    {
        return schema.WithContext<TContext>().Field(propertySelector, fieldSchema);
    }

}
