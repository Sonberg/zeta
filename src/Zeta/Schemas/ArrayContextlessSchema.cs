using Zeta.Core;
using Zeta.Rules;
using Zeta.Validation;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating arrays where each element is validated by an inner schema.
/// </summary>
public sealed class ArrayContextlessSchema<TElement> : ContextlessSchema<TElement[]>
{
    private readonly ISchema<TElement> _elementSchema;

    public ArrayContextlessSchema(ISchema<TElement> elementSchema, ContextlessRuleEngine<TElement[]> rules) : base(rules)
    {
        _elementSchema = elementSchema;
    }

    public override async ValueTask<Result<TElement[]>> ValidateAsync(TElement[] value, ValidationContext context)
    {
        var errors = await Rules.ExecuteAsync(value, context);

        // Validate each element
        for (var i = 0; i < value.Length; i++)
        {
            var elementExecution = context.PushIndex(i);
            var result = await _elementSchema.ValidateAsync(value[i], elementExecution);
            if (!result.IsFailure) continue;

            errors ??= [];
            errors.AddRange(result.Errors);
        }

        return errors == null
            ? Result<TElement[]>.Success(value)
            : Result<TElement[]>.Failure(errors);
    }

    public ArrayContextlessSchema<TElement> MinLength(int min, string? message = null)
    {
        Use(new RefinementRule<TElement[]>((val, exec) =>
            CollectionValidators.MinLength(val, min, exec.Path, message)));
        return this;
    }

    public ArrayContextlessSchema<TElement> MaxLength(int max, string? message = null)
    {
        Use(new RefinementRule<TElement[]>((val, exec) =>
            CollectionValidators.MaxLength(val, max, exec.Path, message)));
        return this;
    }

    public ArrayContextlessSchema<TElement> Length(int exact, string? message = null)
    {
        Use(new RefinementRule<TElement[]>((val, exec) =>
            CollectionValidators.Length(val, exact, exec.Path, message)));
        return this;
    }

    public ArrayContextlessSchema<TElement> NotEmpty(string? message = null)
    {
        Use(new RefinementRule<TElement[]>((val, exec) =>
            CollectionValidators.NotEmpty(val, exec.Path, message)));
        return this;
    }

    public ArrayContextlessSchema<TElement> Refine(Func<TElement[], bool> predicate, string message, string code = "custom_error")
    {
        Use(new RefinementRule<TElement[]>((val, exec) =>
            predicate(val)
                ? null
                : new ValidationError(exec.Path, code, message)));
        return this;
    }

    /// <summary>
    /// Creates a context-aware array schema with all rules from this schema.
    /// The element schema is adapted to work in the context-aware environment.
    /// </summary>
    public ArrayContextSchema<TElement, TContext> WithContext<TContext>()
        => new ArrayContextSchema<TElement, TContext>(_elementSchema, Rules);
}
