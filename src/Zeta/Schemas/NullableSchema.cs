using Zeta.Core;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema that wraps another schema and allows null values.
/// For reference types (classes).
/// </summary>
public sealed class NullableSchema<T> : ContextlessSchema<T?> where T : class
{
    private readonly ISchema<T> _inner;

    public NullableSchema(ISchema<T> inner)
    {
        _inner = inner;
    }

    public override async ValueTask<Result<T?>> ValidateAsync(T? value, ValidationExecutionContext? execution = null)
    {
        if (value is null)
        {
            return Result<T?>.Success(null);
        }

        var result = await _inner.ValidateAsync(value, execution);

        return result.IsSuccess
            ? Result<T?>.Success(value)
            : Result<T?>.Failure(result.Errors);
    }
}

/// <summary>
/// A context-aware schema that wraps another schema and allows null values.
/// For reference types (classes).
/// </summary>
public class NullableSchema<T, TContext> : ContextSchema<T?, TContext> where T : class
{
    private readonly ISchema<T, TContext> _inner;

    public NullableSchema(ISchema<T, TContext> inner)
    {
        _inner = inner;
    }

    public NullableSchema(ISchema<T> inner)
    {
        _inner = new SchemaAdapter<T, TContext>(inner);
    }

    public override async ValueTask<Result> ValidateAsync(T? value, ValidationContext<TContext> context)
    {
        if (value is null)
        {
            return Result.Success();
        }

        return await _inner.ValidateAsync(value, context);
    }
}

/// <summary>
/// A contextless schema that wraps another schema and allows null values.
/// For value types (structs).
/// </summary>
public sealed class NullableValueSchema<T> : ContextlessSchema<T?> where T : struct
{
    private readonly ISchema<T> _inner;

    public NullableValueSchema(ISchema<T> inner)
    {
        _inner = inner;
    }

    public override async ValueTask<Result<T?>> ValidateAsync(T? value, ValidationExecutionContext? execution = null)
    {
        if (!value.HasValue)
        {
            return Result<T?>.Success(null);
        }

        var result = await _inner.ValidateAsync(value.Value, execution);
        return result.IsSuccess
            ? Result<T?>.Success(value)
            : Result<T?>.Failure(result.Errors);
    }
}

/// <summary>
/// A context-aware schema that wraps another schema and allows null values.
/// For value types (structs).
/// </summary>
public class NullableValueSchema<T, TContext> : ContextSchema<T?, TContext> where T : struct
{
    private readonly ISchema<T, TContext> _inner;

    public NullableValueSchema(ISchema<T, TContext> inner)
    {
        _inner = inner;
    }

    public NullableValueSchema(ISchema<T> inner)
    {
        _inner = new SchemaAdapter<T, TContext>(inner);
    }

    public override async ValueTask<Result> ValidateAsync(T? value, ValidationContext<TContext> context)
    {
        if (!value.HasValue)
        {
            return Result.Success();
        }

        return await _inner.ValidateAsync(value.Value, context);
    }
}