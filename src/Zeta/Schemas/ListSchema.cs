using Zeta.Core;
using Zeta.Rules;

namespace Zeta.Schemas;

/// <summary>
/// A schema for validating List&lt;T&gt; where each element is validated by an inner schema.
/// </summary>
public class ListSchema<TElement, TContext> : ISchema<List<TElement>, TContext>
{
    private readonly ISchema<TElement, TContext> _elementSchema;
    private readonly List<ISyncRule<List<TElement>, TContext>> _rules = [];

    public ListSchema(ISchema<TElement, TContext> elementSchema)
    {
        _elementSchema = elementSchema;
    }

    public async ValueTask<Result> ValidateAsync(List<TElement> value, ValidationContext<TContext> context)
    {
        List<ValidationError>? errors = null;

        // Validate list-level rules
        foreach (var rule in _rules)
        {
            var error = rule.Validate(value, context);
            if (error == null) continue;
            errors ??= [];
            errors.Add(error);
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
                errors ??= [];
                errors.AddRange(result.Errors);
            }
        }

        return errors == null
            ? Result.Success()
            : Result.Failure(errors);
    }

    public ListSchema<TElement, TContext> Use(ISyncRule<List<TElement>, TContext> rule)
    {
        _rules.Add(rule);
        return this;
    }

    public ListSchema<TElement, TContext> MinLength(int min, string? message = null)
    {
        Use(new DelegateSyncRule<List<TElement>, TContext>((val, ctx) =>
            val.Count >= min
                ? null
                : new ValidationError(ctx.Execution.Path, "min_length", message ?? $"Must have at least {min} items")));
        return this;
    }

    public ListSchema<TElement, TContext> MaxLength(int max, string? message = null)
    {
        Use(new DelegateSyncRule<List<TElement>, TContext>((val, ctx) =>
            val.Count <= max
                ? null
                : new ValidationError(ctx.Execution.Path, "max_length", message ?? $"Must have at most {max} items")));
        return this;
    }

    public ListSchema<TElement, TContext> Length(int exact, string? message = null)
    {
        Use(new DelegateSyncRule<List<TElement>, TContext>((val, ctx) =>
            val.Count == exact
                ? null
                : new ValidationError(ctx.Execution.Path, "length", message ?? $"Must have exactly {exact} items")));
        return this;
    }

    public ListSchema<TElement, TContext> NotEmpty(string? message = null)
    {
        return MinLength(1, message ?? "Must not be empty");
    }

    public ListSchema<TElement, TContext> Refine(Func<List<TElement>, TContext, bool> predicate, string message, string code = "custom_error")
    {
        Use(new DelegateSyncRule<List<TElement>, TContext>((val, ctx) =>
            predicate(val, ctx.Data)
                ? null
                : new ValidationError(ctx.Execution.Path, code, message)));
        return this;
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

    public async ValueTask<Result<List<TElement>>> ValidateAsync(List<TElement> value, ValidationExecutionContext? execution = null)
    {
        execution ??= ValidationExecutionContext.Empty;
        var context = new ValidationContext<object?>(null, execution);
        var result = await _inner.ValidateAsync(value, context);

        return result.IsSuccess
            ? Result<List<TElement>>.Success(value)
            : Result<List<TElement>>.Failure(result.Errors);
    }

    public async ValueTask<Result> ValidateAsync(List<TElement> value, ValidationContext<object?> context)
    {
        var result = await _inner.ValidateAsync(value, context);

        return result.IsSuccess
            ? Result.Success()
            : Result.Failure(result.Errors);
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
