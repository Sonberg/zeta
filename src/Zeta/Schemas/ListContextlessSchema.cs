using Zeta.Core;
using Zeta.Rules;
using Zeta.Validation;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating List&lt;T&gt; where each element is validated by an inner schema.
/// </summary>
public sealed class ListContextlessSchema<TElement> : ContextlessSchema<List<TElement>>
{
    private readonly ISchema<TElement> _elementSchema;

    public ListContextlessSchema(ISchema<TElement> elementSchema) : this(elementSchema, new ContextlessRuleEngine<List<TElement>>())
    {
    }

    public ListContextlessSchema(ISchema<TElement> elementSchema, ContextlessRuleEngine<List<TElement>> rules) : base(rules)
    {
        _elementSchema = elementSchema;
    }

    public override async ValueTask<Result<List<TElement>>> ValidateAsync(List<TElement> value, ValidationContext execution)
    {
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

    public ListContextlessSchema<TElement> MinLength(int min, string? message = null)
    {
        Use(new RefinementRule<List<TElement>>((val, exec) =>
            CollectionValidators.MinCount(val, min, exec.Path, message)));
        return this;
    }

    public ListContextlessSchema<TElement> MaxLength(int max, string? message = null)
    {
        Use(new RefinementRule<List<TElement>>((val, exec) =>
            CollectionValidators.MaxCount(val, max, exec.Path, message)));
        return this;
    }

    public ListContextlessSchema<TElement> Length(int exact, string? message = null)
    {
        Use(new RefinementRule<List<TElement>>((val, exec) =>
            CollectionValidators.Count(val, exact, exec.Path, message)));
        return this;
    }

    public ListContextlessSchema<TElement> NotEmpty(string? message = null)
    {
        Use(new RefinementRule<List<TElement>>((val, exec) =>
            CollectionValidators.ListNotEmpty(val, exec.Path, message)));
        return this;
    }

    public ListContextlessSchema<TElement> Refine(Func<List<TElement>, bool> predicate, string message, string code = "custom_error")
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
    public ListContextSchema<TElement, TContext> WithContext<TContext>()
        => new ListContextSchema<TElement, TContext>(_elementSchema, Rules);
}
