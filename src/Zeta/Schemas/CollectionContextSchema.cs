using Zeta.Core;
using Zeta.Rules;
using Zeta.Rules.Collection;

namespace Zeta.Schemas;

/// <summary>
/// A context-aware schema for validating arrays where each element is validated by an inner schema.
/// </summary>
public class CollectionContextSchema<TElement, TContext> : ContextSchema<ICollection<TElement>, TContext, CollectionContextSchema<TElement, TContext>>
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

    public override async ValueTask<Result> ValidateAsync(ICollection<TElement>? value, ValidationContext<TContext> context)
    {
        if (value is null)
        {
            return IsNullAllowed
                ? Result.Success()
                : Result.Failure([new ValidationError(context.Path, "null_value", "Value cannot be null")]);
        }

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

    public CollectionContextSchema<TElement, TContext> Each(ISchema<TElement, TContext> elementSchema)
    {
        ElementSchema = elementSchema;
        return this;
    }
}