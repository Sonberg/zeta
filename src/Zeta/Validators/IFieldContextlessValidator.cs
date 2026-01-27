namespace Zeta.Validators;

internal interface IFieldContextlessValidator<T>
{
    ValueTask<IReadOnlyList<ValidationError>> ValidateAsync(T instance, ValidationContext execution);
}