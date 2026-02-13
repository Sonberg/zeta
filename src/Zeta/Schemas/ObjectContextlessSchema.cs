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
    /// Conditionally validates the value as the derived type <typeparamref name="TDerived"/>
    /// when the value is an instance of that type. Combines type checking with type-narrowed validation.
    /// </summary>
    public ObjectContextlessSchema<T> If<TDerived>(
        Action<ObjectContextlessSchema<TDerived>> configure) where TDerived : class, T
    {
        return If(
            value => value is TDerived,
            conditional =>
            {
                var derived = conditional.As<TDerived>();
                configure(derived);
            });
    }

    /// <summary>
    /// Conditionally validates the value as the derived type <typeparamref name="TDerived"/> and promotes
    /// the full schema to context-aware when the configured derived schema uses context.
    /// </summary>
    public ObjectContextSchema<T, TContext> If<TDerived, TContext>(
        Func<ObjectContextlessSchema<TDerived>, ObjectContextSchema<TDerived, TContext>> configure) where TDerived : class, T
    {
        var conditional = configure(Z.Object<TDerived>());
        var promoted = WithContext<TContext>();
        promoted.AddConditional(
            value => value is TDerived,
            new TypeNarrowingSchemaAdapter<T, TDerived, TContext>(conditional));
        return promoted;
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

    // public ObjectContextlessSchema<T> Field(
    //     Expression<Func<T, ICollection<int?>?>> propertySelector,
    //     ISchema<ICollection<int?>?> schema)
    // {
    //     var propertyName = GetPropertyName(propertySelector);
    //     var getter = CreateGetter(propertySelector);
    //     _fields.Add(new FieldContextlessValidator<T, ICollection<int>>(propertyName, getter, schema));
    //     return this;
    // }

    // Field overloads for primitive types are generated by SchemaFactoryGenerator

    /// <summary>
    /// Creates a context-aware object schema with all rules, fields, and conditionals from this schema.
    /// </summary>
    /// <typeparam name="TContext">The context type for context-aware validation.</typeparam>
    public ObjectContextSchema<T, TContext> WithContext<TContext>()
    {
        var schema = new ObjectContextSchema<T, TContext>(Rules, _fields);
        if (AllowNull) schema.Nullable();
        schema.TransferContextlessConditionals(GetConditionals());
        if (_typeAssertion != null)
            schema.SetTypeAssertion(_typeAssertion.ToContext<TContext>());
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
