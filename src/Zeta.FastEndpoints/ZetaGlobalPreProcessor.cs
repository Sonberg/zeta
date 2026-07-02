using FastEndpoints;
using FluentValidation.Results;

namespace Zeta.FastEndpoints;

internal sealed class ZetaGlobalPreProcessor<TRequest> : IGlobalPreProcessor
    where TRequest : notnull
{
    private readonly ISchema<TRequest> _schema;

    internal ZetaGlobalPreProcessor(ISchema<TRequest> schema) => _schema = schema;

    public async Task PreProcessAsync(IPreProcessorContext ctx, CancellationToken ct)
    {
        if (ctx.Request is not TRequest request || ctx.HttpContext.Response.HasStarted) return;

        var validationContext = new ValidationContext(
            cancellationToken: ct,
            serviceProvider: ctx.HttpContext.RequestServices);

        var result = await _schema.ValidateAsync(request, validationContext);
        if (result.IsSuccess) return;

        foreach (var error in result.Errors)
            ctx.ValidationFailures.Add(
                new ValidationFailure(error.PathString, error.Message) { ErrorCode = error.Code });

        await ctx.HttpContext.Response.SendErrorsAsync(ctx.ValidationFailures, cancellation: ct);
    }
}
