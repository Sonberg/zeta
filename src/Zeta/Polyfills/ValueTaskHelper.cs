using System.Runtime.CompilerServices;

namespace Zeta;

internal static class ValueTaskHelper
{
    private static readonly ValueTask<ValidationError?> Null = new((ValidationError?)null);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<ValidationError?> NullError() => Null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<ValidationError?> Error(ValidationError error) => new(error);
}