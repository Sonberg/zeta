namespace Zeta.Schemas;

/// <summary>
/// A schema for validating arrays where each element is validated by an inner schema.
/// </summary>
public class ArraySchema<TElement, TContext> : ISchema<TElement[], TContext>
{
    private readonly ISchema<TElement, TContext> _elementSchema;
    private readonly List<IRule<TElement[], TContext>> _rules = [];

    public ArraySchema(ISchema<TElement, TContext> elementSchema)
    {
        _elementSchema = elementSchema;
    }

    public async Task<Result<TElement[]>> ValidateAsync(TElement[] value, ValidationContext<TContext> context)
    {
        var errors = new List<ValidationError>();

        // Validate array-level rules
        foreach (var rule in _rules)
        {
            var error = await rule.ValidateAsync(value, context);
            if (error != null) errors.Add(error);
        }

        // Validate each element
        for (var i = 0; i < value.Length; i++)
        {
            var elementContext = new ValidationContext<TContext>(
                context.Data,
                context.Execution.PushIndex(i));

            var result = await _elementSchema.ValidateAsync(value[i], elementContext);
            if (result.IsFailure)
            {
                errors.AddRange(result.Errors);
            }
        }

        return errors.Count == 0
            ? Result<TElement[]>.Success(value)
            : Result<TElement[]>.Failure(errors);
    }

    public ArraySchema<TElement, TContext> Use(IRule<TElement[], TContext> rule)
    {
        _rules.Add(rule);
        return this;
    }

    public ArraySchema<TElement, TContext> MinLength(int min, string? message = null)
    {
        return Use(new DelegateRule<TElement[], TContext>((val, ctx) =>
        {
            if (val.Length >= min) return ValueTask.FromResult<ValidationError?>(null);
            return ValueTask.FromResult<ValidationError?>(new ValidationError(
                ctx.Execution.Path, "min_length", message ?? $"Must have at least {min} items"));
        }));
    }

    public ArraySchema<TElement, TContext> MaxLength(int max, string? message = null)
    {
        return Use(new DelegateRule<TElement[], TContext>((val, ctx) =>
        {
            if (val.Length <= max) return ValueTask.FromResult<ValidationError?>(null);
            return ValueTask.FromResult<ValidationError?>(new ValidationError(
                ctx.Execution.Path, "max_length", message ?? $"Must have at most {max} items"));
        }));
    }

    public ArraySchema<TElement, TContext> Length(int exact, string? message = null)
    {
        return Use(new DelegateRule<TElement[], TContext>((val, ctx) =>
        {
            if (val.Length == exact) return ValueTask.FromResult<ValidationError?>(null);
            return ValueTask.FromResult<ValidationError?>(new ValidationError(
                ctx.Execution.Path, "length", message ?? $"Must have exactly {exact} items"));
        }));
    }

    public ArraySchema<TElement, TContext> NotEmpty(string? message = null)
    {
        return MinLength(1, message ?? "Must not be empty");
    }

    public ArraySchema<TElement, TContext> Refine(Func<TElement[], TContext, bool> predicate, string message, string code = "custom_error")
    {
        return Use(new DelegateRule<TElement[], TContext>((val, ctx) =>
        {
            if (predicate(val, ctx.Data)) return ValueTask.FromResult<ValidationError?>(null);
            return ValueTask.FromResult<ValidationError?>(new ValidationError(ctx.Execution.Path, code, message));
        }));
    }

    public ArraySchema<TElement, TContext> Refine(Func<TElement[], bool> predicate, string message, string code = "custom_error")
    {
        return Refine((val, _) => predicate(val), message, code);
    }
}

/// <summary>
/// A schema for validating arrays with default context.
/// </summary>
public sealed class ArraySchema<TElement> : ISchema<TElement[]>
{
    private readonly ArraySchema<TElement, object?> _inner;

    public ArraySchema(ISchema<TElement> elementSchema)
    {
        _inner = new ArraySchema<TElement, object?>(new SchemaContextAdapter<TElement, object?>(elementSchema));
    }

    public ArraySchema(ISchema<TElement, object?> elementSchema)
    {
        _inner = new ArraySchema<TElement, object?>(elementSchema);
    }

    public Task<Result<TElement[]>> ValidateAsync(TElement[] value, ValidationExecutionContext? execution = null)
    {
        execution ??= ValidationExecutionContext.Empty;
        var context = new ValidationContext<object?>(null, execution);
        return _inner.ValidateAsync(value, context);
    }

    public Task<Result<TElement[]>> ValidateAsync(TElement[] value, ValidationContext<object?> context)
    {
        return _inner.ValidateAsync(value, context);
    }

    public ArraySchema<TElement> MinLength(int min, string? message = null)
    {
        _inner.MinLength(min, message);
        return this;
    }

    public ArraySchema<TElement> MaxLength(int max, string? message = null)
    {
        _inner.MaxLength(max, message);
        return this;
    }

    public ArraySchema<TElement> Length(int exact, string? message = null)
    {
        _inner.Length(exact, message);
        return this;
    }

    public ArraySchema<TElement> NotEmpty(string? message = null)
    {
        _inner.NotEmpty(message);
        return this;
    }

    public ArraySchema<TElement> Refine(Func<TElement[], bool> predicate, string message, string code = "custom_error")
    {
        _inner.Refine(predicate, message, code);
        return this;
    }
}
