using System.Linq.Expressions;
using Zeta.Adapters;
using Zeta.Core;
using Zeta.Rules;
using Zeta.Validators;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating object values.
/// </summary>
public sealed partial class ObjectContextlessSchema<T> : ContextlessSchema<T, ObjectContextlessSchema<T>> where T : class
{
    private readonly IReadOnlyList<IFieldContextlessValidator<T>> _fields;
    private readonly ITypeAssertion<T>? _typeAssertion;

    internal ObjectContextlessSchema() : this(new ContextlessRuleEngine<T>(), [], null, false, null)
    {
    }

    internal ObjectContextlessSchema(
        ContextlessRuleEngine<T> rules,
        IReadOnlyList<IFieldContextlessValidator<T>> fields) : this(rules, fields, null, false, null)
    {
    }

    private ObjectContextlessSchema(
        ContextlessRuleEngine<T> rules,
        IReadOnlyList<IFieldContextlessValidator<T>> fields,
        ITypeAssertion<T>? typeAssertion,
        bool allowNull,
        IReadOnlyList<(Func<T, bool>, ISchema<T>)>? conditionals) : base(rules, allowNull, conditionals)
    {
        _fields = fields;
        _typeAssertion = typeAssertion;
    }

    protected override ObjectContextlessSchema<T> CreateInstance() => new();

    protected override ObjectContextlessSchema<T> CreateInstance(
        ContextlessRuleEngine<T> rules,
        bool allowNull,
        IReadOnlyList<(Func<T, bool>, ISchema<T>)>? conditionals)
        => new(rules, _fields, _typeAssertion, allowNull, conditionals);

    internal ObjectContextlessSchema<T> AddField(IFieldContextlessValidator<T> field)
    {
        var newFields = new List<IFieldContextlessValidator<T>>(_fields) { field };
        return new ObjectContextlessSchema<T>(Rules, newFields, _typeAssertion, AllowNull, GetConditionals());
    }

    /// <summary>
    /// Asserts that the value is of the derived type <typeparamref name="TDerived"/>,
    /// enabling type-narrowed field validation for polymorphic types.
    /// </summary>
    public ObjectContextlessSchema<TDerived> As<TDerived>() where TDerived : class, T
    {
        return new ObjectContextlessSchema<TDerived>();
    }

    internal ObjectContextlessSchema<T> WithTypeAssertion(ITypeAssertion<T> assertion)
        => new(Rules, _fields, assertion, AllowNull, GetConditionals());

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
    /// Adds a conditional branch with a context-aware object schema.
    /// This overload exists to avoid ambiguity when a context-aware schema is also assignable to ISchema&lt;TTarget&gt;.
    /// </summary>
    public ObjectContextlessSchema<T> If<TTarget, TContext>(
        Func<T, bool> predicate,
        ObjectContextSchema<TTarget, TContext> schema)
        where TTarget : class, T
    {
        return If<TTarget, TContext>(predicate, (ISchema<TTarget, TContext>)schema);
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
        switch (factories.Count)
        {
            case 0:
                throw new InvalidOperationException(
                    $"No context factory found for {typeof(TTarget).Name}/{typeof(TContext).Name}. " +
                    "Provide a factory via .Using<TContext>(factory).");
            case > 1:
                throw new InvalidOperationException(
                    $"Multiple context factories found for {typeof(TTarget).Name}/{typeof(TContext).Name}. " +
                    "Ensure exactly one factory is defined.");
            default:
            {
                return If(predicate, new SelfResolvingSchema<TTarget, TContext>(schema, factories[0]));
            }
        }
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
        var branchSchema = configure(Z.Schema<T>());
        return base.If(predicate, (ISchema<T>)new TypeNarrowingContextlessSchemaAdapter<T, TTarget>(branchSchema));
    }


    // Stage order for object schemas: fields, then type assertion, then conditionals, then rules.
    // Memoized per instance (schemas are immutable), mirroring the rule engine's materialized cache,
    // so a hot ValidateAsync allocates no stage array or delegates.
    private Func<T, ValidationContext, ValueTask<IReadOnlyList<ValidationError>?>>[]? _stages;
    private Func<T, ValidationContext, ValueTask<IReadOnlyList<ValidationError>?>>[] Stages() => _stages ??=
    [
        ValidateFieldsAsync,
        ValidateTypeAssertionAsync,
        ValidateConditionalsAsync,
        ValidateRulesAsync,
    ];

    public override async ValueTask<Result<T>> ValidateAsync(T? value, ValidationContext execution)
    {
        if (value is null)
        {
            return AllowNull
                ? Result<T>.Success(value!)
                : Result<T>.Failure(new ValidationError(execution.PathSegments, "null_value", "Value cannot be null"));
        }

        var errors = await ValidationPipeline.RunAsync(value, execution, Stages());
        return errors is null
            ? Result<T>.Success(value)
            : Result<T>.Failure(errors);
    }

    private async ValueTask<IReadOnlyList<ValidationError>?> ValidateFieldsAsync(T value, ValidationContext execution)
    {
        List<ValidationError>? errors = null;
        foreach (var field in _fields)
        {
            var fieldErrors = await field.ValidateAsync(value, execution);
            if (fieldErrors.Count <= 0) continue;
            errors ??= [];
            errors.AddRange(fieldErrors);
        }

        return errors;
    }

    private async ValueTask<IReadOnlyList<ValidationError>?> ValidateTypeAssertionAsync(T value, ValidationContext execution)
        => _typeAssertion is null ? null : await _typeAssertion.ValidateAsync(value, execution);

    private async ValueTask<IReadOnlyList<ValidationError>?> ValidateConditionalsAsync(T value, ValidationContext execution)
        => await ExecuteConditionalsAsync(value, execution);

    private async ValueTask<IReadOnlyList<ValidationError>?> ValidateRulesAsync(T value, ValidationContext execution)
        => await Rules.ExecuteAsync(value, execution);

    // Nullable reference / nested-object fields. Reference types need no null adapter:
    // the inner schema already handles null. Nullable value-type fields are served by the
    // source generator's typed overloads.

    /// <summary>
    /// Adds a field validator for a reference-type or nested-object property, using a pre-built schema.
    /// </summary>
    public ObjectContextlessSchema<T> Property<TProperty>(
        Expression<Func<T, TProperty?>> propertySelector,
        ISchema<TProperty> schema)
        where TProperty : class
    {
        var propertyName = GetPropertyName(propertySelector);
        var getter = CreateGetter(propertySelector);
        return AddField(new FieldContextlessValidator<T, TProperty>(propertyName, getter!, schema));
    }

    /// <summary>Adds a field validator for a non-nullable enum property, using a fluent builder.</summary>
    public ObjectContextlessSchema<T> Property<TEnum>(
        Expression<Func<T, TEnum>> propertySelector,
        Func<EnumContextlessSchema<TEnum>, EnumContextlessSchema<TEnum>> schema)
        where TEnum : struct, Enum
    {
        var propertyName = GetPropertyName(propertySelector);
        var getter = CreateGetter(propertySelector);
        return AddField(new FieldContextlessValidator<T, TEnum>(propertyName, getter, schema(Z.Enum<TEnum>())));
    }

    /// <summary>Adds a field validator for a nullable enum property, using a fluent builder. Null skips validation.</summary>
    public ObjectContextlessSchema<T> Property<TEnum>(
        Expression<Func<T, TEnum?>> propertySelector,
        Func<EnumContextlessSchema<TEnum>, EnumContextlessSchema<TEnum>> schema)
        where TEnum : struct, Enum
    {
        var propertyName = GetPropertyName(propertySelector);
        var getter = CreateGetter(propertySelector);
        return AddField(new NullableFieldContextlessValidator<T, TEnum>(propertyName, getter, schema(Z.Enum<TEnum>())));
    }

    // A context-aware property schema promotes this contextless object to context-aware.

    /// <summary>
    /// Adds a context-aware property validator, promoting this schema to context-aware (see <see cref="Using{TContext}()"/>).
    /// </summary>
    public ObjectContextSchema<T, TContext> Property<TProperty, TContext>(
        Expression<Func<T, TProperty?>> propertySelector,
        ISchema<TProperty, TContext> schema)
        where TProperty : class
        => Using<TContext>().Property(propertySelector, schema);

    /// <summary>
    /// Promotes this schema to context-aware using a concrete Zeta context schema.
    /// This overload avoids ambiguity when a context-aware schema is also assignable to ISchema&lt;TProperty&gt;.
    /// </summary>
    public ObjectContextSchema<T, TContext> Property<TProperty, TContext>(
        Expression<Func<T, TProperty?>> propertySelector,
        IContextSchema<TProperty, TContext> schema)
        where TProperty : class
        => Using<TContext>().Property(propertySelector, (ISchema<TProperty, TContext>)schema);

    /// <summary>Attaches an object-level refinement error to a specific property path instead of the root ("$").</summary>
    public ObjectContextlessSchema<T> RefineAt<TProperty>(
        Expression<Func<T, TProperty?>> propertySelector,
        Func<T, bool> predicate,
        string message,
        string code = "custom_error")
    {
        return RefineAt(propertySelector, predicate, _ => message, code);
    }

    /// <summary>Attaches an object-level refinement error, with a dynamic message, to a specific property path instead of the root ("$").</summary>
    public ObjectContextlessSchema<T> RefineAt<TProperty>(
        Expression<Func<T, TProperty?>> propertySelector,
        Func<T, bool> predicate,
        Func<T, string> messageFactory,
        string code = "custom_error")
    {
        var propertyName = ToPathSegment(GetPropertyName(propertySelector));

        return Append(new RefinementRule<T>((val, ctx) =>
            predicate(val)
                ? null
                : new ValidationError(ctx.PathSegments.Append(PathSegment.Property(propertyName)), code, messageFactory(val))));
    }

    /// <summary>
    /// Creates a context-aware object schema with all rules, fields, and conditionals from this schema.
    /// </summary>
    /// <typeparam name="TContext">The context type for context-aware validation.</typeparam>
    public ObjectContextSchema<T, TContext> Using<TContext>()
    {
        var schema = new ObjectContextSchema<T, TContext>(Rules, _fields);
        schema = AllowNull ? schema.Nullable() : schema;
        schema = schema.TransferContextlessConditionals(GetConditionals());
        if (_typeAssertion != null)
            schema = schema.WithTypeAssertion(_typeAssertion.ToContext<TContext>());
        return schema;
    }

    /// <summary>
    /// Creates a context-aware object schema with a factory delegate for creating context data.
    /// </summary>
    public ObjectContextSchema<T, TContext> Using<TContext>(
        Func<T, IServiceProvider, CancellationToken, ValueTask<TContext>> factory)
    {
        return Using<TContext>().WithContextFactory(factory);
    }

    /// <summary>
    /// Creates a context-aware object schema with a factory delegate for creating context data.
    /// </summary>
    public ObjectContextSchema<T, TContext> Using<TContext>(
        Func<T, IServiceProvider, TContext> factory)
    {
        return Using<TContext>().WithContextFactory((arg1, provider, _) => new ValueTask<TContext>(factory(arg1, provider)));
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

    internal static string ToPathSegment(string propertyName)
    {
        return propertyName;
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
