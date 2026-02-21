namespace Zeta.Validators;

internal sealed class RequiredFieldContextContextValidator<TInstance, TProperty, TContext> : IFieldContextValidator<TInstance, TContext>
{
    private readonly string _name;
    private readonly Func<TInstance, TProperty> _getter;
    private readonly string _message;
    
    private static readonly IReadOnlyList<ValidationError> Empty = [];

    public RequiredFieldContextContextValidator(string name, Func<TInstance, TProperty> getter, string? message)
    {
        _name = name;
        _getter = getter;
        _message = message ?? $"{_name} is required";
    }

    public ValueTask<IReadOnlyList<ValidationError>> ValidateAsync(TInstance instance, ValidationContext<TContext> context)
    {
        var value = _getter(instance);
        if (value is null)
        {
            var path = context.PathSegments.Append(PathSegment.Property(_name));
            return new ValueTask<IReadOnlyList<ValidationError>>([new ValidationError(path, "required", _message)]);
        }

        return new ValueTask<IReadOnlyList<ValidationError>>(Empty);
    }
}
