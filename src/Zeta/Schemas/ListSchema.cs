using Zeta.Core;
using Zeta.Rules;
using Zeta.Validation;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating List&lt;T&gt; where each element is validated by an inner schema.
/// </summary>
public sealed class ListSchema<TElement> : ISchema<List<TElement>>
{
    private readonly ISchema<TElement> _elementSchema;
    private readonly RuleEngine<List<TElement>> _rules = new();

    public ListSchema(ISchema<TElement> elementSchema)
    {
        _elementSchema = elementSchema;
    }

    public async ValueTask<Result<List<TElement>>> ValidateAsync(List<TElement> value, ValidationExecutionContext? execution = null)
    {
        execution ??= ValidationExecutionContext.Empty;
        List<ValidationError>? errors = null;

        // Validate list-level rules
        var ruleErrors = await _rules.ExecuteAsync(value, execution);
        if (ruleErrors != null)
        {
            errors = ruleErrors;
        }

        // Validate each element
        for (var i = 0; i < value.Count; i++)
        {
            var elementExecution = execution.PushIndex(i);
            var result = await _elementSchema.ValidateAsync(value[i], elementExecution);
            if (result.IsFailure)
            {
                errors ??= [];
                errors.AddRange(result.Errors);
            }
        }

        return errors == null
            ? Result<List<TElement>>.Success(value)
            : Result<List<TElement>>.Failure(errors);
    }

    public ListSchema<TElement> MinLength(int min, string? message = null)
    {
        _rules.Add(new DelegateValidationRule<List<TElement>>((val, exec) =>
            CollectionValidators.MinCount(val, min, exec.Path, message)));
        return this;
    }

    public ListSchema<TElement> MaxLength(int max, string? message = null)
    {
        _rules.Add(new DelegateValidationRule<List<TElement>>((val, exec) =>
            CollectionValidators.MaxCount(val, max, exec.Path, message)));
        return this;
    }

    public ListSchema<TElement> Length(int exact, string? message = null)
    {
        _rules.Add(new DelegateValidationRule<List<TElement>>((val, exec) =>
            CollectionValidators.Count(val, exact, exec.Path, message)));
        return this;
    }

    public ListSchema<TElement> NotEmpty(string? message = null)
    {
        _rules.Add(new DelegateValidationRule<List<TElement>>((val, exec) =>
            CollectionValidators.ListNotEmpty(val, exec.Path, message)));
        return this;
    }

    public ListSchema<TElement> Refine(Func<List<TElement>, bool> predicate, string message, string code = "custom_error")
    {
        _rules.Add(new DelegateValidationRule<List<TElement>>((val, exec) =>
            predicate(val)
                ? null
                : new ValidationError(exec.Path, code, message)));
        return this;
    }
}

/// <summary>
/// A context-aware schema for validating List&lt;T&gt; where each element is validated by an inner schema.
/// </summary>
public class ListSchema<TElement, TContext> : ISchema<List<TElement>, TContext>
{
    private readonly ISchema<TElement, TContext> _elementSchema;
    private readonly List<ISyncRule<List<TElement>, TContext>> _rules = [];

    public ListSchema(ISchema<TElement, TContext> elementSchema)
    {
        _elementSchema = elementSchema;
    }

    public ListSchema(ISchema<TElement> elementSchema)
    {
        _elementSchema = new SchemaAdapter<TElement, TContext>(elementSchema);
    }

    public async ValueTask<Result> ValidateAsync(List<TElement> value, ValidationContext<TContext> context)
    {
        List<ValidationError>? errors = null;

        // Validate list-level rules
        foreach (var rule in _rules)
        {
            var error = rule.Validate(value, context);
            if (error != null)
            {
                errors ??= [];
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
                errors ??= [];
                errors.AddRange(result.Errors);
            }
        }

        return errors == null
            ? Result.Success()
            : Result.Failure(errors);
    }

    public ListSchema<TElement, TContext> MinLength(int min, string? message = null)
    {
        _rules.Add(new DelegateSyncRule<List<TElement>, TContext>((val, ctx) =>
            val.Count >= min
                ? null
                : new ValidationError(ctx.Execution.Path, "min_length", message ?? $"Must have at least {min} items")));
        return this;
    }

    public ListSchema<TElement, TContext> MaxLength(int max, string? message = null)
    {
        _rules.Add(new DelegateSyncRule<List<TElement>, TContext>((val, ctx) =>
            val.Count <= max
                ? null
                : new ValidationError(ctx.Execution.Path, "max_length", message ?? $"Must have at most {max} items")));
        return this;
    }

    public ListSchema<TElement, TContext> Length(int exact, string? message = null)
    {
        _rules.Add(new DelegateSyncRule<List<TElement>, TContext>((val, ctx) =>
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
        _rules.Add(new DelegateSyncRule<List<TElement>, TContext>((val, ctx) =>
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
