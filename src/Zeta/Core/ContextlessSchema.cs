using Zeta.Rules;

namespace Zeta.Core;

public abstract class ContextlessSchema<T, TSchema> : ISchema<T> where TSchema : ContextlessSchema<T, TSchema>
{
    protected ContextlessRuleEngine<T> Rules { get; }

    public bool AllowNull { get; }

    private readonly IReadOnlyList<(Func<T, bool> Predicate, ISchema<T> Schema)>? _conditionals;

    protected ContextlessSchema() : this(new ContextlessRuleEngine<T>(), false, null)
    {
    }

    protected ContextlessSchema(ContextlessRuleEngine<T> rules) : this(rules, false, null)
    {
    }

    protected ContextlessSchema(
        ContextlessRuleEngine<T> rules,
        bool allowNull,
        IReadOnlyList<(Func<T, bool>, ISchema<T>)>? conditionals)
    {
        Rules = rules;
        AllowNull = allowNull;
        _conditionals = conditionals;
    }

    protected abstract TSchema CreateInstance();

    protected abstract TSchema CreateInstance(
        ContextlessRuleEngine<T> rules,
        bool allowNull,
        IReadOnlyList<(Func<T, bool>, ISchema<T>)>? conditionals);

    protected TSchema Append(IValidationRule<T> rule)
        => CreateInstance(Rules.Add(rule), AllowNull, _conditionals);

    public ValueTask<Result<T>> ValidateAsync(T? value)
    {
        return ValidateAsync(value, ValidationContext.Empty);
    }

    public virtual async ValueTask<Result<T>> ValidateAsync(T? value, ValidationContext context)
    {
        if (value is null)
        {
            return AllowNull
                ? Result<T>.Success(value!)
                : Result<T>.Failure(new ValidationError(context.Path, "null_value", "Value cannot be null"));
        }

        var errors = await Rules.ExecuteAsync(value!, context);

        var conditionalErrors = await ExecuteConditionalsAsync(value, context);
        if (conditionalErrors != null)
        {
            errors ??= [];
            errors.AddRange(conditionalErrors);
        }

        return errors == null
            ? Result<T>.Success(value)
            : Result<T>.Failure(errors);
    }

    internal ContextlessRuleEngine<T> GetRules()
    {
        return Rules;
    }

    public TSchema Nullable()
    {
        return CreateInstance(Rules, true, _conditionals);
    }

    public TSchema Refine(Func<T, bool> predicate, string message, string code = "custom_error")
    {
        return Append(new RefinementRule<T>((val, ctx) =>
            predicate(val)
                ? null
                : new ValidationError(ctx.Path, code, message)));
    }

    public TSchema RefineAsync(Func<T, ValueTask<bool>> predicate, string message, string code = "custom_error")
    {
        return Append(new RefinementRule<T>(async (val, ctx) =>
            await predicate(val)
                ? null
                : new ValidationError(ctx.Path, code, message)));
    }

    public TSchema RefineAsync(Func<T, CancellationToken, ValueTask<bool>> predicate, string message, string code = "custom_error")
    {
        return Append(new RefinementRule<T>(async (val, ctx) =>
            await predicate(val, ctx.CancellationToken)
                ? null
                : new ValidationError(ctx.Path, code, message)));
    }

    public TSchema If(Func<T, bool> predicate, Func<TSchema, TSchema> configure)
    {
        var schema = configure(CreateInstance());
        return AppendConditional(predicate, schema);
    }

    public TSchema If(Func<T, bool> predicate, ISchema<T> schema)
    {
        return AppendConditional(predicate, schema);
    }

    private TSchema AppendConditional(Func<T, bool> predicate, ISchema<T> schema)
    {
        var newConditionals = _conditionals != null
            ? new List<(Func<T, bool>, ISchema<T>)>(_conditionals) { (predicate, schema) }
            : new List<(Func<T, bool>, ISchema<T>)> { (predicate, schema) };
        return CreateInstance(Rules, AllowNull, newConditionals);
    }

    internal IReadOnlyList<(Func<T, bool>, ISchema<T>)>? GetConditionals() => _conditionals;

    protected async ValueTask<List<ValidationError>?> ExecuteConditionalsAsync(T value, ValidationContext context)
    {
        if (_conditionals == null) return null;

        List<ValidationError>? errors = null;

        foreach (var (predicate, schema) in _conditionals)
        {
            if (!predicate(value)) continue;

            var result = await schema.ValidateAsync(value, context);
            if (result.IsFailure)
            {
                errors ??= [];
                errors.AddRange(result.Errors);
            }
        }

        return errors;
    }
}
