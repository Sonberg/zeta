using FastEndpoints;
using Zeta.Sample.FastEndpoints.Api.Repository;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFastEndpoints();

// Register fake repositories (replace with real implementations)
builder.Services.AddScoped<IUserRepository, FakeUserRepository>();

var app = builder.Build();

app.UseFastEndpoints();

app.Run();

// Required for WebApplicationFactory in integration tests
public partial class Program;
