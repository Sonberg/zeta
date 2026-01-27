using Zeta.Core;

namespace Zeta.Schemas;

internal interface IContextlessFieldValidator<T>
{
    ValueTask<IReadOnlyList<ValidationError>> ValidateAsync(T instance, ValidationExecutionContext execution);
}