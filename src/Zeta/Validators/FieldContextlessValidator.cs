namespace Zeta.Validators;

internal sealed class FieldContextlessValidator<TInstance, TProperty> : IFieldContextlessValidator<TInstance>
{
    private static readonly IReadOnlyList<ValidationError> EmptyErrors = [];

    private readonly string _name;
    private readonly Func<TInstance, TProperty> _getter;
    private readonly ISchema<TProperty> _schema;

    public FieldContextlessValidator(string name, Func<TInstance, TProperty> getter, ISchema<TProperty> schema)
    {
        _name = name;
        _getter = getter;
        _schema = schema;
    }

    public async ValueTask<IReadOnlyList<ValidationError>> ValidateAsync(TInstance instance, ValidationContext execution)
    {
        var value = _getter(instance);
        var fieldExecution = execution.Push(_name);
        var result = await _schema.ValidateAsync(value, fieldExecution);
        return result.IsSuccess ? EmptyErrors : result.Errors;
    }
}
