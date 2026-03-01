using Zeta.Sample.FastEndpoints.Api.Models;

namespace Zeta.Sample.FastEndpoints.Api.Endpoints;

/// <summary>
/// Creates a product with contextless validation.
/// Demonstrates using <see cref="ZetaPreProcessor{TRequest}"/> directly via <c>PreProcessors()</c>.
/// </summary>
public class CreateProductEndpoint : Endpoint<CreateProductRequest>
{
    private static readonly ISchema<CreateProductRequest> Schema =
        Z.Object<CreateProductRequest>()
            .Field(p => p.Name, Z.String().MinLength(2).MaxLength(200))
            .Field(p => p.Sku, Z.String()
                .Regex(@"^[A-Z0-9\-]+$", "SKU must contain only uppercase letters, numbers, and hyphens"))
            .Field(p => p.Price, Z.Decimal().Min(0.01m))
            .Field(p => p.StockQuantity, Z.Int().Min(0));

    public override void Configure()
    {
        Post("/api/products");
        AllowAnonymous();
        PreProcessors(new ZetaPreProcessor<CreateProductRequest>(Schema));
    }

    public override async Task HandleAsync(CreateProductRequest req, CancellationToken ct)
    {
        await HttpContext.Response.SendOkAsync(ct);
    }
}