using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Zeta.AspNetCore;

/// <summary>
/// An action filter that validates arguments in controller actions using Zeta schemas.
/// </summary>
public class ZetaValidationActionFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var services = context.HttpContext.RequestServices;

        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument == null) continue;

            // Resolve ISchema<ArgumentType>
            var argumentType = argument.GetType();
            
            // We use standard schema (without TContext) for global filter
            var schemaType = typeof(ISchema<>).MakeGenericType(argumentType);
            var schema = services.GetService(schemaType);

            if (schema != null)
            {
                // Invoke ValidateAsync using reflection because we don't know T at compile time
                // Method: Task<Result<T>> ValidateAsync(T value, ValidationExecutionContext? execution = null);

                var validateMethod = schemaType.GetMethod("ValidateAsync", new[] { argumentType, typeof(ValidationExecutionContext) });
                if (validateMethod != null)
                {
                    // Create context
                    var executionContext = new ValidationExecutionContext(
                        path: "", 
                        services: services, 
                        cancellationToken: context.HttpContext.RequestAborted);

                    var task = (Task)validateMethod.Invoke(schema, new object?[] { argument, executionContext })!;
                    await task.ConfigureAwait(false);

                    // Read Result<T> property from Task<Result<T>>
                    // Get IsSuccess and Errors
                    var resultProperty = task.GetType().GetProperty("Result")!;
                    var resultValue = resultProperty.GetValue(task)!;
                    
                    var isSuccessProp = resultValue.GetType().GetProperty("IsSuccess")!;
                    var isSuccess = (bool)isSuccessProp.GetValue(resultValue)!;

                    if (!isSuccess)
                    {
                        var errorsProp = resultValue.GetType().GetProperty("Errors")!;
                        var errors = (IEnumerable<ValidationError>)errorsProp.GetValue(resultValue)!;

                        var dict = errors
                            .GroupBy(e => e.Path)
                            .ToDictionary(g => g.Key, g => g.Select(m => m.Message).ToArray());

                        context.Result = new BadRequestObjectResult(new ValidationProblemDetails(dict));
                        return;
                    }
                }
            }
        }

        await next();
    }
}
