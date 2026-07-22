using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProsocAPI.Models.Configuration;

namespace ProsocAPI.Services
{
    /// <summary>Planifie la retenue MAASH mensuelle pour tous les agents éligibles.</summary>
    public class AgentMaashRetenueBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AgentMaashRetenueBackgroundService> _logger;
        private DateTime? _derniereExecution;

        public AgentMaashRetenueBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<AgentMaashRetenueBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                AgentMaashOptions options;
                try
                {
                    await using var configScope = _scopeFactory.CreateAsyncScope();
                    var provider = configScope.ServiceProvider.GetRequiredService<IParametresMetierProvider>();
                    options = await provider.GetAgentMaashAsync(stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Impossible de charger la configuration AgentMaash");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                    continue;
                }

                if (!options.RetenueAutomatiqueActivee)
                {
                    _logger.LogInformation(
                        "Retenue MAASH automatique désactivée (AgentMaash:RetenueAutomatiqueActivee = false).");
                    return;
                }

                var intervalle = TimeSpan.FromMinutes(Math.Max(1, options.IntervalleControleMinutes));

                try
                {
                    await VerifierEtExecuterAsync(options, stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Erreur dans le planificateur retenue MAASH");
                }

                await Task.Delay(intervalle, stoppingToken);
            }
        }

        private async Task VerifierEtExecuterAsync(AgentMaashOptions options, CancellationToken ct)
        {
            if (_derniereExecution?.Date == DateTime.Today)
                return;

            await using var scope = _scopeFactory.CreateAsyncScope();
            var service = scope.ServiceProvider.GetRequiredService<IAgentMaashRetenueService>();

            if (!await service.DoitExecuterRetenueAutomatiqueAsync(ct))
                return;

            _logger.LogInformation(
                "Lancement de la retenue MAASH automatique (jour {Jour}, heure ≥ {Heure}h)…",
                options.JourExecution,
                options.HeureExecution);
            await service.ExecuterRetenueAutomatiqueAsync(ct);
            _derniereExecution = DateTime.Now;
        }
    }
}
