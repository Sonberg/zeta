using MediatR;

namespace Zeta.Sample.MediatR;

public sealed record CreateUserCommand(string Email, int Age) : IRequest<Result<string>>;

public sealed class CreateUserHandler(IZetaValidator validator) : IRequestHandler<CreateUserCommand, Result<string>>
{
    private static readonly ISchema<CreateUserCommand> Schema = Z
        .Schema<CreateUserCommand>()
        .Property(x => x.Email, Z.String().Email())
        .Property(x => x.Age, Z.Int().Min(18));
    
    public async Task<Result<string>> Handle(CreateUserCommand cmd, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(cmd, Schema, ct);

        return validation.IsSuccess
            ? Result<string>.Success($"created user {cmd.Email}")
            : Result<string>.Failure(validation.Errors);
    }
}