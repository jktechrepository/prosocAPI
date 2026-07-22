using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;

namespace ProsocAPI.Services
{
    public class CommissionDashboardService : ICommissionDashboardService
    {
        private readonly ProsocDbContext _db;
        private readonly ILogger<CommissionDashboardService> _logger;

        public CommissionDashboardService(ProsocDbContext db, ILogger<CommissionDashboardService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<CommissionDashboardDto> GetDashboardAsync(int agentId)
        {
            try
            {
                _logger.LogInformation("Récupération du dashboard des commissions pour l'agent {AgentId}", agentId);

                var maintenant = DateTime.Now;
                var debutMois = new DateTime(maintenant.Year, maintenant.Month, 1);
                var debutSemaine = maintenant.AddDays(-(int)maintenant.DayOfWeek);
                var debutJour = maintenant.Date;

                // Récupérer le wallet de l'agent
                var wallet = await _db.WalletsAgents
                    .FirstOrDefaultAsync(w => w.AgentId == agentId);

                var devise = "USD"; // TODO: Récupérer depuis les préférences ou configuration

                // Récupérer toutes les commissions de l'agent
                var commissionsQuery = _db.WalletMouvements
                    .Where(m => m.Wallet.AgentId == agentId && 
                               m.Source.Contains("COMMISSION") && 
                               m.TypeOperation == "CREDIT");

                var commissions = await commissionsQuery.ToListAsync();

                // Calculer les statistiques générales
                var totalCommissions = commissions.Sum(m => m.Montant);
                var nombreCommissions = commissions.Count;
                var commissionMoyenne = nombreCommissions > 0 ? totalCommissions / nombreCommissions : 0;
                var commissionMax = commissions.Any() ? commissions.Max(m => m.Montant) : 0;
                var commissionMin = commissions.Any() ? commissions.Min(m => m.Montant) : 0;

                // Calculer les statistiques par période
                var ceMois = commissions.Where(m => m.DateOperation >= debutMois).Sum(m => m.Montant);
                var cetteSemaine = commissions.Where(m => m.DateOperation >= debutSemaine).Sum(m => m.Montant);
                var aujourdHui = commissions.Where(m => m.DateOperation.Date == debutJour).Sum(m => m.Montant);

                var dashboard = new CommissionDashboardDto
                {
                    TotalCommissions = totalCommissions,
                    SoldeActuel = wallet?.SoldeCourant ?? 0,
                    NombreCommissions = nombreCommissions,
                    CommissionMoyenne = commissionMoyenne,
                    CommissionMax = commissionMax,
                    CommissionMin = commissionMin,
                    CeMois = ceMois,
                    LaSemaine = cetteSemaine,
                    Aujourdhui = aujourdHui,
                    Devise = devise,
                    DerniereMiseAJour = maintenant
                };

                _logger.LogInformation("Dashboard récupéré avec succès pour l'agent {AgentId}", agentId);
                return dashboard;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du dashboard pour l'agent {AgentId}", agentId);
                throw;
            }
        }

        public async Task<(List<CommissionItemDto> Commissions, int Total)> GetCommissionsAsync(int agentId, CommissionFilterDto filter)
        {
            try
            {
                _logger.LogInformation("Récupération des commissions pour l'agent {AgentId} avec filtres", agentId);

                var query = _db.WalletMouvements
                    .Where(m => m.Wallet.AgentId == agentId && 
                               m.Source.Contains("COMMISSION") && 
                               m.TypeOperation == "CREDIT");

                // Appliquer les filtres
                if (filter.DateDebut.HasValue)
                    query = query.Where(m => m.DateOperation >= filter.DateDebut.Value);

                if (filter.DateFin.HasValue)
                    query = query.Where(m => m.DateOperation <= filter.DateFin.Value);

                if (filter.MontantMin.HasValue)
                    query = query.Where(m => m.Montant >= filter.MontantMin.Value);

                if (filter.MontantMax.HasValue)
                    query = query.Where(m => m.Montant <= filter.MontantMax.Value);

                if (!string.IsNullOrEmpty(filter.Source))
                    query = query.Where(m => m.Source.Contains(filter.Source));

                // Compter le total avant pagination
                var total = await query.CountAsync();

                // Appliquer le tri
                query = filter.TriPar?.ToLower() switch
                {
                    "montant" => filter.OrdreTri?.ToLower() == "asc" 
                        ? query.OrderBy(m => m.Montant)
                        : query.OrderByDescending(m => m.Montant),
                    _ => filter.OrdreTri?.ToLower() == "asc"
                        ? query.OrderBy(m => m.DateOperation)
                        : query.OrderByDescending(m => m.DateOperation)
                };

                // Appliquer la pagination
                var commissions = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                // Mapper vers les DTOs
                var commissionsDto = new List<CommissionItemDto>();
                foreach (var commission in commissions)
                {
                    var dto = await MapToCommissionItemDto(commission);
                    commissionsDto.Add(dto);
                }

                _logger.LogInformation("Récupéré {Count} commissions sur {Total} pour l'agent {AgentId}", 
                    commissionsDto.Count, total, agentId);

                return (commissionsDto, total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des commissions pour l'agent {AgentId}", agentId);
                throw;
            }
        }

        public async Task<List<CommissionStatsDto>> GetStatsAsync(int agentId, DateTime debut, DateTime fin)
        {
            try
            {
                _logger.LogInformation("Récupération des statistiques pour l'agent {AgentId} du {Debut} au {Fin}", agentId, debut, fin);

                var commissions = await _db.WalletMouvements
                    .Where(m => m.Wallet.AgentId == agentId && 
                               m.Source.Contains("COMMISSION") && 
                               m.TypeOperation == "CREDIT" &&
                               m.DateOperation >= debut &&
                               m.DateOperation <= fin)
                    .ToListAsync();

                var stats = new List<CommissionStatsDto>();

                // Statistiques par jour
                var dailyStats = commissions
                    .GroupBy(m => m.DateOperation.Date)
                    .Select(g => new DailyCommissionDto
                    {
                        Date = g.Key,
                        Total = g.Sum(m => m.Montant),
                        Nombre = g.Count(),
                        Moyenne = g.Count() > 0 ? g.Sum(m => m.Montant) / g.Count() : 0
                    })
                    .OrderBy(d => d.Date)
                    .ToList();

                var statsDto = new CommissionStatsDto
                {
                    Periode = $"{debut:dd/MM/yyyy} - {fin:dd/MM/yyyy}",
                    Total = commissions.Sum(m => m.Montant),
                    Nombre = commissions.Count,
                    Moyenne = commissions.Count > 0 ? commissions.Sum(m => m.Montant) / commissions.Count : 0,
                    DetailsQuotidiens = dailyStats
                };

                stats.Add(statsDto);

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des statistiques pour l'agent {AgentId}", agentId);
                throw;
            }
        }

        public async Task<List<MonthlyCommissionSummaryDto>> GetMonthlySummariesAsync(int agentId, int annee)
        {
            try
            {
                _logger.LogInformation("Récupération des résumés mensuels pour l'agent {AgentId} pour l'année {Annee}", agentId, annee);

                var commissions = await _db.WalletMouvements
                    .Where(m => m.Wallet.AgentId == agentId && 
                               m.Source.Contains("COMMISSION") && 
                               m.TypeOperation == "CREDIT" &&
                               m.DateOperation.Year == annee)
                    .ToListAsync();

                var monthlySummaries = new List<MonthlyCommissionSummaryDto>();

                for (int mois = 1; mois <= 12; mois++)
                {
                    var monthCommissions = commissions.Where(m => m.DateOperation.Month == mois).ToList();
                    var dailyStats = monthCommissions
                        .GroupBy(m => m.DateOperation.Date)
                        .Select(g => new DailyCommissionDto
                        {
                            Date = g.Key,
                            Total = g.Sum(m => m.Montant),
                            Nombre = g.Count(),
                            Moyenne = g.Count() > 0 ? g.Sum(m => m.Montant) / g.Count() : 0
                        })
                        .ToList();

                    var monthSummary = new MonthlyCommissionSummaryDto
                    {
                        Annee = annee,
                        Mois = mois,
                        MoisNom = new DateTime(annee, mois, 1).ToString("MMMM"),
                        TotalCommissions = monthCommissions.Sum(m => m.Montant),
                        NombreCommissions = monthCommissions.Count,
                        MoyenneJournaliere = dailyStats.Any() ? dailyStats.Average(d => d.Total) : 0,
                        MeilleurJour = dailyStats.Any() ? dailyStats.Max(d => d.Total) : 0,
                        PireJour = dailyStats.Any() ? dailyStats.Min(d => d.Total) : 0,
                        DetailsQuotidiens = dailyStats
                    };

                    monthlySummaries.Add(monthSummary);
                }

                return monthlySummaries.Where(m => m.NombreCommissions > 0).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des résumés mensuels pour l'agent {AgentId}", agentId);
                throw;
            }
        }

        public async Task<CommissionExportDto> ExportCommissionsAsync(int agentId, CommissionFilterDto filter, string format = "csv")
        {
            try
            {
                _logger.LogInformation("Export des commissions pour l'agent {AgentId} au format {Format}", agentId, format);

                var (commissions, total) = await GetCommissionsAsync(agentId, filter);
                var dashboard = await GetDashboardAsync(agentId);

                var agent = await _db.Agents.FindAsync(agentId);

                var export = new CommissionExportDto
                {
                    Commissions = commissions,
                    Resume = dashboard,
                    DateGeneration = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                    Periode = $"{filter.DateDebut:dd/MM/yyyy} - {filter.DateFin:dd/MM/yyyy}",
                    AgentNom = agent?.NomComplet ?? "Inconnu",
                    AgentMatricule = agent?.Matricule ?? "Inconnu"
                };

                return export;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'export des commissions pour l'agent {AgentId}", agentId);
                throw;
            }
        }

        public async Task<List<DailyCommissionDto>> GetTrendsAsync(int agentId, DateTime debut, DateTime fin)
        {
            try
            {
                _logger.LogInformation("Récupération des tendances pour l'agent {AgentId} du {Debut} au {Fin}", agentId, debut, fin);

                var commissions = await _db.WalletMouvements
                    .Where(m => m.Wallet.AgentId == agentId && 
                               m.Source.Contains("COMMISSION") && 
                               m.TypeOperation == "CREDIT" &&
                               m.DateOperation >= debut &&
                               m.DateOperation <= fin)
                    .ToListAsync();

                var trends = commissions
                    .GroupBy(m => m.DateOperation.Date)
                    .Select(g => new DailyCommissionDto
                    {
                        Date = g.Key,
                        Total = g.Sum(m => m.Montant),
                        Nombre = g.Count(),
                        Moyenne = g.Count() > 0 ? g.Sum(m => m.Montant) / g.Count() : 0
                    })
                    .OrderBy(d => d.Date)
                    .ToList();

                return trends;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des tendances pour l'agent {AgentId}", agentId);
                throw;
            }
        }

        private async Task<CommissionItemDto> MapToCommissionItemDto(WalletMouvement mouvement)
        {
            var dto = new CommissionItemDto
            {
                Id = mouvement.IdWalletMouvement,
                Montant = mouvement.Montant,
                Devise = "USD", // TODO: Récupérer depuis les préférences
                DateOperation = mouvement.DateOperation,
                Source = mouvement.Source,
                Description = mouvement.Description ?? "",
                SoldeApresOperation = 0 // TODO: Calculer le solde après l'opération
            };

            // Extraire les informations de la description si possible
            if (!string.IsNullOrEmpty(mouvement.Description))
            {
                // Format attendu: "Commission collecte #123 - Affilie NomComplet"
                var parts = mouvement.Description.Split('#');
                if (parts.Length > 1)
                {
                    var collectePart = parts[1].Split('-')[0].Trim();
                    if (int.TryParse(collectePart, out var collecteId))
                    {
                        dto.CollecteId = collecteId;
                    }

                    var affiliePart = mouvement.Description.Split('-').Skip(1).FirstOrDefault();
                    if (!string.IsNullOrEmpty(affiliePart))
                    {
                        dto.AffilieNom = affiliePart.Trim();
                    }
                }
            }

            // Récupérer le montant de la collecte si possible
            if (dto.CollecteId > 0)
            {
                var collecte = await _db.Collectes.FindAsync(dto.CollecteId);
                if (collecte != null)
                {
                    dto.CollecteMontant = collecte.Montant;
                }
            }

            return dto;
        }
    }
}
