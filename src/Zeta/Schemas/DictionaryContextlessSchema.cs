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
        : this(null, null, new ContextlessRuleEngine<IDictionary<TKey, TValue>>(), false, null, null)
    {
    }

    public DictionaryContextlessSchema(
        ISchema<TKey>? keySchema,
        ISchema<TValue>? valueSchema,
        ContextlessRuleEngine<IDictionary<TKey, TValue>> rules)
        : this(keySchema, valueSchema, rules, false, null, null)
    {
    }

    private DictionaryContextlessSchema(
        ISchema<TKey>? keySchema,
        ISchema<TValue>? valueSchema,
        ContextlessRuleEngine<IDictionary<TKey, TValue>> rules,
        bool allowNull,
        IReadOnlyList<(Func<IDictionary<TKey, TValue>, bool>, ISchema<IDictionary<TKey, TValue>>)>? conditionals,
        IReadOnlyList<EntryRefinement<TKey, TValue>>? entryRefinements)
        : base(rules, allowNull, conditionals)
    {
        KeySchema = keySchema;
        ValueSchema = valueSchema;
        _entryRefinements = entryRefinements;
    }

    private ISchema<TKey>? KeySchema { get; }
    private ISchema<TValue>? ValueSchema { get; }
    private readonly IReadOnlyList<EntryRefinement<TKey, TValue>>? _entryRefinements;

    protected override DictionaryContextlessSchema<TKey, TValue> CreateInstance() => new();

    protected override DictionaryContextlessSchema<TKey, TValue> CreateInstance(
        ContextlessRuleEngine<IDictionary<TKey, TValue>> rules,
        bool allowNull,
        IReadOnlyList<(Func<IDictionary<TKey, TValue>, bool>, ISchema<IDictionary<TKey, TValue>>)>? conditionals)
        => new(KeySchema, ValueSchema, rules, allowNull, conditionals, _entryRefinements);

    // Stage order for dictionary schemas: rules, then key/value entries, then entry refinements, then conditionals.
    // Memoized per instance (schemas are immutable), so a hot ValidateAsync allocates no stage array or delegates.
    private Func<IDictionary<TKey, TValue>, ValidationRun, ValueTask<IReadOnlyList<ValidationError>?>>[]? _stages;
    private Func<IDictionary<TKey, TValue>, ValidationRun, ValueTask<IReadOnlyList<ValidationError>?>>[] Stages() => _stages ??=
    [
        ValidateRulesAsync,
        ValidateEntriesAsync,
        ValidateEntryRefinementsAsync,
        ValidateConditionalsAsync,
    ];

    public override async ValueTask<Result<IDictionary<TKey, TValue>>> ValidateAsync(
        IDictionary<TKey, TValue>? value, ValidationRun context)
    {
        if (value is null)
        {
            return AllowNull
                ? Result<IDictionary<TKey, TValue>>.Success(value!)
                : Result<IDictionary<TKey, TValue>>.Failure(
                    new ValidationError(context.PathSegments, "null_value", "Value cannot be null"));
        }

        var errors = await ValidationPipeline.RunAsync(value, context, Stages());
        return errors is null
            ? Result<IDictionary<TKey, TValue>>.Success(value)
            : Result<IDictionary<TKey, TValue>>.Failure(errors);
    }

    private async ValueTask<IReadOnlyList<ValidationError>?> ValidateRulesAsync(IDictionary<TKey, TValue> value, ValidationRun context)
        => await Rules.ExecuteAsync(value, context);

    private async ValueTask<IReadOnlyList<ValidationError>?> ValidateEntriesAsync(IDictionary<TKey, TValue> value, ValidationRun context)
    {
        if (KeySchema is null && ValueSchema is null) return null;

        List<ValidationError>? errors = null;
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

        return errors;
    }

    private async ValueTask<IReadOnlyList<ValidationError>?> ValidateEntryRefinementsAsync(IDictionary<TKey, TValue> value, ValidationRun context)
    {
        if (_entryRefinements is null) return null;

        List<ValidationError>? errors = null;
        foreach (var kvp in value)
        {
            foreach (var refinement in _entryRefinements)
            {
                var passed = await refinement.Predicate(kvp.Key, kvp.Value, context.CancellationToken);
                if (!passed)
                {
                    errors ??= [];
                    errors.Add(new ValidationError(
                        context.PushKey(kvp.Key).PathSegments,
                        refinement.Code, refinement.Message));
                }
            }
        }

        return errors;
    }

    private async ValueTask<IReadOnlyList<ValidationError>?> ValidateConditionalsAsync(IDictionary<TKey, TValue> value, ValidationRun context)
        => await ExecuteConditionalsAsync(value, context);

    public DictionaryContextlessSchema<TKey, TValue> MinLength(int min, string? message = null)
        => Append(new DictionaryMinLengthRule<TKey, TValue>(min, message));

    public DictionaryContextlessSchema<TKey, TValue> MaxLength(int max, string? message = null)
        => Append(new DictionaryMaxLengthRule<TKey, TValue>(max, message));

    public DictionaryContextlessSchema<TKey, TValue> NotEmpty(string? message = null)
        => Append(new DictionaryNotEmptyRule<TKey, TValue>(message));

    public DictionaryContextlessSchema<TKey, TValue> EachKey(ISchema<TKey> keySchema)
        => new(keySchema, ValueSchema, Rules, AllowNull, GetConditionals(), _entryRefinements);

    public DictionaryContextlessSchema<TKey, TValue> EachValue(ISchema<TValue> valueSchema)
        => new(KeySchema, valueSchema, Rules, AllowNull, GetConditionals(), _entryRefinements);

    internal DictionaryContextlessSchema<TKey, TValue> WithKeySchema(ISchema<TKey> keySchema)
        => new(keySchema, ValueSchema, Rules, AllowNull, GetConditionals(), _entryRefinements);

    internal DictionaryContextlessSchema<TKey, TValue> WithValueSchema(ISchema<TValue> valueSchema)
        => new(KeySchema, valueSchema, Rules, AllowNull, GetConditionals(), _entryRefinements);

    /// <summary>
    /// Validates each dictionary entry with the given sync predicate, emitting one error per failing entry
    /// at a bracket-notation path (e.g. <c>$[myKey]</c>).
    /// </summary>
    public DictionaryContextlessSchema<TKey, TValue> RefineEachEntry(
        Func<TKey, TValue, bool> predicate, string message, string code = "entry_invalid")
    {
        var refinement = new EntryRefinement<TKey, TValue>(
            (k, v, _) => ValueTask.FromResult(predicate(k, v)), message, code);
        var list = AppendRefinement(_entryRefinements, refinement);
        return new(KeySchema, ValueSchema, Rules, AllowNull, GetConditionals(), list);
    }

    /// <summary>
    /// Validates each dictionary entry with the given async predicate, emitting one error per failing entry
    /// at a bracket-notation path (e.g. <c>$[myKey]</c>).
    /// </summary>
    public DictionaryContextlessSchema<TKey, TValue> RefineEachEntryAsync(
        Func<TKey, TValue, CancellationToken, ValueTask<bool>> predicate, string message, string code = "entry_invalid")
    {
        var refinement = new EntryRefinement<TKey, TValue>(predicate, message, code);
        var list = AppendRefinement(_entryRefinements, refinement);
        return new(KeySchema, ValueSchema, Rules, AllowNull, GetConditionals(), list);
    }

    private static List<EntryRefinement<TKey, TValue>> AppendRefinement(
        IReadOnlyList<EntryRefinement<TKey, TValue>>? existing, EntryRefinement<TKey, TValue> next)
    {
        if (existing is null)
            return [next];
        var result = new List<EntryRefinement<TKey, TValue>>(existing.Count + 1);
        result.AddRange(existing);
        result.Add(next);
        return result;
    }

    /// <summary>
    /// Creates a context-aware dictionary schema with all rules from this schema.
    /// The key and value schemas are adapted to work in the context-aware environment.
    /// </summary>
    public DictionaryContextSchema<TKey, TValue, TContext> Using<TContext>()
    {
        var schema = new DictionaryContextSchema<TKey, TValue, TContext>(KeySchema, ValueSchema, Rules, _entryRefinements);
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

    /// <summary>
    /// Creates a context-aware dictionary schema with a synchronous factory delegate for creating context data.
    /// </summary>
    public DictionaryContextSchema<TKey, TValue, TContext> Using<TContext>(
        Func<IDictionary<TKey, TValue>, IServiceProvider, TContext> factory)
    {
        return Using<TContext>().WithContextFactory((arg1, provider, _) => new ValueTask<TContext>(factory(arg1, provider)));
    }
}
