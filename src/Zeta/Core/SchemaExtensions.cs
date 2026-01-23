using Zeta.Core;

namespace Zeta;

public static class SchemaExtensions
{
    /// <summary>
    /// Validates a value using a schema that expects no specific context (object?).
    /// </summary>
    public static Task<Result<T>> ValidateAsync<T>(this ISchema<T, object?> schema, T value, ValidationExecutionContext? execution = null)
    {
        execution ??= ValidationExecutionContext.Empty;
        var context = new ValidationContext<object?>(null, execution);
        return schema.ValidateAsync(value, context);
    }

    public static async Task<Result<T>> ValidateAsync<T, TContext>(this ISchema<T, TContext> schema, T value, TContext data)
    {
        return await schema.ValidateAsync(value, new ValidationContext<TContext>(data, ValidationExecutionContext.Empty));
    }
}