using System.Linq.Expressions;

namespace Zeta.Schemas;

/// <summary>
/// A schema for validating complex objects.
/// </summary>
/// <typeparam name="T">The type of object to validate.</typeparam>
public sealed class ObjectSchema<T> : ISchema<T>
{
    private readonly List<IFieldValidator<T>> _fields = new();
    private readonly List<IRule<T>> _rules = new();

    /// <inheritdoc />
    public async Task<Result<T>> ValidateAsync(T value, ValidationContext? context = null)
    {
        context ??= ValidationContext.Empty;
        var errors = new List<ValidationError>();

        // Validate fields
        foreach (var field in _fields)
        {
            var fieldErrors = await field.ValidateAsync(value, context);
            errors.AddRange(fieldErrors);
        }

        // Validate object-level rules
        foreach (var rule in _rules)
        {
            var error = await rule.ValidateAsync(value, context);
            if (error != null)
            {
                errors.Add(error);
            }
        }

        return errors.Count == 0
            ? Result<T>.Success(value)
            : Result<T>.Failure(errors);
    }

    /// <summary>
    /// Defines a validation rule for a specific property.
    /// </summary>
    public ObjectSchema<T> Field<TProperty>(
        Expression<Func<T, TProperty>> propertySelector, 
        ISchema<TProperty> schema)
    {
        var propertyName = GetPropertyName(propertySelector);
        var getter = propertySelector.Compile();

        _fields.Add(new FieldValidator<T, TProperty>(propertyName, getter, schema));
        return this;
    }

    /// <summary>
    /// Adds an object-level refinement rule.
    /// </summary>
    public ObjectSchema<T> Refine(Func<T, bool> predicate, string message, string code = "custom_error")
    {
        _rules.Add(new DelegateRule<T>((val, ctx) =>
        {
            if (predicate(val)) return ValueTask.FromResult<ValidationError?>(null);
            return ValueTask.FromResult<ValidationError?>(new ValidationError(ctx.Path, code, message));
        }));
        return this;
    }

    /// <summary>
    /// Adds an async object-level refinement rule.
    /// </summary>
    public ObjectSchema<T> RefineAsync(Func<T, ValidationContext, Task<bool>> predicate, string message, string code = "custom_error")
    {
         _rules.Add(new DelegateRule<T>(async (val, ctx) =>
        {
            if (await predicate(val, ctx)) return null;
            return new ValidationError(ctx.Path, code, message);
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

internal interface IFieldValidator<T>
{
    Task<IEnumerable<ValidationError>> ValidateAsync(T instance, ValidationContext context);
}

internal sealed class FieldValidator<TInstance, TProperty> : IFieldValidator<TInstance>
{
    private readonly string _name;
    private readonly Func<TInstance, TProperty> _getter;
    private readonly ISchema<TProperty> _schema;

    public FieldValidator(string name, Func<TInstance, TProperty> getter, ISchema<TProperty> schema)
    {
        _name = name; // Capitalized property name usually
        
        // Convert to camelCase for path if desired, but for now keeping 1:1 with property
        // Often APIs want camelCase. Let's start with simple logic: first char lowercase
        if (!string.IsNullOrEmpty(_name) && char.IsUpper(_name[0]))
        {
             _name = char.ToLower(_name[0]) + _name.Substring(1);
        }

        _getter = getter;
        _schema = schema;
    }

    public async Task<IEnumerable<ValidationError>> ValidateAsync(TInstance instance, ValidationContext context)
    {
        var value = _getter(instance);
        var fieldContext = context.Push(_name);
        
        var result = await _schema.ValidateAsync(value, fieldContext);
        
        return result.Errors;
    }
}
