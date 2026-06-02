namespace FacturacionMedica.Api.Middleware;

public class ApiKeyMiddleware
{
    private const string ApiKeyHeader = "X-Api-Key";

    private static readonly PathString[] ExemptPaths =
    [
        new("/openapi"),
        new("/scalar")
    ];

    private readonly RequestDelegate _next;
    private readonly string _apiKey;

    public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _apiKey = configuration["ApiKey:Value"]
            ?? throw new InvalidOperationException("La configuración 'ApiKey:Value' no está definida.");
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (IsExemptPath(context.Request.Path))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(ApiKeyHeader, out var providedKey) ||
            !string.Equals(providedKey, _apiKey, StringComparison.Ordinal))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new
            {
                message = "API Key inválida o no proporcionada."
            });
            return;
        }

        await _next(context);
    }

    private static bool IsExemptPath(PathString requestPath)
        => ExemptPaths.Any(p => requestPath.StartsWithSegments(p));
}
