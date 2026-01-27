namespace Zeta.Schemas;

internal interface IFieldValidator<T, TContext>
{
    ValueTask<IReadOnlyList<ValidationError>> ValidateAsync(T instance, ValidationContext<TContext> context);
}