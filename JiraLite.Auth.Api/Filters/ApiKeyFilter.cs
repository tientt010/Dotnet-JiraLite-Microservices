using System;
using System.Security.Cryptography;
using System.Text;

namespace JiraLite.Auth.Api.Filters;

public class ApiKeyFilter(IConfiguration configuration) : IEndpointFilter
{
    private readonly IConfiguration _configuration = configuration;
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var apiKey = context.HttpContext.Request.Headers["X-Internal-Api-Key"].FirstOrDefault();
        var configurationdApiKey = _configuration["ApiKeys:InternalApiKey"];

        if (string.IsNullOrEmpty(apiKey) || !FixedTimeEquals(apiKey, configurationdApiKey!))
        {
            return TypedResults.Unauthorized();
        }
        return await next(context);
    }
    private static bool FixedTimeEquals(string input, string secret)
    {
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var secretBytes = Encoding.UTF8.GetBytes(secret);
        return CryptographicOperations.FixedTimeEquals(inputBytes, secretBytes);
    }
}
