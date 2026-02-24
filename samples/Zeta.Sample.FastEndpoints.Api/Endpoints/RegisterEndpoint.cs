using FastEndpoints;
using Zeta.FastEndpoints;
using Zeta.Sample.FastEndpoints.Api.Models;

namespace Zeta.Sample.FastEndpoints.Api.Endpoints;

/// <summary>
/// Registers a user with contextless (no DB lookup) validation.
/// Demonstrates <see cref="ZetaPreProcessor{TRequest}"/>.
/// </summary>
public class RegisterEndpoint : Endpoint<RegisterRequest>
{
    private static readonly ISchema<RegisterRequest> Schema =
        Z.Object<RegisterRequest>()
            .Field(r => r.Email, Z.String().Email())
            .Field(r => r.Password, Z.String()
                .MinLength(8)
                .Regex(@"[A-Z]", "Password must contain at least one uppercase letter")
                .Regex(@"[a-z]", "Password must contain at least one lowercase letter")
                .Regex(@"[0-9]", "Password must contain at least one digit"))
            .Field(r => r.Age, Z.Int().Min(18).Max(120));

    public override void Configure()
    {
        Post("/api/users/register");
        AllowAnonymous();
        PreProcessors(new ZetaPreProcessor<RegisterRequest>(Schema));
    }

    public override async Task HandleAsync(RegisterRequest req, CancellationToken ct)
    {
        await HttpContext.Response.SendOkAsync(ct);
    }
}
