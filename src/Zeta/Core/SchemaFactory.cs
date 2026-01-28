namespace Zeta.Core;

using System.Collections.Concurrent;

internal static class SchemaFactory
{
    // Primitive registrations (single source of truth)
    private static readonly ConcurrentDictionary<Type, ISchemaEntry> Entries = new();

    static SchemaFactory()
    {
        Register(Z.String);
        Register(Z.Int);
        Register(Z.Double);
        Register(Z.Decimal);
        Register(Z.Bool);
        Register(Z.Guid);
        Register(Z.DateTime);

#if !NETSTANDARD2_0
        Register(Z.DateOnly);
        Register(Z.TimeOnly);
#endif
    }

    private static void Register<T>(Func<ContextlessSchema<T>> contextless)
    {
        Entries[typeof(T)] = new SchemaEntry<T>(contextless);
    }

    public static ContextlessSchema<TProperty> Create<TProperty>()
    {
        if (Entries.TryGetValue(typeof(TProperty), out var entry))
            if (entry.Create<TProperty>() is { } schema)
                return schema;

        throw new NotSupportedException($"No schema registered for type '{nameof(TProperty)}'");
    }

    private interface ISchemaEntry
    {
        ContextlessSchema<TProperty>? Create<TProperty>();
    }

    private sealed class SchemaEntry<T> : ISchemaEntry
    {
        private readonly Func<ContextlessSchema<T>> _factory;

        public SchemaEntry(Func<ContextlessSchema<T>> factory)
        {
            _factory = factory;
        }

        public ContextlessSchema<TProperty>? Create<TProperty>()
        {
            return _factory() as ContextlessSchema<TProperty>;
        }
    }
}