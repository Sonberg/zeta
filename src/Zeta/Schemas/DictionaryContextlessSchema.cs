using System.Collections.Generic;
using Zeta.Core;
using Zeta.Rules.Dictionary;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating dictionaries where keys and values can be validated by inner schemas.
/// </summary>
public sealed class DictionaryContextlessSchema<TKey, TValue>
    : ContextlessSchema<IDictionary<TKey, TValue>, DictionaryContextlessSchema<TKey, TValue>>
    where TKey : notnull
{
    internal DictionaryContextlessSchema()
        : this(null, null, new ContextlessRuleEngine<IDictionary<TKey, TValue>>(), false, null)
    {
    }

    public DictionaryContextlessSchema(
        ISchema<TKey>? keySchema,
        ISchema<TValue>? valueSchema,
        ContextlessRuleEngine<IDictionary<TKey, TValue>> rules)
        : this(keySchema, valueSchema, rules, false, null)
    {
    }

    private DictionaryContextlessSchema(
        ISchema<TKey>? keySchema,
        ISchema<TValue>? valueSchema,
        ContextlessRuleEngine<IDictionary<TKey, TValue>> rules,
        bool allowNull,
        IReadOnlyList<(Func<IDictionary<TKey, TValue>, bool>, ISchema<IDictionary<TKey, TValue>>)>? conditionals)
        : base(rules, allowNull, conditionals)
    {
        KeySchema = keySchema;
        ValueSchema = valueSchema;
    }

    private ISchema<TKey>? KeySchema { get; }
    private ISchema<TValue>? ValueSchema { get; }

    protected override DictionaryContextlessSchema<TKey, TValue> CreateInstance() => new();

    protected override DictionaryContextlessSchema<TKey, TValue> CreateInstance(
        ContextlessRuleEngine<IDictionary<TKey, TValue>> rules,
        bool allowNull,
        IReadOnlyList<(Func<IDictionary<TKey, TValue>, bool>, ISchema<IDictionary<TKey, TValue>>)>? conditionals)
        => new(KeySchema, ValueSchema, rules, allowNull, conditionals);

    public override async ValueTask<Result<IDictionary<TKey, TValue>>> ValidateAsync(
        IDictionary<TKey, TValue>? value, ValidationContext context)
    {
        if (value is null)
        {
            return AllowNull
                ? Result<IDictionary<TKey, TValue>>.Success(value!)
                : Result<IDictionary<TKey, TValue>>.Failure(
                    new ValidationError(context.Path, "null_value", "Value cannot be null"));
        }

        var errors = await Rules.ExecuteAsync(value, context);

        var index = 0;
        foreach (var kvp in value)
        {
            if (KeySchema is not null)
            {
                var keyContext = context.Push("keys").PushIndex(index);
                var keyResult = await KeySchema.ValidateAsync(kvp.Key, keyContext);
                if (keyResult.IsFailure)
                {
                    errors ??= [];
                    errors.AddRange(keyResult.Errors);
                }
            }

            if (ValueSchema is not null)
            {
                var valueContext = context.Push(kvp.Key.ToString() ?? $"[{index}]");
                var valueResult = await ValueSchema.ValidateAsync(kvp.Value, valueContext);
                if (valueResult.IsFailure)
                {
                    errors ??= [];
                    errors.AddRange(valueResult.Errors);
                }
            }

            index++;
        }

        var conditionalErrors = await ExecuteConditionalsAsync(value, context);
        if (conditionalErrors != null)
        {
            errors ??= [];
            errors.AddRange(conditionalErrors);
        }

        return errors == null
            ? Result<IDictionary<TKey, TValue>>.Success(value)
            : Result<IDictionary<TKey, TValue>>.Failure(errors);
    }

    public DictionaryContextlessSchema<TKey, TValue> MinLength(int min, string? message = null)
        => Append(new DictionaryMinLengthRule<TKey, TValue>(min, message));

    public DictionaryContextlessSchema<TKey, TValue> MaxLength(int max, string? message = null)
        => Append(new DictionaryMaxLengthRule<TKey, TValue>(max, message));

    public DictionaryContextlessSchema<TKey, TValue> NotEmpty(string? message = null)
        => Append(new DictionaryNotEmptyRule<TKey, TValue>(message));

    public DictionaryContextlessSchema<TKey, TValue> EachKey(ISchema<TKey> keySchema)
        => new(keySchema, ValueSchema, Rules, AllowNull, GetConditionals());

    public DictionaryContextlessSchema<TKey, TValue> EachValue(ISchema<TValue> valueSchema)
        => new(KeySchema, valueSchema, Rules, AllowNull, GetConditionals());

    internal DictionaryContextlessSchema<TKey, TValue> WithKeySchema(ISchema<TKey> keySchema)
        => new(keySchema, ValueSchema, Rules, AllowNull, GetConditionals());

    internal DictionaryContextlessSchema<TKey, TValue> WithValueSchema(ISchema<TValue> valueSchema)
        => new(KeySchema, valueSchema, Rules, AllowNull, GetConditionals());

    /// <summary>
    /// Creates a context-aware dictionary schema with all rules from this schema.
    /// The key and value schemas are adapted to work in the context-aware environment.
    /// </summary>
    public DictionaryContextSchema<TKey, TValue, TContext> Using<TContext>()
    {
        var schema = new DictionaryContextSchema<TKey, TValue, TContext>(KeySchema, ValueSchema, Rules);
        schema = AllowNull ? schema.Nullable() : schema;
        schema = schema.TransferContextlessConditionals(GetConditionals());
        return schema;
    }

    /// <summary>
    /// Creates a context-aware dictionary schema with a factory delegate for creating context data.
    /// </summary>
    public DictionaryContextSchema<TKey, TValue, TContext> Using<TContext>(
        Func<IDictionary<TKey, TValue>, IServiceProvider, CancellationToken, ValueTask<TContext>> factory)
    {
        return Using<TContext>().WithContextFactory(factory);
    }
}
