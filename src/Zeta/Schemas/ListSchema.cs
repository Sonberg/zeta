using Zeta.Core;

namespace Zeta.Schemas;

/// <summary>
/// A schema for validating List&lt;T&gt; where each element is validated by an inner schema.
/// </summary>
public class ListSchema<TElement, TContext> : ISchema<List<TElement>, TContext>
{
    private readonly ISchema<TElement, TContext> _elementSchema;
    private readonly List<IRule<List<TElement>, TContext>> _rules = [];

    public ListSchema(ISchema<TElement, TContext> elementSchema)
    {
        _elementSchema = elementSchema;
    }

    public async ValueTask<Result<List<TElement>>> ValidateAsync(List<TElement> value, ValidationContext<TContext> context)
    {
        List<ValidationError>? errors = null;

        // Validate list-level rules
        foreach (var rule in _rules)
        {
            var error = await rule.ValidateAsync(value, context);
            if (error != null)
            {
                errors ??= new List<ValidationError>();
                errors.Add(error);
            }
        }

        // Validate each element
        for (var i = 0; i < value.Count; i++)
        {
            var elementContext = new ValidationContext<TContext>(
                context.Data,
                context.Execution.PushIndex(i));

            var result = await _elementSchema.ValidateAsync(value[i], elementContext);
            if (result.IsFailure)
            {
                errors ??= new List<ValidationError>();
                errors.AddRange(result.Errors);
            }
        }

        return errors == null
            ? Result<List<TElement>>.Success(value)
            : Result<List<TElement>>.Failure(errors);
    }

    public ListSchema<TElement, TContext> Use(IRule<List<TElement>, TContext> rule)
    {
        _rules.Add(rule);
        return this;
    }

    public ListSchema<TElement, TContext> MinLength(int min, string? message = null)
    {
        return Use(new DelegateRule<List<TElement>, TContext>((val, ctx) =>
        {
            if (val.Count >= min) return ValueTaskHelper.NullError();
            return ValueTaskHelper.Error(new ValidationError(
                ctx.Execution.Path, "min_length", message ?? $"Must have at least {min} items"));
        }));
    }

    public ListSchema<TElement, TContext> MaxLength(int max, string? message = null)
    {
        return Use(new DelegateRule<List<TElement>, TContext>((val, ctx) =>
        {
            if (val.Count <= max) return ValueTaskHelper.NullError();
            return ValueTaskHelper.Error(new ValidationError(
                ctx.Execution.Path, "max_length", message ?? $"Must have at most {max} items"));
        }));
    }

    public ListSchema<TElement, TContext> Length(int exact, string? message = null)
    {
        return Use(new DelegateRule<List<TElement>, TContext>((val, ctx) =>
        {
            if (val.Count == exact) return ValueTaskHelper.NullError();
            return ValueTaskHelper.Error(new ValidationError(
                ctx.Execution.Path, "length", message ?? $"Must have exactly {exact} items"));
        }));
    }

    public ListSchema<TElement, TContext> NotEmpty(string? message = null)
    {
        return MinLength(1, message ?? "Must not be empty");
    }

    public ListSchema<TElement, TContext> Refine(Func<List<TElement>, TContext, bool> predicate, string message, string code = "custom_error")
    {
        return Use(new DelegateRule<List<TElement>, TContext>((val, ctx) =>
        {
            if (predicate(val, ctx.Data)) return ValueTaskHelper.NullError();
            return ValueTaskHelper.Error(new ValidationError(ctx.Execution.Path, code, message));
        }));
    }

    public ListSchema<TElement, TContext> Refine(Func<List<TElement>, bool> predicate, string message, string code = "custom_error")
    {
        return Refine((val, _) => predicate(val), message, code);
    }
}

/// <summary>
/// A schema for validating List&lt;T&gt; with default context.
/// </summary>
public sealed class ListSchema<TElement> : ISchema<List<TElement>>
{
    private readonly ListSchema<TElement, object?> _inner;

    public ListSchema(ISchema<TElement> elementSchema)
    {
        _inner = new ListSchema<TElement, object?>(new SchemaContextAdapter<TElement, object?>(elementSchema));
    }

    public ListSchema(ISchema<TElement, object?> elementSchema)
    {
        _inner = new ListSchema<TElement, object?>(elementSchema);
    }

    public ValueTask<Result<List<TElement>>> ValidateAsync(List<TElement> value, ValidationExecutionContext? execution = null)
    {
        execution ??= ValidationExecutionContext.Empty;
        var context = new ValidationContext<object?>(null, execution);
        return _inner.ValidateAsync(value, context);
    }

    public ValueTask<Result<List<TElement>>> ValidateAsync(List<TElement> value, ValidationContext<object?> context)
    {
        return _inner.ValidateAsync(value, context);
    }

    public ListSchema<TElement> MinLength(int min, string? message = null)
    {
        _inner.MinLength(min, message);
        return this;
    }

    public ListSchema<TElement> MaxLength(int max, string? message = null)
    {
        _inner.MaxLength(max, message);
        return this;
    }

    public ListSchema<TElement> Length(int exact, string? message = null)
    {
        _inner.Length(exact, message);
        return this;
    }

    public ListSchema<TElement> NotEmpty(string? message = null)
    {
        _inner.NotEmpty(message);
        return this;
    }

    public ListSchema<TElement> Refine(Func<List<TElement>, bool> predicate, string message, string code = "custom_error")
    {
        _inner.Refine(predicate, message, code);
        return this;
    }
}
