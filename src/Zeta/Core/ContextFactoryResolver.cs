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

    /// <summary>
    /// Resolves the context data for a context-aware schema from the service provider on
    /// <paramref name="context"/>, promotes the context to a typed <see cref="ValidationRun{TContext}"/>,
    /// and validates. This is the single home for the "resolve factory then validate" dance shared by the
    /// contextless <see cref="ISchema{T}"/> bridge and the injectable validator.
    /// </summary>
    internal static async ValueTask<Result<T, TContext>> ResolveAndValidateAsync<T, TContext>(
        ISchema<T, TContext> schema,
        T value,
        ValidationRun context)
    {
        var serviceProvider = context.ServiceProvider
            ?? throw new InvalidOperationException(
                "IServiceProvider is required for context factory resolution. " +
                "Ensure the validation run includes a service provider.");

        TContext contextData;
        try
        {
            contextData = await ResolveAsync(
                value,
                schema.GetContextFactories(),
                serviceProvider,
                context.CancellationToken);
        }
        catch (ContextFactoryValidationException ex)
        {
            // A validation-aware factory reported an expected failure — surface it as a normal
            // aggregated validation error rather than letting it bubble out as an HTTP 500.
            return Result<T, TContext>.Failure(ex.Errors);
        }

        var typedContext = new ValidationRun<TContext>(
            context.PathSegments,
            contextData,
            context.TimeProvider,
            context.CancellationToken,
            context.ServiceProvider,
            context.PathFormattingOptions);

        return await schema.ValidateAsync(value, typedContext);
    }

    private static bool IsTypeNarrowingMismatch(InvalidOperationException ex)
        => ex.GetType().Name == "TypeNarrowingContextFactoryMismatchException";
}
