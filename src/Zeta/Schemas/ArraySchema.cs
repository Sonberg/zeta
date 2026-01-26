using Zeta.Core;
using Zeta.Rules;
using Zeta.Validation;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating arrays where each element is validated by an inner schema.
/// </summary>
public sealed class ArraySchema<TElement> : ISchema<TElement[]>
{
    private readonly ISchema<TElement> _elementSchema;
    private readonly RuleEngine<TElement[]> _rules = new();

    public ArraySchema(ISchema<TElement> elementSchema)
    {
        _elementSchema = elementSchema;
    }

    public async ValueTask<Result<TElement[]>> ValidateAsync(TElement[] value, ValidationExecutionContext? execution = null)
    {
        execution ??= ValidationExecutionContext.Empty;
        List<ValidationError>? errors = null;

        // Validate array-level rules
        var ruleErrors = await _rules.ExecuteAsync(value, execution);
        if (ruleErrors != null)
        {
            errors = ruleErrors;
        }

        // Validate each element
        for (var i = 0; i < value.Length; i++)
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
            ? Result<TElement[]>.Success(value)
            : Result<TElement[]>.Failure(errors);
    }

    public ArraySchema<TElement> MinLength(int min, string? message = null)
    {
        _rules.Add(new DelegateValidationRule<TElement[]>((val, exec) =>
            CollectionValidators.MinLength(val, min, exec.Path, message)));
        return this;
    }

    public ArraySchema<TElement> MaxLength(int max, string? message = null)
    {
        _rules.Add(new DelegateValidationRule<TElement[]>((val, exec) =>
            CollectionValidators.MaxLength(val, max, exec.Path, message)));
        return this;
    }

    public ArraySchema<TElement> Length(int exact, string? message = null)
    {
        _rules.Add(new DelegateValidationRule<TElement[]>((val, exec) =>
            CollectionValidators.Length(val, exact, exec.Path, message)));
        return this;
    }

    public ArraySchema<TElement> NotEmpty(string? message = null)
    {
        _rules.Add(new DelegateValidationRule<TElement[]>((val, exec) =>
            CollectionValidators.NotEmpty(val, exec.Path, message)));
        return this;
    }

    public ArraySchema<TElement> Refine(Func<TElement[], bool> predicate, string message, string code = "custom_error")
    {
        _rules.Add(new DelegateValidationRule<TElement[]>((val, exec) =>
            predicate(val)
                ? null
                : new ValidationError(exec.Path, code, message)));
        return this;
    }
}

/// <summary>
/// A context-aware schema for validating arrays where each element is validated by an inner schema.
/// </summary>
public class ArraySchema<TElement, TContext> : ISchema<TElement[], TContext>
{
    private readonly ISchema<TElement, TContext> _elementSchema;
    private readonly List<IValidationRule<TElement[], TContext>> _rules = [];

    public ArraySchema(ISchema<TElement, TContext> elementSchema)
    {
        _elementSchema = elementSchema;
    }

    public ArraySchema(ISchema<TElement> elementSchema)
    {
        _elementSchema = new SchemaAdapter<TElement, TContext>(elementSchema);
    }

    public async ValueTask<Result> ValidateAsync(TElement[] value, ValidationContext<TContext> context)
    {
        List<ValidationError>? errors = null;

        // Validate array-level rules
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
        for (var i = 0; i < value.Length; i++)
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

    public ArraySchema<TElement, TContext> MinLength(int min, string? message = null)
    {
        _rules.Add(new DelegateValidationRule<TElement[], TContext>((val, ctx) =>
            val.Length >= min
                ? null
                : new ValidationError(ctx.Execution.Path, "min_length", message ?? $"Must have at least {min} items")));
        return this;
    }

    public ArraySchema<TElement, TContext> MaxLength(int max, string? message = null)
    {
        _rules.Add(new DelegateValidationRule<TElement[], TContext>((val, ctx) =>
            val.Length <= max
                ? null
                : new ValidationError(ctx.Execution.Path, "max_length", message ?? $"Must have at most {max} items")));
        return this;
    }

    public ArraySchema<TElement, TContext> Length(int exact, string? message = null)
    {
        _rules.Add(new DelegateValidationRule<TElement[], TContext>((val, ctx) =>
            val.Length == exact
                ? null
                : new ValidationError(ctx.Execution.Path, "length", message ?? $"Must have exactly {exact} items")));
        return this;
    }

    public ArraySchema<TElement, TContext> NotEmpty(string? message = null)
    {
        return MinLength(1, message ?? "Must not be empty");
    }

    public ArraySchema<TElement, TContext> Refine(Func<TElement[], TContext, bool> predicate, string message, string code = "custom_error")
    {
        _rules.Add(new DelegateValidationRule<TElement[], TContext>((val, ctx) =>
            predicate(val, ctx.Data)
                ? null
                : new ValidationError(ctx.Execution.Path, code, message)));
        return this;
    }

    public ArraySchema<TElement, TContext> Refine(Func<TElement[], bool> predicate, string message, string code = "custom_error")
    {
        return Refine((val, _) => predicate(val), message, code);
    }
}
