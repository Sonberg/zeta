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

    public async ValueTask<Result> ValidateAsync(T? value, ValidationContext<TContext> context)
    {
        if (value is null)
        {
            return Result.Success();
        }

        return await _inner.ValidateAsync(value, context);
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

    public async ValueTask<Result<T?>> ValidateAsync(T? value, ValidationExecutionContext? execution = null)
    {
        execution ??= ValidationExecutionContext.Empty;
        var context = new ValidationContext<object?>(null, execution);
        var result = await ValidateAsync(value, context);
        
        return result.IsSuccess
            ? Result<T?>.Success(value)
            : Result<T?>.Failure(result.Errors);
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

    public async ValueTask<Result> ValidateAsync(T? value, ValidationContext<TContext> context)
    {
        if (!value.HasValue)
        {
            return Result.Success();
        }

        return await _inner.ValidateAsync(value.Value, context);
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

    public async ValueTask<Result<T?>> ValidateAsync(T? value, ValidationExecutionContext? execution = null)
    {
        execution ??= ValidationExecutionContext.Empty;
        var context = new ValidationContext<object?>(null, execution);
        var result = await ValidateAsync(value, context);
        
        return result.IsSuccess
            ? Result<T?>.Success(value)
            : Result<T?>.Failure(result.Errors);
    }
}