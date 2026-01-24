#if NETSTANDARD2_0
// Polyfill for init-only properties in .NET Standard 2.0
// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}
#endif
