using System.Runtime.CompilerServices;

namespace Zeta.Core;

/// <summary>
/// Helper methods for creating ValueTask instances in the most optimal way
/// depending on the target framework.
/// </summary>
internal static class ValueTaskHelper
{
    /// <summary>
    /// Creates a ValueTask from a result value using the most optimal method.
    /// On .NET Standard 2.1+ and .NET Core 3.0+, uses ValueTask.FromResult().
    /// On .NET Standard 2.0, uses new ValueTask(value) constructor.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<T> FromResult<T>(T value)
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER || NET5_0_OR_GREATER
        return ValueTask.FromResult(value);
#else
        return new ValueTask<T>(value);
#endif
    }
}
