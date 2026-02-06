using Zeta.Core;

namespace Zeta.Schemas;

/// <summary>
/// A context-aware schema that wraps another schema and allows null values.
/// For reference types (classes).
/// </summary>
[Obsolete("This wrapper class is deprecated. Nullable schemas now use the IsNullable flag instead of wrappers. Use schema.Nullable() which sets the flag directly.", false)]
public class NullableContextSchema<T, TContext> : ContextSchema<T?, TContext> where T : class
{
    private readonly ISchema<T, TContext> _inner;

    public NullableContextSchema(ISchema<T, TContext> inner)
    {
        _inner = inner;
    }

    public NullableContextSchema(ISchema<T> inner)
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
/// A context-aware schema that wraps another schema and allows null values.
/// For value types (structs).
/// </summary>
[Obsolete("This wrapper class is deprecated. Nullable schemas now use the IsNullable flag instead of wrappers. Use schema.Nullable() which sets the flag directly.", false)]
public class NullableValueContextSchema<T, TContext> : ContextSchema<T?, TContext> where T : struct
{
    private readonly ISchema<T, TContext> _inner;

    public NullableValueContextSchema(ISchema<T, TContext> inner)
    {
        _inner = inner;
    }

    public NullableValueContextSchema(ISchema<T> inner)
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
