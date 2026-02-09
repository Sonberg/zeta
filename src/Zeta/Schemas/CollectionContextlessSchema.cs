using Zeta.Core;
using Zeta.Rules.Collection;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating collections where each element is validated by an inner schema.
/// </summary>
public sealed class CollectionContextlessSchema<TElement> : ContextlessSchema<ICollection<TElement>, CollectionContextlessSchema<TElement>>
{
    internal CollectionContextlessSchema()
    {
    }

    private ISchema<TElement>? ElementSchema { get; set; }

    public CollectionContextlessSchema(ISchema<TElement>? elementSchema, ContextlessRuleEngine<ICollection<TElement>> rules) : base(rules)
    {
        ElementSchema = elementSchema;
    }

    public override async ValueTask<Result<ICollection<TElement>>> ValidateAsync(ICollection<TElement>? value, ValidationContext context)
    {
        if (value is null)
        {
            return AllowNull
                ? Result<ICollection<TElement>>.Success(value!)
                : Result<ICollection<TElement>>.Failure(new ValidationError(context.Path, "null_value", "Value cannot be null"));
        }

        var errors = await Rules.ExecuteAsync(value, context);

        // Validate each element if element schema is provided
        if (ElementSchema is not null)
        {
            var index = 0;
            foreach (var item in value)
            {
                var elementExecution = context.PushIndex(index);
                var result = await ElementSchema.ValidateAsync(item, elementExecution);
                if (result.IsFailure)
                {
                    errors ??= [];
                    errors.AddRange(result.Errors);
                }

                index++;
            }
        }

        return errors == null
            ? Result<ICollection<TElement>>.Success(value)
            : Result<ICollection<TElement>>.Failure(errors);
    }

    public CollectionContextlessSchema<TElement> MinLength(int min, string? message = null)
    {
        Use(new MinLengthRule<TElement>(min, message));
        return this;
    }

    public CollectionContextlessSchema<TElement> MaxLength(int max, string? message = null)
    {
        Use(new MaxLengthRule<TElement>(max, message));
        return this;
    }

    public CollectionContextlessSchema<TElement> Length(int exact, string? message = null)
    {
        Use(new LengthRule<TElement>(exact, message));
        return this;
    }

    public CollectionContextlessSchema<TElement> NotEmpty(string? message = null)
    {
        Use(new NotEmptyRule<TElement>(message));
        return this;
    }

    public CollectionContextlessSchema<TElement> Each(ISchema<TElement> elementSchema)
    {
        ElementSchema = elementSchema;
        return this;
    }

    /// <summary>
    /// Creates a context-aware array schema with all rules from this schema.
    /// The element schema is adapted to work in the context-aware environment.
    /// </summary>
    public CollectionContextSchema<TElement, TContext> WithContext<TContext>() => new(ElementSchema, Rules);
}