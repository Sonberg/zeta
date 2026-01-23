using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Zeta.AspNetCore;

/// <summary>
/// An action filter that validates arguments in controller actions using Zeta schemas.
/// Supports [ZetaIgnore] to skip validation and [ZetaValidate(typeof(...))] to specify a schema.
/// </summary>
public class ZetaValidationActionFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var services = context.HttpContext.RequestServices;
        var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
        var parameters = actionDescriptor?.MethodInfo.GetParameters();

        foreach (var kvp in context.ActionArguments)
        {
            var parameterName = kvp.Key;
            var argument = kvp.Value;

            if (argument == null) continue;

            // Find the parameter info to check for attributes
            var parameterInfo = parameters?.FirstOrDefault(p => p.Name == parameterName);

            // Check for [ZetaIgnore] - skip validation
            if (parameterInfo?.GetCustomAttribute<ZetaIgnoreAttribute>() != null)
            {
                continue;
            }

            var argumentType = argument.GetType();
            object? schema = null;

            // Check for [ZetaValidate(typeof(...))] - use specified schema
            var validateAttr = parameterInfo?.GetCustomAttribute<ZetaValidateAttribute>();
            if (validateAttr != null)
            {
                schema = ResolveSchema(services, validateAttr.SchemaType, argumentType);
            }
            else
            {
                // Fallback: resolve ISchema<T> from DI
                var schemaInterfaceType = typeof(ISchema<>).MakeGenericType(argumentType);
                schema = services.GetService(schemaInterfaceType);
            }

            if (schema == null) continue;

            var validationResult = await ValidateAsync(schema, argument, argumentType, services, context.HttpContext.RequestAborted);

            if (validationResult != null)
            {
                context.Result = new BadRequestObjectResult(new ValidationProblemDetails(validationResult));
                return;
            }
        }

        await next();
    }

    private static object? ResolveSchema(IServiceProvider services, Type schemaType, Type argumentType)
    {
        // First try to resolve from DI
        var schema = services.GetService(schemaType);
        if (schema != null) return schema;

        // Then try to create an instance if it has a parameterless constructor
        if (schemaType.GetConstructor(Type.EmptyTypes) != null)
        {
            return Activator.CreateInstance(schemaType);
        }

        return null;
    }

    private static async Task<Dictionary<string, string[]>?> ValidateAsync(
        object schema,
        object argument,
        Type argumentType,
        IServiceProvider services,
        CancellationToken cancellationToken)
    {
        // Find ValidateAsync method on ISchema<T>
        var schemaInterfaceType = typeof(ISchema<>).MakeGenericType(argumentType);
        var validateMethod = schemaInterfaceType.GetMethod("ValidateAsync", new[] { argumentType, typeof(ValidationExecutionContext) });

        if (validateMethod == null) return null;

        var executionContext = new ValidationExecutionContext(
            path: "",
            services: services,
            cancellationToken: cancellationToken);

        var task = (Task)validateMethod.Invoke(schema, new object?[] { argument, executionContext })!;
        await task.ConfigureAwait(false);

        // Read Result<T> from completed task
        var resultProperty = task.GetType().GetProperty("Result")!;
        var resultValue = resultProperty.GetValue(task)!;

        var isSuccessProp = resultValue.GetType().GetProperty("IsSuccess")!;
        var isSuccess = (bool)isSuccessProp.GetValue(resultValue)!;

        if (isSuccess) return null;

        var errorsProp = resultValue.GetType().GetProperty("Errors")!;
        var errors = (IEnumerable<ValidationError>)errorsProp.GetValue(resultValue)!;

        return errors
            .GroupBy(e => e.Path)
            .ToDictionary(g => g.Key, g => g.Select(m => m.Message).ToArray());
    }
}
