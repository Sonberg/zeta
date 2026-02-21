namespace Zeta.Validators;

internal sealed class FieldContextContextValidator<TInstance, TProperty, TContext> : IFieldContextValidator<TInstance, TContext>
{
    private static readonly IReadOnlyList<ValidationError> EmptyErrors = Array.Empty<ValidationError>();

    private readonly string _name;
    private readonly Func<TInstance, TProperty> _getter;
    private readonly ISchema<TProperty, TContext> _schema;

    public FieldContextContextValidator(string name, Func<TInstance, TProperty> getter, ISchema<TProperty, TContext> schema)
    {
        _name = name;
        _getter = getter;
        _schema = schema;
    }

    public async ValueTask<IReadOnlyList<ValidationError>> ValidateAsync(TInstance instance, ValidationContext<TContext> context)
    {
        var value = _getter(instance);
        var result = await _schema.ValidateAsync(value, context);
        if (result.IsSuccess)
            return EmptyErrors;

        var basePath = context.PathSegments;
        var fieldPath = context.PathSegments.Append(PathSegment.Property(_name));
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
