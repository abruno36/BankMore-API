namespace BankMore.Transferencia.API.Middleware
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private const string API_KEY_HEADER = "X-API-Key";
        private const string VALID_API_KEY = "BankMore-Transfer-2024-Secure";

        public ApiKeyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {

            if (!context.Request.Path.StartsWithSegments("/api/transferencia"))
            {
                await _next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue(API_KEY_HEADER, out var extractedApiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new
                {
                    Success = false,
                    Mensagem = "API Key não fornecida. Use o header X-API-Key"
                });
                return;
            }

            if (!VALID_API_KEY.Equals(extractedApiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new
                {
                    Success = false,
                    Mensagem = "API Key inválida"
                });
                return;
            }

            await _next(context);
        }
    }
}