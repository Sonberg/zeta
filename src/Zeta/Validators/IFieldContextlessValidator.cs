using Zeta.Core;

namespace Zeta.Validators;

internal interface IFieldContextlessValidator<T>
{
    ValueTask<IReadOnlyList<ValidationError>> ValidateAsync(T instance, ValidationExecutionContext execution);
}