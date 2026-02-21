using System.Collections.Generic;
using System.Linq;
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
    private readonly IReadOnlyList<EntryRefinement<TKey, TValue, TContext>>? _entryRefinements;

    internal DictionaryContextSchema() : this(
        (ISchema<TKey, TContext>?)null,
        (ISchema<TValue, TContext>?)null,
        new ContextRuleEngine<IDictionary<TKey, TValue>, TContext>(),
        false, null, null, null)
    {
    }

    public DictionaryContextSchema(
        ISchema<TKey, TContext>? keySchema,
        ISchema<TValue, TContext>? valueSchema,
        ContextRuleEngine<IDictionary<TKey, TValue>, TContext> rules)
        : this(keySchema, valueSchema, rules, false, null, null, null)
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
            false, null, null, null)
    {
    }

    /// <summary>
    /// Transfer constructor: called from <see cref="DictionaryContextlessSchema{TKey,TValue}.Using{TContext}()"/>
    /// to promote contextless entry refinements.
    /// </summary>
    internal DictionaryContextSchema(
        ISchema<TKey>? keySchema,
        ISchema<TValue>? valueSchema,
        ContextlessRuleEngine<IDictionary<TKey, TValue>> rules,
        IReadOnlyList<EntryRefinement<TKey, TValue>>? contextlessEntryRefinements)
        : this(
            keySchema is not null ? new SchemaAdapter<TKey, TContext>(keySchema) : null,
            valueSchema is not null ? new SchemaAdapter<TValue, TContext>(valueSchema) : null,
            rules.ToContext<TContext>(),
            false, null, null,
            contextlessEntryRefinements?.Select(r => r.ToContextAware<TContext>()).ToList())
    {
    }

    private DictionaryContextSchema(
        ISchema<TKey, TContext>? keySchema,
        ISchema<TValue, TContext>? valueSchema,
        ContextRuleEngine<IDictionary<TKey, TValue>, TContext> rules,
        bool allowNull,
        IReadOnlyList<ISchemaConditional<IDictionary<TKey, TValue>, TContext>>? conditionals,
        Func<IDictionary<TKey, TValue>, IServiceProvider, CancellationToken, ValueTask<TContext>>? contextFactory,
        IReadOnlyList<EntryRefinement<TKey, TValue, TContext>>? entryRefinements)
        : base(rules, allowNull, conditionals, contextFactory)
    {
        KeySchema = keySchema;
        ValueSchema = valueSchema;
        _entryRefinements = entryRefinements;
    }

    protected override DictionaryContextSchema<TKey, TValue, TContext> CreateInstance() => new();

    private protected override DictionaryContextSchema<TKey, TValue, TContext> CreateInstance(
        ContextRuleEngine<IDictionary<TKey, TValue>, TContext> rules,
        bool allowNull,
        IReadOnlyList<ISchemaConditional<IDictionary<TKey, TValue>, TContext>>? conditionals,
        Func<IDictionary<TKey, TValue>, IServiceProvider, CancellationToken, ValueTask<TContext>>? contextFactory)
        => new(KeySchema, ValueSchema, rules, allowNull, conditionals, contextFactory, _entryRefinements);

    public override async ValueTask<Result<IDictionary<TKey, TValue>, TContext>> ValidateAsync(
        IDictionary<TKey, TValue>? value, ValidationContext<TContext> context)
    {
        if (value is null)
        {
            return AllowNull
                ? Result<IDictionary<TKey, TValue>, TContext>.Success(value!, context.Data)
                : Result<IDictionary<TKey, TValue>, TContext>.Failure(
                    [new ValidationError(context.PathSegments, "null_value", "Value cannot be null")]);
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
                var valueContext = context.Push("values").PushIndex(index);
                var valueResult = await ValueSchema.ValidateAsync(kvp.Value, valueContext);
                if (valueResult.IsFailure)
                {
                    errors ??= [];
                    errors.AddRange(valueResult.Errors);
                }
            }

            index++;
        }

        if (_entryRefinements is not null)
        {
            foreach (var kvp in value)
            {
                foreach (var refinement in _entryRefinements)
                {
                    var passed = await refinement.Predicate(kvp.Key, kvp.Value, context.Data, context.CancellationToken);
                    if (!passed)
                    {
                        errors ??= [];
                        errors.Add(new ValidationError(
                            context.PushKey(kvp.Key).Path,
                            refinement.Code, refinement.Message));
                    }
                }
            }
        }

        var conditionalErrors = await ExecuteConditionalsAsync(value, context);
        if (conditionalErrors != null)
        {
            errors ??= [];
            errors.AddRange(conditionalErrors);
        }

        return errors == null
            ? Result<IDictionary<TKey, TValue>, TContext>.Success(value, context.Data)
            : Result<IDictionary<TKey, TValue>, TContext>.Failure(errors);
    }

    public DictionaryContextSchema<TKey, TValue, TContext> MinLength(int min, string? message = null)
        => Append(new DictionaryMinLengthRule<TKey, TValue, TContext>(min, message));

    public DictionaryContextSchema<TKey, TValue, TContext> MaxLength(int max, string? message = null)
        => Append(new DictionaryMaxLengthRule<TKey, TValue, TContext>(max, message));

    public DictionaryContextSchema<TKey, TValue, TContext> NotEmpty(string? message = null)
        => Append(new DictionaryNotEmptyRule<TKey, TValue, TContext>(message));

    public DictionaryContextSchema<TKey, TValue, TContext> EachKey(ISchema<TKey, TContext> keySchema)
        => new(keySchema, ValueSchema, Rules, AllowNull, GetConditionals(), ContextFactory, _entryRefinements);

    public DictionaryContextSchema<TKey, TValue, TContext> EachValue(ISchema<TValue, TContext> valueSchema)
        => new(KeySchema, valueSchema, Rules, AllowNull, GetConditionals(), ContextFactory, _entryRefinements);

    internal DictionaryContextSchema<TKey, TValue, TContext> WithKeySchema(ISchema<TKey, TContext> keySchema)
        => new(keySchema, ValueSchema, Rules, AllowNull, GetConditionals(), ContextFactory, _entryRefinements);

    internal DictionaryContextSchema<TKey, TValue, TContext> WithValueSchema(ISchema<TValue, TContext> valueSchema)
        => new(KeySchema, valueSchema, Rules, AllowNull, GetConditionals(), ContextFactory, _entryRefinements);

    /// <summary>
    /// Validates each dictionary entry with the given sync value-only predicate, emitting one error per
    /// failing entry at a bracket-notation path (e.g. <c>$[myKey]</c>).
    /// </summary>
    public DictionaryContextSchema<TKey, TValue, TContext> RefineEachEntry(
        Func<TKey, TValue, bool> predicate, string message, string code = "entry_invalid")
    {
        var refinement = new EntryRefinement<TKey, TValue, TContext>(
            (k, v, _, _) => ValueTaskHelper.FromResult(predicate(k, v)), message, code);
        return new(KeySchema, ValueSchema, Rules, AllowNull, GetConditionals(), ContextFactory, AppendRefinement(_entryRefinements, refinement));
    }

    /// <summary>
    /// Validates each dictionary entry with the given sync value+context predicate, emitting one error per
    /// failing entry at a bracket-notation path (e.g. <c>$[myKey]</c>).
    /// </summary>
    public DictionaryContextSchema<TKey, TValue, TContext> RefineEachEntry(
        Func<TKey, TValue, TContext, bool> predicate, string message, string code = "entry_invalid")
    {
        var refinement = new EntryRefinement<TKey, TValue, TContext>(
            (k, v, ctx, _) => ValueTaskHelper.FromResult(predicate(k, v, ctx)), message, code);
        return new(KeySchema, ValueSchema, Rules, AllowNull, GetConditionals(), ContextFactory, AppendRefinement(_entryRefinements, refinement));
    }

    /// <summary>
    /// Validates each dictionary entry with the given async value-only predicate, emitting one error per
    /// failing entry at a bracket-notation path (e.g. <c>$[myKey]</c>).
    /// </summary>
    public DictionaryContextSchema<TKey, TValue, TContext> RefineEachEntryAsync(
        Func<TKey, TValue, CancellationToken, ValueTask<bool>> predicate, string message, string code = "entry_invalid")
    {
        var refinement = new EntryRefinement<TKey, TValue, TContext>(
            (k, v, _, ct) => predicate(k, v, ct), message, code);
        return new(KeySchema, ValueSchema, Rules, AllowNull, GetConditionals(), ContextFactory, AppendRefinement(_entryRefinements, refinement));
    }

    /// <summary>
    /// Validates each dictionary entry with the given async value+context+CT predicate, emitting one error per
    /// failing entry at a bracket-notation path (e.g. <c>$[myKey]</c>).
    /// </summary>
    public DictionaryContextSchema<TKey, TValue, TContext> RefineEachEntryAsync(
        Func<TKey, TValue, TContext, CancellationToken, ValueTask<bool>> predicate, string message, string code = "entry_invalid")
    {
        var refinement = new EntryRefinement<TKey, TValue, TContext>(predicate, message, code);
        return new(KeySchema, ValueSchema, Rules, AllowNull, GetConditionals(), ContextFactory, AppendRefinement(_entryRefinements, refinement));
    }

    private static List<EntryRefinement<TKey, TValue, TContext>> AppendRefinement(
        IReadOnlyList<EntryRefinement<TKey, TValue, TContext>>? existing, EntryRefinement<TKey, TValue, TContext> next)
    {
        if (existing is null)
            return [next];
        var result = new List<EntryRefinement<TKey, TValue, TContext>>(existing.Count + 1);
        result.AddRange(existing);
        result.Add(next);
        return result;
    }
}
