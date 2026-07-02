using System.Reflection;
using FastEndpoints;

namespace Zeta.FastEndpoints;

/// <summary>
/// Extension methods for <see cref="EndpointDefinition"/> to enable convention-based Zeta validation.
/// </summary>
public static class ZetaEndpointExtensions
{
    private static readonly Type _openProcessorType = typeof(ZetaGlobalPreProcessor<>);
    private static readonly Type _openSchemaType = typeof(ISchema<>);

    /// <summary>
    /// Auto-discovers a static <c>ISchema&lt;TRequest&gt;</c> field on the endpoint class
    /// and registers Zeta validation as a pre-processor.
    /// Safe to call on endpoints with no schema — does nothing in that case.
    /// </summary>
    /// <remarks>
    /// Register once for all endpoints in <c>Program.cs</c>:
    /// <code>
    /// app.UseFastEndpoints(c => c.Endpoints.Configurator = ep => ep.UseZetaValidation());
    /// </code>
    /// Any endpoint with a static <c>ISchema&lt;TRequest&gt;</c> field will have validation wired
    /// automatically without calling <c>Validate(Schema)</c> or <c>PreProcessors(...)</c> manually.
    /// </remarks>
    public static void UseZetaValidation(this EndpointDefinition ep)
    {
        var requestType = ep.ReqDtoType;
        var endpointType = ep.EndpointType;

        var schema = FindSchema(endpointType, requestType);
        if (schema is null) return;

        var processorType = _openProcessorType.MakeGenericType(requestType);
        var processor = (IGlobalPreProcessor)Activator.CreateInstance(
            processorType,
            BindingFlags.NonPublic | BindingFlags.Instance,
            binder: null,
            args: [schema],
            culture: null)!;

        ep.PreProcessors(Order.Before, processor);
    }

    private static object? FindSchema(Type endpointType, Type requestType)
    {
        var schemaInterfaceType = _openSchemaType.MakeGenericType(requestType);
        var type = endpointType;

        while (type is not null && !IsFastEndpointsBase(type))
        {
            foreach (var field in type.GetFields(
                BindingFlags.Static | BindingFlags.Public |
                BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            {
                if (schemaInterfaceType.IsAssignableFrom(field.FieldType))
                    return field.GetValue(null);
            }

            type = type.BaseType;
        }

        return null;
    }

    private static bool IsFastEndpointsBase(Type type)
    {
        if (!type.IsGenericType) return false;
        var def = type.GetGenericTypeDefinition();
        return def == typeof(Endpoint<>) || def == typeof(Endpoint<,>);
    }
}
