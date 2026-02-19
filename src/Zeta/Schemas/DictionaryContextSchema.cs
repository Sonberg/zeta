using System.Collections.Generic;
using Zeta.Core;
using Zeta.Rules;
using Zeta.Rules.Dictionary;

namespace Zeta.Schemas;

/// <summary>
/// A context-aware schema for validating dictionaries where keys and values can be validated by inner schemas.
/// </summary>
public class DictionaryContextSchema<TKey, TValue, TContext>
    : ContextSchema<IDictionary<TKey, TValue>, TContext, DictionaryContextSchema<TKey, TValue, TContext>>
    where TKey : notnull
{
    private ISchema<TKey, TContext>? KeySchema { get; }
    private ISchema<TValue, TContext>? ValueSchema { get; }

    internal DictionaryContextSchema() : this(
        (ISchema<TKey, TContext>?)null,
        (ISchema<TValue, TContext>?)null,
        new ContextRuleEngine<IDictionary<TKey, TValue>, TContext>(),
        false, null, null)
    {
    }

    public DictionaryContextSchema(
        ISchema<TKey, TContext>? keySchema,
        ISchema<TValue, TContext>? valueSchema,
        ContextRuleEngine<IDictionary<TKey, TValue>, TContext> rules)
        : this(keySchema, valueSchema, rules, false, null, null)
    {
    }

    public DictionaryContextSchema(
        ISchema<TKey>? keySchema,
        ISchema<TValue>? valueSchema,
        ContextlessRuleEngine<IDictionary<TKey, TValue>> rules)
        : this(
            keySchema is not null ? new SchemaAdapter<TKey, TContext>(keySchema) : null,
            valueSchema is not null ? new SchemaAdapter<TValue, TContext>(valueSchema) : null,
            rules.ToContext<TContext>(),
            false, null, null)
    {
    }

    private DictionaryContextSchema(
        ISchema<TKey, TContext>? keySchema,
        ISchema<TValue, TContext>? valueSchema,
        ContextRuleEngine<IDictionary<TKey, TValue>, TContext> rules,
        bool allowNull,
        IReadOnlyList<ISchemaConditional<IDictionary<TKey, TValue>, TContext>>? conditionals,
        Func<IDictionary<TKey, TValue>, IServiceProvider, CancellationToken, ValueTask<TContext>>? contextFactory)
        : base(rules, allowNull, conditionals, contextFactory)
    {
        KeySchema = keySchema;
        ValueSchema = valueSchema;
    }

    protected override DictionaryContextSchema<TKey, TValue, TContext> CreateInstance() => new();

    private protected override DictionaryContextSchema<TKey, TValue, TContext> CreateInstance(
        ContextRuleEngine<IDictionary<TKey, TValue>, TContext> rules,
        bool allowNull,
        IReadOnlyList<ISchemaConditional<IDictionary<TKey, TValue>, TContext>>? conditionals,
        Func<IDictionary<TKey, TValue>, IServiceProvider, CancellationToken, ValueTask<TContext>>? contextFactory)
        => new(KeySchema, ValueSchema, rules, allowNull, conditionals, contextFactory);

    public override async ValueTask<Result> ValidateAsync(
        IDictionary<TKey, TValue>? value, ValidationContext<TContext> context)
    {
        if (value is null)
        {
            return AllowNull
                ? Result.Success()
                : Result.Failure([new ValidationError(context.Path, "null_value", "Value cannot be null")]);
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
            ? Result.Success()
            : Result.Failure(errors);
    }

    public DictionaryContextSchema<TKey, TValue, TContext> MinLength(int min, string? message = null)
        => Append(new DictionaryMinLengthRule<TKey, TValue, TContext>(min, message));

    public DictionaryContextSchema<TKey, TValue, TContext> MaxLength(int max, string? message = null)
        => Append(new DictionaryMaxLengthRule<TKey, TValue, TContext>(max, message));

    public DictionaryContextSchema<TKey, TValue, TContext> NotEmpty(string? message = null)
        => Append(new DictionaryNotEmptyRule<TKey, TValue, TContext>(message));

    public DictionaryContextSchema<TKey, TValue, TContext> EachKey(ISchema<TKey, TContext> keySchema)
        => new(keySchema, ValueSchema, Rules, AllowNull, GetConditionals(), ContextFactory);

    public DictionaryContextSchema<TKey, TValue, TContext> EachValue(ISchema<TValue, TContext> valueSchema)
        => new(KeySchema, valueSchema, Rules, AllowNull, GetConditionals(), ContextFactory);

    internal DictionaryContextSchema<TKey, TValue, TContext> WithKeySchema(ISchema<TKey, TContext> keySchema)
        => new(keySchema, ValueSchema, Rules, AllowNull, GetConditionals(), ContextFactory);

    internal DictionaryContextSchema<TKey, TValue, TContext> WithValueSchema(ISchema<TValue, TContext> valueSchema)
        => new(KeySchema, valueSchema, Rules, AllowNull, GetConditionals(), ContextFactory);
}
