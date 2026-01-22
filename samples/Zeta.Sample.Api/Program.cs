using Zeta;
using Zeta.AspNetCore;
using Zeta.Schemas;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
// Register context factory
builder.Services.AddScoped<IValidationContextFactory<User, UserContext>, UserContextFactory>();
// Register fake repo
builder.Services.AddScoped<IUserRepository, FakeUserRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment()) app.MapOpenApi();

// Define Schema using Context
var userSchema = Zeta.Zeta.Object<User, UserContext>()
    .Field(u => u.Email, Zeta.Zeta.String<UserContext>()
        .Email()
        .Refine((email, ctx) => !ctx.EmailExists, "Email already exists"))
    .Field(u => u.Name, Zeta.Zeta.String<UserContext>()
        .MinLength(3)
        .Refine((name, ctx) => !ctx.IsMaintenanceMode, "Cannot register during maintenance"));

app.MapPost("/users", (User user) => 
{
    return Results.Ok(new { Message = "User created", User = user });
})
.WithValidation(userSchema);

app.Run();

// Domain Objects
public record User(string Name, string Email);

// Validation Context
public record UserContext(bool EmailExists, bool IsMaintenanceMode);

// Factory
public class UserContextFactory : IValidationContextFactory<User, UserContext>
{
    private readonly IUserRepository _repo;
    public UserContextFactory(IUserRepository repo) => _repo = repo;

    public async Task<UserContext> CreateAsync(User input, IServiceProvider services, CancellationToken ct)
    {
        return new UserContext(
            EmailExists: await _repo.EmailExistsAsync(input.Email),
            IsMaintenanceMode: await _repo.IsMaintenanceModeAsync()
        );
    }
}

// Fake Services
public interface IUserRepository
{
    Task<bool> EmailExistsAsync(string email);
    Task<bool> IsMaintenanceModeAsync();
}

public class FakeUserRepository : IUserRepository
{
    public Task<bool> EmailExistsAsync(string email) => Task.FromResult(email == "taken@example.com");
    public Task<bool> IsMaintenanceModeAsync() => Task.FromResult(false);
}
