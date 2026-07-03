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

    // Stage order for collection schemas: rules, then elements, then conditionals.
    // Memoized per instance (schemas are immutable), so a hot ValidateAsync allocates no stage array or delegates.
    private Func<ICollection<TElement>, ValidationRun<TContext>, ValueTask<IReadOnlyList<ValidationError>?>>[]? _stages;
    private Func<ICollection<TElement>, ValidationRun<TContext>, ValueTask<IReadOnlyList<ValidationError>?>>[] Stages() => _stages ??=
    [
        ValidateRulesAsync,
        ValidateElementsAsync,
        ValidateConditionalsAsync,
    ];

    public override async ValueTask<Result<ICollection<TElement>, TContext>> ValidateAsync(ICollection<TElement>? value, ValidationRun<TContext> context)
    {
        if (value is null)
        {
            return AllowNull
                ? Result<ICollection<TElement>, TContext>.Success(value!, context.Data)
                : Result<ICollection<TElement>, TContext>.Failure([new ValidationError(context.PathSegments, "null_value", "Value cannot be null")]);
        }

        var errors = await ValidationPipeline.RunAsync(value, context, Stages());
        return errors is null
            ? Result<ICollection<TElement>, TContext>.Success(value!, context.Data)
            : Result<ICollection<TElement>, TContext>.Failure(errors);
    }

    private async ValueTask<IReadOnlyList<ValidationError>?> ValidateRulesAsync(ICollection<TElement> value, ValidationRun<TContext> context)
        => await Rules.ExecuteAsync(value, context);

    private async ValueTask<IReadOnlyList<ValidationError>?> ValidateElementsAsync(ICollection<TElement> value, ValidationRun<TContext> context)
    {
        if (ElementSchema is null) return null;

        List<ValidationError>? errors = null;
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

        return errors;
    }

    private async ValueTask<IReadOnlyList<ValidationError>?> ValidateConditionalsAsync(ICollection<TElement> value, ValidationRun<TContext> context)
        => await ExecuteConditionalsAsync(value, context);

    public CollectionContextSchema<TElement, TContext> MinLength(int min, string? message = null)
        => AppendRule(new MinLengthRule<TElement>(min, message));

    public CollectionContextSchema<TElement, TContext> MaxLength(int max, string? message = null)
        => AppendRule(new MaxLengthRule<TElement>(max, message));

    public CollectionContextSchema<TElement, TContext> Length(int exact, string? message = null)
        => AppendRule(new LengthRule<TElement>(exact, message));

    public CollectionContextSchema<TElement, TContext> NotEmpty(string? message = null)
        => AppendRule(new NotEmptyRule<TElement>(message));

    public CollectionContextSchema<TElement, TContext> Each(ISchema<TElement, TContext> elementSchema)
        => new(elementSchema, Rules, AllowNull, GetConditionals(), ContextFactory);

    internal CollectionContextSchema<TElement, TContext> WithElementSchema(ISchema<TElement, TContext> elementSchema)
        => new(elementSchema, Rules, AllowNull, GetConditionals(), ContextFactory);
}
