using FastEndpoints;
using FluentValidation.Results;

namespace Zeta.FastEndpoints;

/// <summary>
/// A FastEndpoints pre-processor that validates the request using a contextless Zeta schema.
/// </summary>
/// <typeparam name="TRequest">The request type to validate.</typeparam>
public sealed class ZetaPreProcessor<TRequest> : IPreProcessor<TRequest>
    where TRequest : notnull
{
    private readonly ISchema<TRequest> _schema;

    /// <summary>
    /// Initializes a new instance of the <see cref="ZetaPreProcessor{TRequest}"/> class.
    /// </summary>
    /// <param name="schema">The schema to use for validation.</param>
    public ZetaPreProcessor(ISchema<TRequest> schema) => _schema = schema;

    /// <summary>
    /// Validates the request and short-circuits with a 400 response if validation fails.
    /// </summary>
    public async Task PreProcessAsync(IPreProcessorContext<TRequest> ctx, CancellationToken ct)
    {
        if (ctx.Request is null) return;

        var validationContext = new ValidationContext(
            cancellationToken: ct,
            serviceProvider: ctx.HttpContext.RequestServices);

        var result = await _schema.ValidateAsync(ctx.Request, validationContext);
        if (result.IsSuccess) return;

        foreach (var error in result.Errors)
            ctx.ValidationFailures.Add(new ValidationFailure(error.PathString, error.Message) { ErrorCode = error.Code });

        await ctx.HttpContext.Response.SendErrorsAsync(ctx.ValidationFailures, cancellation: ct);
    }
}
