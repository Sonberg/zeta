namespace Zeta.Validators;

internal static class FieldError
{
    /// <summary>
    /// Rebases each error's path onto <paramref name="fieldPath"/>, relative to <paramref name="basePath"/>.
    /// Shared by every field validator so path remapping lives in one place.
    /// </summary>
    public static IReadOnlyList<ValidationError> PrependFieldPath(
        ValidationPath basePath,
        ValidationPath fieldPath,
        IReadOnlyList<ValidationError> errors)
    {
        var mapped = new ValidationError[errors.Count];
        for (var i = 0; i < errors.Count; i++)
        {
            var error = errors[i];
            var relativePath = error.Path.RelativeTo(basePath);
            mapped[i] = new ValidationError(fieldPath.Concat(relativePath), error.Code, error.Message)
            {
                AttemptedValue = error.AttemptedValue,
                HasAttemptedValue = error.HasAttemptedValue,
            };
        }

        return mapped;
    }
}
