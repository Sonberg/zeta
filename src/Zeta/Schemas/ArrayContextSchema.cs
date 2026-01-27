using Zeta.Core;
using Zeta.Rules;

namespace Zeta.Schemas;

/// <summary>
/// A context-aware schema for validating arrays where each element is validated by an inner schema.
/// </summary>
public class ArrayContextSchema<TElement, TContext> : ContextSchema<TElement[], TContext>
{
    private readonly ISchema<TElement, TContext> _elementSchema;

    public ArrayContextSchema(ISchema<TElement, TContext> elementSchema, ContextRuleEngine<TElement[], TContext> rules) : base(rules)
    {
        _elementSchema = elementSchema;
    }

    public ArrayContextSchema(ISchema<TElement> elementSchema, ContextlessRuleEngine<TElement[]> rules) : base(rules.ToContext<TContext>())
    {
        _elementSchema = new SchemaAdapter<TElement, TContext>(elementSchema);
    }

    public override async ValueTask<Result> ValidateAsync(TElement[] value, ValidationContext<TContext> context)
    {
        var errors = await Rules.ExecuteAsync(value, context);

        // Validate each element
        for (var i = 0; i < value.Length; i++)
        {
            var elementContext = context.PushIndex(i);
            var result = await _elementSchema.ValidateAsync(value[i], elementContext);
            if (!result.IsFailure) continue;

            errors ??= [];
            errors.AddRange(result.Errors);
        }

        return errors == null
            ? Result.Success()
            : Result.Failure(errors);
    }

    public ArrayContextSchema<TElement, TContext> MinLength(int min, string? message = null)
    {
        Use(new RefinementRule<TElement[], TContext>((val, ctx) =>
            val.Length >= min
                ? null
                : new ValidationError(ctx.Path, "min_length", message ?? $"Must have at least {min} items")));
        return this;
    }

    public ArrayContextSchema<TElement, TContext> MaxLength(int max, string? message = null)
    {
        Use(new RefinementRule<TElement[], TContext>((val, ctx) =>
            val.Length <= max
                ? null
                : new ValidationError(ctx.Path, "max_length", message ?? $"Must have at most {max} items")));
        return this;
    }

    public ArrayContextSchema<TElement, TContext> Length(int exact, string? message = null)
    {
        Use(new RefinementRule<TElement[], TContext>((val, ctx) =>
            val.Length == exact
                ? null
                : new ValidationError(ctx.Path, "length", message ?? $"Must have exactly {exact} items")));
        return this;
    }

    public ArrayContextSchema<TElement, TContext> NotEmpty(string? message = null)
    {
        return MinLength(1, message ?? "Must not be empty");
    }

    public ArrayContextSchema<TElement, TContext> Refine(Func<TElement[], TContext, bool> predicate, string message, string code = "custom_error")
    {
        Use(new RefinementRule<TElement[], TContext>((val, ctx) =>
            predicate(val, ctx.Data)
                ? null
                : new ValidationError(ctx.Path, code, message)));
        return this;
    }

    public ArrayContextSchema<TElement, TContext> Refine(Func<TElement[], bool> predicate, string message, string code = "custom_error")
    {
        return Refine((val, _) => predicate(val), message, code);
    }
}