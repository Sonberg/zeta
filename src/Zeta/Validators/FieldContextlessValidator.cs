using Zeta.Core;

namespace Zeta.Validators;

internal sealed class FieldContextlessValidator<TInstance, TProperty> : IFieldContextlessValidator<TInstance>
{
    private static readonly IReadOnlyList<ValidationError> EmptyErrors = Array.Empty<ValidationError>();

    private readonly string _name;
    private readonly Func<TInstance, TProperty> _getter;
    private readonly ISchema<TProperty> _schema;
    private readonly Func<TInstance, bool>? _isNull;

    public FieldContextlessValidator(string name, Func<TInstance, TProperty> getter, ISchema<TProperty> schema,
        Func<TInstance, bool>? isNull = null)
    {
        _name = name;
        if (!string.IsNullOrEmpty(_name) && char.IsUpper(_name[0]))
            _name = char.ToLower(_name[0]) + _name.Substring(1);
        _getter = getter;
        _schema = schema;
        _isNull = isNull;
    }

    public async ValueTask<IReadOnlyList<ValidationError>> ValidateAsync(TInstance instance, ValidationContext execution)
    {
        if (_isNull != null && _isNull(instance))
            return EmptyErrors;

        var value = _getter(instance);
        var fieldExecution = execution.Push(_name);
        var result = await _schema.ValidateAsync(value, fieldExecution);
        return result.IsSuccess ? EmptyErrors : result.Errors;
    }
}