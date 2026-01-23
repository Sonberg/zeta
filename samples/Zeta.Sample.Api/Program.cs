using Zeta;
using Zeta.AspNetCore;
using Zeta.Sample.Api.Models;
using Zeta.Sample.Api.Repository;
using Zeta.Sample.Api.Validation;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

// Register Zeta services (includes IZetaValidator) and scan for context factories
builder.Services.AddZeta(typeof(Program).Assembly);

builder.Services.AddControllers();

// Register fake repo
builder.Services.AddScoped<IUserRepository, FakeUserRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment()) app.MapOpenApi();

// Define Schema using Context
var userAsyncSchema = Z.Object<User, UserContext>()
    .Field(u => u.Email, Z.String<UserContext>()
        .Email()
        .Refine((email, ctx) => !ctx.EmailExists, "Email already exists"))
    .Field(u => u.Name, Z.String<UserContext>()
        .MinLength(3)
        .Refine((name, ctx) => !ctx.IsMaintenanceMode, "Cannot register during maintenance"));


var userSyncSchema = Z.Object<User>()
    .Field(u => u.Email, Z.String().Email())
    .Field(u => u.Name, Z.String().MinLength(3));

var addressSchema = Z.Object<Address>()
    .Field(a => a.Street, Z.String().MinLength(5))
    .Field(a => a.City, Z.String().MinLength(2))
    .Field(a => a.ZipCode, Z.String().Regex(@"^\d{5}(-\d{4})?$"));

var userWithAddressSchema = Z.Object<UserWithAddress>()
    .Field(u => u.Email, Z.String().Email())
    .Field(u => u.Name, Z.String().MinLength(3))
    .Field(u => u.Address, addressSchema);

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


public partial class Program
{
}