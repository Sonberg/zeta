using System.Linq.Expressions;
using Zeta.Adapters;
using Zeta.Core;
using Zeta.Validators;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating object values.
/// </summary>
public sealed partial class ObjectContextlessSchema<T> : ContextlessSchema<T, ObjectContextlessSchema<T>> where T : class
{
    private readonly List<IFieldContextlessValidator<T>> _fields;
    private ITypeAssertion<T>? _typeAssertion;

    internal ObjectContextlessSchema() : this(new ContextlessRuleEngine<T>(), [])
    {
    }

    internal ObjectContextlessSchema(
        ContextlessRuleEngine<T> rules,
        List<IFieldContextlessValidator<T>> fields) : base(rules)
    {
        _fields = fields;
    }

    protected override ObjectContextlessSchema<T> CreateInstance() => new();

    /// <summary>
    /// Asserts that the value is of the derived type <typeparamref name="TDerived"/>,
    /// enabling type-narrowed field validation for polymorphic types.
    /// </summary>
    public ObjectContextlessSchema<TDerived> As<TDerived>() where TDerived : class, T
    {
        var schema = new ObjectContextlessSchema<TDerived>();
        _typeAssertion = new ContextlessTypeAssertion<T, TDerived>(schema);
        return schema;
    }

    /// <summary>
    /// Adds a conditional branch to the object schema.
    /// Types are automatically inferred from the return value of the configure lambda.
    /// </summary>
    public ObjectContextlessSchema<T> If<TTarget>(
        Func<T, bool> predicate,
        ISchema<TTarget> schema)
        where TTarget : class, T
    {
        return base.If(predicate, (ISchema<T>)new TypeNarrowingContextlessSchemaAdapter<T, TTarget>(schema));
    }

    /// <summary>
    /// Adds a conditional branch with a context-aware schema. The schema must have a context factory
    /// defined via <c>.Using&lt;TContext&gt;(factory)</c>. The factory is resolved during validation
    /// using <see cref="IServiceProvider"/> from the <see cref="ValidationContext"/>.
    /// The root schema remains contextless.
    /// </summary>
    public ObjectContextlessSchema<T> If<TTarget, TContext>(
        Func<T, bool> predicate,
        ISchema<TTarget, TContext> schema)
        where TTarget : class, T
    {
        var factories = schema.GetContextFactories().ToList();
        if (factories.Count == 0)
            throw new InvalidOperationException(
                $"No context factory found for {typeof(TTarget).Name}/{typeof(TContext).Name}. " +
                "Provide a factory via .Using<TContext>(factory).");
        if (factories.Count > 1)
            throw new InvalidOperationException(
                $"Multiple context factories found for {typeof(TTarget).Name}/{typeof(TContext).Name}. " +
                "Ensure exactly one factory is defined.");

        var selfResolving = new SelfResolvingSchema<TTarget, TContext>(schema, factories[0]);
        return If(predicate, selfResolving);
    }

    /// <summary>
    /// Adds a conditional branch to the object schema.
    /// Types are automatically inferred from the return value of the configure lambda.
    /// </summary>
    public ObjectContextlessSchema<T> If<TTarget>(
        Func<T, bool> predicate,
        Func<ObjectContextlessSchema<T>, ObjectContextlessSchema<TTarget>> configure)
        where TTarget : class, T
    {
        var branchSchema = configure(Z.Object<T>());
        AddConditional(predicate, new TypeNarrowingContextlessSchemaAdapter<T, TTarget>(branchSchema));
        return this;
    }

    internal void SetTypeAssertion(ITypeAssertion<T>? assertion) => _typeAssertion = assertion;


    public override async ValueTask<Result<T>> ValidateAsync(T? value, ValidationContext execution)
    {
        if (value is null)
        {
            return AllowNull
                ? Result<T>.Success(value!)
                : Result<T>.Failure(new ValidationError(execution.Path, "null_value", "Value cannot be null"));
        }

        List<ValidationError>? errors = null;

        // Validate rules
        var ruleErrors = await Rules.ExecuteAsync(value, execution);
        if (ruleErrors != null)
        {
            errors ??= [];
            errors.AddRange(ruleErrors);
        }

        // Validate fields
        foreach (var field in _fields)
        {
            var fieldErrors = await field.ValidateAsync(value, execution);
            if (fieldErrors.Count <= 0) continue;

            errors ??= [];
            errors.AddRange(fieldErrors);
        }

        // Validate type assertion
        if (_typeAssertion != null)
        {
            var assertionErrors = await _typeAssertion.ValidateAsync(value, execution);
            if (assertionErrors.Count > 0)
            {
                errors ??= [];
                errors.AddRange(assertionErrors);
            }
        }

        // Validate conditionals
        var conditionalErrors = await ExecuteConditionalsAsync(value, execution);
        if (conditionalErrors != null)
        {
            errors ??= [];
            errors.AddRange(conditionalErrors);
        }

        return errors == null
            ? Result<T>.Success(value)
            : Result<T>.Failure(errors);
    }

    public ObjectContextlessSchema<T> Field<TProperty>(
        Expression<Func<T, TProperty?>> propertySelector,
        ISchema<TProperty> schema)
    {
        var propertyName = GetPropertyName(propertySelector);
        var getter = CreateGetter(propertySelector);
        var wrapper = NullableAdapterFactory.CreateContextlessWrapper(schema);
        _fields.Add(new FieldContextlessValidator<T, TProperty?>(propertyName, getter, wrapper));
        return this;
    }

    /// <summary>
    /// Creates a context-aware object schema with all rules, fields, and conditionals from this schema.
    /// </summary>
    /// <typeparam name="TContext">The context type for context-aware validation.</typeparam>
    public ObjectContextSchema<T, TContext> Using<TContext>()
    {
        var schema = new ObjectContextSchema<T, TContext>(Rules, _fields);
        if (AllowNull) schema.Nullable();
        schema.TransferContextlessConditionals(GetConditionals());
        if (_typeAssertion != null)
            schema.SetTypeAssertion(_typeAssertion.ToContext<TContext>());
        return schema;
    }

    /// <summary>
    /// Creates a context-aware object schema with a factory delegate for creating context data.
    /// </summary>
    public ObjectContextSchema<T, TContext> Using<TContext>(
        Func<T, IServiceProvider, CancellationToken, ValueTask<TContext>> factory)
    {
        var schema = Using<TContext>();
        schema.SetContextFactory(factory);
        return schema;
    }

    /// <summary>
    /// Creates a context-aware object schema with a factory delegate for creating context data.
    /// </summary>
    public ObjectContextSchema<T, TContext> Using<TContext>(
        Func<T, IServiceProvider, TContext> factory)
    {
        var schema = Using<TContext>();
        schema.SetContextFactory((arg1, provider, _) => new ValueTask<TContext>(factory(arg1, provider)));
        return schema;
    }

    internal static string GetPropertyName<TProperty>(Expression<Func<T, TProperty>> expr)
    {
        var body = expr.Body;
        if (body is UnaryExpression { NodeType: ExpressionType.Convert } u)
            body = u.Operand;
        if (body is MemberExpression m)
            return m.Member.Name;
        throw new ArgumentException("Expression must be a property access");
    }

    internal static Func<T, TProperty> CreateGetter<TProperty>(
        Expression<Func<T, TProperty>> expr)
    {
        if (expr.Body is UnaryExpression { NodeType: ExpressionType.Convert } u)
        {
            return Expression
                .Lambda<Func<T, TProperty>>(u, expr.Parameters)
                .Compile();
        }

        return expr.Compile();
    }
}
