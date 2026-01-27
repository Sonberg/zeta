namespace Zeta.Conditional;

internal interface IConditionalBranch<T, TContext>
{
    ValueTask<IReadOnlyList<ValidationError>> ValidateAsync(T instance, ValidationContext<TContext> context);
}