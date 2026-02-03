using Zeta.Core;
using Zeta.Rules;
using Zeta.Rules.Collection;

namespace Zeta.Schemas;

/// <summary>
/// A context-aware schema for validating arrays where each element is validated by an inner schema.
/// </summary>
public class CollectionContextSchema<TElement, TContext> : ContextSchema<ICollection<TElement>, TContext>
{
    private ISchema<TElement, TContext>? ElementSchema { get; set; }

    public CollectionContextSchema(ISchema<TElement, TContext>? elementSchema, ContextRuleEngine<ICollection<TElement>, TContext> rules) : base(rules)
    {
        ElementSchema = elementSchema;
    }

    public CollectionContextSchema(ISchema<TElement>? elementSchema, ContextlessRuleEngine<ICollection<TElement>> rules) : base(rules.ToContext<TContext>())
    {
        ElementSchema = elementSchema is not null
            ? new SchemaAdapter<TElement, TContext>(elementSchema)
            : null;
    }

    public override async ValueTask<Result> ValidateAsync(ICollection<TElement> value, ValidationContext<TContext> context)
    {
        var errors = await Rules.ExecuteAsync(value, context);

        // Validate each element if element schema is provided
        if (ElementSchema is not null)
        {
            var index = 0;
            foreach (var item in value)
            {
                var elementContext = context.PushIndex(index);
                var result = await ElementSchema.ValidateAsync(item, elementContext);
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
        Use(new MinLengthRule<TElement, TContext>(min, message));
        return this;
    }

    public CollectionContextSchema<TElement, TContext> MaxLength(int max, string? message = null)
    {
        Use(new MaxLengthRule<TElement, TContext>(max, message));
        return this;
    }

    public CollectionContextSchema<TElement, TContext> Length(int exact, string? message = null)
    {
        Use(new LengthRule<TElement, TContext>(exact, message));
        return this;
    }

    public CollectionContextSchema<TElement, TContext> NotEmpty(string? message = null)
    {
        Use(new NotEmptyRule<TElement, TContext>(message));
        return this;
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

    public CollectionContextSchema<TElement, TContext> If(
        Func<ICollection<TElement>, TContext, bool> condition,
        Func<CollectionContextSchema<TElement, TContext>, CollectionContextSchema<TElement, TContext>> configure)
    {
        var inner = configure(new CollectionContextSchema<TElement, TContext>(
            (ISchema<TElement, TContext>?)null, new ContextRuleEngine<ICollection<TElement>, TContext>()));
        foreach (var rule in inner.Rules.GetRules())
            Use(new ConditionalRule<ICollection<TElement>, TContext>(condition, rule));
        return this;
    }

    public CollectionContextSchema<TElement, TContext> If(
        Func<ICollection<TElement>, bool> condition,
        Func<CollectionContextSchema<TElement, TContext>, CollectionContextSchema<TElement, TContext>> configure)
        => If((val, _) => condition(val), configure);

    public CollectionContextSchema<TElement, TContext> Each(ISchema<TElement, TContext> elementSchema)
    {
        ElementSchema = elementSchema;

        return this;
    }
}
