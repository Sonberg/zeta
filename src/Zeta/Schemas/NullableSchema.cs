using Zeta.Core;

namespace Zeta.Schemas;

/// <summary>
/// A schema that wraps another schema and allows null values.
/// For reference types (classes).
/// </summary>
public class NullableSchema<T, TContext> : ISchema<T?, TContext> where T : class
{
    private readonly ISchema<T, TContext> _inner;

    public NullableSchema(ISchema<T, TContext> inner)
    {
        _inner = inner;
    }

    public async ValueTask<Result<T?>> ValidateAsync(T? value, ValidationContext<TContext> context)
    {
        if (value is null)
        {
            return Result<T?>.Success(null);
        }

        var result = await _inner.ValidateAsync(value, context);
        return result.Map<T?>(v => v);
    }
}

/// <summary>
/// A schema that wraps another schema and allows null values.
/// For reference types with default context.
/// </summary>
public sealed class NullableSchema<T> : NullableSchema<T, object?>, ISchema<T?> where T : class
{
    public NullableSchema(ISchema<T, object?> inner) : base(inner)
    {
    }

    public ValueTask<Result<T?>> ValidateAsync(T? value, ValidationExecutionContext? execution = null)
    {
        execution ??= ValidationExecutionContext.Empty;
        var context = new ValidationContext<object?>(null, execution);
        return ValidateAsync(value, context);
    }
}

/// <summary>
/// A schema that wraps another schema and allows null values.
/// For value types (structs).
/// </summary>
public class NullableValueSchema<T, TContext> : ISchema<T?, TContext> where T : struct
{
    private readonly ISchema<T, TContext> _inner;

    public NullableValueSchema(ISchema<T, TContext> inner)
    {
        _inner = inner;
    }

    public async ValueTask<Result<T?>> ValidateAsync(T? value, ValidationContext<TContext> context)
    {
        if (!value.HasValue)
        {
            return Result<T?>.Success(null);
        }

        var result = await _inner.ValidateAsync(value.Value, context);
        return result.Map<T?>(v => v);
    }
}

/// <summary>
/// A schema that wraps another schema and allows null values.
/// For value types with default context.
/// </summary>
public sealed class NullableValueSchema<T> : NullableValueSchema<T, object?>, ISchema<T?> where T : struct
{
    public NullableValueSchema(ISchema<T, object?> inner) : base(inner)
    {
    }

    public ValueTask<Result<T?>> ValidateAsync(T? value, ValidationExecutionContext? execution = null)
    {
        execution ??= ValidationExecutionContext.Empty;
        var context = new ValidationContext<object?>(null, execution);
        return ValidateAsync(value, context);
    }
}
