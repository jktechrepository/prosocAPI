using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProsocAPI.Models.Configuration;

namespace ProsocAPI.Services
{
    /// <summary>Planifie la génération quotidienne des arriérés affilié.</summary>
    public class ArrieresGenerationBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ArrieresGenerationBackgroundService> _logger;
        private DateTime? _derniereExecution;

        public ArrieresGenerationBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<ArrieresGenerationBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                ArrieresOptions options;
                try
                {
                    await using var configScope = _scopeFactory.CreateAsyncScope();
                    var provider = configScope.ServiceProvider.GetRequiredService<IParametresMetierProvider>();
                    options = await provider.GetArrieresAsync(stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Impossible de charger la configuration Arrieres");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                    continue;
                }

                if (!options.GenerationAutomatiqueActivee)
                {
                    _logger.LogInformation("Génération automatique des arriérés désactivée.");
                    return;
                }

                var intervalle = TimeSpan.FromMinutes(Math.Max(1, options.IntervalleControleMinutes));

                try
                {
                    await VerifierEtExecuterAsync(stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Erreur dans le planificateur arriérés");
                }

                await Task.Delay(intervalle, stoppingToken);
            }
        }

        private async Task VerifierEtExecuterAsync(CancellationToken ct)
        {
            if (_derniereExecution?.Date == DateTime.Today)
                return;

            await using var scope = _scopeFactory.CreateAsyncScope();
            var service = scope.ServiceProvider.GetRequiredService<IArrieresAffilieService>();

            if (!await service.DoitExecuterGenerationAutomatiqueAsync(ct))
                return;

            _logger.LogInformation("Lancement de la génération automatique des arriérés…");
            await service.ExecuterGenerationAutomatiqueAsync(ct);

            var penaliteService = scope.ServiceProvider.GetRequiredService<IPenaliteAffilieService>();
            await penaliteService.AppliquerPenalitesRetardCotisationAsync(DateTime.Today, ct);

            _derniereExecution = DateTime.Now;
        }
    }
}
