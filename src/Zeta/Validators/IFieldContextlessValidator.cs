using Zeta.Core;

namespace Zeta.Schemas;

internal interface IFieldContextlessValidator<T>
{
    ValueTask<IReadOnlyList<ValidationError>> ValidateAsync(T instance, ValidationExecutionContext execution);
}