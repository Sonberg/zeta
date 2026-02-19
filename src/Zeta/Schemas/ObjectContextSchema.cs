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
    private readonly IReadOnlyList<IFieldContextValidator<T, TContext>> _fields;
    private readonly ITypeAssertion<T, TContext>? _typeAssertion;

    internal ObjectContextSchema() : this(
        new ContextRuleEngine<T, TContext>(), [], null, false, null, null)
    {
    }

    internal ObjectContextSchema(ContextRuleEngine<T, TContext> rules, IReadOnlyList<IFieldContextValidator<T, TContext>> fields)
        : this(rules, fields, null, false, null, null)
    {
    }

    internal ObjectContextSchema(
        ContextlessRuleEngine<T> rules,
        IReadOnlyList<IFieldContextlessValidator<T>> fields)
        : this(
            rules.ToContext<TContext>(),
            fields.Select(f => (IFieldContextValidator<T, TContext>)new FieldContextlessValidatorAdapter<T, TContext>(f)).ToList(),
            null, false, null, null)
    {
    }

    private ObjectContextSchema(
        ContextRuleEngine<T, TContext> rules,
        IReadOnlyList<IFieldContextValidator<T, TContext>> fields,
        ITypeAssertion<T, TContext>? typeAssertion,
        bool allowNull,
        IReadOnlyList<ISchemaConditional<T, TContext>>? conditionals,
        Func<T, IServiceProvider, CancellationToken, ValueTask<TContext>>? contextFactory)
        : base(rules, allowNull, conditionals, contextFactory)
    {
        _fields = fields;
        _typeAssertion = typeAssertion;
    }

    protected override ObjectContextSchema<T, TContext> CreateInstance() => new();

    private protected override ObjectContextSchema<T, TContext> CreateInstance(
        ContextRuleEngine<T, TContext> rules,
        bool allowNull,
        IReadOnlyList<ISchemaConditional<T, TContext>>? conditionals,
        Func<T, IServiceProvider, CancellationToken, ValueTask<TContext>>? contextFactory)
        => new(rules, _fields, _typeAssertion, allowNull, conditionals, contextFactory);

    internal ObjectContextSchema<T, TContext> AddField(IFieldContextValidator<T, TContext> field)
    {
        var newFields = new List<IFieldContextValidator<T, TContext>>(_fields) { field };
        return new ObjectContextSchema<T, TContext>(Rules, newFields, _typeAssertion, AllowNull, GetConditionals(), ContextFactory);
    }

    internal ObjectContextSchema<T, TContext> AddContextlessField(IFieldContextlessValidator<T> field)
    {
        var newFields = new List<IFieldContextValidator<T, TContext>>(_fields)
        {
            new FieldContextlessValidatorAdapter<T, TContext>(field)
        };
        return new ObjectContextSchema<T, TContext>(Rules, newFields, _typeAssertion, AllowNull, GetConditionals(), ContextFactory);
    }

    /// <summary>
    /// Asserts that the value is of the derived type <typeparamref name="TDerived"/>,
    /// enabling type-narrowed field validation for polymorphic types.
    /// </summary>
    public ObjectContextSchema<TDerived, TContext> As<TDerived>() where TDerived : class, T
    {
        return new ObjectContextSchema<TDerived, TContext>();
    }

    internal ObjectContextSchema<T, TContext> WithTypeAssertion(ITypeAssertion<T, TContext> assertion)
        => new(Rules, _fields, assertion, AllowNull, GetConditionals(), ContextFactory);

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
        return AddConditional(predicate, new TypeNarrowingSchemaAdapter<T, TTarget, TContext>(branchSchema));
    }

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

    public override async ValueTask<Result<T, TContext>> ValidateAsync(T? value, ValidationContext<TContext> context)
    {
        if (value is null)
        {
            return AllowNull
                ? Result<T, TContext>.Success(value!, context.Data)
                : Result<T, TContext>.Failure([new ValidationError(context.Path, "null_value", "Value cannot be null")]);
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
            ? Result<T, TContext>.Success(value!, context.Data)
            : Result<T, TContext>.Failure(errors);
    }

    public ObjectContextSchema<T, TContext> Field<TProperty>(
        Expression<Func<T, TProperty?>> propertySelector,
        ISchema<TProperty, TContext> schema)
    {
        var propertyName = ObjectContextlessSchema<T>.GetPropertyName(propertySelector);
        var getter = ObjectContextlessSchema<T>.CreateGetter(propertySelector);

        // This validates nullable property TProperty? using schema for TProperty.
        var wrapper = NullableAdapterFactory.CreateContextWrapper(schema);

        return AddField(new FieldContextContextValidator<T, TProperty?, TContext>(propertyName, getter, wrapper));
    }

    public ObjectContextSchema<T, TContext> Property<TProperty>(
        Expression<Func<T, TProperty?>> propertySelector,
        ISchema<TProperty, TContext> schema)
    {
        return Field(propertySelector, schema);
    }

    /// <summary>
    /// Adds a nullable field with a concrete context-aware schema type from Zeta.
    /// This overload avoids ambiguity when the schema is also assignable to ISchema&lt;TProperty&gt;.
    /// </summary>
    public ObjectContextSchema<T, TContext> Field<TProperty>(
        Expression<Func<T, TProperty?>> propertySelector,
        IContextSchema<TProperty, TContext> schema)
    {
        return Field(propertySelector, (ISchema<TProperty, TContext>)schema);
    }

    public ObjectContextSchema<T, TContext> Property<TProperty>(
        Expression<Func<T, TProperty?>> propertySelector,
        IContextSchema<TProperty, TContext> schema)
    {
        return Field(propertySelector, schema);
    }

    public ObjectContextSchema<T, TContext> Field<TProperty>(
        Expression<Func<T, TProperty?>> propertySelector,
        ISchema<TProperty> schema)
    {
        var propertyName = ObjectContextlessSchema<T>.GetPropertyName(propertySelector);
        var getter = ObjectContextlessSchema<T>.CreateGetter(propertySelector);
        var wrapper = NullableAdapterFactory.CreateContextlessWrapper(schema);
        return AddContextlessField(new FieldContextlessValidator<T, TProperty?>(propertyName, getter, wrapper));
    }

    /// <summary>
    /// Adds a field with a schema resolved at runtime via a factory function.
    /// </summary>
    public ObjectContextSchema<T, TContext> Field<TProperty>(
        Expression<Func<T, TProperty?>> propertySelector,
        Func<T, IServiceProvider, ISchema<TProperty>> schemaFactory)
    {
        var propertyName = ObjectContextlessSchema<T>.GetPropertyName(propertySelector);
        var getter = ObjectContextlessSchema<T>.CreateGetter(propertySelector);
        return AddField(new DelegatedFieldContextlessValidatorAdapter<T, TProperty?, TContext>(
            new DelegatedFieldContextlessValidator<T, TProperty?>(propertyName, getter, (inst, sp) => (ISchema<TProperty?>)schemaFactory(inst, sp))));
    }

    /// <summary>
    /// Creates a context-aware object schema with a factory delegate for creating context data.
    /// </summary>
    public ObjectContextSchema<T, TContext> Using(
        Func<T, IServiceProvider, CancellationToken, ValueTask<TContext>> factory)
    {
        return WithContextFactory(factory);
    }

    /// <summary>
    /// Creates a new context-aware object schema by chaining a new context factory onto the existing one.
    /// The new factory has access to the current input, the previous context data, and the service provider.
    /// </summary>
    public ObjectContextSchema<T, TNewContext> Using<TNewContext>(
        Func<T, TContext, IServiceProvider, CancellationToken, ValueTask<TNewContext>> factory)
    {
        var oldFactories = GetContextFactoriesCore().ToList();
        Func<T, IServiceProvider, CancellationToken, ValueTask<TContext>> oldContextResolver = (val, sp, ct) 
            => ContextFactoryResolver.ResolveAsync(val, oldFactories, sp, ct);

        // Adapt existing rules
        var newRules = Rules.Adapt<TNewContext>(oldContextResolver);

        // Adapt existing fields
        var newFields = _fields.Select(f => (IFieldContextValidator<T, TNewContext>)new FieldContextValidatorAdapter<T, TContext, TNewContext>(f, oldContextResolver)).ToList();

        // Adapt existing conditionals
        var newConditionals = GetConditionals()?.Select(c => c.Adapt<TNewContext>(oldContextResolver)).ToList();

        var schema = new ObjectContextSchema<T, TNewContext>(newRules, newFields, null, AllowNull, newConditionals, async (val, sp, ct) => {
            var prevContext = await oldContextResolver(val, sp, ct);
            return await factory(val, prevContext, sp, ct);
        });

        return schema;
    }

    /// <summary>
    /// Adds a field with a context-aware schema resolved at runtime via a factory function.
    /// </summary>
    public ObjectContextSchema<T, TContext> Field<TProperty>(
        Expression<Func<T, TProperty?>> propertySelector,
        Func<T, TContext, IServiceProvider, ISchema<TProperty, TContext>> schemaFactory)
    {
        var propertyName = ObjectContextlessSchema<T>.GetPropertyName(propertySelector);
        var getter = ObjectContextlessSchema<T>.CreateGetter(propertySelector);
        return AddField(new DelegatedFieldContextValidator<T, TProperty, TContext>(propertyName, getter, schemaFactory));
    }

    public ObjectContextSchema<T, TContext> Property<TProperty>(
        Expression<Func<T, TProperty?>> propertySelector,
        ISchema<TProperty> schema)
    {
        return Field(propertySelector, schema);
    }

    public ObjectContextSchema<T, TContext> Field<TEnum>(
        Expression<Func<T, TEnum>> propertySelector,
        Func<EnumContextlessSchema<TEnum>, EnumContextSchema<TEnum, TContext>> schema)
        where TEnum : struct, Enum
    {
        var propertyName = ObjectContextlessSchema<T>.GetPropertyName(propertySelector);
        var getter = ObjectContextlessSchema<T>.CreateGetter(propertySelector);
        return AddField(new FieldContextContextValidator<T, TEnum, TContext>(propertyName, getter, schema(Z.Enum<TEnum>())));
    }

    public ObjectContextSchema<T, TContext> Property<TEnum>(
        Expression<Func<T, TEnum>> propertySelector,
        Func<EnumContextlessSchema<TEnum>, EnumContextSchema<TEnum, TContext>> schema)
        where TEnum : struct, Enum
    {
        return Field(propertySelector, schema);
    }

    public ObjectContextSchema<T, TContext> Field<TEnum>(
        Expression<Func<T, TEnum>> propertySelector,
        Func<EnumContextlessSchema<TEnum>, EnumContextlessSchema<TEnum>> schema)
        where TEnum : struct, Enum
    {
        var propertyName = ObjectContextlessSchema<T>.GetPropertyName(propertySelector);
        var getter = ObjectContextlessSchema<T>.CreateGetter(propertySelector);
        var configuredSchema = schema(Z.Enum<TEnum>());
        return AddField(new FieldContextContextValidator<T, TEnum, TContext>(propertyName, getter, configuredSchema.Using<TContext>()));
    }

    public ObjectContextSchema<T, TContext> Property<TEnum>(
        Expression<Func<T, TEnum>> propertySelector,
        Func<EnumContextlessSchema<TEnum>, EnumContextlessSchema<TEnum>> schema)
        where TEnum : struct, Enum
    {
        return Field(propertySelector, schema);
    }

    public ObjectContextSchema<T, TContext> Field<TEnum>(
        Expression<Func<T, TEnum?>> propertySelector,
        Func<EnumContextlessSchema<TEnum>, EnumContextSchema<TEnum, TContext>> schema)
        where TEnum : struct, Enum
    {
        var propertyName = ObjectContextlessSchema<T>.GetPropertyName(propertySelector);
        var getter = ObjectContextlessSchema<T>.CreateGetter(propertySelector);
        return AddField(new NullableFieldContextContextValidator<T, TEnum, TContext>(propertyName, getter, schema(Z.Enum<TEnum>())));
    }

    public ObjectContextSchema<T, TContext> Property<TEnum>(
        Expression<Func<T, TEnum?>> propertySelector,
        Func<EnumContextlessSchema<TEnum>, EnumContextSchema<TEnum, TContext>> schema)
        where TEnum : struct, Enum
    {
        return Field(propertySelector, schema);
    }

    public ObjectContextSchema<T, TContext> Field<TEnum>(
        Expression<Func<T, TEnum?>> propertySelector,
        Func<EnumContextlessSchema<TEnum>, EnumContextlessSchema<TEnum>> schema)
        where TEnum : struct, Enum
    {
        var propertyName = ObjectContextlessSchema<T>.GetPropertyName(propertySelector);
        var getter = ObjectContextlessSchema<T>.CreateGetter(propertySelector);
        var configuredSchema = schema(Z.Enum<TEnum>());
        return AddField(new NullableFieldContextContextValidator<T, TEnum, TContext>(propertyName, getter, configuredSchema.Using<TContext>()));
    }

    public ObjectContextSchema<T, TContext> Property<TEnum>(
        Expression<Func<T, TEnum?>> propertySelector,
        Func<EnumContextlessSchema<TEnum>, EnumContextlessSchema<TEnum>> schema)
        where TEnum : struct, Enum
    {
        return Field(propertySelector, schema);
    }

    public ObjectContextSchema<T, TContext> RefineAt<TProperty>(
        Expression<Func<T, TProperty?>> propertySelector,
        Func<T, TContext, bool> predicate,
        string message)
    {
        return RefineAt(propertySelector, predicate, (_, _) => message, "custom_error");
    }

    public ObjectContextSchema<T, TContext> RefineAt<TProperty>(
        Expression<Func<T, TProperty?>> propertySelector,
        Func<T, TContext, bool> predicate,
        string code,
        string message)
    {
        return RefineAt(propertySelector, predicate, (_, _) => message, code);
    }

    public ObjectContextSchema<T, TContext> RefineAt<TProperty>(
        Expression<Func<T, TProperty?>> propertySelector,
        Func<T, TContext, bool> predicate,
        Func<T, TContext, string> messageFactory,
        string code = "custom_error")
    {
        var propertyName = ObjectContextlessSchema<T>.ToPathSegment(ObjectContextlessSchema<T>.GetPropertyName(propertySelector));

        return Append(new RefinementRule<T, TContext>((val, ctx) =>
            predicate(val, ctx.Data)
                ? null
                : new ValidationError(ctx.Push(propertyName).Path, code, messageFactory(val, ctx.Data))));
    }

    public ObjectContextSchema<T, TContext> RefineAt<TProperty>(
        Expression<Func<T, TProperty?>> propertySelector,
        Func<T, bool> predicate,
        string message)
    {
        return RefineAt(propertySelector, (val, _) => predicate(val), (_, _) => message, "custom_error");
    }

    public ObjectContextSchema<T, TContext> RefineAt<TProperty>(
        Expression<Func<T, TProperty?>> propertySelector,
        Func<T, bool> predicate,
        string code,
        string message)
    {
        return RefineAt(propertySelector, (val, _) => predicate(val), (val, _) => message, code);
    }

    public ObjectContextSchema<T, TContext> RefineAt<TProperty>(
        Expression<Func<T, TProperty?>> propertySelector,
        Func<T, bool> predicate,
        Func<T, string> messageFactory,
        string code = "custom_error")
    {
        return RefineAt(propertySelector, (val, _) => predicate(val), (val, _) => messageFactory(val), code);
    }

    // Field overloads for primitive types with fluent builders are generated by SchemaFactoryGenerator
}
