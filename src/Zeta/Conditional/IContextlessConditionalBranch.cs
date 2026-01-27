namespace Zeta.Conditional;

internal interface IContextlessConditionalBranch<T>
{
    ValueTask<IReadOnlyList<ValidationError>> ValidateAsync(T instance, ValidationContext context);
}