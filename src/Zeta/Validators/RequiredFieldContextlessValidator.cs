using Zeta.Core;

namespace Zeta.Validators;

internal sealed class RequiredFieldContextlessValidator<TInstance, TProperty> : IFieldContextlessValidator<TInstance>
{
    private readonly string _name;
    private readonly Func<TInstance, TProperty> _getter;
    private readonly string _message;
    private static readonly IReadOnlyList<ValidationError> EmptyErrors = Array.Empty<ValidationError>();

    public RequiredFieldContextlessValidator(string name, Func<TInstance, TProperty> getter, string? message)
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