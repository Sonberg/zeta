using Zeta.Core;
using Zeta.Rules;
using Zeta.Validation;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating List&lt;T&gt; where each element is validated by an inner schema.
/// </summary>
public sealed class ListSchema<TElement> : ContextlessSchema<List<TElement>>
{
    private readonly ISchema<TElement> _elementSchema;

    public ListSchema(ISchema<TElement> elementSchema) : this(elementSchema, new ContextlessRuleEngine<List<TElement>>())
    {
    }

    public ListSchema(ISchema<TElement> elementSchema, ContextlessRuleEngine<List<TElement>> rules) : base(rules)
    {
        _elementSchema = elementSchema;
    }

    public override async ValueTask<Result<List<TElement>>> ValidateAsync(List<TElement> value, ValidationExecutionContext? execution = null)
    {
        execution ??= ValidationExecutionContext.Empty;
        var errors = await Rules.ExecuteAsync(value, execution);
        
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
        Use(new RefinementRule<List<TElement>>((val, exec) =>
            CollectionValidators.MinCount(val, min, exec.Path, message)));
        return this;
    }

    public ListSchema<TElement> MaxLength(int max, string? message = null)
    {
        Use(new RefinementRule<List<TElement>>((val, exec) =>
            CollectionValidators.MaxCount(val, max, exec.Path, message)));
        return this;
    }

    public ListSchema<TElement> Length(int exact, string? message = null)
    {
        Use(new RefinementRule<List<TElement>>((val, exec) =>
            CollectionValidators.Count(val, exact, exec.Path, message)));
        return this;
    }

    public ListSchema<TElement> NotEmpty(string? message = null)
    {
        Use(new RefinementRule<List<TElement>>((val, exec) =>
            CollectionValidators.ListNotEmpty(val, exec.Path, message)));
        return this;
    }

    public ListSchema<TElement> Refine(Func<List<TElement>, bool> predicate, string message, string code = "custom_error")
    {
        Use(new RefinementRule<List<TElement>>((val, exec) =>
            predicate(val)
                ? null
                : new ValidationError(exec.Path, code, message)));
        return this;
    }

    /// <summary>
    /// Creates a context-aware list schema with all rules from this schema.
    /// The element schema is adapted to work in the context-aware environment.
    /// </summary>
    public ListSchema<TElement, TContext> WithContext<TContext>()
        => new ListSchema<TElement, TContext>(_elementSchema, Rules);
}

/// <summary>
/// A context-aware schema for validating List&lt;T&gt; where each element is validated by an inner schema.
/// </summary>
public class ListSchema<TElement, TContext> : ContextSchema<List<TElement>, TContext>
{
    private readonly ISchema<TElement, TContext> _elementSchema;

    public ListSchema(ISchema<TElement, TContext> elementSchema) : this(elementSchema, new ContextRuleEngine<List<TElement>, TContext>())
    {
    }

    public ListSchema(ISchema<TElement, TContext> elementSchema, ContextRuleEngine<List<TElement>, TContext> rules) : base(rules)
    {
        _elementSchema = elementSchema;
    }

    public ListSchema(ISchema<TElement> elementSchema) : this(new SchemaAdapter<TElement, TContext>(elementSchema))
    {
    }

    public ListSchema(ISchema<TElement> elementSchema, ContextlessRuleEngine<List<TElement>> rules) : base(rules.ToContext<TContext>())
    {
        _elementSchema = new SchemaAdapter<TElement, TContext>(elementSchema);
    }

    public override async ValueTask<Result> ValidateAsync(List<TElement> value, ValidationContext<TContext> context)
    {
        var errors = await Rules.ExecuteAsync(value, context);
        
        // Validate each element
        for (var i = 0; i < value.Count; i++)
        {
            var elementContext = new ValidationContext<TContext>(
                context.Data,
                context.Execution.PushIndex(i));

            var result = await _elementSchema.ValidateAsync(value[i], elementContext);
            if (!result.IsFailure) continue;
            
            errors ??= [];
            errors.AddRange(result.Errors);
        }

        return errors == null
            ? Result.Success()
            : Result.Failure(errors);
    }

    public ListSchema<TElement, TContext> MinLength(int min, string? message = null)
    {
        Use(new RefinementRule<List<TElement>, TContext>((val, ctx) =>
            val.Count >= min
                ? null
                : new ValidationError(ctx.Execution.Path, "min_length", message ?? $"Must have at least {min} items")));
        return this;
    }

    public ListSchema<TElement, TContext> MaxLength(int max, string? message = null)
    {
        Use(new RefinementRule<List<TElement>, TContext>((val, ctx) =>
            val.Count <= max
                ? null
                : new ValidationError(ctx.Execution.Path, "max_length", message ?? $"Must have at most {max} items")));
        return this;
    }

    public ListSchema<TElement, TContext> Length(int exact, string? message = null)
    {
        Use(new RefinementRule<List<TElement>, TContext>((val, ctx) =>
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
        Use(new RefinementRule<List<TElement>, TContext>((val, ctx) =>
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
