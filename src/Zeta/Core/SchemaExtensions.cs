using Zeta.Core;
using Zeta.Schemas;

namespace Zeta;

public static class SchemaExtensions
{
    /// <summary>
    /// Validates a value using a schema that expects no specific context (object?).
    /// </summary>
    public static ValueTask<Result<T>> ValidateAsync<T>(this ISchema<T, object?> schema, T value, ValidationExecutionContext? execution = null)
    {
        execution ??= ValidationExecutionContext.Empty;
        var context = new ValidationContext<object?>(null, execution);
        return schema.ValidateAsync(value, context);
    }

    public static ValueTask<Result<T>> ValidateAsync<T, TContext>(this ISchema<T, TContext> schema, T value, TContext data)
    {
        return schema.ValidateAsync(value, new ValidationContext<TContext>(data, ValidationExecutionContext.Empty));
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
}
