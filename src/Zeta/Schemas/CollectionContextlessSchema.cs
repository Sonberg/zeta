using Zeta.Core;
using Zeta.Rules.Collection;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating collections where each element is validated by an inner schema.
/// </summary>
public sealed class CollectionContextlessSchema<TElement> : ContextlessSchema<ICollection<TElement>, CollectionContextlessSchema<TElement>>
{
    internal CollectionContextlessSchema() : this(null, null, new ContextlessRuleEngine<ICollection<TElement>>(), false, null)
    {
    }

    public CollectionContextlessSchema(ISchema<TElement>? elementSchema, ContextlessRuleEngine<ICollection<TElement>> rules)
        : this(elementSchema, null, rules, false, null)
    {
    }

    private CollectionContextlessSchema(
        ISchema<TElement>? elementSchema,
        Func<TElement, int, ISchema<TElement>>? elementSchemaFactory,
        ContextlessRuleEngine<ICollection<TElement>> rules,
        bool allowNull,
        IReadOnlyList<(Func<ICollection<TElement>, bool>, ISchema<ICollection<TElement>>)>? conditionals)
        : base(rules, allowNull, conditionals)
    {
        ElementSchema = elementSchema;
        _elementSchemaFactory = elementSchemaFactory;
    }

    protected override CollectionContextlessSchema<TElement> CreateInstance() => new();

    protected override CollectionContextlessSchema<TElement> CreateInstance(
        ContextlessRuleEngine<ICollection<TElement>> rules,
        bool allowNull,
        IReadOnlyList<(Func<ICollection<TElement>, bool>, ISchema<ICollection<TElement>>)>? conditionals)
        => new(ElementSchema, _elementSchemaFactory, rules, allowNull, conditionals);

    private ISchema<TElement>? ElementSchema { get; }
    private readonly Func<TElement, int, ISchema<TElement>>? _elementSchemaFactory;

    public override async ValueTask<Result<ICollection<TElement>>> ValidateAsync(ICollection<TElement>? value, ValidationContext context)
    {
        if (value is null)
        {
            return AllowNull
                ? Result<ICollection<TElement>>.Success(value!)
                : Result<ICollection<TElement>>.Failure(new ValidationError(context.Path, "null_value", "Value cannot be null"));
        }

        var errors = await Rules.ExecuteAsync(value, context);

        // Validate each element if element schema or factory is provided
        if (ElementSchema is not null || _elementSchemaFactory is not null)
        {
            var index = 0;
            foreach (var item in value)
            {
                var elementExecution = context.PushIndex(index);
                var schema = _elementSchemaFactory?.Invoke(item, index) ?? ElementSchema!;
                var result = await schema.ValidateAsync(item, elementExecution);
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
            ? Result<ICollection<TElement>>.Success(value)
            : Result<ICollection<TElement>>.Failure(errors);
    }

    public CollectionContextlessSchema<TElement> MinLength(int min, string? message = null)
        => Append(new MinLengthRule<TElement>(min, message));

    public CollectionContextlessSchema<TElement> MaxLength(int max, string? message = null)
        => Append(new MaxLengthRule<TElement>(max, message));

    public CollectionContextlessSchema<TElement> Length(int exact, string? message = null)
        => Append(new LengthRule<TElement>(exact, message));

    public CollectionContextlessSchema<TElement> NotEmpty(string? message = null)
        => Append(new NotEmptyRule<TElement>(message));

    public CollectionContextlessSchema<TElement> Each(ISchema<TElement> elementSchema)
        => new(elementSchema, null, Rules, AllowNull, GetConditionals());

    /// <summary>
    /// Validates each element in the collection using a schema resolved via a factory function.
    /// </summary>
    public CollectionContextlessSchema<TElement> Each(Func<TElement, int, ISchema<TElement>> elementSchemaFactory)
        => new(null, elementSchemaFactory, Rules, AllowNull, GetConditionals());

    internal CollectionContextlessSchema<TElement> WithElementSchema(ISchema<TElement> elementSchema)
        => new(elementSchema, null, Rules, AllowNull, GetConditionals());

    /// <summary>
    /// Creates a context-aware array schema with all rules from this schema.
    /// The element schema is adapted to work in the context-aware environment.
    /// </summary>
    public CollectionContextSchema<TElement, TContext> Using<TContext>()
    {
        var schema = new CollectionContextSchema<TElement, TContext>(ElementSchema, _elementSchemaFactory, Rules);
        schema = AllowNull ? schema.Nullable() : schema;
        schema = schema.TransferContextlessConditionals(GetConditionals());
        return schema;
    }

    /// <summary>
    /// Creates a context-aware collection schema with a factory delegate for creating context data.
    /// </summary>
    public CollectionContextSchema<TElement, TContext> Using<TContext>(
        Func<ICollection<TElement>, IServiceProvider, CancellationToken, ValueTask<TContext>> factory)
    {
        return Using<TContext>().WithContextFactory(factory);
    }
}
