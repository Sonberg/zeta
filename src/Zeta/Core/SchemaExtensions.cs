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