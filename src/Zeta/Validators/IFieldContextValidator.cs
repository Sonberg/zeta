namespace Zeta.Schemas;

internal interface IFieldContextValidator<T, TContext>
{
    ValueTask<IReadOnlyList<ValidationError>> ValidateAsync(T instance, ValidationContext<TContext> context);
}