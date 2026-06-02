using FacturacionMedica.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace FacturacionMedica.Test.Middleware;

public class ApiKeyMiddlewareTests
{
    private const string ValidApiKey = "test-api-key";

    private static ApiKeyMiddleware BuildMiddleware(RequestDelegate next)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ApiKey:Value", ValidApiKey }
            })
            .Build();

        return new ApiKeyMiddleware(next, config);
    }

    private static DefaultHttpContext BuildContext(string? apiKey = null)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        if (apiKey is not null)
            context.Request.Headers["X-Api-Key"] = apiKey;

        return context;
    }

    [Fact]
    public async Task InvokeAsync_ApiKeyValida_LlamaSiguienteMiddleware()
    {
        var nextCalled = false;
        var middleware = BuildMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });
        var context = BuildContext(ValidApiKey);

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_SinApiKey_Retorna401()
    {
        var middleware = BuildMiddleware(_ => Task.CompletedTask);
        var context = BuildContext(); // sin header

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_ApiKeyIncorrecta_Retorna401()
    {
        var middleware = BuildMiddleware(_ => Task.CompletedTask);
        var context = BuildContext("clave-incorrecta");

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_ApiKeyValida_NoAlteraStatusCode()
    {
        var middleware = BuildMiddleware(_ => Task.CompletedTask);
        var context = BuildContext(ValidApiKey);

        await middleware.InvokeAsync(context);

        // El status no debe ser 401
        Assert.NotEqual(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_SinApiKey_NoLlamaSiguienteMiddleware()
    {
        var nextCalled = false;
        var middleware = BuildMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });
        var context = BuildContext(); // sin header

        await middleware.InvokeAsync(context);

        Assert.False(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_ApiKeyIncorrecta_NoLlamaSiguienteMiddleware()
    {
        var nextCalled = false;
        var middleware = BuildMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });
        var context = BuildContext("wrong-key");

        await middleware.InvokeAsync(context);

        Assert.False(nextCalled);
    }

    [Fact]
    public async Task Constructor_SinConfiguracionDeApiKey_LanzaExcepcion()
    {
        var emptyConfig = new ConfigurationBuilder().Build();

        Assert.Throws<InvalidOperationException>(() =>
            new ApiKeyMiddleware(_ => Task.CompletedTask, emptyConfig));
    }
}
