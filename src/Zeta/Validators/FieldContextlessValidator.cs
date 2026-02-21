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
        var result = await _schema.ValidateAsync(value, execution);
        if (result.IsSuccess)
            return EmptyErrors;

        var basePath = execution.PathSegments;
        var fieldPath = execution.PathSegments.Append(PathSegment.Property(_name));
        return PrependFieldPath(basePath, fieldPath, result.Errors);
    }

    private static IReadOnlyList<ValidationError> PrependFieldPath(
        ValidationPath basePath,
        ValidationPath fieldPath,
        IReadOnlyList<ValidationError> errors)
    {
        var mapped = new ValidationError[errors.Count];
        for (var i = 0; i < errors.Count; i++)
        {
            var error = errors[i];
            var relativePath = error.Path.RelativeTo(basePath);
            mapped[i] = new ValidationError(fieldPath.Concat(relativePath), error.Code, error.Message);
        }

        return mapped;
    }
}
