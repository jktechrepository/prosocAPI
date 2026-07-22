using Microsoft.Extensions.Logging;
using ProsocAPI.Services;

namespace ProsocAPI.Middleware
{
    public class GeographicDataInitializationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GeographicDataInitializationMiddleware> _logger;
        private readonly IServiceProvider _serviceProvider;

        public GeographicDataInitializationMiddleware(
            RequestDelegate next, 
            ILogger<GeographicDataInitializationMiddleware> logger,
            IServiceProvider serviceProvider)
        {
            _next = next;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Exécuter la mise à jour géographique une seule fois au démarrage
            await EnsureGeographicDataInitializedAsync();
            
            await _next(context);
        }

        private async Task EnsureGeographicDataInitializedAsync()
        {
            // Utiliser un scope pour le service
            using var scope = _serviceProvider.CreateScope();
            var geographicDataService = scope.ServiceProvider.GetRequiredService<IGeographicDataService>();

            try
            {
                _logger.LogInformation("Initialisation des données géographiques au démarrage de l'API...");
                await geographicDataService.EnsureProvincesAndCommunesAsync();
                _logger.LogInformation("Initialisation des données géographiques terminée avec succès");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'initialisation des données géographiques");
                // Ne pas bloquer le démarrage de l'API en cas d'erreur
            }
        }
    }
}
