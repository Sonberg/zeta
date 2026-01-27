namespace Zeta.Schemas;

internal sealed class RequiredFieldValidator<TInstance, TProperty, TContext> : IFieldValidator<TInstance, TContext>
{
    private readonly string _name;
    private readonly Func<TInstance, TProperty> _getter;
    private readonly string _message;
    
    private static readonly IReadOnlyList<ValidationError> Empty = [];

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

        return new ValueTask<IReadOnlyList<ValidationError>>(Empty);
    }
}