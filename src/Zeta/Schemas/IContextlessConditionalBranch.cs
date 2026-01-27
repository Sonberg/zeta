using Zeta.Core;

namespace Zeta.Schemas;

internal interface IContextlessConditionalBranch<T>
{
    ValueTask<IReadOnlyList<ValidationError>> ValidateAsync(T instance, ValidationExecutionContext execution);
}