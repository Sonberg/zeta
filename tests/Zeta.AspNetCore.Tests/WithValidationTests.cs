using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Xunit;
using Zeta.Schemas;

namespace Zeta.AspNetCore.Tests;

public class WithValidationTests
{
    private record Widget(string Name);
    private record WidgetContext(bool AllowAnyName);

    private static WebApplication BuildApp()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddZeta();

        var contextlessSchema = Z.Object<Widget>()
            .Field(w => w.Name, s => s.MinLength(3));

        // Declared as ISchema<T, TContext> so the call site below resolves to the
        // WithValidation<T, TContext>(ISchema<T, TContext>) overload rather than the
        // IContextSchema<T, TContext> disambiguation overload (which wins by betterness
        // when the argument's static type is the concrete, IContextSchema-implementing class).
        ISchema<Widget, WidgetContext> contextAwareSchema = Z.Object<Widget>()
            .Field(w => w.Name, s => s.MinLength(3))
            .Using<WidgetContext>(async (_, _, _) =>
            {
                await Task.Yield();
                return new WidgetContext(false);
            })
            .Refine((widget, ctx) => ctx.AllowAnyName || widget.Name != "Forbidden", "Name is forbidden");

        ObjectContextSchema<Widget, WidgetContext> disambiguatedSchema = Z.Object<Widget>()
            .Field(w => w.Name, s => s.MinLength(3))
            .Using<WidgetContext>(async (_, _, _) =>
            {
                await Task.Yield();
                return new WidgetContext(false);
            })
            .Refine((widget, ctx) => ctx.AllowAnyName || widget.Name != "Forbidden", "Name is forbidden");

        var app = builder.Build();

        app.MapPost("/contextless", (Widget widget) => Results.Ok(widget))
            .WithValidation(contextlessSchema);

        app.MapPost("/context-aware", (Widget widget) => Results.Ok(widget))
            .WithValidation(contextAwareSchema);

        app.MapPost("/context-aware-disambiguated", (Widget widget) => Results.Ok(widget))
            .WithValidation((IContextSchema<Widget, WidgetContext>)disambiguatedSchema);

        return app;
    }

    [Fact]
    public async Task WithValidation_ContextlessSchema_ValidRequest_ReturnsOk()
    {
        await using var app = BuildApp();
        await app.StartAsync();
        var client = app.GetTestClient();

        var response = await client.PostAsJsonAsync("/contextless", new Widget("Gadget"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task WithValidation_ContextlessSchema_InvalidRequest_ReturnsValidationProblem()
    {
        await using var app = BuildApp();
        await app.StartAsync();
        var client = app.GetTestClient();

        var response = await client.PostAsJsonAsync("/contextless", new Widget("Ga"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task WithValidation_ContextAwareSchema_ValidRequest_ReturnsOk()
    {
        await using var app = BuildApp();
        await app.StartAsync();
        var client = app.GetTestClient();

        var response = await client.PostAsJsonAsync("/context-aware", new Widget("Gadget"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task WithValidation_ContextAwareSchema_ContextRefinementFails_ReturnsValidationProblem()
    {
        await using var app = BuildApp();
        await app.StartAsync();
        var client = app.GetTestClient();

        var response = await client.PostAsJsonAsync("/context-aware", new Widget("Forbidden"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task WithValidation_ContextSchemaOverload_ValidRequest_ReturnsOk()
    {
        await using var app = BuildApp();
        await app.StartAsync();
        var client = app.GetTestClient();

        var response = await client.PostAsJsonAsync("/context-aware-disambiguated", new Widget("Gadget"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task WithValidation_ContextSchemaOverload_ContextRefinementFails_ReturnsValidationProblem()
    {
        await using var app = BuildApp();
        await app.StartAsync();
        var client = app.GetTestClient();

        var response = await client.PostAsJsonAsync("/context-aware-disambiguated", new Widget("Forbidden"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
