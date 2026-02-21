using Zeta.Core;
using Zeta.Rules;
using Zeta.Rules.Collection;

namespace Zeta.Schemas;

/// <summary>
/// A context-aware schema for validating arrays where each element is validated by an inner schema.
/// </summary>
public class CollectionContextSchema<TElement, TContext> : ContextSchema<ICollection<TElement>, TContext, CollectionContextSchema<TElement, TContext>>
{
    private ISchema<TElement, TContext>? ElementSchema { get; }

    internal CollectionContextSchema() : this(
        (ISchema<TElement, TContext>?)null,
        new ContextRuleEngine<ICollection<TElement>, TContext>(),
        false, null, null)
    {
    }

    public CollectionContextSchema(ISchema<TElement, TContext>? elementSchema, ContextRuleEngine<ICollection<TElement>, TContext> rules)
        : this(elementSchema, rules, false, null, null)
    {
    }

    public CollectionContextSchema(ISchema<TElement>? elementSchema, ContextlessRuleEngine<ICollection<TElement>> rules)
        : this(
            elementSchema is not null ? new SchemaAdapter<TElement, TContext>(elementSchema) : null,
            rules.ToContext<TContext>(),
            false, null, null)
    {
    }

    private CollectionContextSchema(
        ISchema<TElement, TContext>? elementSchema,
        ContextRuleEngine<ICollection<TElement>, TContext> rules,
        bool allowNull,
        IReadOnlyList<ISchemaConditional<ICollection<TElement>, TContext>>? conditionals,
        Func<ICollection<TElement>, IServiceProvider, CancellationToken, ValueTask<TContext>>? contextFactory)
        : base(rules, allowNull, conditionals, contextFactory)
    {
        ElementSchema = elementSchema;
    }

    protected override CollectionContextSchema<TElement, TContext> CreateInstance() => new();

    private protected override CollectionContextSchema<TElement, TContext> CreateInstance(
        ContextRuleEngine<ICollection<TElement>, TContext> rules,
        bool allowNull,
        IReadOnlyList<ISchemaConditional<ICollection<TElement>, TContext>>? conditionals,
        Func<ICollection<TElement>, IServiceProvider, CancellationToken, ValueTask<TContext>>? contextFactory)
        => new(ElementSchema, rules, allowNull, conditionals, contextFactory);

    public override async ValueTask<Result<ICollection<TElement>, TContext>> ValidateAsync(ICollection<TElement>? value, ValidationContext<TContext> context)
    {
        if (value is null)
        {
            return AllowNull
                ? Result<ICollection<TElement>, TContext>.Success(value!, context.Data)
                : Result<ICollection<TElement>, TContext>.Failure([new ValidationError(context.PathSegments, "null_value", "Value cannot be null")]);
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

        // Validate conditionals
        var conditionalErrors = await ExecuteConditionalsAsync(value, context);
        if (conditionalErrors != null)
        {
            errors ??= [];
            errors.AddRange(conditionalErrors);
        }

        return errors == null
            ? Result<ICollection<TElement>, TContext>.Success(value!, context.Data)
            : Result<ICollection<TElement>, TContext>.Failure(errors);
    }

    public CollectionContextSchema<TElement, TContext> MinLength(int min, string? message = null)
        => Append(new MinLengthRule<TElement, TContext>(min, message));

    public CollectionContextSchema<TElement, TContext> MaxLength(int max, string? message = null)
        => Append(new MaxLengthRule<TElement, TContext>(max, message));

    public CollectionContextSchema<TElement, TContext> Length(int exact, string? message = null)
        => Append(new LengthRule<TElement, TContext>(exact, message));

    public CollectionContextSchema<TElement, TContext> NotEmpty(string? message = null)
        => Append(new NotEmptyRule<TElement, TContext>(message));

    public CollectionContextSchema<TElement, TContext> Each(ISchema<TElement, TContext> elementSchema)
        => new(elementSchema, Rules, AllowNull, GetConditionals(), ContextFactory);

    internal CollectionContextSchema<TElement, TContext> WithElementSchema(ISchema<TElement, TContext> elementSchema)
        => new(elementSchema, Rules, AllowNull, GetConditionals(), ContextFactory);
}
