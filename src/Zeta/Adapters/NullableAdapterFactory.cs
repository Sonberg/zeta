namespace Zeta.Adapters;

internal static class NullableAdapterFactory
{
    public static ISchema<T?, TContext> CreateContextWrapper<T, TContext>(ISchema<T, TContext> schema)
    {
        if (typeof(T).IsValueType)
        {
            var wrapperType = typeof(NullableStructContextWrapper<,>).MakeGenericType(typeof(T), typeof(TContext));
            return (ISchema<T?, TContext>)Activator.CreateInstance(wrapperType, schema)!;
        }
        else
        {
            var wrapperType = typeof(NullableReferenceContextWrapper<,>).MakeGenericType(typeof(T), typeof(TContext));
            return (ISchema<T?, TContext>)Activator.CreateInstance(wrapperType, schema)!;
        }
    }

    public static ISchema<T?> CreateContextlessWrapper<T>(ISchema<T> schema)
    {
        if (typeof(T).IsValueType)
        {
            var wrapperType = typeof(NullableStructContextlessAdapter<>).MakeGenericType(typeof(T));
            return (ISchema<T?>)Activator.CreateInstance(wrapperType, schema)!;
        }
        else
        {
            var wrapperType = typeof(NullableReferenceContextlessAdapter<>).MakeGenericType(typeof(T));
            return (ISchema<T?>)Activator.CreateInstance(wrapperType, schema)!;
        }
    }
}
