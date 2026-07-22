using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Prosoc.Data;
using Prosoc.Utilities;
using ProsocAPI.Models;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Services;
using ProsocAPI.Utilities;

namespace ProsocAPI.Services.Repositories
{
    public class DashboardAgentService : IDashboardAgentRepository
    {
        private readonly ProsocDbContext _db;
        private readonly IDeviseConversionService _deviseConversion;
        private readonly IAffilieConformiteService _conformiteService;
        private readonly ILogger<DashboardAgentService> _logger;

        public DashboardAgentService(
            ProsocDbContext db,
            IDeviseConversionService deviseConversion,
            IAffilieConformiteService conformiteService,
            ILogger<DashboardAgentService> logger)
        {
            _db = db;
            _deviseConversion = deviseConversion;
            _conformiteService = conformiteService;
            _logger = logger;
        }

        public async Task<AgentKpisDto> GetAgentKpisAsync(int agentId, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Récupération des KPIs pour l'agent {AgentId}", agentId);

                var debutMois = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var finMois = debutMois.AddMonths(1).AddDays(-1);

                // Total affiliés de l'agent
                var totalAffilies = await _db.Adhesions
                    .Where(a => a.AgentId == agentId && a.Statut)
                    .CountAsync(ct);

                // Collectes du mois
                var collectesMois = await _db.Collectes
                    .Include(c => c.Affilie)
                    .Where(c => c.AgentId == agentId && 
                               c.DateCollecte >= debutMois && 
                               c.DateCollecte <= finMois)
                    .ToListAsync(ct);

                // Nouvelles adhésions du mois
                var nouvellesAdhesions = await _db.Adhesions
                    .Where(a => a.AgentId == agentId && 
                               a.DateCreation >= debutMois && 
                               a.DateCreation <= finMois)
                    .CountAsync(ct);

                // Collectes en attente
                var collectesEnAttente = collectesMois.Count(c => CollecteStatutPaiementRegles.EstEnAttente(c.StatutPaiement));

                // Calcul des KPIs
                var devisePrincipale = await GetDevisePrincipaleAsync(ct);
                var totalCollectesMois = SommerCollectes(collectesMois);
                var totalCommissions = await GetCommissionsAgentAsync(agentId, debutMois, finMois, ct);
                var tauxConversion = totalAffilies > 0 ? (decimal)nouvellesAdhesions / totalAffilies * 100 : 0;
                var moyenneCollecte = collectesMois.Any() ? totalCollectesMois / collectesMois.Count : 0;

                // Objectif mensuel (exemple : 500000 FC)
                var objectifMois = 500000m;
                var progressionObjectif = objectifMois > 0 ? (totalCollectesMois / objectifMois) * 100 : 0;

                var kpis = new AgentKpisDto
                {
                    TotalAffilies = totalAffilies,
                    CollectesMois = collectesMois.Count,
                    TotalCommissionsMois = totalCommissions,
                    TotalCollectesMois = totalCollectesMois,
                    DevisePrincipaleCode = devisePrincipale.Code,
                    NouvellesAdhesionsMois = nouvellesAdhesions,
                    CollectesEnAttente = collectesEnAttente,
                    TauxConversion = Math.Round(tauxConversion, 2),
                    MoyenneCollecte = Math.Round(moyenneCollecte, 2),
                    ObjectifMois = objectifMois,
                    ProgressionObjectif = Math.Round(progressionObjectif, 2)
                };

                _logger.LogInformation("KPIs récupérés pour l'agent {AgentId}: {TotalAffilies} affiliés, {CollectesMois} collectes", 
                    agentId, kpis.TotalAffilies, kpis.CollectesMois);

                return kpis;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des KPIs pour l'agent {AgentId}", agentId);
                throw;
            }
        }

        public async Task<AgentGraphsDto> GetAgentGraphsAsync(int agentId, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Récupération des graphiques pour l'agent {AgentId}", agentId);

                var graphs = new AgentGraphsDto
                {
                    CollectesMensuelles = await GetPerformancesMensuellesAsync(agentId, 12, ct),
                    AdhesionsMensuelles = await GetAdhesionsMensuellesAsync(agentId, 12, ct),
                    CommissionsMensuelles = await GetCommissionsMensuellesAsync(agentId, 12, ct),
                    RepartitionPrestations = await GetPrestationsStatsAsync(agentId, ct),
                    ActiviteQuotidienne = await GetActiviteQuotidienneAsync(agentId, 30, ct)
                };

                _logger.LogInformation("Graphiques récupérés pour l'agent {AgentId}", agentId);
                return graphs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des graphiques pour l'agent {AgentId}", agentId);
                throw;
            }
        }

        public async Task<AgentPerformanceDto> GetAgentPerformanceAsync(int agentId, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Récupération de la performance pour l'agent {AgentId}", agentId);

                // Récupérer l'agent
                var agent = await _db.Agents
                    .FirstOrDefaultAsync(a => a.IdAgent == agentId, ct);

                if (agent == null)
                    throw new ArgumentException($"Agent {agentId} non trouvé");

                // Statistiques de l'agent
                var debutMois = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var debutAnnee = new DateTime(DateTime.Now.Year, 1, 1);

                var totalAffilies = await _db.Adhesions
                    .Where(a => a.AgentId == agentId && a.Statut)
                    .CountAsync(ct);

                var collectesAgent = await _db.Collectes
                    .AsNoTracking()
                    .Where(c => c.AgentId == agentId)
                    .ToListAsync(ct);

                var collectesMoisList = collectesAgent
                    .Where(c => c.DateCollecte >= debutMois)
                    .ToList();
                var collectesAnneeList = collectesAgent
                    .Where(c => c.DateCollecte >= debutAnnee)
                    .ToList();

                var totalCollectes = SommerCollectes(collectesAgent);
                var totalCommissions = await GetCommissionsAgentAsync(agentId, null, null, ct);
                var collectesMois = SommerCollectes(collectesMoisList);
                var collectesAnnee = SommerCollectes(collectesAnneeList);

                // Classement (simple basé sur le total des collectes)
                var classement = await GetClassementAgentAsync(agentId, ct);

                var performance = new AgentPerformanceDto
                {
                    AgentId = agentId,
                    AgentNom = agent.NomComplet,
                    TotalAffilies = totalAffilies,
                    TotalCollectes = totalCollectes,
                    TotalCommissions = totalCommissions,
                    Classement = classement,
                    ProgressionMois = collectesMois,
                    ProgressionAnnee = collectesAnnee,
                    TauxReussite = totalAffilies > 0 ? (decimal)totalAffilies / 100 * 100 : 0 // Simplifié
                };

                _logger.LogInformation("Performance récupérée pour l'agent {AgentId}: classement {Classement}", agentId, classement);
                return performance;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de la performance pour l'agent {AgentId}", agentId);
                throw;
            }
        }

        public async Task<List<AgentAffilieRecentDto>> GetAffiliesRecentsAsync(int agentId, int limit = 10, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Récupération des {Limit} affiliés récents pour l'agent {AgentId}", limit, agentId);

                var affilies = await _db.Adhesions
                    .Include(a => a.Affilie)
                    .Include(a => a.TypeAdhesion)
                    .Where(a => a.AgentId == agentId && a.Statut)
                    .OrderByDescending(a => a.DateCreation)
                    .Take(limit)
                    .Select(a => new AgentAffilieRecentDto
                    {
                        IdAffilie = a.Affilie.IdAffilie,
                        Nom = a.Affilie.Nom ?? "",
                        Prenom = a.Affilie.Prenom ?? "",
                        Telephone = a.Affilie.Telephone ?? "",
                        DateAdhesion = a.DateCreation,
                        TypeAdhesion = a.TypeAdhesion.Libelle ?? "",
                        StatutDossier = a.StatutDossier ?? "",
                        NombreCollectes = _db.Collectes.Count(c => c.AffilieId == a.AffilieId),
                        TotalCollectes = 0,
                        DerniereCollecte = 0,
                        DerniereCollecteDate = null
                    })
                    .ToListAsync(ct);

                if (affilies.Count > 0)
                {
                    var affilieIds = affilies.Select(a => a.IdAffilie).ToList();
                    var collectesParAffilie = (await _db.Collectes
                            .AsNoTracking()
                            .Where(c => affilieIds.Contains(c.AffilieId))
                            .ToListAsync(ct))
                        .GroupBy(c => c.AffilieId)
                        .ToDictionary(g => g.Key, g => g.ToList());

                    foreach (var affilie in affilies)
                    {
                        if (!collectesParAffilie.TryGetValue(affilie.IdAffilie, out var collectes))
                            continue;

                        affilie.TotalCollectes = SommerCollectes(collectes);
                        var derniere = collectes.OrderByDescending(c => c.DateCollecte).FirstOrDefault();
                        if (derniere != null)
                        {
                            affilie.DerniereCollecte = CollecteStatutPaiementRegles.MontantEnDevisePrincipale(derniere);
                            affilie.DerniereCollecteDate = derniere.DateCollecte;
                        }
                    }
                }

                _logger.LogInformation("Affiliés récents récupérés: {Count}", affilies.Count);
                return affilies;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des affiliés récents pour l'agent {AgentId}", agentId);
                throw;
            }
        }

        public async Task<List<AgentCollecteEnAttenteDto>> GetCollectesEnAttenteAsync(int agentId, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Récupération des collectes en attente pour l'agent {AgentId}", agentId);

                var collectes = await _db.Collectes
                    .Include(c => c.Affilie)
                    .Where(c => c.AgentId == agentId && c.StatutPaiement == CollecteStatutPaiement.EnAttente && c.Statut)
                    .OrderByDescending(c => c.DateCollecte)
                    .Select(c => new AgentCollecteEnAttenteDto
                    {
                        IdCollecte = c.IdCollecte,
                        NomAffilie = c.Affilie != null ? c.Affilie.Nom ?? "" : "Inconnu",
                        PrenomAffilie = c.Affilie != null ? c.Affilie.Prenom ?? "" : "",
                        TelephoneAffilie = c.Affilie != null ? c.Affilie.Telephone ?? "" : "",
                        Montant = c.MontantDevisePrincipale ?? c.Montant,
                        ReferencePaiement = c.ReferencePaiement ?? "",
                        ModePaiement = c.ModePaiement ?? "",
                        DateCollecte = c.DateCollecte,
                        StatutPaiement = c.StatutPaiement ?? "",
                        // Les calculs seront faits après la récupération
                        HeuresAttente = 0,
                        Priorite = 3
                    })
                    .ToListAsync(ct);

                // Faire les calculs de temps après la récupération des données
                foreach (var collecte in collectes)
                {
                    var tempsAttente = DateTime.Now - collecte.DateCollecte;
                    collecte.HeuresAttente = (int)tempsAttente.TotalHours;
                    collecte.Priorite = tempsAttente.TotalHours > 24 ? 1 : 
                                       tempsAttente.TotalHours > 12 ? 2 : 3;
                }

                _logger.LogInformation("Collectes en attente récupérées: {Count}", collectes.Count);
                return collectes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des collectes en attente pour l'agent {AgentId}", agentId);
                throw;
            }
        }

        public async Task<List<AgentCommissionDto>> GetCommissionsAsync(int agentId, int mois, int annee, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Récupération des commissions pour l'agent {AgentId}, mois {Mois}, année {Annee}", agentId, mois, annee);

                var debutMois = new DateTime(annee, mois, 1);
                var finMois = debutMois.AddMonths(1).AddDays(-1);

                var collectes = await _db.Collectes
                    .Where(c => c.AgentId == agentId && 
                               c.DateCollecte >= debutMois && 
                               c.DateCollecte <= finMois)
                    .ToListAsync(ct);

                var montantCommissions = await GetCommissionsAgentAsync(agentId, debutMois, finMois, ct);
                var montantCollectes = SommerCollectes(collectes);
                var tauxCommission = montantCollectes > 0
                    ? (montantCommissions / montantCollectes) * 100
                    : 0;

                var commissions = new List<AgentCommissionDto>
                {
                    new AgentCommissionDto
                    {
                        Mois = debutMois,
                        MontantCommission = montantCommissions,
                        MontantCollectes = montantCollectes,
                        NombreCollectes = collectes.Count,
                        TauxCommission = tauxCommission,
                        ObjectifMois = 100000m, // Exemple
                        AtteinteObjectif = montantCommissions / 100000m * 100
                    }
                };

                _logger.LogInformation("Commissions récupérées: {Count}", commissions.Count);
                return commissions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des commissions pour l'agent {AgentId}", agentId);
                throw;
            }
        }

        // Méthodes privées helpers

        private async Task<List<MonthlyAdhesionDto>> GetAdhesionsMensuellesAsync(int agentId, int mois, CancellationToken ct)
        {
            var debutPeriode = DateTime.Now.AddMonths(-mois + 1);
            debutPeriode = new DateTime(debutPeriode.Year, debutPeriode.Month, 1);

            var adhesions = new List<MonthlyAdhesionDto>();

            for (int i = 0; i < mois; i++)
            {
                var debutMois = debutPeriode.AddMonths(i);
                var finMois = debutMois.AddMonths(1).AddDays(-1);

                var adhesionsMois = await _db.Adhesions
                    .Where(a => a.AgentId == agentId && 
                               a.DateCreation >= debutMois && 
                               a.DateCreation <= finMois)
                    .CountAsync(ct);

                adhesions.Add(new MonthlyAdhesionDto
                {
                    Mois = debutMois.ToString("MMM yyyy"),
                    NombreAdhesions = adhesionsMois,
                    ValeurTotale = adhesionsMois * 10000m // Valeur estimée
                });
            }

            return adhesions;
        }

        private async Task<List<AgentCommissionGraphDto>> GetCommissionsMensuellesAsync(int agentId, int mois, CancellationToken ct)
        {
            var debutPeriode = DateTime.Now.AddMonths(-mois + 1);
            debutPeriode = new DateTime(debutPeriode.Year, debutPeriode.Month, 1);

            var commissions = new List<AgentCommissionGraphDto>();

            for (int i = 0; i < mois; i++)
            {
                var debutMois = debutPeriode.AddMonths(i);
                var finMois = debutMois.AddMonths(1).AddDays(-1);

                var montantCommission = await GetCommissionsAgentAsync(agentId, debutMois, finMois, ct);
                var objectif = 100000m; // Exemple

                commissions.Add(new AgentCommissionGraphDto
                {
                    Mois = debutMois.ToString("MMM yyyy"),
                    Montant = montantCommission,
                    Objectif = objectif,
                    Progression = objectif > 0 ? (montantCommission / objectif) * 100 : 0
                });
            }

            return commissions;
        }

        public async Task<List<PrestationStatsDto>> GetPrestationsStatsAsync(int agentId, CancellationToken ct)
        {
            var stats = await _db.SouscriptionsPrestations
                .Include(sp => sp.Prestation)
                .Where(sp => sp.Affilie != null && 
                           _db.Adhesions.Any(a => a.AffilieId == sp.AffilieId && a.AgentId == agentId))
                .GroupBy(sp => sp.Prestation.NomPrestation)
                .Select(g => new PrestationStatsDto
                {
                    NomPrestation = g.Key ?? "",
                    NombreSouscriptions = g.Count(),
                    MontantTotal = g.Sum(sp => sp.Prestation.ProduitMutuel != null ? sp.Prestation.ProduitMutuel.Montant : 0),
                    Pourcentage = 0 // Calculé après
                })
                .ToListAsync(ct);

            var total = stats.Sum(s => s.NombreSouscriptions);
            foreach (var stat in stats)
            {
                stat.Pourcentage = total > 0 ? (decimal)stat.NombreSouscriptions / total * 100 : 0;
            }

            return stats;
        }

        public async Task<List<DailyActivityDto>> GetActiviteQuotidienneAsync(int agentId, int jours, CancellationToken ct)
        {
            var debutPeriode = DateTime.Now.AddDays(-jours + 1);
            debutPeriode = new DateTime(debutPeriode.Year, debutPeriode.Month, debutPeriode.Day);

            var activite = new List<DailyActivityDto>();

            for (int i = 0; i < jours; i++)
            {
                var date = debutPeriode.AddDays(i);
                var finJour = date.AddDays(1);

                var collectesJour = await _db.Collectes
                    .Where(c => c.AgentId == agentId && 
                               c.DateCollecte >= date && 
                               c.DateCollecte < finJour)
                    .ToListAsync(ct);

                var adhesionsJour = await _db.Adhesions
                    .Where(a => a.AgentId == agentId && 
                               a.DateCreation >= date && 
                               a.DateCreation < finJour)
                    .CountAsync(ct);

                activite.Add(new DailyActivityDto
                {
                    Date = date,
                    NombreVisites = collectesJour.Count + adhesionsJour, // Simplifié
                    NombreAdhesions = adhesionsJour,
                    NombreCollectes = collectesJour.Count,
                    MontantCollectes = SommerCollectes(collectesJour)
                });
            }

            return activite;
        }

        public async Task<List<MonthlyCollectionDto>> GetPerformancesMensuellesAsync(int agentId, int mois, CancellationToken ct = default)
        {
            var dateFin = DateTime.Now;
            var dateDebut = dateFin.AddMonths(-mois);

            var collectes = await _db.Collectes
                .AsNoTracking()
                .Where(c => c.AgentId == agentId && c.DateCollecte >= dateDebut && c.DateCollecte <= dateFin)
                .ToListAsync(ct);

            return collectes
                .GroupBy(c => new { c.DateCollecte.Year, c.DateCollecte.Month })
                .Select(g =>
                {
                    var montant = SommerCollectes(g);
                    var nombre = g.Count();
                    return new
                    {
                        g.Key.Year,
                        g.Key.Month,
                        Montant = montant,
                        NombreCollectes = nombre
                    };
                })
                .OrderByDescending(p => p.Year)
                .ThenByDescending(p => p.Month)
                .Select(p => new MonthlyCollectionDto
                {
                    Mois = $"{p.Year}-{p.Month:D2}",
                    Montant = p.Montant,
                    NombreCollectes = p.NombreCollectes,
                    Moyenne = p.NombreCollectes > 0 ? p.Montant / p.NombreCollectes : 0
                })
                .ToList();
        }

        public async Task<AgentObjectifDto> GetObjectifsAsync(int agentId, int mois, int annee, CancellationToken ct = default)
        {
            var debutMois = new DateTime(annee, mois, 1);
            var finMois = debutMois.AddMonths(1);

            var roleId = await TargetAgentRoleResolver.ResolveRoleIdForAgentAsync(_db, agentId, ct);
            TargetAgent? targetMensuel = null;

            if (roleId.HasValue)
            {
                targetMensuel = await _db.TargetsAgents
                    .AsNoTracking()
                    .Where(t => t.RoleId == roleId.Value
                        && t.Statut
                        && t.Periodicite == PeriodiciteTarget.Mensuelle)
                    .OrderByDescending(t => t.DateCreation)
                    .FirstOrDefaultAsync(ct);
            }

            var objectifAdhesions = targetMensuel?.Nombre ?? 100;
            var collectesMoisList = await _db.Collectes
                .AsNoTracking()
                .Where(c => c.AgentId == agentId && c.DateCollecte >= debutMois && c.DateCollecte < finMois)
                .ToListAsync(ct);
            var collectesMois = SommerCollectes(collectesMoisList);
            var adhesionsMois = await _db.Adhesions
                .CountAsync(a => a.AgentId == agentId && a.DateCreation >= debutMois && a.DateCreation < finMois, ct);
            var commissionsMois = await GetCommissionsAgentAsync(agentId, debutMois, finMois.AddDays(-1), ct);

            return new AgentObjectifDto
            {
                Mois = mois,
                Annee = annee,
                ObjectifCollectes = 500000m,
                ObjectifAdhesions = objectifAdhesions,
                ObjectifCommissions = commissionsMois > 0 ? commissionsMois * 1.2m : 100000m,
                ProgressionCollectes = 500000m > 0 ? Math.Round(collectesMois / 500000m * 100, 2) : 0,
                ProgressionAdhesions = objectifAdhesions > 0
                    ? Math.Round((decimal)adhesionsMois / objectifAdhesions * 100, 2)
                    : 0,
                ProgressionCommissions = commissionsMois > 0 ? 100 : 0
            };
        }

        public async Task<AgentPrimesResumeDto> GetPrimesGenereesAsync(
            int agentId,
            int? mois,
            int? annee,
            int limitDetails,
            CancellationToken ct = default)
        {
            var m = mois ?? DateTime.Now.Month;
            var a = annee ?? DateTime.Now.Year;
            var debut = new DateTime(a, m, 1);
            var fin = debut.AddMonths(1);
            if (limitDetails <= 0) limitDetails = 50;

            var collectes = await _db.Collectes
                .AsNoTracking()
                .Include(c => c.Affilie)
                .Include(c => c.SouscriptionPrestationRef!)
                    .ThenInclude(sp => sp!.Prestation)
                        .ThenInclude(p => p.ProduitAssureur)
                .Include(c => c.SouscriptionPrestationRef!)
                    .ThenInclude(sp => sp!.Prestation)
                        .ThenInclude(p => p.ProduitMutuel)
                .Where(c => c.AgentId == agentId
                    && c.TypeCollecte == TypeCollecte.Souscription
                    && c.Statut
                    && c.DateCollecte >= debut
                    && c.DateCollecte < fin)
                .OrderByDescending(c => c.DateCollecte)
                .ToListAsync(ct);

            var details = collectes.Take(limitDetails).Select(c =>
            {
                var prestation = c.SouscriptionPrestationRef?.Prestation;
                var estAssurance = prestation?.ProduitAssureurId != null;
                var produit = estAssurance
                    ? prestation?.ProduitAssureur?.Nom
                    : prestation?.ProduitMutuel?.Nom;

                return new AgentPrimeDetailDto
                {
                    IdCollecte = c.IdCollecte,
                    AffilieId = c.AffilieId,
                    NomAffilie = c.Affilie?.NomComplet ?? $"{c.Affilie?.Prenom} {c.Affilie?.Nom}",
                    NomProduit = produit ?? prestation?.NomPrestation ?? "Souscription",
                    TypeProduit = estAssurance ? "Assurance" : "Mutuelle",
                    MontantPrime = CollecteStatutPaiementRegles.MontantEnDevisePrincipale(c),
                    DateCollecte = c.DateCollecte,
                    StatutPaiement = c.StatutPaiement ?? ""
                };
            }).ToList();

            return new AgentPrimesResumeDto
            {
                TotalPrimesMois = SommerCollectes(collectes),
                TotalPrimesAssuranceMois = SommerCollectes(collectes
                    .Where(c => c.SouscriptionPrestationRef?.Prestation?.ProduitAssureurId != null)),
                TotalPrimesMutuelleMois = SommerCollectes(collectes
                    .Where(c => c.SouscriptionPrestationRef?.Prestation?.ProduitMutuelId != null)),
                NombreSouscriptionsMois = collectes.Count,
                Details = details
            };
        }

        public async Task<AgentCommissionsResumeDto> GetCommissionsResumeAsync(
            int agentId,
            int limitMouvements,
            CancellationToken ct = default)
        {
            if (limitMouvements <= 0) limitMouvements = 20;

            var devisePrincipale = await GetDevisePrincipaleAsync(ct);

            var wallets = await _db.WalletsAgents.AsNoTracking()
                .Where(w => w.AgentId == agentId && w.Statut)
                .Select(w => new { w.SoldeCourant, w.DeviseId })
                .ToListAsync(ct);

            var debutMois = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var debutAnnee = new DateTime(DateTime.Now.Year, 1, 1);

            var mouvements = await _db.WalletMouvements
                .AsNoTracking()
                .Include(m => m.Wallet)
                .Where(m => m.Wallet.AgentId == agentId
                    && m.TypeOperation == "CREDIT"
                    && m.Statut
                    && (m.Source == "COMM_COLLECTE" || m.Source.Contains("COMMISSION")))
                .OrderByDescending(m => m.DateOperation)
                .Take(limitMouvements)
                .ToListAsync(ct);

            var collecteIds = mouvements
                .Select(m => WalletMouvementDescriptionBuilder.TryExtractCollecteId(m.Description))
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();

            var collectesMap = await _db.Collectes.AsNoTracking()
                .Include(c => c.Affilie)
                .Where(c => collecteIds.Contains(c.IdCollecte))
                .ToDictionaryAsync(c => c.IdCollecte, ct);

            var mouvementDtos = new List<AgentCommissionMouvementDto>();
            foreach (var m in mouvements)
            {
                collectesMap.TryGetValue(WalletMouvementDescriptionBuilder.TryExtractCollecteId(m.Description) ?? 0, out var collecte);
                var montantMouvement = m.Montant;
                if (devisePrincipale.Id.HasValue)
                {
                    montantMouvement = await DashboardDeviseConsolidation.MontantMouvementEnDevisePrincipaleAsync(
                        _deviseConversion,
                        m.Montant,
                        m.DeviseId,
                        devisePrincipale.Id,
                        m.DateOperation,
                        ct);
                }

                mouvementDtos.Add(new AgentCommissionMouvementDto
                {
                    IdWalletMouvement = m.IdWalletMouvement,
                    Montant = montantMouvement,
                    Source = m.Source,
                    Description = m.Description,
                    DateOperation = m.DateOperation,
                    NomAffilie = collecte?.Affilie?.NomComplet,
                    MontantCollecteLiee = collecte != null
                        ? CollecteStatutPaiementRegles.MontantEnDevisePrincipale(collecte)
                        : null
                });
            }

            var soldeWallet = 0m;
            if (devisePrincipale.Id.HasValue)
            {
                var now = DateTime.Now;
                foreach (var wallet in wallets)
                {
                    soldeWallet += await DashboardDeviseConsolidation.MontantMouvementEnDevisePrincipaleAsync(
                        _deviseConversion,
                        wallet.SoldeCourant,
                        wallet.DeviseId,
                        devisePrincipale.Id,
                        now,
                        ct);
                }
            }
            else
            {
                soldeWallet = wallets.Sum(w => w.SoldeCourant);
            }

            return new AgentCommissionsResumeDto
            {
                SoldeWallet = soldeWallet,
                TotalCommissionsMois = await GetCommissionsAgentAsync(agentId, debutMois, null, ct),
                TotalCommissionsAnnee = await GetCommissionsAgentAsync(agentId, debutAnnee, null, ct),
                NombreMouvementsMois = await _db.WalletMouvements.CountAsync(m =>
                    m.Wallet.AgentId == agentId
                    && m.TypeOperation == "CREDIT"
                    && m.Statut
                    && (m.Source == "COMM_COLLECTE" || m.Source.Contains("COMMISSION"))
                    && m.DateOperation >= debutMois, ct),
                MouvementsRecents = mouvementDtos
            };
        }

        public async Task<List<AgentSuiviAdherentDto>> GetSuiviAdherentsAsync(
            int agentId,
            int limit,
            string? statutGlobal = null,
            CancellationToken ct = default)
        {
            if (limit <= 0) limit = 50;

            var adhesions = await _db.Adhesions
                .AsNoTracking()
                .Include(a => a.Affilie)
                .Include(a => a.TypeAdhesion)
                .Where(a => a.AgentId == agentId && a.Statut)
                .OrderByDescending(a => a.DateCreation)
                .Take(limit * 3)
                .ToListAsync(ct);

            var affilieIds = adhesions.Select(a => a.AffilieId).Distinct().ToList();
            var conformites = await _conformiteService.GetConformiteParAffiliesAsync(affilieIds, ct);

            var result = new List<AgentSuiviAdherentDto>();

            foreach (var adhesion in adhesions)
            {
                var affilieId = adhesion.AffilieId;
                conformites.TryGetValue(affilieId, out var conformite);
                conformite ??= new AffilieConformiteDto { AffilieId = affilieId };

                if (!string.IsNullOrWhiteSpace(statutGlobal)
                    && !string.Equals(conformite.StatutGlobal, statutGlobal, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var cotisationAJour = await EstCotisationAJourAsync(affilieId, adhesion.TypeAdhesionId, ct);

                var collectes = await _db.Collectes.AsNoTracking()
                    .Where(c => c.AffilieId == affilieId && c.Statut)
                    .ToListAsync(ct);

                var nombrePrimes = collectes.Count(c => c.TypeCollecte == TypeCollecte.Souscription);
                var statutDossier = adhesion.StatutDossier ?? "";
                string? alerte = null;

                if (!string.Equals(statutDossier, AdhesionNiveau2Regles.StatutValide, StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(statutDossier, "VALIDÉ", StringComparison.OrdinalIgnoreCase))
                    alerte = "Dossier à compléter (encodeur)";

                if (!cotisationAJour)
                    alerte = alerte == null ? "Cotisation non à jour" : $"{alerte} ; Cotisation non à jour";

                if (conformite.StatutGlobal == AffilieConformiteStatuts.HorsOrdre)
                    alerte = alerte == null ? "Arriérés impayés" : $"{alerte} ; Arriérés impayés";

                result.Add(new AgentSuiviAdherentDto
                {
                    IdAffilie = affilieId,
                    IdAdhesion = adhesion.IdAdhesion,
                    CodeAdhesion = adhesion.Affilie.CodeAdhesion,
                    NomComplet = adhesion.Affilie.NomComplet,
                    Telephone = adhesion.Affilie.Telephone,
                    DateAdhesion = adhesion.DateCreation,
                    StatutDossier = statutDossier,
                    TypeAdhesion = adhesion.TypeAdhesion?.Libelle ?? "",
                    CotisationAJour = cotisationAJour,
                    StatutGlobal = conformite.StatutGlobal,
                    StatutCotisation = conformite.StatutCotisation,
                    StatutPrestation = conformite.StatutPrestation,
                    NombreArrieresOuverts = conformite.NombreArrieresOuverts,
                    MontantRestantDu = conformite.MontantRestantDu,
                    TotalCollectes = SommerCollectes(collectes),
                    NombrePrimes = nombrePrimes,
                    DerniereActivite = collectes.OrderByDescending(c => c.DateCollecte).FirstOrDefault()?.DateCollecte,
                    Alerte = alerte
                });

                if (result.Count >= limit)
                    break;
            }

            return result;
        }

        public async Task<AgentTerrainDashboardDto> GetDashboardTerrainAsync(int agentId, CancellationToken ct = default)
        {
            var agent = await _db.Agents.AsNoTracking()
                .FirstOrDefaultAsync(a => a.IdAgent == agentId, ct);

            var mois = DateTime.Now.Month;
            var annee = DateTime.Now.Year;

            var kpis = await GetAgentKpisAsync(agentId, ct);

            return new AgentTerrainDashboardDto
            {
                AgentId = agentId,
                NomAgent = agent?.NomComplet ?? "",
                Kpis = kpis,
                Primes = await GetPrimesGenereesAsync(agentId, mois, annee, 20, ct),
                Commissions = await GetCommissionsResumeAsync(agentId, 15, ct),
                SuiviAdherents = await GetSuiviAdherentsAsync(agentId, 30, null, ct),
                AffiliesRecents = await GetAffiliesRecentsAsync(agentId, 5, ct),
                CollectesEnAttente = await GetCollectesEnAttenteAsync(agentId, ct),
                Objectifs = await GetObjectifsAsync(agentId, mois, annee, ct),
                DateGeneration = DateTime.Now,
                DevisePrincipaleCode = kpis.DevisePrincipaleCode
            };
        }

        private async Task<bool> EstCotisationAJourAsync(int affilieId, int typeAdhesionId, CancellationToken ct)
        {
            try
            {
                await ProduitEligibiliteRules.ValidateCotisationAJourAsync(_db, affilieId, ct, typeAdhesionId);
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        public async Task<List<AgentAffilieRecentDto>> GetTopAffiliesAsync(int agentId, int limit = 5, CancellationToken ct = default)
        {
            var topAffilies = await _db.Adhesions
                .Include(a => a.Affilie)
                .Where(a => a.AgentId == agentId && a.Statut)
                .Select(a => new AgentAffilieRecentDto
                {
                    IdAffilie = a.Affilie.IdAffilie,
                    Nom = a.Affilie.Nom ?? "",
                    Prenom = a.Affilie.Prenom ?? "",
                    Telephone = a.Affilie.Telephone ?? "",
                    DateAdhesion = a.DateCreation,
                    TypeAdhesion = "",
                    NombreCollectes = _db.Collectes.Count(c => c.AffilieId == a.AffilieId),
                    TotalCollectes = 0
                })
                .ToListAsync(ct);

            if (topAffilies.Count > 0)
            {
                var affilieIds = topAffilies.Select(a => a.IdAffilie).ToList();
                var collectesParAffilie = (await _db.Collectes
                        .AsNoTracking()
                        .Where(c => affilieIds.Contains(c.AffilieId))
                        .ToListAsync(ct))
                    .GroupBy(c => c.AffilieId)
                    .ToDictionary(g => g.Key, g => SommerCollectes(g));

                foreach (var affilie in topAffilies)
                    affilie.TotalCollectes = collectesParAffilie.GetValueOrDefault(affilie.IdAffilie);
            }

            return topAffilies
                .OrderByDescending(a => a.TotalCollectes)
                .Take(limit)
                .ToList();
        }

        private async Task<int> GetClassementAgentAsync(int agentId, CancellationToken ct)
        {
            // Classement simplifié basé sur le total des collectes du mois
            var debutMois = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            var collectesMois = await _db.Collectes
                .AsNoTracking()
                .Where(c => c.DateCollecte >= debutMois)
                .ToListAsync(ct);

            var classements = collectesMois
                .GroupBy(c => c.AgentId)
                .Select(g => new { AgentId = g.Key, Total = SommerCollectes(g) })
                .OrderByDescending(x => x.Total)
                .ToList();

            return classements.FindIndex(x => x.AgentId == agentId) + 1;
        }

        private async Task<(int? Id, string? Code)> GetDevisePrincipaleAsync(CancellationToken ct) =>
            await _db.Devises
                .AsNoTracking()
                .Where(d => d.EstDevisePrincipale && d.Statut)
                .Select(d => new ValueTuple<int?, string?>((int?)d.IdDevise, d.Code))
                .FirstOrDefaultAsync(ct);

        private static decimal SommerCollectes(IEnumerable<Collecte> collectes) =>
            DashboardDeviseConsolidation.SommerCollectesEnDevisePrincipale(collectes);

        private async Task<decimal> GetCommissionsAgentAsync(int agentId, DateTime? debut, DateTime? fin, CancellationToken ct)
        {
            var query = _db.WalletMouvements
                .AsNoTracking()
                .Where(m => m.Wallet.AgentId == agentId
                    && m.TypeOperation == "CREDIT"
                    && m.Statut
                    && (m.Source == "COMM_COLLECTE" || m.Source.Contains("COMMISSION")));

            if (debut.HasValue)
                query = query.Where(m => m.DateOperation >= debut.Value);

            if (fin.HasValue)
                query = query.Where(m => m.DateOperation <= fin.Value);

            var mouvements = await query
                .Select(m => new { m.Montant, m.DeviseId, m.DateOperation })
                .ToListAsync(ct);

            var devisePrincipale = await GetDevisePrincipaleAsync(ct);
            if (!devisePrincipale.Id.HasValue)
                return mouvements.Sum(m => m.Montant);

            return await DashboardDeviseConsolidation.SommerMouvementsEnDevisePrincipaleAsync(
                _deviseConversion,
                mouvements.Select(m => (m.Montant, m.DeviseId, m.DateOperation)),
                devisePrincipale.Id.Value,
                ct);
        }
    }
}
