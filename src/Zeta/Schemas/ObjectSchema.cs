using System.Linq.Expressions;
using System.Reflection;
using Zeta.Conditional;
using Zeta.Core;
using Zeta.Rules;
using Zeta.Validators;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating object values.
/// </summary>
public sealed class ObjectSchema<T> : ContextlessSchema<T> where T : class
{
    private readonly List<IFieldContextlessValidator<T>> _fields;
    private readonly List<IContextlessConditionalBranch<T>> _conditionals;

    public ObjectSchema() : this(new ContextlessRuleEngine<T>(), [], [])
    {
    }

    internal ObjectSchema(
        ContextlessRuleEngine<T> rules,
        List<IFieldContextlessValidator<T>> fields,
        List<IContextlessConditionalBranch<T>> conditionals) : base(rules)
    {
        _fields = fields;
        _conditionals = conditionals;
    }

    public override async ValueTask<Result<T>> ValidateAsync(T value, ValidationExecutionContext? execution = null)
    {
        execution ??= ValidationExecutionContext.Empty;
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

        // Validate conditionals
        foreach (var conditional in _conditionals)
        {
            var conditionalErrors = await conditional.ValidateAsync(value, execution);
            if (conditionalErrors.Count <= 0) continue;

            errors ??= [];
            errors.AddRange(conditionalErrors);
        }

        return errors == null
            ? Result<T>.Success(value)
            : Result<T>.Failure(errors);
    }

    public ObjectSchema<T> Field<TProperty>(
        Expression<Func<T, TProperty>> propertySelector,
        ISchema<TProperty> schema)
    {
        var propertyName = GetPropertyName(propertySelector);
        var getter = CreateGetter(propertySelector);
        _fields.Add(new FieldContextlessValidator<T, TProperty>(propertyName, getter, schema));
        return this;
    }

    public ObjectSchema<T> When(
        Func<T, bool> condition,
        Action<ContextlessConditionalBuilder<T>> thenBranch,
        Action<ContextlessConditionalBuilder<T>>? elseBranch = null)
    {
        var thenBuilder = new ContextlessConditionalBuilder<T>();
        thenBranch(thenBuilder);

        ContextlessConditionalBuilder<T>? elseBuilder = null;
        if (elseBranch != null)
        {
            elseBuilder = new ContextlessConditionalBuilder<T>();
            elseBranch(elseBuilder);
        }

        _conditionals.Add(new ContextlessConditionalBranch<T>(condition, thenBuilder, elseBuilder));
        return this;
    }

    public ObjectSchema<T> Refine(Func<T, bool> predicate, string message, string code = "custom_error")
    {
        Use(new RefinementRule<T>((val, exec) =>
            predicate(val)
                ? null
                : new ValidationError(exec.Path, code, message)));
        return this;
    }

    /// <summary>
    /// Creates a context-aware object schema with all rules, fields, and conditionals from this schema.
    /// </summary>
    /// <typeparam name="TContext">The context type for context-aware validation.</typeparam>
    public ObjectSchema<T, TContext> WithContext<TContext>()
        => new ObjectSchema<T, TContext>(Rules, _fields, _conditionals);

    public static string GetPropertyName<TProperty>(Expression<Func<T, TProperty>> expr)
    {
        var body = expr.Body;
        if (body is UnaryExpression { NodeType: ExpressionType.Convert } u)
            body = u.Operand;
        if (body is MemberExpression m)
            return m.Member.Name;
        throw new ArgumentException("Expression must be a property access");
    }

    public static Func<T, TProperty> CreateGetter<TProperty>(Expression<Func<T, TProperty>> expr)
    {
        var member = (MemberExpression)expr.Body;
        var prop = (PropertyInfo)member.Member;
        return instance => (TProperty)prop.GetValue(instance)!;
    }
}

/// <summary>
/// A context-aware schema for validating object values.
/// </summary>
public class ObjectSchema<T, TContext> : ContextSchema<T, TContext> where T : class
{
    private readonly List<IFieldContextValidator<T, TContext>> _fields;
    private readonly List<IConditionalBranch<T, TContext>> _conditionals;

    public ObjectSchema() : this(new ContextRuleEngine<T, TContext>(), [], [])
    {
    }

    internal ObjectSchema(ContextRuleEngine<T, TContext> rules, List<IFieldContextValidator<T, TContext>> fields, List<IConditionalBranch<T, TContext>> conditionals) : base(rules)
    {
        _fields = fields;
        _conditionals = conditionals;
    }

    internal ObjectSchema(
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

    public ObjectSchema<T, TContext> Field<TProperty>(
        Expression<Func<T, TProperty>> propertySelector,
        ISchema<TProperty, TContext> schema)
    {
        var propertyName = ObjectSchema<T>.GetPropertyName(propertySelector);
        var getter = ObjectSchema<T>.CreateGetter(propertySelector);
        _fields.Add(new FieldContextContextValidator<T, TProperty, TContext>(propertyName, getter, schema));
        return this;
    }

    public ObjectSchema<T, TContext> Field<TProperty>(
        Expression<Func<T, TProperty>> propertySelector,
        ISchema<TProperty> schema)
    {
        return Field(propertySelector, new SchemaAdapter<TProperty, TContext>(schema));
    }

    public ObjectSchema<T, TContext> When(
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
    public ObjectSchema<T, TContext> When(
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

    public ObjectSchema<T, TContext> Refine(Func<T, TContext, bool> predicate, string message, string code = "custom_error")
    {
        Use(new RefinementRule<T, TContext>((val, ctx) =>
            predicate(val, ctx.Data)
                ? null
                : new ValidationError(ctx.Execution.Path, code, message)));
        return this;
    }

    public ObjectSchema<T, TContext> Refine(Func<T, bool> predicate, string message, string code = "custom_error")
    {
        return Refine((val, _) => predicate(val), message, code);
    }

}