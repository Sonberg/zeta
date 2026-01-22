using Zeta;
using Zeta.AspNetCore;
using Zeta.Schemas;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Define schema
var userSchema = Zeta.Zeta.Object<User>()
    .Field(u => u.Name, Zeta.Zeta.String().MinLength(3).NotEmpty())
    .Field(u => u.Email, Zeta.Zeta.String().Email())
    .Field(u => u.Age, Zeta.Zeta.Int().Min(18));

// Use it in an endpoint
app.MapPost("/users", (User user) => 
{
    return Results.Ok(new { Message = "User created", User = user });
})
.WithValidation(userSchema);

app.Run();

public record User(string Name, string Email, int Age);
