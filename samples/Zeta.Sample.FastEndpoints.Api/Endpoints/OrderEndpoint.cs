using Zeta.Sample.FastEndpoints.Api.Models;

namespace Zeta.Sample.FastEndpoints.Api.Endpoints;

/// <summary>
/// Creates an order using auto-discovered schema validation.
/// Demonstrates <see cref="ZetaEndpointExtensions.UseZetaValidation"/> — no <c>Validate()</c> call needed.
/// The global configurator in Program.cs discovers the static Schema field automatically.
/// </summary>
public class OrderEndpoint : Endpoint<OrderRequest>
{
    private static readonly ISchema<OrderRequest> Schema =
        Z.Object<OrderRequest>()
            .Field(r => r.ProductId, s => s.NotEmpty())
            .Field(r => r.Quantity, s => s.Min(1).Max(1000));

    public override void Configure()
    {
        Post("/api/orders");
        AllowAnonymous();
        // Note: no Validate(Schema) call — global configurator handles it via UseZetaValidation()
    }

    public override async Task HandleAsync(OrderRequest req, CancellationToken ct)
        => await HttpContext.Response.SendOkAsync(ct);
}
