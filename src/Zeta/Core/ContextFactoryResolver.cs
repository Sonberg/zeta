namespace Zeta.Core;

internal static class ContextFactoryResolver
{
    internal static async ValueTask<TContext> ResolveAsync<T, TContext>(
        T value,
        IEnumerable<Func<T, IServiceProvider, CancellationToken, ValueTask<TContext>>> factories,
        IServiceProvider serviceProvider,
        CancellationToken ct)
    {
        var factoryList = factories.ToList();
        if (factoryList.Count == 0)
        {
            throw new InvalidOperationException(
                $"No context factory for {typeof(T).Name}/{typeof(TContext).Name}. " +
                "Provide a factory via .Using<TContext>(factory).");
        }

        var applicableCount = 0;
        TContext? contextData = default;

        foreach (var factory in factoryList)
        {
            try
            {
                var candidate = await factory(value, serviceProvider, ct);
                applicableCount++;

                if (applicableCount > 1)
                {
                    throw new InvalidOperationException(
                        $"Multiple applicable context factories for {typeof(T).Name}/{typeof(TContext).Name} were found for value type {value?.GetType().Name ?? "null"}. " +
                        "Ensure each value matches exactly one context factory.");
                }

                contextData = candidate;
            }
            catch (InvalidOperationException ex) when (IsTypeNarrowingMismatch(ex))
            {
                // Ignore non-matching polymorphic branch factories.
            }
        }

        if (applicableCount == 1)
        {
            return contextData!;
        }

        throw new InvalidOperationException(
            $"No applicable context factory for {typeof(T).Name}/{typeof(TContext).Name} and value type {value?.GetType().Name ?? "null"}. " +
            "Provide a matching factory via .Using<TContext>(factory).");
    }

    private static bool IsTypeNarrowingMismatch(InvalidOperationException ex)
        => ex.GetType().Name == "TypeNarrowingContextFactoryMismatchException";
}
