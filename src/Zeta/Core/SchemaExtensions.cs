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
    /// Marks this string schema as nullable, allowing null values to pass validation.
    /// </summary>
    public static NullableContextSchema<string, TContext> Nullable<TContext>(this StringContextSchema<TContext> schema)
    {
        return new NullableContextSchema<string, TContext>(schema);
    }

    /// <summary>
    /// Marks this string schema as nullable, allowing null values to pass validation.
    /// </summary>
    public static NullableContextlessSchema<string> Nullable(this StringContextlessSchema schema)
    {
        return new NullableContextlessSchema<string>(schema);
    }

    /// <summary>
    /// Marks this schema as optional, allowing null values to pass validation.
    /// </summary>
    public static NullableContextSchema<string, TContext> Optional<TContext>(this StringContextSchema<TContext> schema)
        => schema.Nullable();

    /// <summary>
    /// Marks this schema as optional, allowing null values to pass validation.
    /// </summary>
    public static NullableContextlessSchema<string> Optional(this StringContextlessSchema schema)
        => schema.Nullable();

    // ==================== Int Schema ====================

    /// <summary>
    /// Marks this int schema as nullable, allowing null values to pass validation.
    /// </summary>
    public static IntContextSchema<TContext> Nullable<TContext>(this IntContextSchema<TContext> schema)
    {
        schema.MarkAsNullable();
        return schema;
    }

    /// <summary>
    /// Marks this int schema as nullable, allowing null values to pass validation.
    /// </summary>
    public static IntContextlessSchema Nullable(this IntContextlessSchema schema)
    {
        schema.MarkAsNullable();
        return schema;
    }

    /// <summary>
    /// Marks this schema as optional, allowing null values to pass validation.
    /// </summary>
    public static IntContextSchema<TContext> Optional<TContext>(this IntContextSchema<TContext> schema)
        => schema.Nullable();

    /// <summary>
    /// Marks this schema as optional, allowing null values to pass validation.
    /// </summary>
    public static IntContextlessSchema Optional(this IntContextlessSchema schema)
        => schema.Nullable();

    // ==================== Double Schema ====================

    /// <summary>
    /// Marks this double schema as nullable, allowing null values to pass validation.
    /// </summary>
    public static DoubleContextSchema<TContext> Nullable<TContext>(this DoubleContextSchema<TContext> schema)
    {
        schema.MarkAsNullable();
        return schema;
    }

    /// <summary>
    /// Marks this double schema as nullable, allowing null values to pass validation.
    /// </summary>
    public static DoubleContextlessSchema Nullable(this DoubleContextlessSchema schema)
    {
        schema.MarkAsNullable();
        return schema;
    }

    /// <summary>
    /// Marks this schema as optional, allowing null values to pass validation.
    /// </summary>
    public static DoubleContextSchema<TContext> Optional<TContext>(this DoubleContextSchema<TContext> schema)
        => schema.Nullable();

    /// <summary>
    /// Marks this schema as optional, allowing null values to pass validation.
    /// </summary>
    public static DoubleContextlessSchema Optional(this DoubleContextlessSchema schema)
        => schema.Nullable();

    // ==================== Decimal Schema ====================

    /// <summary>
    /// Marks this decimal schema as nullable, allowing null values to pass validation.
    /// </summary>
    public static DecimalContextSchema<TContext> Nullable<TContext>(this DecimalContextSchema<TContext> schema)
    {
        schema.MarkAsNullable();
        return schema;
    }

    /// <summary>
    /// Marks this decimal schema as nullable, allowing null values to pass validation.
    /// </summary>
    public static DecimalContextlessSchema Nullable(this DecimalContextlessSchema schema)
    {
        schema.MarkAsNullable();
        return schema;
    }

    /// <summary>
    /// Marks this schema as optional, allowing null values to pass validation.
    /// </summary>
    public static DecimalContextSchema<TContext> Optional<TContext>(this DecimalContextSchema<TContext> schema)
        => schema.Nullable();

    /// <summary>
    /// Marks this schema as optional, allowing null values to pass validation.
    /// </summary>
    public static DecimalContextlessSchema Optional(this DecimalContextlessSchema schema)
        => schema.Nullable();

    // ==================== Object Schema ====================

    /// <summary>
    /// Marks this object schema as nullable, allowing null values to pass validation.
    /// </summary>
    public static NullableContextSchema<T, TContext> Nullable<T, TContext>(this ObjectContextSchema<T, TContext> schema) where T : class
    {
        return new NullableContextSchema<T, TContext>(schema);
    }

    /// <summary>
    /// Marks this object schema as nullable, allowing null values to pass validation.
    /// </summary>
    public static NullableContextlessSchema<T> Nullable<T>(this ObjectContextlessSchema<T> schema) where T : class
    {
        return new NullableContextlessSchema<T>(schema);
    }

    /// <summary>
    /// Marks this schema as optional, allowing null values to pass validation.
    /// </summary>
    public static NullableContextSchema<T, TContext> Optional<T, TContext>(this ObjectContextSchema<T, TContext> schema) where T : class
        => schema.Nullable();

    /// <summary>
    /// Marks this schema as optional, allowing null values to pass validation.
    /// </summary>
    public static NullableContextlessSchema<T> Optional<T>(this ObjectContextlessSchema<T> schema) where T : class
        => schema.Nullable();

    // ==================== Array Schema ====================

    /// <summary>
    /// Marks this array schema as nullable, allowing null values to pass validation.
    /// </summary>
    public static NullableContextSchema<ICollection<TElement>, TContext> Nullable<TElement, TContext>(this CollectionContextSchema<TElement, TContext> schema)
    {
        return new NullableContextSchema<ICollection<TElement>, TContext>(schema);
    }

    /// <summary>
    /// Marks this collection schema as nullable, allowing null values to pass validation.
    /// </summary>
    public static NullableContextlessSchema<ICollection<TElement>> Nullable<TElement>(this CollectionContextlessSchema<TElement> schema)
    {
        return new NullableContextlessSchema<ICollection<TElement>>(schema);
    }

    /// <summary>
    /// Marks this schema as optional, allowing null values to pass validation.
    /// </summary>
    public static NullableContextSchema<ICollection<TElement>, TContext> Optional<TElement, TContext>(this CollectionContextSchema<TElement, TContext> schema)
        => schema.Nullable();

    /// <summary>
    /// Marks this schema as optional, allowing null values to pass validation.
    /// </summary>
    public static NullableContextlessSchema<ICollection<TElement>> Optional<TElement>(this CollectionContextlessSchema<TElement> schema)
        => schema.Nullable();
    
    // ==================== DateTime Schema ====================

    /// <summary>
    /// Marks this DateTime schema as nullable, allowing null values to pass validation.
    /// </summary>
    public static DateTimeContextSchema<TContext> Nullable<TContext>(this DateTimeContextSchema<TContext> schema)
    {
        schema.MarkAsNullable(); return schema;
    }

    /// <summary>
    /// Marks this DateTime schema as nullable, allowing null values to pass validation.
    /// </summary>
    public static DateTimeContextlessSchema Nullable(this DateTimeContextlessSchema schema)
    {
        schema.MarkAsNullable(); return schema;
    }

    /// <summary>
    /// Marks this schema as optional, allowing null values to pass validation.
    /// </summary>
    public static DateTimeContextSchema<TContext> Optional<TContext>(this DateTimeContextSchema<TContext> schema)
        => schema.Nullable();

    /// <summary>
    /// Marks this schema as optional, allowing null values to pass validation.
    /// </summary>
    public static DateTimeContextlessSchema Optional(this DateTimeContextlessSchema schema)
        => schema.Nullable();

#if !NETSTANDARD2_0
    // ==================== DateOnly Schema ====================

    /// <summary>
    /// Marks this DateOnly schema as nullable, allowing null values to pass validation.
    /// </summary>
    public static DateOnlyContextSchema<TContext> Nullable<TContext>(this DateOnlyContextSchema<TContext> schema)
    {
        schema.MarkAsNullable(); return schema;
    }

    /// <summary>
    /// Marks this DateOnly schema as nullable, allowing null values to pass validation.
    /// </summary>
    public static DateOnlyContextlessSchema Nullable(this DateOnlyContextlessSchema schema)
    {
        schema.MarkAsNullable(); return schema;
    }

    /// <summary>
    /// Marks this schema as optional, allowing null values to pass validation.
    /// </summary>
    public static DateOnlyContextSchema<TContext> Optional<TContext>(this DateOnlyContextSchema<TContext> schema)
        => schema.Nullable();

    /// <summary>
    /// Marks this schema as optional, allowing null values to pass validation.
    /// </summary>
    public static DateOnlyContextlessSchema Optional(this DateOnlyContextlessSchema schema)
        => schema.Nullable();

    // ==================== TimeOnly Schema ====================

    /// <summary>
    /// Marks this TimeOnly schema as nullable, allowing null values to pass validation.
    /// </summary>
    public static TimeOnlyContextSchema<TContext> Nullable<TContext>(this TimeOnlyContextSchema<TContext> schema)
    {
        schema.MarkAsNullable(); return schema;
    }

    /// <summary>
    /// Marks this TimeOnly schema as nullable, allowing null values to pass validation.
    /// </summary>
    public static TimeOnlyContextlessSchema Nullable(this TimeOnlyContextlessSchema schema)
    {
        schema.MarkAsNullable(); return schema;
    }

    /// <summary>
    /// Marks this schema as optional, allowing null values to pass validation.
    /// </summary>
    public static TimeOnlyContextSchema<TContext> Optional<TContext>(this TimeOnlyContextSchema<TContext> schema)
        => schema.Nullable();

    /// <summary>
    /// Marks this schema as optional, allowing null values to pass validation.
    /// </summary>
    public static TimeOnlyContextlessSchema Optional(this TimeOnlyContextlessSchema schema)
        => schema.Nullable();
#endif

    // ==================== Guid Schema ====================

    /// <summary>
    /// Marks this Guid schema as nullable, allowing null values to pass validation.
    /// </summary>
    public static GuidContextSchema<TContext> Nullable<TContext>(this GuidContextSchema<TContext> schema)
    {
        schema.MarkAsNullable(); return schema;
    }

    /// <summary>
    /// Marks this Guid schema as nullable, allowing null values to pass validation.
    /// </summary>
    public static GuidContextlessSchema Nullable(this GuidContextlessSchema schema)
    {
        schema.MarkAsNullable(); return schema;
    }

    /// <summary>
    /// Marks this schema as optional, allowing null values to pass validation.
    /// </summary>
    public static GuidContextSchema<TContext> Optional<TContext>(this GuidContextSchema<TContext> schema)
        => schema.Nullable();

    /// <summary>
    /// Marks this schema as optional, allowing null values to pass validation.
    /// </summary>
    public static GuidContextlessSchema Optional(this GuidContextlessSchema schema)
        => schema.Nullable();

    // ==================== Bool Schema ====================

    /// <summary>
    /// Marks this bool schema as nullable, allowing null values to pass validation.
    /// </summary>
    public static BoolContextSchema<TContext> Nullable<TContext>(this BoolContextSchema<TContext> schema)
    {
        schema.MarkAsNullable(); return schema;
    }

    /// <summary>
    /// Marks this bool schema as nullable, allowing null values to pass validation.
    /// </summary>
    public static BoolContextlessSchema Nullable(this BoolContextlessSchema schema)
    {
        schema.MarkAsNullable(); return schema;
    }

    /// <summary>
    /// Marks this schema as optional, allowing null values to pass validation.
    /// </summary>
    public static BoolContextSchema<TContext> Optional<TContext>(this BoolContextSchema<TContext> schema)
        => schema.Nullable();

    /// <summary>
    /// Marks this schema as optional, allowing null values to pass validation.
    /// </summary>
    public static BoolContextlessSchema Optional(this BoolContextlessSchema schema)
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