namespace Identity.API.Filters;

public class ApiKeyFilter(IConfiguration configuration) : IEndpointFilter
{
    private const string ApiKeyHeader = "X-Api-Key";

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeader, out var providedKey))
        {
            return Results.Unauthorized();
        }

        var validKey = configuration["ApiKeys:InternalApiKey"];
        if (string.IsNullOrEmpty(validKey) || !string.Equals(providedKey, validKey, StringComparison.Ordinal))
        {
            return Results.Unauthorized();
        }

        return await next(context);
    }
}
