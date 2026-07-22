using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Threading.Tasks;

namespace ProsocAPI.Middleware
{
    /// <summary>
    /// Middleware pour gérer les tokens avec ou sans préfixe "Bearer"
    /// </summary>
    public class FlexibleTokenMiddleware
    {
        private readonly RequestDelegate _next;

        public FlexibleTokenMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Vérifier si c'est une requête avec Authorization header
            if (context.Request.Headers.ContainsKey("Authorization"))
            {
                var authHeader = context.Request.Headers["Authorization"].ToString();
                
                // Si le token n'a pas "Bearer ", l'ajouter automatiquement
                if (!string.IsNullOrWhiteSpace(authHeader) && !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    // Remplacer le header avec le préfixe Bearer ajouté
                    context.Request.Headers["Authorization"] = $"Bearer {authHeader}";
                }
            }

            await _next(context);
        }
    }

    /// <summary>
    /// Extension method pour enregistrer le middleware
    /// </summary>
    public static class FlexibleTokenMiddlewareExtensions
    {
        public static IApplicationBuilder UseFlexibleToken(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<FlexibleTokenMiddleware>();
        }
    }
}
