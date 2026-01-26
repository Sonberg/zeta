using System.Linq.Expressions;
using System.Reflection;
using Zeta.Core;
using Zeta.Rules;

namespace Zeta.Schemas;

/// <summary>
/// A contextless schema for validating object values.
/// </summary>
public sealed class ObjectSchema<T> : ISchema<T>
{
    private readonly RuleEngine<T> _rules = new();
    private readonly List<IContextlessFieldValidator<T>> _fields = [];
    private readonly List<IContextlessConditionalBranch<T>> _conditionals = [];

    public async ValueTask<Result<T>> ValidateAsync(T value, ValidationExecutionContext? execution = null)
    {
        execution ??= ValidationExecutionContext.Empty;
        List<ValidationError>? errors = null;

        // Validate rules
        var ruleErrors = await _rules.ExecuteAsync(value, execution);
        if (ruleErrors != null)
        {
            errors = ruleErrors;
        }

        // Validate fields
        foreach (var field in _fields)
        {
            var fieldErrors = await field.ValidateAsync(value, execution);
            if (fieldErrors.Count > 0)
            {
                errors ??= [];
                errors.AddRange(fieldErrors);
            }
        }

        // Validate conditionals
        foreach (var conditional in _conditionals)
        {
            var conditionalErrors = await conditional.ValidateAsync(value, execution);
            if (conditionalErrors.Count > 0)
            {
                errors ??= [];
                errors.AddRange(conditionalErrors);
            }
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
        _fields.Add(new ContextlessFieldValidator<T, TProperty>(propertyName, getter, schema));
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
        _rules.Add(new DelegateValidationRule<T>((val, exec) =>
            predicate(val)
                ? null
                : new ValidationError(exec.Path, code, message)));
        return this;
    }

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
public class ObjectSchema<T, TContext> : BaseSchema<T, TContext>
{
    private readonly List<IFieldValidator<T, TContext>> _fields = [];
    private readonly List<IConditionalBranch<T, TContext>> _conditionals = [];

    public override async ValueTask<Result> ValidateAsync(T value, ValidationContext<TContext> context)
    {
        var result = await base.ValidateAsync(value, context);
        var errors = result.Errors.Count > 0 ? result.Errors.ToList() : null;

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
        _fields.Add(new FieldValidator<T, TProperty, TContext>(propertyName, getter, schema));
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

    public ObjectSchema<T, TContext> Refine(Func<T, TContext, bool> predicate, string message, string code = "custom_error")
    {
        Use(new DelegateSyncRule<T, TContext>((val, ctx) =>
            predicate(val, ctx.Data)
                ? null
                : new ValidationError(ctx.Execution.Path, code, message)));
        return this;
    }
}

// ==================== Contextless Field Validators ====================

internal interface IContextlessFieldValidator<T>
{
    ValueTask<IReadOnlyList<ValidationError>> ValidateAsync(T instance, ValidationExecutionContext execution);
}

internal sealed class ContextlessFieldValidator<TInstance, TProperty> : IContextlessFieldValidator<TInstance>
{
    private readonly string _name;
    private readonly Func<TInstance, TProperty> _getter;
    private readonly ISchema<TProperty> _schema;

    public ContextlessFieldValidator(string name, Func<TInstance, TProperty> getter, ISchema<TProperty> schema)
    {
        _name = name;
        if (!string.IsNullOrEmpty(_name) && char.IsUpper(_name[0]))
            _name = char.ToLower(_name[0]) + _name.Substring(1);
        _getter = getter;
        _schema = schema;
    }

    public async ValueTask<IReadOnlyList<ValidationError>> ValidateAsync(TInstance instance, ValidationExecutionContext execution)
    {
        var value = _getter(instance);
        var fieldExecution = execution.Push(_name);
        var result = await _schema.ValidateAsync(value, fieldExecution);
        return result.Errors.ToList();
    }
}

internal sealed class ContextlessRequiredFieldValidator<TInstance, TProperty> : IContextlessFieldValidator<TInstance>
{
    private readonly string _name;
    private readonly Func<TInstance, TProperty> _getter;
    private readonly string _message;
    private static readonly IReadOnlyList<ValidationError> EmptyErrors = Array.Empty<ValidationError>();

    public ContextlessRequiredFieldValidator(string name, Func<TInstance, TProperty> getter, string? message)
    {
        _name = name;
        if (!string.IsNullOrEmpty(_name) && char.IsUpper(_name[0]))
            _name = char.ToLower(_name[0]) + _name.Substring(1);
        _getter = getter;
        _message = message ?? $"{_name} is required";
    }

    public ValueTask<IReadOnlyList<ValidationError>> ValidateAsync(TInstance instance, ValidationExecutionContext execution)
    {
        var value = _getter(instance);
        if (value is null)
        {
            var path = string.IsNullOrEmpty(execution.Path) ? _name : $"{execution.Path}.{_name}";
            return new ValueTask<IReadOnlyList<ValidationError>>([new ValidationError(path, "required", _message)]);
        }
        return new ValueTask<IReadOnlyList<ValidationError>>(EmptyErrors);
    }
}

// ==================== Contextless Conditional Builder ====================

public sealed class ContextlessConditionalBuilder<T>
{
    internal List<IContextlessFieldValidator<T>> Validators { get; } = [];

    public ContextlessConditionalBuilder<T> Require<TProperty>(
        Expression<Func<T, TProperty>> propertySelector,
        string? message = null)
    {
        var propertyName = ObjectSchema<T>.GetPropertyName(propertySelector);
        var getter = ObjectSchema<T>.CreateGetter(propertySelector);
        Validators.Add(new ContextlessRequiredFieldValidator<T, TProperty>(propertyName, getter, message));
        return this;
    }

    public ContextlessConditionalBuilder<T> Field<TProperty>(
        Expression<Func<T, TProperty>> propertySelector,
        ISchema<TProperty> schema)
    {
        var propertyName = ObjectSchema<T>.GetPropertyName(propertySelector);
        var getter = ObjectSchema<T>.CreateGetter(propertySelector);
        Validators.Add(new ContextlessFieldValidator<T, TProperty>(propertyName, getter, schema));
        return this;
    }
}

internal interface IContextlessConditionalBranch<T>
{
    ValueTask<IReadOnlyList<ValidationError>> ValidateAsync(T instance, ValidationExecutionContext execution);
}

internal sealed class ContextlessConditionalBranch<T> : IContextlessConditionalBranch<T>
{
    private readonly Func<T, bool> _condition;
    private readonly ContextlessConditionalBuilder<T> _thenBranch;
    private readonly ContextlessConditionalBuilder<T>? _elseBranch;
    private static readonly IReadOnlyList<ValidationError> EmptyErrors = Array.Empty<ValidationError>();

    public ContextlessConditionalBranch(
        Func<T, bool> condition,
        ContextlessConditionalBuilder<T> thenBranch,
        ContextlessConditionalBuilder<T>? elseBranch)
    {
        _condition = condition;
        _thenBranch = thenBranch;
        _elseBranch = elseBranch;
    }

    public async ValueTask<IReadOnlyList<ValidationError>> ValidateAsync(T instance, ValidationExecutionContext execution)
    {
        var branch = _condition(instance) ? _thenBranch : _elseBranch;
        if (branch == null)
            return EmptyErrors;

        List<ValidationError>? errors = null;
        foreach (var validator in branch.Validators)
        {
            var fieldErrors = await validator.ValidateAsync(instance, execution);
            if (fieldErrors.Count > 0)
            {
                errors ??= [];
                errors.AddRange(fieldErrors);
            }
        }
        return errors ?? EmptyErrors;
    }
}

// ==================== Context-Aware Field Validators ====================

internal interface IFieldValidator<T, TContext>
{
    ValueTask<IReadOnlyList<ValidationError>> ValidateAsync(T instance, ValidationContext<TContext> context);
}

internal sealed class FieldValidator<TInstance, TProperty, TContext> : IFieldValidator<TInstance, TContext>
{
    private readonly string _name;
    private readonly Func<TInstance, TProperty> _getter;
    private readonly ISchema<TProperty, TContext> _schema;

    public FieldValidator(string name, Func<TInstance, TProperty> getter, ISchema<TProperty, TContext> schema)
    {
        _name = name;
        if (!string.IsNullOrEmpty(_name) && char.IsUpper(_name[0]))
            _name = char.ToLower(_name[0]) + _name.Substring(1);
        _getter = getter;
        _schema = schema;
    }

    public async ValueTask<IReadOnlyList<ValidationError>> ValidateAsync(TInstance instance, ValidationContext<TContext> context)
    {
        var value = _getter(instance);
        var fieldContext = context.Push(_name);
        var result = await _schema.ValidateAsync(value, fieldContext);
        return result.Errors.ToList();
    }
}

internal sealed class RequiredFieldValidator<TInstance, TProperty, TContext> : IFieldValidator<TInstance, TContext>
{
    private readonly string _name;
    private readonly Func<TInstance, TProperty> _getter;
    private readonly string _message;
    private static readonly IReadOnlyList<ValidationError> EmptyErrors = Array.Empty<ValidationError>();

    public RequiredFieldValidator(string name, Func<TInstance, TProperty> getter, string? message)
    {
        _name = name;
        if (!string.IsNullOrEmpty(_name) && char.IsUpper(_name[0]))
            _name = char.ToLower(_name[0]) + _name.Substring(1);
        _getter = getter;
        _message = message ?? $"{_name} is required";
    }

    public ValueTask<IReadOnlyList<ValidationError>> ValidateAsync(TInstance instance, ValidationContext<TContext> context)
    {
        var value = _getter(instance);
        if (value is null)
        {
            var path = string.IsNullOrEmpty(context.Execution.Path) ? _name : $"{context.Execution.Path}.{_name}";
            return new ValueTask<IReadOnlyList<ValidationError>>([new ValidationError(path, "required", _message)]);
        }
        return new ValueTask<IReadOnlyList<ValidationError>>(EmptyErrors);
    }
}

// ==================== Context-Aware Conditional Builder ====================

public sealed class ConditionalBuilder<T, TContext>
{
    internal List<IFieldValidator<T, TContext>> Validators { get; } = [];

    public ConditionalBuilder<T, TContext> Require<TProperty>(
        Expression<Func<T, TProperty>> propertySelector,
        string? message = null)
    {
        var propertyName = ObjectSchema<T>.GetPropertyName(propertySelector);
        var getter = ObjectSchema<T>.CreateGetter(propertySelector);
        Validators.Add(new RequiredFieldValidator<T, TProperty, TContext>(propertyName, getter, message));
        return this;
    }

    public ConditionalBuilder<T, TContext> Field<TProperty>(
        Expression<Func<T, TProperty>> propertySelector,
        ISchema<TProperty, TContext> schema)
    {
        var propertyName = ObjectSchema<T>.GetPropertyName(propertySelector);
        var getter = ObjectSchema<T>.CreateGetter(propertySelector);
        Validators.Add(new FieldValidator<T, TProperty, TContext>(propertyName, getter, schema));
        return this;
    }

    public ConditionalBuilder<T, TContext> Field<TProperty>(
        Expression<Func<T, TProperty>> propertySelector,
        ISchema<TProperty> schema)
    {
        return Field(propertySelector, new SchemaAdapter<TProperty, TContext>(schema));
    }
}

internal interface IConditionalBranch<T, TContext>
{
    ValueTask<IReadOnlyList<ValidationError>> ValidateAsync(T instance, ValidationContext<TContext> context);
}

internal sealed class ConditionalBranch<T, TContext> : IConditionalBranch<T, TContext>
{
    private readonly Func<T, bool> _condition;
    private readonly ConditionalBuilder<T, TContext> _thenBranch;
    private readonly ConditionalBuilder<T, TContext>? _elseBranch;
    private static readonly IReadOnlyList<ValidationError> EmptyErrors = Array.Empty<ValidationError>();

    public ConditionalBranch(
        Func<T, bool> condition,
        ConditionalBuilder<T, TContext> thenBranch,
        ConditionalBuilder<T, TContext>? elseBranch)
    {
        _condition = condition;
        _thenBranch = thenBranch;
        _elseBranch = elseBranch;
    }

    public async ValueTask<IReadOnlyList<ValidationError>> ValidateAsync(T instance, ValidationContext<TContext> context)
    {
        var branch = _condition(instance) ? _thenBranch : _elseBranch;
        if (branch == null)
            return EmptyErrors;

        List<ValidationError>? errors = null;
        foreach (var validator in branch.Validators)
        {
            var fieldErrors = await validator.ValidateAsync(instance, context);
            if (fieldErrors.Count > 0)
            {
                errors ??= [];
                errors.AddRange(fieldErrors);
            }
        }
        return errors ?? EmptyErrors;
    }
}
