using System.Linq.Expressions;
using Zeta.Conditional;
using Zeta.Core;
using Zeta.Rules;
using Zeta.Validators;

namespace Zeta.Schemas;

/// <summary>
/// A context-aware schema for validating object values.
/// </summary>
public class ObjectContextSchema<T, TContext> : ContextSchema<T, TContext> where T : class
{
    private readonly List<IFieldContextValidator<T, TContext>> _fields;
    private readonly List<IConditionalBranch<T, TContext>> _conditionals;

    public ObjectContextSchema() : this(new ContextRuleEngine<T, TContext>(), [], [])
    {
    }

    internal ObjectContextSchema(ContextRuleEngine<T, TContext> rules, List<IFieldContextValidator<T, TContext>> fields, List<IConditionalBranch<T, TContext>> conditionals) : base(rules)
    {
        _fields = fields;
        _conditionals = conditionals;
    }

    internal ObjectContextSchema(
        ContextlessRuleEngine<T> rules,
        IReadOnlyList<IFieldContextlessValidator<T>> fields,
        IReadOnlyList<IContextlessConditionalBranch<T>> conditionals) : base(rules.ToContext<TContext>())
    {
        _fields = fields.Select(f => (IFieldContextValidator<T, TContext>)new FieldContextlessValidatorAdapter<T, TContext>(f)).ToList();
        _conditionals = conditionals.Select(c => (IConditionalBranch<T, TContext>)new ContextlessConditionalBranchAdapter<T, TContext>(c)).ToList();
    }

    public override async ValueTask<Result> ValidateAsync(T value, ValidationContext<TContext> context)
    {
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

        foreach (var conditional in _conditionals)
        {
            var conditionalErrors = await conditional.ValidateAsync(value, context);
            if (conditionalErrors.Count <= 0) continue;
            errors ??= [];
            errors.AddRange(conditionalErrors);
        }

        return errors == null
            ? Result.Success()
            : Result.Failure(errors);
    }

    public ObjectContextSchema<T, TContext> Field<TProperty>(
        Expression<Func<T, TProperty>> propertySelector,
        ISchema<TProperty, TContext> schema)
    {
        var propertyName = ObjectContextlessSchema<T>.GetPropertyName(propertySelector);
        var getter = ObjectContextlessSchema<T>.CreateGetter(propertySelector);
        _fields.Add(new FieldContextContextValidator<T, TProperty, TContext>(propertyName, getter, schema));
        return this;
    }

    public ObjectContextSchema<T, TContext> Field<TProperty>(
        Expression<Func<T, TProperty>> propertySelector,
        ISchema<TProperty> schema)
    {
        return Field(propertySelector, new SchemaAdapter<TProperty, TContext>(schema));
    }

    public ObjectContextSchema<T, TContext> Field(
        Expression<Func<T, string>> propertySelector,
        Func<StringContextSchema<TContext>, StringContextSchema<TContext>> schema)
    {
        var propertyName = ObjectContextlessSchema<T>.GetPropertyName(propertySelector);
        var getter = ObjectContextlessSchema<T>.CreateGetter(propertySelector);
        _fields.Add(new FieldContextContextValidator<T, string, TContext>(propertyName, getter, schema(new StringContextSchema<TContext>())));
        return this;
    }

    public ObjectContextSchema<T, TContext> Field(
        Expression<Func<T, int>> propertySelector,
        Func<IntContextSchema<TContext>, IntContextSchema<TContext>> schema)
    {
        var propertyName = ObjectContextlessSchema<T>.GetPropertyName(propertySelector);
        var getter = ObjectContextlessSchema<T>.CreateGetter(propertySelector);
        _fields.Add(new FieldContextContextValidator<T, int, TContext>(propertyName, getter, schema(new IntContextSchema<TContext>())));
        return this;
    }

    public ObjectContextSchema<T, TContext> Field(
        Expression<Func<T, double>> propertySelector,
        Func<DoubleContextSchema<TContext>, DoubleContextSchema<TContext>> schema)
    {
        var propertyName = ObjectContextlessSchema<T>.GetPropertyName(propertySelector);
        var getter = ObjectContextlessSchema<T>.CreateGetter(propertySelector);
        _fields.Add(new FieldContextContextValidator<T, double, TContext>(propertyName, getter, schema(new DoubleContextSchema<TContext>())));
        return this;
    }

    public ObjectContextSchema<T, TContext> Field(
        Expression<Func<T, decimal>> propertySelector,
        Func<DecimalContextSchema<TContext>, DecimalContextSchema<TContext>> schema)
    {
        var propertyName = ObjectContextlessSchema<T>.GetPropertyName(propertySelector);
        var getter = ObjectContextlessSchema<T>.CreateGetter(propertySelector);
        _fields.Add(new FieldContextContextValidator<T, decimal, TContext>(propertyName, getter, schema(new DecimalContextSchema<TContext>())));
        return this;
    }

    public ObjectContextSchema<T, TContext> Field(
        Expression<Func<T, bool>> propertySelector,
        Func<BoolContextSchema<TContext>, BoolContextSchema<TContext>> schema)
    {
        var propertyName = ObjectContextlessSchema<T>.GetPropertyName(propertySelector);
        var getter = ObjectContextlessSchema<T>.CreateGetter(propertySelector);
        _fields.Add(new FieldContextContextValidator<T, bool, TContext>(propertyName, getter, schema(new BoolContextSchema<TContext>())));
        return this;
    }

    public ObjectContextSchema<T, TContext> Field(
        Expression<Func<T, Guid>> propertySelector,
        Func<GuidContextSchema<TContext>, GuidContextSchema<TContext>> schema)
    {
        var propertyName = ObjectContextlessSchema<T>.GetPropertyName(propertySelector);
        var getter = ObjectContextlessSchema<T>.CreateGetter(propertySelector);
        _fields.Add(new FieldContextContextValidator<T, Guid, TContext>(propertyName, getter, schema(new GuidContextSchema<TContext>())));
        return this;
    }

    public ObjectContextSchema<T, TContext> Field(
        Expression<Func<T, DateTime>> propertySelector,
        Func<DateTimeContextSchema<TContext>, DateTimeContextSchema<TContext>> schema)
    {
        var propertyName = ObjectContextlessSchema<T>.GetPropertyName(propertySelector);
        var getter = ObjectContextlessSchema<T>.CreateGetter(propertySelector);
        _fields.Add(new FieldContextContextValidator<T, DateTime, TContext>(propertyName, getter, schema(new DateTimeContextSchema<TContext>())));
        return this;
    }

#if !NETSTANDARD2_0
    public ObjectContextSchema<T, TContext> Field(
        Expression<Func<T, DateOnly>> propertySelector,
        Func<DateOnlyContextSchema<TContext>, DateOnlyContextSchema<TContext>> schema)
    {
        var propertyName = ObjectContextlessSchema<T>.GetPropertyName(propertySelector);
        var getter = ObjectContextlessSchema<T>.CreateGetter(propertySelector);
        _fields.Add(new FieldContextContextValidator<T, DateOnly, TContext>(propertyName, getter, schema(new DateOnlyContextSchema<TContext>())));
        return this;
    }

    public ObjectContextSchema<T, TContext> Field(
        Expression<Func<T, TimeOnly>> propertySelector,
        Func<TimeOnlyContextSchema<TContext>, TimeOnlyContextSchema<TContext>> schema)
    {
        var propertyName = ObjectContextlessSchema<T>.GetPropertyName(propertySelector);
        var getter = ObjectContextlessSchema<T>.CreateGetter(propertySelector);
        _fields.Add(new FieldContextContextValidator<T, TimeOnly, TContext>(propertyName, getter, schema(new TimeOnlyContextSchema<TContext>())));
        return this;
    }
#endif

    public ObjectContextSchema<T, TContext> When(
        Func<T, bool> condition,
        Action<ConditionalBuilder<T, TContext>> thenBranch,
        Action<ConditionalBuilder<T, TContext>>? elseBranch = null)
    {
        var thenBuilder = new ConditionalBuilder<T, TContext>();
        thenBranch(thenBuilder);

        ConditionalBuilder<T, TContext>? elseBuilder = null;
        if (elseBranch != null)
        {
            elseBuilder = new ConditionalBuilder<T, TContext>();
            elseBranch(elseBuilder);
        }

        _conditionals.Add(new ConditionalBranch<T, TContext>(condition, thenBuilder, elseBuilder));
        return this;
    }

    /// <summary>
    /// Conditionally validates fields based on a context-aware predicate.
    /// </summary>
    public ObjectContextSchema<T, TContext> When(
        Func<T, TContext, bool> condition,
        Action<ConditionalBuilder<T, TContext>> thenBranch,
        Action<ConditionalBuilder<T, TContext>>? elseBranch = null)
    {
        var thenBuilder = new ConditionalBuilder<T, TContext>();
        thenBranch(thenBuilder);

        ConditionalBuilder<T, TContext>? elseBuilder = null;
        if (elseBranch != null)
        {
            elseBuilder = new ConditionalBuilder<T, TContext>();
            elseBranch(elseBuilder);
        }

        _conditionals.Add(new ContextAwareConditionalBranch<T, TContext>(condition, thenBuilder, elseBuilder));
        return this;
    }

    public ObjectContextSchema<T, TContext> Refine(Func<T, TContext, bool> predicate, string message, string code = "custom_error")
    {
        Use(new RefinementRule<T, TContext>((val, ctx) =>
            predicate(val, ctx.Data)
                ? null
                : new ValidationError(ctx.Path, code, message)));
        return this;
    }

    public ObjectContextSchema<T, TContext> Refine(Func<T, bool> predicate, string message, string code = "custom_error")
    {
        return Refine((val, _) => predicate(val), message, code);
    }

}
