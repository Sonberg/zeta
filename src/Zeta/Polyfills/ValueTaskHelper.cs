using System.Threading.Tasks;

namespace Zeta;

internal static class ValueTaskHelper
{
    public static ValueTask<ValidationError?> NullError() => new ValueTask<ValidationError?>((ValidationError?)null);

    public static ValueTask<ValidationError?> Error(ValidationError error) => new ValueTask<ValidationError?>(error);
}
