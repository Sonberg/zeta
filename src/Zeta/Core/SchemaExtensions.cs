using System.Linq.Expressions;
using Zeta.Adapters;
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
        var result = await schema.ValidateAsync(value, new ValidationContext<TContext>(data));

        return result.IsSuccess
            ? Result<T>.Success(value)
            : Result<T>.Failure(result.Errors);
    }

    public static async ValueTask<Result<T>> ValidateAsync<T>(this ISchema<T> schema, T? value) where T : class
    {
        return await schema.ValidateAsync(value, ValidationContext.Empty);
    }

    // ==================== Implicit Promotion Extensions ====================

    /// <summary>
    /// Adds a field with a context-aware schema, automatically promoting the object schema to context-aware.
    /// </summary>
    public static ObjectContextSchema<T, TContext> Field<T, TProperty, TContext>(
        this ObjectContextlessSchema<T> schema,
        Expression<Func<T, TProperty>> propertySelector,
        ISchema<TProperty, TContext> fieldSchema) where T : class
    {
        return PromoteField(schema, propertySelector, fieldSchema);
    }

    public static ObjectContextSchema<T, TContext> Property<T, TProperty, TContext>(
        this ObjectContextlessSchema<T> schema,
        Expression<Func<T, TProperty>> propertySelector,
        ISchema<TProperty, TContext> fieldSchema) where T : class
    {
        return Field(schema, propertySelector, fieldSchema);
    }

    /// <summary>
    /// Adds a field with a context-aware schema type from Zeta, automatically promoting the object schema to context-aware.
    /// This overload avoids ambiguity when a context-aware schema is also assignable to ISchema&lt;TProperty&gt;.
    /// </summary>
    public static ObjectContextSchema<T, TContext> Field<T, TProperty, TContext>(
        this ObjectContextlessSchema<T> schema,
        Expression<Func<T, TProperty>> propertySelector,
        IContextSchema<TProperty, TContext> fieldSchema)
        where T : class
    {
        return PromoteField(schema, propertySelector, (ISchema<TProperty, TContext>)fieldSchema);
    }

    public static ObjectContextSchema<T, TContext> Property<T, TProperty, TContext>(
        this ObjectContextlessSchema<T> schema,
        Expression<Func<T, TProperty>> propertySelector,
        IContextSchema<TProperty, TContext> fieldSchema)
        where T : class
    {
        return Field(schema, propertySelector, fieldSchema);
    }

    private static ObjectContextSchema<T, TContext> PromoteField<T, TProperty, TContext>(
        ObjectContextlessSchema<T> schema,
        Expression<Func<T, TProperty>> propertySelector,
        ISchema<TProperty, TContext> fieldSchema)
        where T : class
    {
        return schema.Using<TContext>().Field(propertySelector, fieldSchema);
    }

    // ==================== Nullability Adaption Extensions ====================

    /// <summary>
    /// Defines a field validation for a nullable reference type property using a non-nullable contextless schema.
    /// </summary>
    public static ObjectContextSchema<T, TContext> Field<T, TContext, TProperty>(
        this ObjectContextSchema<T, TContext> schema,
        Expression<Func<T, TProperty?>> propertySelector,
        ISchema<TProperty> fieldSchema)
        where T : class
        where TProperty : class
    {
        return schema.Field(propertySelector, new NullableReferenceContextAdapter<TProperty, TContext>(fieldSchema));
    }

    public static ObjectContextSchema<T, TContext> Property<T, TContext, TProperty>(
        this ObjectContextSchema<T, TContext> schema,
        Expression<Func<T, TProperty?>> propertySelector,
        ISchema<TProperty> fieldSchema)
        where T : class
        where TProperty : class
    {
        return Field(schema, propertySelector, fieldSchema);
    }

    /// <summary>
    /// Defines a field validation for a nullable value type property using a non-nullable contextless schema.
    /// </summary>
    public static ObjectContextSchema<T, TContext> Field<T, TContext, TProperty>(
        this ObjectContextSchema<T, TContext> schema,
        Expression<Func<T, TProperty?>> propertySelector,
        ISchema<TProperty> fieldSchema)
        where T : class
        where TProperty : struct
    {
        return schema.Field(propertySelector, new NullableStructContextAdapter<TProperty, TContext>(fieldSchema));
    }

    public static ObjectContextSchema<T, TContext> Property<T, TContext, TProperty>(
        this ObjectContextSchema<T, TContext> schema,
        Expression<Func<T, TProperty?>> propertySelector,
        ISchema<TProperty> fieldSchema)
        where T : class
        where TProperty : struct
    {
        return Field(schema, propertySelector, fieldSchema);
    }

    /// <summary>
    /// Defines a field validation for a nullable reference type property using a non-nullable schema.
    /// </summary>
    public static ObjectContextlessSchema<T> Field<T, TProperty>(
        this ObjectContextlessSchema<T> schema,
        Expression<Func<T, TProperty?>> propertySelector,
        ISchema<TProperty> fieldSchema)
        where T : class
        where TProperty : class
    {
         return schema.Field(propertySelector, new NullableReferenceContextlessAdapter<TProperty>(fieldSchema));
    }

    public static ObjectContextlessSchema<T> Property<T, TProperty>(
        this ObjectContextlessSchema<T> schema,
        Expression<Func<T, TProperty?>> propertySelector,
        ISchema<TProperty> fieldSchema)
        where T : class
        where TProperty : class
    {
        return Field(schema, propertySelector, fieldSchema);
    }

     /// <summary>
     /// Defines a field validation for a nullable value type property using a non-nullable schema.
     /// </summary>
     public static ObjectContextlessSchema<T> Field<T, TProperty>(
        this ObjectContextlessSchema<T> schema,
        Expression<Func<T, TProperty?>> propertySelector,
        ISchema<TProperty> fieldSchema)
        where T : class
        where TProperty : struct
    {
         return schema.Field(propertySelector, new NullableStructContextlessAdapter<TProperty>(fieldSchema));
    }

    public static ObjectContextlessSchema<T> Property<T, TProperty>(
        this ObjectContextlessSchema<T> schema,
        Expression<Func<T, TProperty?>> propertySelector,
        ISchema<TProperty> fieldSchema)
        where T : class
        where TProperty : struct
    {
        return Field(schema, propertySelector, fieldSchema);
    }
}
