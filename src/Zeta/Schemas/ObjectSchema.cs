using System.Linq.Expressions;

namespace Zeta.Schemas;

public class ObjectSchema<T, TContext> : ISchema<T, TContext>
{
    private readonly List<IFieldValidator<T, TContext>> _fields = new();
    private readonly List<IRule<T, TContext>> _rules = new();

    public async Task<Result<T>> ValidateAsync(T value, ValidationContext<TContext> context)
    {
        var errors = new List<ValidationError>();

        foreach (var field in _fields)
        {
            var fieldErrors = await field.ValidateAsync(value, context);
            errors.AddRange(fieldErrors);
        }

        foreach (var rule in _rules)
        {
            var error = await rule.ValidateAsync(value, context);
            if (error != null) errors.Add(error);
        }

        return errors.Count == 0
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
        // Adapt ISchema<TProperty> to ISchema<TProperty, TContext>
        // We need a wrapper that ignores the context.
        return Field(propertySelector, new SchemaContextAdapter<TProperty, TContext>(schema));
    }

    public ObjectSchema<T, TContext> Refine(Func<T, TContext, bool> predicate, string message, string code = "custom_error")
    {
        _rules.Add(new DelegateRule<T, TContext>((val, ctx) =>
        {
            if (predicate(val, ctx.Data)) return ValueTask.FromResult<ValidationError?>(null);
            return ValueTask.FromResult<ValidationError?>(new ValidationError(ctx.Execution.Path, code, message));
        }));
        return this;
    }

    private static string GetPropertyName<TProperty>(Expression<Func<T, TProperty>> propertySelector)
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
    public Task<Result<T>> ValidateAsync(T value, ValidationExecutionContext? execution = null)
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
}

internal interface IFieldValidator<T, TContext>
{
    Task<IEnumerable<ValidationError>> ValidateAsync(T instance, ValidationContext<TContext> context);
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

    public async Task<IEnumerable<ValidationError>> ValidateAsync(TInstance instance, ValidationContext<TContext> context)
    {
        var value = _getter(instance);
        var fieldContext = context.Push(_name);
        var result = await _schema.ValidateAsync(value, fieldContext);
        return result.Errors;
    }
}

internal sealed class SchemaContextAdapter<T, TContext> : ISchema<T, TContext>
{
    private readonly ISchema<T> _inner;

    public SchemaContextAdapter(ISchema<T> inner)
    {
        _inner = inner;
    }

    public Task<Result<T>> ValidateAsync(T value, ValidationContext<TContext> context)
    {
        return _inner.ValidateAsync(value, context.Execution);
    }
}
