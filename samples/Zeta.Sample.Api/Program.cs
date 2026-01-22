using Zeta;
using Zeta.AspNetCore;
using Zeta.Schemas;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
// Register Zeta and scan for factories
builder.Services.AddZeta(typeof(Program).Assembly)
                .AddZetaControllers();

builder.Services.AddControllers();

// Register Schema for Implicit Validation (Controllers)
builder.Services.AddSingleton<ISchema<User>>(Zeta.Zeta.Object<User>()
    .Field(u => u.Name, Zeta.Zeta.String().MinLength(3))
    .Field(u => u.Email, Zeta.Zeta.String().Email()));

// Register fake repo
builder.Services.AddScoped<IUserRepository, FakeUserRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment()) app.MapOpenApi();

// Define Schema using Context
var userAsyncSchema = Zeta.Zeta.Object<User, UserContext>()
    .Field(u => u.Email, Zeta.Zeta.String<UserContext>()
        .Email()
        .Refine((email, ctx) => !ctx.EmailExists, "Email already exists"))
    .Field(u => u.Name, Zeta.Zeta.String<UserContext>()
        .MinLength(3)
        .Refine((name, ctx) => !ctx.IsMaintenanceMode, "Cannot register during maintenance"));


var userSyncSchema = Zeta.Zeta.Object<User>()
    .Field(u => u.Email, Zeta.Zeta.String().Email())
    .Field(u => u.Name, Zeta.Zeta.String().MinLength(3));

app.MapPost("/async/users", (User user) => Results.Ok(new
    {
        Message = "User created",
        User = user
    }))
    .WithValidation(userAsyncSchema);

app.MapPost("/sync/users", (User user) => Results.Ok(new
    {
        Message = "User created",
        User = user
    }))
    .WithValidation(userSyncSchema);
    
app.MapControllers();

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

public partial class Program { }