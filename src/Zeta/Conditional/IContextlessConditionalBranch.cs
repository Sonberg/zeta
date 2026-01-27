using Zeta.Core;

namespace Zeta.Conditional;

internal interface IContextlessConditionalBranch<T>
{
    ValueTask<IReadOnlyList<ValidationError>> ValidateAsync(T instance, ValidationExecutionContext execution);
}