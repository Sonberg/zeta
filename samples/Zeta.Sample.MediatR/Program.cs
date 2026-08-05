using System.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Zeta;
using Zeta.Sample.MediatR;

var services = new ServiceCollection();

services.AddScoped<IZetaValidator, ZetaValidator>(); // core Zeta — no ASP.NET reference needed
services.AddSingleton<IOrderRepo, InMemoryOrderRepo>();
services.AddMediatR(cfg => { cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()); });

await using var provider = services.BuildServiceProvider();
var mediator = provider.GetRequiredService<IMediator>();

await Run("valid user", new CreateUserCommand("ada@example.com", 30));
await Run("invalid user", new CreateUserCommand("not-an-email", 12));
await Run("update existing order", new UpdateOrderCommand(OrderId: 42, Note: "ship fast"));
await Run("update missing order", new UpdateOrderCommand(OrderId: 99, Note: "ship fast"));
return;

async Task Run<TResult>(string label, IRequest<Result<TResult>> request)
{
    var result = await mediator.Send(request);
    
    Console.WriteLine(result.IsSuccess
        ? $"[OK]   {label}: {result.Value}"
        : $"[FAIL] {label}: {string.Join("; ", result.Errors.Select(e => $"{e.PathString} {e.Code} \"{e.Message}\""))}");
}
