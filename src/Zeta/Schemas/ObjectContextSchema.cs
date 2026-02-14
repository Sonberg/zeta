using System.Linq.Expressions;
using Zeta.Adapters;
using Zeta.Core;
using Zeta.Rules;
using Zeta.Validators;

namespace Zeta.Schemas;

/// <summary>
/// A context-aware schema for validating object values.
/// </summary>
public partial class ObjectContextSchema<T, TContext> : ContextSchema<T, TContext, ObjectContextSchema<T, TContext>> where T : class
{
    private readonly List<IFieldContextValidator<T, TContext>> _fields;
    private ITypeAssertion<T, TContext>? _typeAssertion;

    internal ObjectContextSchema() : this(new ContextRuleEngine<T, TContext>(), [])
    {
    }

    internal ObjectContextSchema(ContextRuleEngine<T, TContext> rules, List<IFieldContextValidator<T, TContext>> fields) : base(rules)
    {
        _fields = fields;
    }

    internal ObjectContextSchema(
        ContextlessRuleEngine<T> rules,
        IReadOnlyList<IFieldContextlessValidator<T>> fields) : base(rules.ToContext<TContext>())
    {
        _fields = fields.Select(f => (IFieldContextValidator<T, TContext>)new FieldContextlessValidatorAdapter<T, TContext>(f)).ToList();
    }

    protected override ObjectContextSchema<T, TContext> CreateInstance() => new();

    /// <summary>
    /// Asserts that the value is of the derived type <typeparamref name="TDerived"/>,
    /// enabling type-narrowed field validation for polymorphic types.
    /// </summary>
    public ObjectContextSchema<TDerived, TContext> As<TDerived>() where TDerived : class, T
    {
        var schema = new ObjectContextSchema<TDerived, TContext>();
        _typeAssertion = new ContextAwareTypeAssertion<T, TDerived, TContext>(schema);
        return schema;
    }

    /// <summary>
    /// Adds a conditional type-narrowed branch to the object schema.
    /// Types are automatically inferred from the return value of the configure lambda.
    /// </summary>
    public ObjectContextSchema<T, TContext> If<TTarget>(
        Func<T, bool> predicate,
        ISchema<TTarget, TContext> schema)
        where TTarget : class, T
    {
        return base.If(predicate, (ISchema<T, TContext>)new TypeNarrowingSchemaAdapter<T, TTarget, TContext>(schema));
    }

    /// <summary>
    /// Adds a conditional type-narrowed branch to the object schema.
    /// Types are automatically inferred from the return value of the configure lambda.
    /// </summary>
    public ObjectContextSchema<T, TContext> If<TTarget>(
        Func<T, bool> predicate,
        Func<ObjectContextSchema<T, TContext>, ObjectContextSchema<TTarget, TContext>> configure)
        where TTarget : class, T
    {
        var branchSchema = configure(new ObjectContextSchema<T, TContext>());
        AddConditional(predicate, new TypeNarrowingSchemaAdapter<T, TTarget, TContext>(branchSchema));
        return this;
    }

    public ObjectContextSchema<T, TContext> WhenType<TTarget>(
        Func<ObjectContextSchema<TTarget, TContext>, ObjectContextSchema<TTarget, TContext>> configure)
        where TTarget : class, T
    {
        var branchSchema = configure(new ObjectContextSchema<TTarget, TContext>());
        return If(x => x is TTarget, branchSchema);
    }

    internal void SetTypeAssertion(ITypeAssertion<T, TContext>? assertion) => _typeAssertion = assertion;

    protected override IEnumerable<Func<T, IServiceProvider, CancellationToken, ValueTask<TContext>>> GetContextFactoriesCore()
    {
        foreach (var factory in base.GetContextFactoriesCore())
        {
            yield return factory;
        }

        if (_typeAssertion == null) yield break;
        foreach (var factory in _typeAssertion.GetContextFactories())
        {
            yield return factory;
        }
    }

    public override async ValueTask<Result> ValidateAsync(T? value, ValidationContext<TContext> context)
    {
        if (value is null)
        {
            return AllowNull
                ? Result.Success()
                : Result.Failure([new ValidationError(context.Path, "null_value", "Value cannot be null")]);
        }

        List<ValidationError>? errors = null;

        var ruleErrors = await Rules.ExecuteAsync(value, context);
        if (ruleErrors != null)
        {
            errors ??= [];
            errors.AddRange(ruleErrors);
        }

        foreach (var field in _fields)
        {
            var fieldErrors = await field.ValidateAsync(value, context);
            if (fieldErrors.Count <= 0) continue;
            errors ??= [];
            errors.AddRange(fieldErrors);
        }

        // Validate type assertion
        if (_typeAssertion != null)
        {
            var assertionErrors = await _typeAssertion.ValidateAsync(value, context);
            if (assertionErrors.Count > 0)
            {
                errors ??= [];
                errors.AddRange(assertionErrors);
            }
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

    public ObjectContextSchema<T, TContext> Field<TProperty>(
        Expression<Func<T, TProperty?>> propertySelector,
        ISchema<TProperty, TContext> schema)
    {
        var propertyName = ObjectContextlessSchema<T>.GetPropertyName(propertySelector);
        var getter = ObjectContextlessSchema<T>.CreateGetter(propertySelector);

        // This validates nullable property TProperty? using schema for TProperty.
        var wrapper = NullableAdapterFactory.CreateContextWrapper(schema);

        _fields.Add(new FieldContextContextValidator<T, TProperty?, TContext>(propertyName, getter, wrapper));
        return this;
    }

    // Field overloads for primitive types with fluent builders are generated by SchemaFactoryGenerator
}
