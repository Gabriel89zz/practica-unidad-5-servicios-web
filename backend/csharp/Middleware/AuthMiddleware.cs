using System.Text.Json;

namespace FacturacionApi.Middleware
{
    public class RestAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private const string ValidBearer = "Bearer test_token_2026";
        private const string ValidApiKey = "secret_key_cs";

        public RestAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Omitir autenticación para swagger, wsdl y healthcheck
            var path = context.Request.Path.Value ?? "";
            if (context.Request.Method == "OPTIONS" ||
                !path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            var authHeader = context.Request.Headers["Authorization"].ToString();
            var apiKeyHeader = context.Request.Headers["X-API-Key"].ToString();

            bool authorized = (authHeader == ValidBearer) || (apiKeyHeader == ValidApiKey);

            if (!authorized)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                var err = new
                {
                    error = "Unauthorized",
                    message = "Token Bearer o API-Key inválida o ausente. Use 'Authorization: Bearer test_token_2026' o 'X-API-Key: secret_key_cs'",
                    statusCode = 401,
                    timestamp = DateTime.UtcNow.ToString("o")
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(err));
                return;
            }

            await _next(context);
        }
    }
}
