using System.Linq.Expressions;
using Zeta.Core;

namespace Zeta.Schemas;

public class ObjectSchema<T, TContext> : ISchema<T, TContext>
{
    private readonly List<IFieldValidator<T, TContext>> _fields = new();
    private readonly List<IRule<T, TContext>> _rules = new();
    private readonly List<IConditionalBranch<T, TContext>> _conditionals = new();

    public async ValueTask<Result<T>> ValidateAsync(T value, ValidationContext<TContext> context)
    {
        List<ValidationError>? errors = null;

        foreach (var field in _fields)
        {
            var fieldErrors = await field.ValidateAsync(value, context);
            if (fieldErrors.Count > 0)
            {
                errors ??= new List<ValidationError>();
                errors.AddRange(fieldErrors);
            }
        }

        foreach (var conditional in _conditionals)
        {
            var conditionalErrors = await conditional.ValidateAsync(value, context);
            if (conditionalErrors.Count > 0)
            {
                errors ??= new List<ValidationError>();
                errors.AddRange(conditionalErrors);
            }
        }

        foreach (var rule in _rules)
        {
            var error = await rule.ValidateAsync(value, context);
            if (error != null)
            {
                errors ??= new List<ValidationError>();
                errors.Add(error);
            }
        }

        return errors == null
            ? Result<T>.Success(value)
            : Result<T>.Failure(errors);
    }

    public ObjectSchema<T, TContext> Field<TProperty>(
        Expression<Func<T, TProperty>> propertySelector,
        ISchema<TProperty, TContext> schema)
    {
        var propertyName = GetPropertyName(propertySelector);
        var getter = propertySelector.Compile();
        _fields.Add(new FieldValidator<T, TProperty, TContext>(propertyName, getter, schema));
        return this;
    }

    // Support simple schemas that don't need context (auto-adapt)
    public ObjectSchema<T, TContext> Field<TProperty>(
        Expression<Func<T, TProperty>> propertySelector,
        ISchema<TProperty> schema)
    {
        return Field(propertySelector, new SchemaContextAdapter<TProperty, TContext>(schema));
    }

    /// <summary>
    /// Conditionally validates fields based on a predicate.
    /// </summary>
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
        _rules.Add(new DelegateRule<T, TContext>((val, ctx) =>
        {
            if (predicate(val, ctx.Data)) return ValueTaskHelper.NullError();
            return ValueTaskHelper.Error(new ValidationError(ctx.Execution.Path, code, message));
        }));
        return this;
    }

    internal static string GetPropertyName<TProperty>(Expression<Func<T, TProperty>> propertySelector)
    {
        if (propertySelector.Body is MemberExpression member)
        {
            return member.Member.Name;
        }
        throw new ArgumentException("Expression must be a simple property access", nameof(propertySelector));
    }
}

/// <summary>
/// Default object schema with no context.
/// </summary>
public sealed class ObjectSchema<T> : ObjectSchema<T, object?>, ISchema<T>
{
    public ValueTask<Result<T>> ValidateAsync(T value, ValidationExecutionContext? execution = null)
    {
        execution ??= ValidationExecutionContext.Empty;
        var context = new ValidationContext<object?>(null, execution);
        return ValidateAsync(value, context);
    }

    public new ObjectSchema<T> Field<TProperty>(Expression<Func<T, TProperty>> propertySelector, ISchema<TProperty, object?> schema)
    {
        base.Field(propertySelector, schema);
        return this;
    }

    public new ObjectSchema<T> Field<TProperty>(Expression<Func<T, TProperty>> propertySelector, ISchema<TProperty> schema)
    {
        base.Field(propertySelector, schema);
        return this;
    }

    public new ObjectSchema<T> Refine(Func<T, object?, bool> predicate, string message, string code = "custom_error")
    {
        base.Refine(predicate, message, code);
        return this;
    }

    public new ObjectSchema<T> When(
        Func<T, bool> condition,
        Action<ConditionalBuilder<T, object?>> thenBranch,
        Action<ConditionalBuilder<T, object?>>? elseBranch = null)
    {
        base.When(condition, thenBranch, elseBranch);
        return this;
    }
}

/// <summary>
/// Builder for conditional validation branches.
/// </summary>
public sealed class ConditionalBuilder<T, TContext>
{
    internal List<IFieldValidator<T, TContext>> Validators { get; } = new();

    /// <summary>
    /// Marks a field as required (not null).
    /// </summary>
    public ConditionalBuilder<T, TContext> Require<TProperty>(
        Expression<Func<T, TProperty>> propertySelector,
        string? message = null)
    {
        var propertyName = ObjectSchema<T, TContext>.GetPropertyName(propertySelector);
        var getter = propertySelector.Compile();
        Validators.Add(new RequiredFieldValidator<T, TProperty, TContext>(propertyName, getter, message));
        return this;
    }

    /// <summary>
    /// Validates a field with a specific schema.
    /// </summary>
    public ConditionalBuilder<T, TContext> Field<TProperty>(
        Expression<Func<T, TProperty>> propertySelector,
        ISchema<TProperty, TContext> schema)
    {
        var propertyName = ObjectSchema<T, TContext>.GetPropertyName(propertySelector);
        var getter = propertySelector.Compile();
        Validators.Add(new FieldValidator<T, TProperty, TContext>(propertyName, getter, schema));
        return this;
    }

    /// <summary>
    /// Validates a field with a specific schema.
    /// </summary>
    public ConditionalBuilder<T, TContext> Field<TProperty>(
        Expression<Func<T, TProperty>> propertySelector,
        ISchema<TProperty> schema)
    {
        return Field(propertySelector, new SchemaContextAdapter<TProperty, TContext>(schema));
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
        {
            return EmptyErrors;
        }

        List<ValidationError>? errors = null;
        foreach (var validator in branch.Validators)
        {
            var fieldErrors = await validator.ValidateAsync(instance, context);
            if (fieldErrors.Count > 0)
            {
                errors ??= new List<ValidationError>();
                errors.AddRange(fieldErrors);
            }
        }

        return errors ?? EmptyErrors;
    }
}

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
        {
             _name = char.ToLower(_name[0]) + _name.Substring(1);
        }
        _getter = getter;
        _schema = schema;
    }

    public async ValueTask<IReadOnlyList<ValidationError>> ValidateAsync(TInstance instance, ValidationContext<TContext> context)
    {
        var value = _getter(instance);
        var fieldContext = context.Push(_name);
        var result = await _schema.ValidateAsync(value, fieldContext);
        return result.Errors;
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
        {
            _name = char.ToLower(_name[0]) + _name.Substring(1);
        }
        _getter = getter;
        _message = message ?? $"{_name} is required";
    }

    public ValueTask<IReadOnlyList<ValidationError>> ValidateAsync(TInstance instance, ValidationContext<TContext> context)
    {
        var value = _getter(instance);
        if (value is null)
        {
            var path = string.IsNullOrEmpty(context.Execution.Path)
                ? _name
                : $"{context.Execution.Path}.{_name}";
            return new ValueTask<IReadOnlyList<ValidationError>>(
                new[] { new ValidationError(path, "required", _message) });
        }

        return new ValueTask<IReadOnlyList<ValidationError>>(EmptyErrors);
    }
}

internal sealed class SchemaContextAdapter<T, TContext> : ISchema<T, TContext>
{
    private readonly ISchema<T> _inner;

    public SchemaContextAdapter(ISchema<T> inner)
    {
        _inner = inner;
    }

    public ValueTask<Result<T>> ValidateAsync(T value, ValidationContext<TContext> context)
    {
        return _inner.ValidateAsync(value, context.Execution);
    }
}
