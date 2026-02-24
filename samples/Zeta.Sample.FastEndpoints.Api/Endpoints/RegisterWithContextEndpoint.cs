using FastEndpoints;
using Microsoft.Extensions.DependencyInjection;
using Zeta.FastEndpoints;
using Zeta.Sample.FastEndpoints.Api.Models;
using Zeta.Sample.FastEndpoints.Api.Repository;

namespace Zeta.Sample.FastEndpoints.Api.Endpoints;

public record UserContext(bool EmailExists);

/// <summary>
/// Registers a user with context-aware validation (async email uniqueness check).
/// Demonstrates <see cref="ZetaPreProcessor{TRequest}"/> with a context-aware schema.
/// Context-aware schemas with a factory delegate implement <see cref="ISchema{T}"/> directly,
/// so no separate pre-processor type is needed.
/// </summary>
public class RegisterWithContextEndpoint : Endpoint<RegisterRequest>
{
    private static readonly ISchema<RegisterRequest> Schema =
        Z.Object<RegisterRequest>()
            .Using<UserContext>(async (value, sp, ct) =>
            {
                var repo = sp.GetRequiredService<IUserRepository>();
                var emailExists = await repo.EmailExistsAsync(value.Email, ct);
                return new UserContext(emailExists);
            })
            .Field(r => r.Email, Z.String().Email())
            .Field(r => r.Password, Z.String().MinLength(8))
            .Field(r => r.Age, Z.Int().Min(18).Max(120))
            .Refine((r, ctx) => !ctx.EmailExists, "Email is already registered", "email_taken");

    public override void Configure()
    {
        Post("/api/users/register-ctx");
        AllowAnonymous();
        PreProcessors(new ZetaPreProcessor<RegisterRequest>(Schema));
    }

    public override async Task HandleAsync(RegisterRequest req, CancellationToken ct)
    {
        await HttpContext.Response.SendOkAsync(ct);
    }
}
