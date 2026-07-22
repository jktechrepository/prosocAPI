using System.Net;
using System.Text.Json;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Services;

namespace ProsocAPI.Middleware
{
    /// <summary>
    /// Intercepte uniquement les exceptions non gérées par les contrôleurs.
    /// Les catch existants (StatusCode 500 générique, etc.) ne sont pas modifiés.
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ErrorService errorService)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                if (context.Response.HasStarted)
                {
                    _logger.LogError(ex, "Exception après début de la réponse HTTP");
                    throw;
                }

                _logger.LogError(ex, "Exception non gérée sur {Method} {Path}", context.Request.Method, context.Request.Path);

                var errorResponse = errorService.CreateTechnicalError(
                    ErrorCodes.TECHNICAL_INTERNAL_ERROR,
                    "Une erreur technique est survenue",
                    ex);

                context.Response.Clear();
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, JsonOptions));
            }
        }
    }

    public static class GlobalExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
        {
            return app.UseMiddleware<GlobalExceptionMiddleware>();
        }
    }
}
