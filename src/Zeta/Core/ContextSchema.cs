using Zeta.Rules;

namespace Zeta.Core;

internal interface ISchemaConditional<T, TContext>
{
    ValueTask<IReadOnlyList<ValidationError>> ValidateAsync(T value, ValidationContext<TContext> context);

    IEnumerable<Func<T, IServiceProvider, CancellationToken, Task<TContext>>> GetContextFactories();
}

internal sealed class ContextlessSchemaConditional<T, TContext> : ISchemaConditional<T, TContext>
{
    private readonly Func<T, bool> _predicate;
    private readonly ISchema<T> _schema;

    public ContextlessSchemaConditional(Func<T, bool> predicate, ISchema<T> schema)
    {
        _predicate = predicate;
        _schema = schema;
    }

    public async ValueTask<IReadOnlyList<ValidationError>> ValidateAsync(T value, ValidationContext<TContext> context)
    {
        if (!_predicate(value)) return [];

        var result = await _schema.ValidateAsync(value, context);
        return result.IsFailure ? result.Errors : [];
    }

    public IEnumerable<Func<T, IServiceProvider, CancellationToken, Task<TContext>>> GetContextFactories()
    {
        return [];
    }
}

internal sealed class ContextAwareSchemaConditional<T, TContext> : ISchemaConditional<T, TContext>
{
    private readonly Func<T, TContext, bool> _predicate;
    private readonly ISchema<T, TContext> _schema;

    public ContextAwareSchemaConditional(Func<T, TContext, bool> predicate, ISchema<T, TContext> schema)
    {
        _predicate = predicate;
        _schema = schema;
    }

    public async ValueTask<IReadOnlyList<ValidationError>> ValidateAsync(T value, ValidationContext<TContext> context)
    {
        if (!_predicate(value, context.Data)) return [];

        var result = await _schema.ValidateAsync(value, context);
        return result.IsFailure ? result.Errors : [];
    }

    public IEnumerable<Func<T, IServiceProvider, CancellationToken, Task<TContext>>> GetContextFactories()
    {
        return _schema.GetContextFactories();
    }
}

internal sealed class ValueOnlySchemaConditional<T, TContext> : ISchemaConditional<T, TContext>
{
    private readonly Func<T, bool> _predicate;
    private readonly ISchema<T, TContext> _schema;

    public ValueOnlySchemaConditional(Func<T, bool> predicate, ISchema<T, TContext> schema)
    {
        _predicate = predicate;
        _schema = schema;
    }

    public async ValueTask<IReadOnlyList<ValidationError>> ValidateAsync(T value, ValidationContext<TContext> context)
    {
        if (!_predicate(value)) return [];

        var result = await _schema.ValidateAsync(value, context);
        return result.IsFailure ? result.Errors : [];
    }

    public IEnumerable<Func<T, IServiceProvider, CancellationToken, Task<TContext>>> GetContextFactories()
    {
        return _schema.GetContextFactories();
    }
}

/// <summary>
/// Base class for context-aware schemas.
/// </summary>
public abstract class ContextSchema<T, TContext, TSchema> : ISchema<T, TContext>, IContextFactorySchema<T, TContext> where TSchema : ContextSchema<T, TContext, TSchema>
{
    protected ContextRuleEngine<T, TContext> Rules { get; }

    public bool AllowNull { get; private set; }

    public Func<T, IServiceProvider, CancellationToken, Task<TContext>>? ContextFactory { get; private set; }

    private List<ISchemaConditional<T, TContext>>? _conditionals;

    protected ContextSchema() : this(new ContextRuleEngine<T, TContext>())
    {
    }

    protected ContextSchema(ContextRuleEngine<T, TContext> rules)
    {
        Rules = rules;
    }

    protected abstract TSchema CreateInstance();

    public virtual async ValueTask<Result> ValidateAsync(T? value, ValidationContext<TContext> context)
    {
        if (value is null)
        {
            return AllowNull
                ? Result.Success()
                : Result<T>.Failure(new ValidationError(context.Path, "null_value", "Value cannot be null"));
        }

        var errors = await Rules.ExecuteAsync(value, context);

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

    protected void Use(IValidationRule<T, TContext> rule)
    {
        Rules.Add(rule);
    }
    
    internal ContextRuleEngine<T, TContext> GetRules()
    {
        return Rules;
    }

    public TSchema Nullable()
    {
        AllowNull = true;
        return this as TSchema ?? throw new InvalidOperationException();
    }

    internal void SetContextFactory(Func<T, IServiceProvider, CancellationToken, Task<TContext>> factory)
        => ContextFactory = factory;

    IEnumerable<Func<T, IServiceProvider, CancellationToken, Task<TContext>>> ISchema<T, TContext>.GetContextFactories()
    {
        return GetContextFactoriesCore();
    }

    protected virtual IEnumerable<Func<T, IServiceProvider, CancellationToken, Task<TContext>>> GetContextFactoriesCore()
    {
        if (ContextFactory is not null)
        {
            yield return ContextFactory;
        }

        if (_conditionals == null) yield break;
        foreach (var conditional in _conditionals)
        {
            foreach (var factory in conditional.GetContextFactories())
            {
                yield return factory;
            }
        }
    }

    public TSchema Refine(Func<T, TContext, bool> predicate, string message, string code = "custom_error")
    {
        Use(new RefinementRule<T, TContext>((val, ctx) =>
            predicate(val, ctx.Data)
                ? null
                : new ValidationError(ctx.Path, code, message)));
        return this as TSchema ?? throw new InvalidOperationException();
    }

    public TSchema RefineAsync(Func<T, TContext, ValueTask<bool>> predicate, string message, string code = "custom_error")
    {
        Use(new RefinementRule<T, TContext>(async (val, ctx) =>
            await predicate(val, ctx.Data)
                ? null
                : new ValidationError(ctx.Path, code, message)));
        return this as TSchema ?? throw new InvalidOperationException();
    }

    public TSchema RefineAsync(Func<T, TContext, CancellationToken, ValueTask<bool>> predicate, string message, string code = "custom_error")
    {
        Use(new RefinementRule<T, TContext>(async (val, ctx) =>
            await predicate(val, ctx.Data, ctx.CancellationToken)
                ? null
                : new ValidationError(ctx.Path, code, message)));
        return this as TSchema ?? throw new InvalidOperationException();
    }

    public TSchema Refine(Func<T, bool> predicate, string message, string code = "custom_error")
    {
        return Refine((val, _) => predicate(val), message, code);
    }

    public TSchema RefineAsync(Func<T, ValueTask<bool>> predicate, string message, string code = "custom_error")
    {
        Use(new RefinementRule<T, TContext>(async (val, ctx) =>
            await predicate(val)
                ? null
                : new ValidationError(ctx.Path, code, message)));
        return this as TSchema ?? throw new InvalidOperationException();
    }

    public TSchema RefineAsync(Func<T, CancellationToken, ValueTask<bool>> predicate, string message, string code = "custom_error")
    {
        Use(new RefinementRule<T, TContext>(async (val, ctx) =>
            await predicate(val, ctx.CancellationToken)
                ? null
                : new ValidationError(ctx.Path, code, message)));
        return this as TSchema ?? throw new InvalidOperationException();
    }

    public TSchema If(Func<T, bool> predicate, Action<TSchema> configure)
    {
        var schema = CreateInstance();
        configure(schema);
        _conditionals ??= [];
        _conditionals.Add(new ValueOnlySchemaConditional<T, TContext>(predicate, schema));
        return (TSchema)this;
    }

    public TSchema If(Func<T, TContext, bool> predicate, Action<TSchema> configure)
    {
        var schema = CreateInstance();
        configure(schema);
        _conditionals ??= [];
        _conditionals.Add(new ContextAwareSchemaConditional<T, TContext>(predicate, schema));
        return (TSchema)this;
    }

    internal void TransferContextlessConditionals(IReadOnlyList<(Func<T, bool>, ISchema<T>)>? conditionals)
    {
        if (conditionals == null) return;

        _conditionals ??= [];
        foreach (var (predicate, schema) in conditionals)
        {
            _conditionals.Add(new ContextlessSchemaConditional<T, TContext>(predicate, schema));
        }
    }

    internal void AddConditional(Func<T, bool> predicate, ISchema<T, TContext> schema)
    {
        _conditionals ??= [];
        _conditionals.Add(new ValueOnlySchemaConditional<T, TContext>(predicate, schema));
    }

    protected async ValueTask<List<ValidationError>?> ExecuteConditionalsAsync(T value, ValidationContext<TContext> context)
    {
        if (_conditionals == null) return null;

        List<ValidationError>? errors = null;

        foreach (var conditional in _conditionals)
        {
            var conditionalErrors = await conditional.ValidateAsync(value, context);
            if (conditionalErrors.Count <= 0) continue;

            errors ??= [];
            errors.AddRange(conditionalErrors);
        }

        return errors;
    }
}
