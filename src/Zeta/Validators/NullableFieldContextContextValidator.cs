namespace Zeta.Validators;

/// <summary>
/// Field validator for nullable value type properties in context-aware schemas.
/// Null values skip validation automatically. Non-null values are unwrapped and validated.
/// </summary>
internal sealed class NullableFieldContextContextValidator<TInstance, TProperty, TContext> : IFieldContextValidator<TInstance, TContext>
    where TProperty : struct
{
    private static readonly IReadOnlyList<ValidationError> EmptyErrors = [];

    private readonly string _name;
    private readonly Func<TInstance, TProperty?> _getter;
    private readonly ISchema<TProperty, TContext> _schema;

    public NullableFieldContextContextValidator(string name, Func<TInstance, TProperty?> getter, ISchema<TProperty, TContext> schema)
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

        if (!value.HasValue)
        {
            return _schema.AllowNull
                ? EmptyErrors
                : [new ValidationError(fieldContext.Path, "null_value", $"{_name} cannot be null")];
        }

        var result = await _schema.ValidateAsync(value.Value, fieldContext);
        return result.IsSuccess ? EmptyErrors : result.Errors;
    }
}