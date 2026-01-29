using Zeta.Core;
using Zeta.Rules;

namespace Zeta.Schemas;

/// <summary>
/// A context-aware schema for validating arrays where each element is validated by an inner schema.
/// </summary>
public class CollectionContextSchema<TElement, TContext> : ContextSchema<ICollection<TElement>, TContext>
{
    private readonly ISchema<TElement, TContext>? _elementSchema;

    public CollectionContextSchema(ISchema<TElement, TContext>? elementSchema, ContextRuleEngine<ICollection<TElement>, TContext> rules) : base(rules)
    {
        _elementSchema = elementSchema;
    }

    public CollectionContextSchema(ISchema<TElement>? elementSchema, ContextlessRuleEngine<ICollection<TElement>> rules) : base(rules.ToContext<TContext>())
    {
        _elementSchema = elementSchema is not null
            ? new SchemaAdapter<TElement, TContext>(elementSchema)
            : null;
    }

    public override async ValueTask<Result> ValidateAsync(ICollection<TElement> value, ValidationContext<TContext> context)
    {
        var errors = await Rules.ExecuteAsync(value, context);

        // Validate each element if element schema is provided
        if (_elementSchema is not null)
        {
            var index = 0;
            foreach (var item in value)
            {
                var elementContext = context.PushIndex(index);
                var result = await _elementSchema.ValidateAsync(item, elementContext);
                if (result.IsFailure)
                {
                    errors ??= [];
                    errors.AddRange(result.Errors);
                }

                index++;
            }
        }

        return errors == null
            ? Result.Success()
            : Result.Failure(errors);
    }

    public CollectionContextSchema<TElement, TContext> MinLength(int min, string? message = null)
    {
        Use(new RefinementRule<ICollection<TElement>, TContext>((val, ctx) =>
            val.Count >= min
                ? null
                : new ValidationError(ctx.Path, "min_length", message ?? $"Must have at least {min} items")));
        return this;
    }

    public CollectionContextSchema<TElement, TContext> MaxLength(int max, string? message = null)
    {
        Use(new RefinementRule<ICollection<TElement>, TContext>((val, ctx) =>
            val.Count <= max
                ? null
                : new ValidationError(ctx.Path, "max_length", message ?? $"Must have at most {max} items")));
        return this;
    }

    public CollectionContextSchema<TElement, TContext> Length(int exact, string? message = null)
    {
        Use(new RefinementRule<ICollection<TElement>, TContext>((val, ctx) =>
            val.Count == exact
                ? null
                : new ValidationError(ctx.Path, "length", message ?? $"Must have exactly {exact} items")));
        return this;
    }

    public CollectionContextSchema<TElement, TContext> NotEmpty(string? message = null)
    {
        return MinLength(1, message ?? "Must not be empty");
    }

    public CollectionContextSchema<TElement, TContext> Refine(Func<ICollection<TElement>, TContext, bool> predicate, string message, string code = "custom_error")
    {
        Use(new RefinementRule<ICollection<TElement>, TContext>((val, ctx) =>
            predicate(val, ctx.Data)
                ? null
                : new ValidationError(ctx.Path, code, message)));
        return this;
    }

    public CollectionContextSchema<TElement, TContext> Refine(Func<ICollection<TElement>, bool> predicate, string message, string code = "custom_error")
    {
        return Refine((val, _) => predicate(val), message, code);
    }
}