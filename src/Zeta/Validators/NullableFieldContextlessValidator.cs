namespace Zeta.Validators;

/// <summary>
/// Field validator for nullable value type properties in contextless schemas.
/// When AllowNull is true (via .Nullable()), null values pass validation. Otherwise, null produces a "null_value" error.
/// Non-null values are unwrapped and validated by the inner schema.
/// </summary>
internal sealed class NullableFieldContextlessValidator<TInstance, TProperty> : IFieldContextlessValidator<TInstance>
    where TProperty : struct
{
    private static readonly IReadOnlyList<ValidationError> EmptyErrors = [];

    private readonly string _name;
    private readonly Func<TInstance, TProperty?> _getter;
    private readonly ISchema<TProperty> _schema;

    public NullableFieldContextlessValidator(string name, Func<TInstance, TProperty?> getter, ISchema<TProperty> schema)
    {
        _name = name;
        _getter = getter;
        _schema = schema;
    }

    public async ValueTask<IReadOnlyList<ValidationError>> ValidateAsync(TInstance instance, ValidationContext context)
    {
        var value = _getter(instance);
        var fieldPath = context.PathSegments.Append(PathSegment.Property(_name));

        if (!value.HasValue)
        {
            return _schema.AllowNull
                ? EmptyErrors
                : [new ValidationError(fieldPath, "null_value", $"{_name} cannot be null")];
        }

        var result = await _schema.ValidateAsync(value.Value, context);
        if (result.IsSuccess)
            return EmptyErrors;

        return PrependFieldPath(context.PathSegments, fieldPath, result.Errors);
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
