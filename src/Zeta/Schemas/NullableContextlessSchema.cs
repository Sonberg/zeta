using Zeta.Core;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema that wraps another schema and allows null values.
/// For reference types (classes).
/// </summary>
public sealed class NullableContextlessSchema<T> : ContextlessSchema<T?> where T : class
{
    private readonly ISchema<T> _inner;

    public NullableContextlessSchema(ISchema<T> inner)
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

    public NullableContextSchema<T, TContext> WithContext<TContext>()
        => new NullableContextSchema<T, TContext>(_inner);
}

/// <summary>
/// A contextless schema that wraps another schema and allows null values.
/// For value types (structs).
/// </summary>
public sealed class NullableValueContextlessSchema<T> : ContextlessSchema<T?> where T : struct
{
    private readonly ISchema<T> _inner;

    public NullableValueContextlessSchema(ISchema<T> inner)
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

    public NullableValueContextSchema<T, TContext> WithContext<TContext>()
        => new NullableValueContextSchema<T, TContext>(_inner);
}
