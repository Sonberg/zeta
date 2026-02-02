using Zeta.Core;
using Zeta.Rules;
using Zeta.Rules.Collection;
using Zeta.Validation;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating collections where each element is validated by an inner schema.
/// </summary>
public sealed class CollectionContextlessSchema<TElement> : ContextlessSchema<ICollection<TElement>>
{
    private readonly ISchema<TElement>? _elementSchema;

    public CollectionContextlessSchema(ISchema<TElement>? elementSchema, ContextlessRuleEngine<ICollection<TElement>> rules) : base(rules)
    {
        _elementSchema = elementSchema;
    }

    public override async ValueTask<Result<ICollection<TElement>>> ValidateAsync(ICollection<TElement> value, ValidationContext context)
    {
        var errors = await Rules.ExecuteAsync(value, context);

        // Validate each element if element schema is provided
        if (_elementSchema is not null)
        {
            var index = 0;
            foreach (var item in value)
            {
                var elementExecution = context.PushIndex(index);
                var result = await _elementSchema.ValidateAsync(item, elementExecution);
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

    public CollectionContextlessSchema<TElement> Refine(Func<ICollection<TElement>, bool> predicate, string message, string code = "custom_error")
    {
        Use(new RefinementRule<ICollection<TElement>>((val, exec) =>
            predicate(val)
                ? null
                : new ValidationError(exec.Path, code, message)));
        return this;
    }

    /// <summary>
    /// Creates a context-aware array schema with all rules from this schema.
    /// The element schema is adapted to work in the context-aware environment.
    /// </summary>
    public CollectionContextSchema<TElement, TContext> WithContext<TContext>() => new(_elementSchema, Rules);
}