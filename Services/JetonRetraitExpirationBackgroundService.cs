using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProsocAPI.Models.Configuration;
using ProsocAPI.Services.Repositories;

namespace ProsocAPI.Services
{
    /// <summary>Expire périodiquement les jetons de retrait périmés (libération solde + rejet demande).</summary>
    public class JetonRetraitExpirationBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<JetonRetraitExpirationBackgroundService> _logger;

        public JetonRetraitExpirationBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<JetonRetraitExpirationBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                RetraitAgentOptions options;
                try
                {
                    await using var configScope = _scopeFactory.CreateAsyncScope();
                    var provider = configScope.ServiceProvider.GetRequiredService<IParametresMetierProvider>();
                    options = await provider.GetRetraitAgentAsync(stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Impossible de charger la configuration RetraitAgent");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                    continue;
                }

                var intervalle = TimeSpan.FromMinutes(Math.Max(1, options.IntervalleExpirationMinutes));

                if (!options.ExpirationAutomatiqueActivee)
                {
                    _logger.LogDebug(
                        "Expiration automatique des jetons retrait désactivée — prochain contrôle dans {Minutes} min",
                        intervalle.TotalMinutes);
                    await Task.Delay(intervalle, stoppingToken);
                    continue;
                }

                try
                {
                    await using var scope = _scopeFactory.CreateAsyncScope();
                    var service = scope.ServiceProvider.GetRequiredService<IDemandeRetraitAgentRepository>();
                    var count = await service.ExpireJetonsExpiresAsync(stoppingToken);
                    if (count > 0)
                    {
                        _logger.LogInformation(
                            "Job expiration jetons retrait : {Count} traité(s)",
                            count);
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Erreur dans le job d'expiration des jetons de retrait");
                }

                await Task.Delay(intervalle, stoppingToken);
            }
        }
    }
}
