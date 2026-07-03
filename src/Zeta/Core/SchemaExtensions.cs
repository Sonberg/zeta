namespace Zeta;

public static class SchemaExtensions
{
    /// <summary>
    /// Validates a value using a context-aware schema with context data.
    /// </summary>
    public static ValueTask<Result<T, TContext>> ValidateAsync<T, TContext>(this ISchema<T, TContext> schema, T value, TContext data)
    {
        return schema.ValidateAsync(value, new ValidationRun<TContext>(data));
    }

    public static async ValueTask<Result<T>> ValidateAsync<T>(this ISchema<T> schema, T? value)
    {
        return await schema.ValidateAsync(value, ValidationRun.Empty);
    }
}
