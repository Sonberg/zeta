namespace Zeta.Validators;

internal interface IFieldContextValidator<T, TContext>
{
    ValueTask<IReadOnlyList<ValidationError>> ValidateAsync(T instance, ValidationRun<TContext> context);
}