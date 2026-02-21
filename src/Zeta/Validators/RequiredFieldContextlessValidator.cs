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
        _getter = getter;
        _message = message ?? $"{_name} is required";
    }

    public ValueTask<IReadOnlyList<ValidationError>> ValidateAsync(TInstance instance, ValidationContext execution)
    {
        var value = _getter(instance);
        if (value is null)
        {
            var path = execution.PathSegments.Append(PathSegment.Property(_name));
            return new ValueTask<IReadOnlyList<ValidationError>>([new ValidationError(path, "required", _message)]);
        }

        return new ValueTask<IReadOnlyList<ValidationError>>(EmptyErrors);
    }
}
