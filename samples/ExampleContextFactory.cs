// See https://aka.ms/new-console-template for more information

using Microsoft.AspNetCore.Mvc;
using Zeta;
using Zeta.AspNetCore;

Console.WriteLine("Hello, World!");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddZeta();
builder.Services.AddScoped<IUserRepository, FakeUserRepository>();
builder.Services.AddScoped<IFeatureFlags, FakeFeatureFlags>();

var schema = Z.Object<UserRegistrationRequest>()
    .Using<UserRegistrationContext>(async (value, sp, ct) =>
    {
        var userRepository = sp.GetRequiredService<IUserRepository>();
        var featureFlags = sp.GetRequiredService<IFeatureFlags>();

        var allowCreationTask = featureFlags.IsEnabledAsync("AllowUserCreation", ct);
        var userExistsTask = userRepository.UserExistsAsync(value.Email, ct);

        await Task.WhenAll(allowCreationTask, userExistsTask);

        return new UserRegistrationContext(allowCreationTask.Result, userExistsTask.Result);
    })
    .Field(x => x.FirstName, x => x.MinLength(2).MaxLength(50).Alphanumeric())
    .Field(x => x.LastName, x => x.MinLength(2).Alphanumeric())
    .Field(x => x.Email, x => x.Email())
    .Field(x => x.Password, x => x.MinLength(8).MaxLength(100).Regex("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d).+$", "Password must contain at least one uppercase letter, one lowercase letter, and one digit."))
    .Refine(x => x.Password == x.RepeatedPassword, "Passwords do not match.", "passwords_do_not_match")
    .Refine((_, ctx) => !ctx.AllowCreation, "User creation is not allowed.", "user_creation_not_allowed")
    .Refine((_, ctx) => ctx.Exists, "User with this email already exists.", "user_already_exists");

var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app
    .MapPost("/users/registration", async (
            [FromServices] IUserRepository repository,
            [FromBody] UserRegistrationRequest request,
            CancellationToken cancellationToken) => await repository.CreateAsync(request, cancellationToken)
    )
    .WithValidation(schema);

app.Run();

// Record representing the input
public record UserRegistrationRequest(string FirstName, string LastName, string Email, string Password, string RepeatedPassword);

// Injected into validation
public record UserRegistrationContext(bool AllowCreation, bool Exists);


// Fakes
public interface IUserRepository
{
    Task<bool> UserExistsAsync(string email, CancellationToken cancellationToken);
    Task<bool> CreateAsync(UserRegistrationRequest request, CancellationToken cancellationToken);
}

public interface IFeatureFlags
{
    Task<bool> IsEnabledAsync(string featureName, CancellationToken cancellationToken);
}

public class FakeUserRepository : IUserRepository
{
    public Task<bool> UserExistsAsync(string email, CancellationToken cancellationToken)
    {
        // Simulate that the user does not exist
        return Task.FromResult(false);
    }

    public Task<bool> CreateAsync(UserRegistrationRequest request, CancellationToken cancellationToken)
    {
        // Simulate user creation
        return Task.FromResult(true);
    }
}


public class FakeFeatureFlags : IFeatureFlags
{
    public Task<bool> IsEnabledAsync(string featureName, CancellationToken cancellationToken)
    {
        // Simulate that all features are enabled
        return Task.FromResult(true);
    }
}
