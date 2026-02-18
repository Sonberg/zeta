using Zeta.Rules;

namespace Zeta.Core;

internal interface ISchemaConditional<T, TContext>
{
    ValueTask<IReadOnlyList<ValidationError>> ValidateAsync(T value, ValidationContext<TContext> context);

    IEnumerable<Func<T, IServiceProvider, CancellationToken, ValueTask<TContext>>> GetContextFactories();
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

    public IEnumerable<Func<T, IServiceProvider, CancellationToken, ValueTask<TContext>>> GetContextFactories()
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

    public IEnumerable<Func<T, IServiceProvider, CancellationToken, ValueTask<TContext>>> GetContextFactories()
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

    public IEnumerable<Func<T, IServiceProvider, CancellationToken, ValueTask<TContext>>> GetContextFactories()
    {
        return _schema.GetContextFactories();
    }
}

/// <summary>
/// Base class for context-aware schemas.
/// </summary>
public abstract class ContextSchema<T, TContext, TSchema> : IContextSchema<T, TContext>, IContextFactorySchema<T, TContext> where TSchema : ContextSchema<T, TContext, TSchema>
{
    protected ContextRuleEngine<T, TContext> Rules { get; }

    public bool AllowNull { get; }

    public Func<T, IServiceProvider, CancellationToken, ValueTask<TContext>>? ContextFactory { get; }

    private readonly IReadOnlyList<ISchemaConditional<T, TContext>>? _conditionals;

    protected ContextSchema() : this(new ContextRuleEngine<T, TContext>(), false, null, null)
    {
    }

    protected ContextSchema(ContextRuleEngine<T, TContext> rules) : this(rules, false, null, null)
    {
    }

    private protected ContextSchema(
        ContextRuleEngine<T, TContext> rules,
        bool allowNull,
        IReadOnlyList<ISchemaConditional<T, TContext>>? conditionals,
        Func<T, IServiceProvider, CancellationToken, ValueTask<TContext>>? contextFactory)
    {
        Rules = rules;
        AllowNull = allowNull;
        _conditionals = conditionals;
        ContextFactory = contextFactory;
    }

    protected abstract TSchema CreateInstance();

    private protected abstract TSchema CreateInstance(
        ContextRuleEngine<T, TContext> rules,
        bool allowNull,
        IReadOnlyList<ISchemaConditional<T, TContext>>? conditionals,
        Func<T, IServiceProvider, CancellationToken, ValueTask<TContext>>? contextFactory);

    protected TSchema Append(IValidationRule<T, TContext> rule)
        => CreateInstance(Rules.Add(rule), AllowNull, _conditionals, ContextFactory);

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

    async ValueTask<Result<T>> ISchema<T>.ValidateAsync(T? value, ValidationContext context)
    {
        if (value is null)
        {
            return AllowNull
                ? Result<T>.Success(value!)
                : Result<T>.Failure(new ValidationError(context.Path, "null_value", "Value cannot be null"));
        }

        var serviceProvider = context.ServiceProvider
            ?? throw new InvalidOperationException(
                "IServiceProvider is required for context factory resolution. " +
                "Ensure the validation context includes a service provider.");

        var contextData = await ContextFactoryResolver.ResolveAsync(
            value,
            GetContextFactoriesCore(),
            serviceProvider,
            context.CancellationToken);
        var typedContext = new ValidationContext<TContext>(
            context.Path,
            contextData,
            context.TimeProvider,
            context.CancellationToken,
            context.ServiceProvider);

        var result = await ValidateAsync(value, typedContext);
        return result.IsSuccess
            ? Result<T>.Success(value)
            : Result<T>.Failure(result.Errors);
    }

    internal ContextRuleEngine<T, TContext> GetRules()
    {
        return Rules;
    }

    internal IReadOnlyList<ISchemaConditional<T, TContext>>? GetConditionals() => _conditionals;

    public TSchema Nullable()
    {
        return CreateInstance(Rules, true, _conditionals, ContextFactory);
    }

    internal TSchema WithContextFactory(Func<T, IServiceProvider, CancellationToken, ValueTask<TContext>> factory)
        => CreateInstance(Rules, AllowNull, _conditionals, factory);

    IEnumerable<Func<T, IServiceProvider, CancellationToken, ValueTask<TContext>>> ISchema<T, TContext>.GetContextFactories()
    {
        return GetContextFactoriesCore();
    }

    protected virtual IEnumerable<Func<T, IServiceProvider, CancellationToken, ValueTask<TContext>>> GetContextFactoriesCore()
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
        return Append(new RefinementRule<T, TContext>((val, ctx) =>
            predicate(val, ctx.Data)
                ? null
                : new ValidationError(ctx.Path, code, message)));
    }

    public TSchema RefineAsync(Func<T, TContext, ValueTask<bool>> predicate, string message, string code = "custom_error")
    {
        return Append(new RefinementRule<T, TContext>(async (val, ctx) =>
            await predicate(val, ctx.Data)
                ? null
                : new ValidationError(ctx.Path, code, message)));
    }

    public TSchema RefineAsync(Func<T, TContext, CancellationToken, ValueTask<bool>> predicate, string message, string code = "custom_error")
    {
        return Append(new RefinementRule<T, TContext>(async (val, ctx) =>
            await predicate(val, ctx.Data, ctx.CancellationToken)
                ? null
                : new ValidationError(ctx.Path, code, message)));
    }

    public TSchema Refine(Func<T, bool> predicate, string message, string code = "custom_error")
    {
        return Refine((val, _) => predicate(val), message, code);
    }

    public TSchema RefineAsync(Func<T, ValueTask<bool>> predicate, string message, string code = "custom_error")
    {
        return Append(new RefinementRule<T, TContext>(async (val, ctx) =>
            await predicate(val)
                ? null
                : new ValidationError(ctx.Path, code, message)));
    }

    public TSchema RefineAsync(Func<T, CancellationToken, ValueTask<bool>> predicate, string message, string code = "custom_error")
    {
        return Append(new RefinementRule<T, TContext>(async (val, ctx) =>
            await predicate(val, ctx.CancellationToken)
                ? null
                : new ValidationError(ctx.Path, code, message)));
    }

    public TSchema If(Func<T, bool> predicate, Func<TSchema, TSchema> configure)
    {
        var schema = configure(CreateInstance());
        return AppendConditional(new ValueOnlySchemaConditional<T, TContext>(predicate, schema));
    }

    public TSchema If(Func<T, bool> predicate, ISchema<T, TContext> schema)
    {
        return AppendConditional(new ValueOnlySchemaConditional<T, TContext>(predicate, schema));
    }

    public TSchema If(Func<T, TContext, bool> predicate, Func<TSchema, TSchema> configure)
    {
        var schema = configure(CreateInstance());
        return AppendConditional(new ContextAwareSchemaConditional<T, TContext>(predicate, schema));
    }

    public TSchema If(Func<T, TContext, bool> predicate, ISchema<T, TContext> schema)
    {
        return AppendConditional(new ContextAwareSchemaConditional<T, TContext>(predicate, schema));
    }

    private TSchema AppendConditional(ISchemaConditional<T, TContext> conditional)
    {
        var newConditionals = _conditionals != null
            ? new List<ISchemaConditional<T, TContext>>(_conditionals) { conditional }
            : new List<ISchemaConditional<T, TContext>> { conditional };
        return CreateInstance(Rules, AllowNull, newConditionals, ContextFactory);
    }

    internal TSchema TransferContextlessConditionals(IReadOnlyList<(Func<T, bool>, ISchema<T>)>? conditionals)
    {
        if (conditionals == null) return (TSchema)this;

        var newConditionals = _conditionals != null
            ? new List<ISchemaConditional<T, TContext>>(_conditionals)
            : new List<ISchemaConditional<T, TContext>>();

        foreach (var (predicate, schema) in conditionals)
        {
            newConditionals.Add(new ContextlessSchemaConditional<T, TContext>(predicate, schema));
        }

        return CreateInstance(Rules, AllowNull, newConditionals, ContextFactory);
    }

    internal TSchema AddConditional(Func<T, bool> predicate, ISchema<T, TContext> schema)
    {
        return AppendConditional(new ValueOnlySchemaConditional<T, TContext>(predicate, schema));
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
