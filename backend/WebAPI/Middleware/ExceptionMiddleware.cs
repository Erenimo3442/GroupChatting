using System.Net;
using System.Text.Json;
using Application.Exceptions;

namespace WebAPI.Middleware
{
    public class ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
        IHostEnvironment env
    )
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<ExceptionMiddleware> _logger = logger;
        private readonly IHostEnvironment _env = env;
        private readonly JsonSerializerOptions _options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception has occurred.");
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = ex switch
                {
                    NotFoundException => (int)HttpStatusCode.NotFound,
                    ForbiddenAccessException => (int)HttpStatusCode.Forbidden,
                    UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                    _ => (int)HttpStatusCode.InternalServerError,
                };
                var response = _env.IsDevelopment()
                    ? new { message = ex.Message, details = ex.StackTrace?.ToString() }
                    : new
                    {
                        message = "An internal server error has occurred.",
                        details = (string?)"Please try again later.",
                    };

                var json = JsonSerializer.Serialize(response, _options);

                await context.Response.WriteAsync(json);
            }
        }
    }
}
