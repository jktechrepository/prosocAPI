using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Services;
using Prosoc.Utilities;

namespace ProsocAPI.Services.Repositories
{
    public class DashboardChefEquipeService : IDashboardChefEquipeRepository
    {
        private readonly ProsocDbContext _db;
        private readonly IDashboardAgentRepository _dashboardAgent;
        private readonly ILogger<DashboardChefEquipeService> _logger;

        public DashboardChefEquipeService(
            ProsocDbContext db,
            IDashboardAgentRepository dashboardAgent,
            ILogger<DashboardChefEquipeService> logger)
        {
            _db = db;
            _dashboardAgent = dashboardAgent;
            _logger = logger;
        }

        public async Task<ChefEquipeKpisDto> GetKpisAsync(int chefAgentId, CancellationToken ct = default)
        {
            var zoneId = await ChefEquipeZoneScopeHelper.GetZoneSocialeIdAsync(_db, chefAgentId, ct);
            if (zoneId is null or <= 0)
            {
                return new ChefEquipeKpisDto();
            }

            var zoneNom = await _db.ZonesSociales.AsNoTracking()
                .Where(z => z.IdZoneSociale == zoneId)
                .Select(z => z.Nom)
                .FirstOrDefaultAsync(ct);

            var agentIds = await ChefEquipeZoneScopeHelper.GetAgentIdsAtDansZoneAsync(_db, chefAgentId, ct);
            var debutMois = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var finMois = debutMois.AddMonths(1).AddTicks(-1);

            var collectesMois = agentIds.Count == 0
                ? new List<Collecte>()
                : await _db.Collectes.AsNoTracking()
                    .Where(c => c.AgentId.HasValue
                        && agentIds.Contains(c.AgentId.Value)
                        && c.Statut
                        && c.DateCollecte >= debutMois
                        && c.DateCollecte <= finMois)
                    .ToListAsync(ct);

            var deviseCode = await _db.Devises.AsNoTracking()
                .Where(d => d.EstDevisePrincipale && d.Statut)
                .Select(d => d.Code)
                .FirstOrDefaultAsync(ct);

            return new ChefEquipeKpisDto
            {
                ZoneSocialeId = zoneId.Value,
                ZoneSocialeNom = zoneNom,
                NombreAgentsAt = agentIds.Count,
                CollectesMoisZone = collectesMois.Count,
                TotalCollectesMoisZone = DashboardDeviseConsolidation.SommerCollectesEnDevisePrincipale(collectesMois),
                DevisePrincipaleCode = deviseCode,
                CollectesEnAttenteZone = collectesMois.Count(c =>
                    CollecteStatutPaiementRegles.EstEnAttente(c.StatutPaiement)),
                TransactionsValidesMoisZone = collectesMois.Count(c =>
                    CollecteStatutPaiementRegles.EstValide(c.StatutPaiement))
            };
        }

        public async Task<List<ChefEquipeAgentResumeDto>> GetAgentsZoneAsync(
            int chefAgentId,
            CancellationToken ct = default)
        {
            var agentIds = await ChefEquipeZoneScopeHelper.GetAgentIdsAtDansZoneAsync(_db, chefAgentId, ct);
            if (agentIds.Count == 0)
                return new List<ChefEquipeAgentResumeDto>();

            var debutMois = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var finMois = debutMois.AddMonths(1).AddTicks(-1);

            var agents = await _db.Agents.AsNoTracking()
                .Where(a => agentIds.Contains(a.IdAgent))
                .ToListAsync(ct);

            var collectesMois = await _db.Collectes.AsNoTracking()
                .Where(c => c.AgentId.HasValue
                    && agentIds.Contains(c.AgentId.Value)
                    && c.Statut
                    && c.DateCollecte >= debutMois
                    && c.DateCollecte <= finMois)
                .ToListAsync(ct);

            var parAgent = collectesMois
                .Where(c => c.AgentId.HasValue)
                .GroupBy(c => c.AgentId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            return agents
                .OrderBy(a => a.NomComplet)
                .Select(a =>
                {
                    parAgent.TryGetValue(a.IdAgent, out var collectes);
                    collectes ??= new List<Collecte>();
                    return new ChefEquipeAgentResumeDto
                    {
                        AgentId = a.IdAgent,
                        NomComplet = a.NomComplet ?? string.Empty,
                        Matricule = a.Matricule ?? string.Empty,
                        Telephone = a.Phone,
                        Statut = a.Statut,
                        CollectesMois = collectes.Count,
                        TotalCollectesMois = DashboardDeviseConsolidation.SommerCollectesEnDevisePrincipale(collectes),
                        CollectesEnAttente = collectes.Count(c =>
                            CollecteStatutPaiementRegles.EstEnAttente(c.StatutPaiement))
                    };
                })
                .ToList();
        }

        public async Task<AgentCommissionsResumeDto> GetMouvementsWalletAgentAsync(
            int chefAgentId,
            int targetAgentId,
            int limit,
            CancellationToken ct = default)
        {
            await EnsureAgentDansZoneAsync(chefAgentId, targetAgentId, ct);
            if (limit <= 0) limit = 20;
            return await _dashboardAgent.GetCommissionsResumeAsync(targetAgentId, limit, ct);
        }

        public async Task<List<ChefEquipeCollecteResumeDto>> GetCollectesAgentAsync(
            int chefAgentId,
            int targetAgentId,
            int limit,
            CancellationToken ct = default)
        {
            await EnsureAgentDansZoneAsync(chefAgentId, targetAgentId, ct);
            if (limit <= 0) limit = 50;

            var collectes = await _db.Collectes.AsNoTracking()
                .Include(c => c.Affilie)
                .Include(c => c.Agent)
                .Where(c => c.AgentId == targetAgentId && c.Statut)
                .OrderByDescending(c => c.DateCollecte)
                .Take(limit)
                .ToListAsync(ct);

            return collectes.Select(c => new ChefEquipeCollecteResumeDto
            {
                IdCollecte = c.IdCollecte,
                AgentId = c.AgentId ?? 0,
                AgentNom = c.Agent?.NomComplet,
                AffilieNom = c.Affilie?.NomComplet,
                Montant = CollecteStatutPaiementRegles.MontantEnDevisePrincipale(c),
                StatutPaiement = c.StatutPaiement,
                ModePaiement = c.ModePaiement,
                DateCollecte = c.DateCollecte
            }).ToList();
        }

        private async Task EnsureAgentDansZoneAsync(int chefAgentId, int targetAgentId, CancellationToken ct)
        {
            if (targetAgentId == chefAgentId)
                return;

            var zoneId = await ChefEquipeZoneScopeHelper.GetZoneSocialeIdAsync(_db, chefAgentId, ct);
            if (zoneId is null or <= 0)
            {
                throw new UnauthorizedAccessException(
                    "Aucune zone sociale affectée à votre fiche agent.");
            }

            var dansZone = await _db.Agents.AsNoTracking()
                .AnyAsync(a => a.IdAgent == targetAgentId
                    && a.ZoneSocialeId == zoneId
                    && a.Statut, ct);

            if (!dansZone)
            {
                throw new UnauthorizedAccessException(
                    "Cet agent n'appartient pas à votre zone.");
            }
        }
    }
}
