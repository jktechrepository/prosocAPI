using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProsocAPI.Models;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Services;
using Prosoc.Data;

namespace ProsocAPI.Services.Repositories
{
    public class DashboardFinancierService : IDashboardFinancierRepository
    {
        private const string CommissionCollecteSource = "COMM_COLLECTE";

        private readonly ProsocDbContext _db;
        private readonly IDeviseConversionService _deviseConversion;
        private readonly ILogger<DashboardFinancierService> _logger;

        public DashboardFinancierService(
            ProsocDbContext db,
            IDeviseConversionService deviseConversion,
            ILogger<DashboardFinancierService> logger)
        {
            _db = db;
            _deviseConversion = deviseConversion;
            _logger = logger;
        }

        public async Task<FinancierKpisDto> GetKpisFinanciersAsync(CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Récupération des KPIs financiers");

                var dateDebut = DateTime.Now.AddMonths(-12);
                var dateDebutPrecedente = dateDebut.AddMonths(-12);

                // Période actuelle
                var caActuel = await GetChiffreAffairesAsync(dateDebut, DateTime.Now, ct);
                var collectesActuel = await GetMontantCollectesAsync(dateDebut, DateTime.Now, ct);
                var commissionsActuel = await GetMontantCommissionsAsync(dateDebut, DateTime.Now, ct);
                var adhesionsActuel = await GetNombreAdhesionsAsync(dateDebut, DateTime.Now, ct);

                // Période précédente
                var caPrecedent = await GetChiffreAffairesAsync(dateDebutPrecedente, dateDebut, ct);
                var collectesPrecedent = await GetMontantCollectesAsync(dateDebutPrecedente, dateDebut, ct);
                var commissionsPrecedent = await GetMontantCommissionsAsync(dateDebutPrecedente, dateDebut, ct);
                var devisePrincipale = await GetDevisePrincipaleAsync(ct);

                var kpis = new FinancierKpisDto
                {
                    ChiffreAffairesTotal = caActuel,
                    MontantTotalCollectes = collectesActuel,
                    CodeDeviseConsolidation = devisePrincipale.Code ?? "USD",
                    MontantTotalCommissions = commissionsActuel,
                    NombreTotalAdhesions = adhesionsActuel,
                    NombreTotalAgents = await _db.Agents.CountAsync(ct),
                    PanierMoyen = adhesionsActuel > 0 ? collectesActuel / adhesionsActuel : 0,
                    TauxConversion = adhesionsActuel > 0 ? (decimal)adhesionsActuel / (collectesActuel / 10000) : 0, // Simplifié
                    TauxCroissanceCA = caPrecedent > 0 ? ((caActuel - caPrecedent) / caPrecedent) * 100 : 0,
                    TauxCroissanceCollectes = collectesPrecedent > 0 ? ((collectesActuel - collectesPrecedent) / collectesPrecedent) * 100 : 0,
                    TauxCroissanceCommissions = commissionsPrecedent > 0 ? ((commissionsActuel - commissionsPrecedent) / commissionsPrecedent) * 100 : 0
                };

                _logger.LogInformation("KPIs financiers récupérés avec succès");
                return kpis;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des KPIs financiers");
                throw;
            }
        }

        public async Task<FinancierGraphsDto> GetGraphsFinanciersAsync(CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Récupération des graphiques financiers");

                var graphs = new FinancierGraphsDto
                {
                    PerformancesMensuelles = await GetPerformancesMensuellesAsync(12, ct),
                    RevenusParSource = await GetRevenusParSourceAsync(ct),
                    Tendances = await GetTendancesFinancieresAsync(30, ct),
                    TransactionsParPeriode = await GetTransactionsParPeriodeAsync(30, ct),
                    RevenusParRegion = await GetRevenusParRegionAsync(ct)
                };

                _logger.LogInformation("Graphiques financiers récupérés avec succès");
                return graphs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des graphiques financiers");
                throw;
            }
        }

        public async Task<List<PerformanceMensuelleDto>> GetPerformancesMensuellesAsync(int mois = 12, CancellationToken ct = default)
        {
            try
            {
                var dateFin = DateTime.Now;
                var dateDebut = dateFin.AddMonths(-mois);
                var devisePrincipale = await GetDevisePrincipaleAsync(ct);

                var collectesParMois = await _db.Collectes
                    .Where(c => c.DateCollecte >= dateDebut && c.DateCollecte <= dateFin)
                    .GroupBy(c => new { c.DateCollecte.Year, c.DateCollecte.Month })
                    .Select(g => new
                    {
                        g.Key.Year,
                        g.Key.Month,
                        Montant = g.Sum(c => c.MontantDevisePrincipale ?? c.Montant),
                        NombreCollectes = g.Count()
                    })
                    .ToListAsync(ct);

                var mouvementsCommissions = await _db.WalletMouvements
                    .AsNoTracking()
                    .Where(m => m.Source == CommissionCollecteSource
                        && m.DateOperation >= dateDebut
                        && m.DateOperation <= dateFin)
                    .Select(m => new { m.Montant, m.DeviseId, m.DateOperation })
                    .ToListAsync(ct);

                var commissionsLookup = await GrouperCommissionsParMoisAsync(
                    mouvementsCommissions.Select(m => (m.Montant, m.DeviseId, m.DateOperation)),
                    devisePrincipale.Id,
                    ct);

                const decimal objectifCa = 1_000_000m;
                var performances = collectesParMois
                    .Select(c =>
                    {
                        commissionsLookup.TryGetValue((c.Year, c.Month), out var montantCommissions);
                        return new PerformanceMensuelleDto
                        {
                            Mois = FormatYearMonth(c.Year, c.Month),
                            ChiffreAffaires = c.Montant,
                            MontantCollectes = c.Montant,
                            MontantCommissions = montantCommissions,
                            NombreCollectes = c.NombreCollectes,
                            NombreAdhesions = 0,
                            PanierMoyen = c.NombreCollectes > 0 ? c.Montant / c.NombreCollectes : 0,
                            TauxConversion = 0,
                            ObjectifCA = objectifCa,
                            AtteinteObjectifCA = objectifCa > 0 ? (c.Montant / objectifCa) * 100 : 0
                        };
                    })
                    .OrderBy(x => x.Mois)
                    .ToList();

                return performances;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des performances mensuelles");
                throw;
            }
        }

        public async Task<List<RevenusSourceDto>> GetRevenusParSourceAsync(CancellationToken ct = default)
        {
            try
            {
                var revenus = new List<RevenusSourceDto>();

                var totalCollectes = await _db.Collectes.SumAsync(c => c.MontantDevisePrincipale ?? c.Montant, ct);
                revenus.Add(new RevenusSourceDto
                {
                    Source = "Collectes",
                    Montant = totalCollectes,
                    Pourcentage = 100,
                    NombreTransactions = await _db.Collectes.CountAsync(ct)
                });

                revenus.Add(new RevenusSourceDto
                {
                    Source = "Commissions",
                    Montant = await SommerToutesCommissionsAsync(ct),
                    Pourcentage = 25,
                    NombreTransactions = await _db.Collectes.CountAsync(ct)
                });

                // Calcul des pourcentages
                var totalRevenus = revenus.Sum(r => r.Montant);
                foreach (var revenu in revenus)
                {
                    revenu.Pourcentage = totalRevenus > 0 ? (revenu.Montant / totalRevenus) * 100 : 0;
                }

                return revenus;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des revenus par source");
                throw;
            }
        }

        public async Task<List<TopAgentPerformanceDto>> GetTopAgentsPerformanceAsync(int limit = 10, CancellationToken ct = default)
        {
            try
            {
                var devisePrincipale = await GetDevisePrincipaleAsync(ct);
                var commissionsParAgent = await GrouperCommissionsParAgentAsync(devisePrincipale.Id, ct);

                var topAgents = await _db.Agents
                    .Select(a => new TopAgentPerformanceDto
                    {
                        AgentId = a.IdAgent,
                        NomAgent = a.NomComplet ?? "",
                        ChiffreAffaires = _db.Collectes.Where(c => c.AgentId == a.IdAgent).Sum(c => c.MontantDevisePrincipale ?? c.Montant),
                        MontantCommissions = 0,
                        NombreAdhesions = _db.Adhesions.Where(ad => ad.AgentId == a.IdAgent).Count(),
                        NombreCollectes = _db.Collectes.Where(c => c.AgentId == a.IdAgent).Count(),
                        PanierMoyen = 0,
                        TauxConversion = 0,
                        Rang = 0
                    })
                    .OrderByDescending(a => a.ChiffreAffaires)
                    .Take(limit)
                    .ToListAsync(ct);

                for (int i = 0; i < topAgents.Count; i++)
                {
                    var agent = topAgents[i];
                    agent.Rang = i + 1;
                    agent.MontantCommissions = commissionsParAgent.GetValueOrDefault(agent.AgentId);
                    agent.PanierMoyen = agent.NombreCollectes > 0 ? agent.ChiffreAffaires / agent.NombreCollectes : 0;
                }

                return topAgents;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des top agents");
                throw;
            }
        }

        public async Task<List<CommissionAgentDto>> GetCommissionsAgentsAsync(CancellationToken ct = default)
        {
            try
            {
                var devisePrincipale = await GetDevisePrincipaleAsync(ct);
                var commissionsParAgent = await GrouperCommissionsParAgentAsync(devisePrincipale.Id, ct);

                var agents = await _db.Agents.AsNoTracking()
                    .Select(a => new { a.IdAgent, NomAgent = a.NomComplet ?? "" })
                    .ToListAsync(ct);

                var collectes = await _db.Collectes.AsNoTracking().ToListAsync(ct);
                var collectesParAgent = collectes
                    .GroupBy(c => c.AgentId)
                    .Select(g => new
                    {
                        AgentId = g.Key,
                        MontantCollectes = DashboardDeviseConsolidation.SommerCollectesEnDevisePrincipale(g),
                        NombreCollectes = g.Count()
                    })
                    .ToList();

                var collectesLookup = collectesParAgent.ToDictionary(x => x.AgentId);

                var commissions = agents.Select(a =>
                {
                    collectesLookup.TryGetValue(a.IdAgent, out var collectes);
                    var montantCollectes = collectes?.MontantCollectes ?? 0;
                    var montantCommission = commissionsParAgent.GetValueOrDefault(a.IdAgent);
                    return new CommissionAgentDto
                    {
                        AgentId = a.IdAgent,
                        NomAgent = a.NomAgent,
                        MontantCommission = montantCommission,
                        MontantCollectes = montantCollectes,
                        NombreCollectes = collectes?.NombreCollectes ?? 0,
                        TauxCommission = montantCollectes > 0
                            ? montantCommission / montantCollectes * 100
                            : 0,
                        Progression = 0
                    };
                }).OrderByDescending(a => a.MontantCommission).ToList();

                return commissions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des commissions par agent");
                throw;
            }
        }

        public async Task<List<ProduitStatsDto>> GetProduitsStatsAsync(CancellationToken ct = default)
        {
            try
            {
                var stats = await _db.SouscriptionsPrestations
                    .Include(sp => sp.Prestation)
                    .GroupBy(sp => sp.Prestation.NomPrestation)
                    .Select(g => new ProduitStatsDto
                    {
                        NomProduit = g.Key ?? "",
                        NombreSouscriptions = g.Count(),
                        MontantTotal = g.Sum(sp => sp.Prestation.ProduitMutuel != null ? sp.Prestation.ProduitMutuel.Montant : 0),
                        Pourcentage = 0, // Calculé après
                        Croissance = 0, // Simplifié
                        MontantMoyen = 0 // Calculé après
                    })
                    .ToListAsync(ct);

                // Calcul des pourcentages et montants moyens
                var totalSouscriptions = stats.Sum(s => s.NombreSouscriptions);
                foreach (var stat in stats)
                {
                    stat.Pourcentage = totalSouscriptions > 0 ? (decimal)stat.NombreSouscriptions / totalSouscriptions * 100 : 0;
                    stat.MontantMoyen = stat.NombreSouscriptions > 0 ? stat.MontantTotal / stat.NombreSouscriptions : 0;
                }

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des statistiques des produits");
                throw;
            }
        }

        public async Task<List<TendanceFinanciereDto>> GetTendancesFinancieresAsync(int jours = 30, CancellationToken ct = default)
        {
            try
            {
                var dateFin = DateTime.Now;
                var dateDebut = dateFin.AddDays(-jours);
                var devisePrincipale = await GetDevisePrincipaleAsync(ct);

                var collectesParJour = await _db.Collectes
                    .Where(c => c.DateCollecte >= dateDebut && c.DateCollecte <= dateFin)
                    .GroupBy(c => c.DateCollecte.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        Montant = g.Sum(c => c.MontantDevisePrincipale ?? c.Montant)
                    })
                    .ToListAsync(ct);

                var mouvementsCommissions = await _db.WalletMouvements
                    .AsNoTracking()
                    .Where(m => m.Source == CommissionCollecteSource
                        && m.DateOperation >= dateDebut
                        && m.DateOperation <= dateFin)
                    .Select(m => new { m.Montant, m.DeviseId, m.DateOperation })
                    .ToListAsync(ct);

                var commissionsLookup = await GrouperCommissionsParJourAsync(
                    mouvementsCommissions.Select(m => (m.Montant, m.DeviseId, m.DateOperation)),
                    devisePrincipale.Id,
                    ct);

                var tendances = collectesParJour
                    .Select(c =>
                    {
                        commissionsLookup.TryGetValue(c.Date, out var montantCommissions);
                        return new TendanceFinanciereDto
                        {
                            Periode = FormatDate(c.Date),
                            ChiffreAffaires = c.Montant,
                            MontantCollectes = c.Montant,
                            MontantCommissions = montantCommissions,
                            NombreAdhesions = 0,
                            TauxCroissanceCA = 0,
                            TauxCroissanceCollectes = 0,
                            TauxCroissanceCommissions = 0
                        };
                    })
                    .OrderBy(x => x.Periode)
                    .ToList();

                for (var i = 1; i < tendances.Count; i++)
                {
                    var actuel = tendances[i];
                    var precedent = tendances[i - 1];

                    actuel.TauxCroissanceCA = precedent.ChiffreAffaires > 0
                        ? ((actuel.ChiffreAffaires - precedent.ChiffreAffaires) / precedent.ChiffreAffaires) * 100
                        : 0;
                    actuel.TauxCroissanceCollectes = precedent.MontantCollectes > 0
                        ? ((actuel.MontantCollectes - precedent.MontantCollectes) / precedent.MontantCollectes) * 100
                        : 0;
                    actuel.TauxCroissanceCommissions = precedent.MontantCommissions > 0
                        ? ((actuel.MontantCommissions - precedent.MontantCommissions) / precedent.MontantCommissions) * 100
                        : 0;
                }

                return tendances;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des tendances financières");
                throw;
            }
        }

        public async Task<List<TransactionPeriodeDto>> GetTransactionsParPeriodeAsync(int jours = 30, CancellationToken ct = default)
        {
            try
            {
                var dateFin = DateTime.Now;
                var dateDebut = dateFin.AddDays(-jours);

                var agregats = await _db.Collectes
                    .Where(c => c.DateCollecte >= dateDebut && c.DateCollecte <= dateFin)
                    .GroupBy(c => c.DateCollecte.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        NombreTransactions = g.Count(),
                        MontantTotal = g.Sum(c => c.MontantDevisePrincipale ?? c.Montant),
                        MontantMin = g.Min(c => c.MontantDevisePrincipale ?? c.Montant),
                        MontantMax = g.Max(c => c.MontantDevisePrincipale ?? c.Montant),
                        TransactionsReussies = g.Count(c => c.StatutPaiement != null && CollecteStatutPaiementRegles.ValeursSqlValideEtLegacy.Contains(c.StatutPaiement)),
                        TransactionsEchouees = g.Count(c => c.StatutPaiement == CollecteStatutPaiement.EnAttente)
                    })
                    .ToListAsync(ct);

                var transactions = agregats
                    .Select(g =>
                    {
                        var dto = new TransactionPeriodeDto
                        {
                            Periode = FormatDate(g.Date),
                            NombreTransactions = g.NombreTransactions,
                            MontantTotal = g.MontantTotal,
                            MontantMoyen = g.NombreTransactions > 0 ? g.MontantTotal / g.NombreTransactions : 0,
                            MontantMin = g.MontantMin,
                            MontantMax = g.MontantMax,
                            TransactionsReussies = g.TransactionsReussies,
                            TransactionsEchouees = g.TransactionsEchouees
                        };
                        dto.TauxSucces = dto.NombreTransactions > 0
                            ? (decimal)dto.TransactionsReussies / dto.NombreTransactions * 100
                            : 0;
                        return dto;
                    })
                    .OrderBy(x => x.Periode)
                    .ToList();

                return transactions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des transactions par période");
                throw;
            }
        }

        public async Task<List<ObjectifFinancierDto>> GetObjectifsFinanciersAsync(CancellationToken ct = default)
        {
            try
            {
                var objectifs = new List<ObjectifFinancierDto>();

                // Objectif de chiffre d'affaires
                var caActuel = await GetChiffreAffairesAsync(DateTime.Now.AddMonths(-1), DateTime.Now, ct);
                var caObjectif = 1000000m; // Exemple
                objectifs.Add(new ObjectifFinancierDto
                {
                    TypeObjectif = "Chiffre d'Affaires",
                    Objectif = caObjectif,
                    Realise = caActuel,
                    Atteinte = caObjectif > 0 ? (caActuel / caObjectif) * 100 : 0,
                    Restant = Math.Max(0, caObjectif - caActuel),
                    Periode = "Mensuel",
                    ProgressionPrecedente = 0 // Simplifié
                });

                // Objectif de collectes
                var collectesActuel = await GetMontantCollectesAsync(DateTime.Now.AddMonths(-1), DateTime.Now, ct);
                var collectesObjectif = 800000m; // Exemple
                objectifs.Add(new ObjectifFinancierDto
                {
                    TypeObjectif = "Collectes",
                    Objectif = collectesObjectif,
                    Realise = collectesActuel,
                    Atteinte = collectesObjectif > 0 ? (collectesActuel / collectesObjectif) * 100 : 0,
                    Restant = Math.Max(0, collectesObjectif - collectesActuel),
                    Periode = "Mensuel",
                    ProgressionPrecedente = 0 // Simplifié
                });

                return objectifs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des objectifs financiers");
                throw;
            }
        }

        public async Task<ObjectifsAgentsFinancierDto> GetObjectifsAgentsAsync(
            int? mois = null,
            int? annee = null,
            CancellationToken ct = default)
        {
            try
            {
                var now = DateTime.Now;
                var moisEffectif = mois is >= 1 and <= 12 ? mois.Value : now.Month;
                var anneeEffectif = annee is >= 2000 and <= 2100 ? annee.Value : now.Year;
                var debutMois = new DateTime(anneeEffectif, moisEffectif, 1);
                var finMois = debutMois.AddMonths(1);

                var targetsMensuels = await _db.TargetsAgents
                    .AsNoTracking()
                    .Include(t => t.Role)
                    .Where(t => t.Statut && t.Periodicite == PeriodiciteTarget.Mensuelle)
                    .OrderByDescending(t => t.DateCreation)
                    .ToListAsync(ct);

                // Un target actif par rôle (le plus récent)
                var targetParRole = targetsMensuels
                    .GroupBy(t => t.RoleId)
                    .ToDictionary(g => g.Key, g => g.First());

                var result = new ObjectifsAgentsFinancierDto
                {
                    Mois = moisEffectif,
                    Annee = anneeEffectif
                };

                if (targetParRole.Count == 0)
                    return result;

                var roleIdsCibles = targetParRole.Keys.ToHashSet();

                var agentsActifs = await _db.Agents
                    .AsNoTracking()
                    .Where(a => a.Statut)
                    .Select(a => new { a.IdAgent, a.NomComplet, a.RoleAgent })
                    .ToListAsync(ct);

                if (agentsActifs.Count == 0)
                    return result;

                var agentIds = agentsActifs.Select(a => a.IdAgent).ToList();

                var roleIdParAgent = await _db.Utilisateurs
                    .AsNoTracking()
                    .Where(u => u.AgentId != null && agentIds.Contains(u.AgentId.Value) && u.RoleId != null)
                    .Select(u => new { AgentId = u.AgentId!.Value, RoleId = u.RoleId!.Value })
                    .ToListAsync(ct);

                var roleIdFromUser = roleIdParAgent
                    .GroupBy(x => x.AgentId)
                    .ToDictionary(g => g.Key, g => g.First().RoleId);

                var roleNomsFallback = agentsActifs
                    .Where(a => !roleIdFromUser.ContainsKey(a.IdAgent) && !string.IsNullOrWhiteSpace(a.RoleAgent))
                    .Select(a => a.RoleAgent!.Trim())
                    .Distinct()
                    .ToList();

                var rolesByNom = roleNomsFallback.Count == 0
                    ? new Dictionary<string, int>(StringComparer.Ordinal)
                    : (await _db.Roles
                        .AsNoTracking()
                        .Where(r => r.Statut && roleNomsFallback.Contains(r.Nom))
                        .Select(r => new { r.Nom, r.IdRole })
                        .ToListAsync(ct))
                        .ToDictionary(r => r.Nom, r => r.IdRole, StringComparer.Ordinal);

                var agentsAvecTarget = new List<(int AgentId, string AgentNom, int RoleId, TargetAgent Target)>();

                foreach (var agent in agentsActifs)
                {
                    int? roleId = null;
                    if (roleIdFromUser.TryGetValue(agent.IdAgent, out var fromUser))
                        roleId = fromUser;
                    else if (!string.IsNullOrWhiteSpace(agent.RoleAgent)
                             && rolesByNom.TryGetValue(agent.RoleAgent.Trim(), out var fromNom))
                        roleId = fromNom;

                    if (!roleId.HasValue || !roleIdsCibles.Contains(roleId.Value))
                        continue;

                    agentsAvecTarget.Add((agent.IdAgent, agent.NomComplet, roleId.Value, targetParRole[roleId.Value]));
                }

                if (agentsAvecTarget.Count == 0)
                    return result;

                var agentIdsRapport = agentsAvecTarget.Select(a => a.AgentId).ToList();
                var adhesionsParAgent = await _db.Adhesions
                    .AsNoTracking()
                    .Where(a => a.AgentId != null
                        && agentIdsRapport.Contains(a.AgentId.Value)
                        && a.DateCreation >= debutMois
                        && a.DateCreation < finMois)
                    .GroupBy(a => a.AgentId!.Value)
                    .Select(g => new { AgentId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.AgentId, x => x.Count, ct);

                var details = agentsAvecTarget.Select(a =>
                {
                    var realise = adhesionsParAgent.TryGetValue(a.AgentId, out var c) ? c : 0;
                    var objectif = a.Target.Nombre;
                    return new ObjectifAgentDetailDto
                    {
                        AgentId = a.AgentId,
                        AgentNom = a.AgentNom,
                        RoleId = a.RoleId,
                        RoleNom = a.Target.Role?.Nom ?? string.Empty,
                        ObjectifAdhesions = objectif,
                        RealiseAdhesions = realise,
                        Progression = objectif > 0
                            ? Math.Round((decimal)realise / objectif * 100, 2)
                            : 0
                    };
                })
                .OrderByDescending(d => d.Progression)
                .ThenBy(d => d.AgentNom)
                .ToList();

                var synthese = agentsAvecTarget
                    .GroupBy(a => a.RoleId)
                    .Select(g =>
                    {
                        var target = g.First().Target;
                        var nombreAgents = g.Count();
                        var objectifUnitaire = target.Nombre;
                        var objectifTotal = objectifUnitaire * nombreAgents;
                        var realiseTotal = g.Sum(a =>
                            adhesionsParAgent.TryGetValue(a.AgentId, out var c) ? c : 0);

                        return new ObjectifAgentRoleSyntheseDto
                        {
                            RoleId = g.Key,
                            RoleNom = target.Role?.Nom ?? string.Empty,
                            LibelleTarget = target.LibelleTarget,
                            ObjectifUnitaire = objectifUnitaire,
                            NombreAgents = nombreAgents,
                            ObjectifTotal = objectifTotal,
                            RealiseTotal = realiseTotal,
                            Progression = objectifTotal > 0
                                ? Math.Round((decimal)realiseTotal / objectifTotal * 100, 2)
                                : 0
                        };
                    })
                    .OrderBy(s => s.RoleNom)
                    .ToList();

                result.SyntheseParRole = synthese;
                result.DetailParAgent = details;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des objectifs agents (Financier)");
                throw;
            }
        }

        public async Task<List<RevenuGeographiqueDto>> GetRevenusParRegionAsync(CancellationToken ct = default)
        {
            try
            {
                // Simplifié - regrouper par agent (peut être amélioré avec de vraies données géographiques)
                var revenus = await _db.Agents
                    .Select(a => new RevenuGeographiqueDto
                    {
                        Region = a.NomComplet ?? "Inconnue", // Simplifié
                        Montant = _db.Collectes.Where(c => c.AgentId == a.IdAgent).Sum(c => c.MontantDevisePrincipale ?? c.Montant),
                        Pourcentage = 0, // Calculé après
                        NombreClients = _db.Adhesions.Where(ad => ad.AgentId == a.IdAgent).Count(),
                        NombreAgents = 1,
                        Croissance = 0 // Simplifié
                    })
                    .Where(r => r.Montant > 0)
                    .OrderByDescending(r => r.Montant)
                    .ToListAsync(ct);

                // Calcul des pourcentages
                var totalRevenus = revenus.Sum(r => r.Montant);
                foreach (var revenu in revenus)
                {
                    revenu.Pourcentage = totalRevenus > 0 ? (revenu.Montant / totalRevenus) * 100 : 0;
                }

                return revenus;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des revenus par région");
                throw;
            }
        }

        public async Task<RentabiliteDto> GetRentabiliteAsync(CancellationToken ct = default)
        {
            try
            {
                var dateDebut = DateTime.Now.AddMonths(-12);
                
                var caTotal = await GetChiffreAffairesAsync(dateDebut, DateTime.Now, ct);
                var commissions = await GetMontantCommissionsAsync(dateDebut, DateTime.Now, ct);
                var nombreClients = await GetNombreAdhesionsAsync(dateDebut, DateTime.Now, ct);

                return new RentabiliteDto
                {
                    MargeBrute = caTotal - commissions,
                    TauxMargeBrute = caTotal > 0 ? ((caTotal - commissions) / caTotal) * 100 : 0,
                    CoutAcquisition = nombreClients > 0 ? commissions / nombreClients : 0,
                    ValeurClient = nombreClients > 0 ? caTotal / nombreClients : 0,
                    RetourInvestissement = commissions > 0 ? (caTotal - commissions) / commissions * 100 : 0,
                    TauxRetention = 85, // Exemple
                    ChurnRate = 15, // Exemple
                    LTV = nombreClients > 0 ? (caTotal / nombreClients) * 12 : 0 // Simplifié
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des indicateurs de rentabilité");
                throw;
            }
        }

        public async Task<DashboardFinancierDto> GetDashboardFinancierAsync(CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Récupération du dashboard financier complet");

                var dashboard = new DashboardFinancierDto
                {
                    Kpis = await GetKpisFinanciersAsync(ct),
                    Graphs = await GetGraphsFinanciersAsync(ct),
                    TopAgents = await GetTopAgentsPerformanceAsync(10, ct),
                    CommissionsAgents = await GetCommissionsAgentsAsync(ct),
                    ProduitsStats = await GetProduitsStatsAsync(ct),
                    Objectifs = await GetObjectifsFinanciersAsync(ct),
                    Rentabilite = await GetRentabiliteAsync(ct),
                    DerniereMiseAJour = DateTime.Now
                };
                dashboard.CodeDeviseConsolidation = dashboard.Kpis.CodeDeviseConsolidation;

                _logger.LogInformation("Dashboard financier récupéré avec succès");
                return dashboard;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du dashboard financier");
                throw;
            }
        }

        public async Task<decimal> GetChiffreAffairesAsync(DateTime debut, DateTime fin, CancellationToken ct = default)
        {
            var collectes = await _db.Collectes
                .AsNoTracking()
                .Where(c => c.DateCollecte >= debut && c.DateCollecte <= fin)
                .ToListAsync(ct);
            return DashboardDeviseConsolidation.SommerCollectesEnDevisePrincipale(collectes);
        }

        public async Task<decimal> GetMontantCollectesAsync(DateTime debut, DateTime fin, CancellationToken ct = default)
        {
            return await GetChiffreAffairesAsync(debut, fin, ct);
        }

        public async Task<decimal> GetMontantCommissionsAsync(DateTime debut, DateTime fin, CancellationToken ct = default)
        {
            var devisePrincipale = await GetDevisePrincipaleAsync(ct);
            var mouvements = await _db.WalletMouvements
                .AsNoTracking()
                .Where(m => m.Source == CommissionCollecteSource
                    && m.DateOperation >= debut
                    && m.DateOperation <= fin)
                .Select(m => new { m.Montant, m.DeviseId, m.DateOperation })
                .ToListAsync(ct);

            if (devisePrincipale.Id == null)
                return mouvements.Sum(m => m.Montant);

            return await DashboardDeviseConsolidation.SommerMouvementsEnDevisePrincipaleAsync(
                _deviseConversion,
                mouvements.Select(m => (m.Montant, m.DeviseId, m.DateOperation)),
                devisePrincipale.Id.Value,
                ct);
        }

        private async Task<(int? Id, string? Code)> GetDevisePrincipaleAsync(CancellationToken ct)
        {
            var devise = await _db.Devises
                .AsNoTracking()
                .Where(d => d.EstDevisePrincipale && d.Statut)
                .Select(d => new { d.IdDevise, d.Code })
                .FirstOrDefaultAsync(ct);

            return devise == null ? (null, null) : (devise.IdDevise, devise.Code);
        }

        private async Task<decimal> SommerToutesCommissionsAsync(CancellationToken ct)
        {
            var devisePrincipale = await GetDevisePrincipaleAsync(ct);
            var mouvements = await _db.WalletMouvements
                .AsNoTracking()
                .Where(m => m.Source == CommissionCollecteSource)
                .Select(m => new { m.Montant, m.DeviseId, m.DateOperation })
                .ToListAsync(ct);

            if (devisePrincipale.Id == null)
                return mouvements.Sum(m => m.Montant);

            return await DashboardDeviseConsolidation.SommerMouvementsEnDevisePrincipaleAsync(
                _deviseConversion,
                mouvements.Select(m => (m.Montant, m.DeviseId, m.DateOperation)),
                devisePrincipale.Id.Value,
                ct);
        }

        private async Task<Dictionary<int, decimal>> GrouperCommissionsParAgentAsync(
            int? devisePrincipaleId,
            CancellationToken ct)
        {
            var mouvements = await (
                from m in _db.WalletMouvements.AsNoTracking()
                join w in _db.WalletsAgents.AsNoTracking() on m.WalletId equals w.IdWalletAgent
                where m.Source == CommissionCollecteSource
                select new
                {
                    w.AgentId,
                    m.Montant,
                    m.DeviseId,
                    m.DateOperation
                }).ToListAsync(ct);

            var totals = new Dictionary<int, decimal>();
            foreach (var mouvement in mouvements)
            {
                var montant = await DashboardDeviseConsolidation.MontantMouvementEnDevisePrincipaleAsync(
                    _deviseConversion,
                    mouvement.Montant,
                    mouvement.DeviseId,
                    devisePrincipaleId,
                    mouvement.DateOperation,
                    ct);
                totals[mouvement.AgentId] = totals.GetValueOrDefault(mouvement.AgentId) + montant;
            }

            return totals;
        }

        private async Task<Dictionary<(int Year, int Month), decimal>> GrouperCommissionsParMoisAsync(
            IEnumerable<(decimal Montant, int DeviseId, DateTime DateOperation)> mouvements,
            int? devisePrincipaleId,
            CancellationToken ct)
        {
            var totals = new Dictionary<(int Year, int Month), decimal>();
            foreach (var mouvement in mouvements)
            {
                var montant = await DashboardDeviseConsolidation.MontantMouvementEnDevisePrincipaleAsync(
                    _deviseConversion,
                    mouvement.Montant,
                    mouvement.DeviseId,
                    devisePrincipaleId,
                    mouvement.DateOperation,
                    ct);
                var key = (mouvement.DateOperation.Year, mouvement.DateOperation.Month);
                totals[key] = totals.GetValueOrDefault(key) + montant;
            }

            return totals;
        }

        private async Task<Dictionary<DateTime, decimal>> GrouperCommissionsParJourAsync(
            IEnumerable<(decimal Montant, int DeviseId, DateTime DateOperation)> mouvements,
            int? devisePrincipaleId,
            CancellationToken ct)
        {
            var totals = new Dictionary<DateTime, decimal>();
            foreach (var mouvement in mouvements)
            {
                var montant = await DashboardDeviseConsolidation.MontantMouvementEnDevisePrincipaleAsync(
                    _deviseConversion,
                    mouvement.Montant,
                    mouvement.DeviseId,
                    devisePrincipaleId,
                    mouvement.DateOperation,
                    ct);
                var key = mouvement.DateOperation.Date;
                totals[key] = totals.GetValueOrDefault(key) + montant;
            }

            return totals;
        }

        private static string FormatYearMonth(int year, int month) =>
            $"{year}-{month:D2}";

        private static string FormatDate(DateTime date) =>
            date.ToString("yyyy-MM-dd");

        public async Task<int> GetNombreAdhesionsAsync(DateTime debut, DateTime fin, CancellationToken ct = default)
        {
            return await _db.Adhesions
                .Where(a => a.DateCreation >= debut && a.DateCreation <= fin)
                .CountAsync(ct);
        }

        public async Task<List<TendanceFinanciereDto>> GetEvolutionRevenusAsync(int mois = 12, CancellationToken ct = default)
        {
            return await GetPerformancesMensuellesAsync(mois, ct)
                .ContinueWith(t => t.Result.Select(p => new TendanceFinanciereDto
                {
                    Periode = p.Mois,
                    ChiffreAffaires = p.ChiffreAffaires,
                    MontantCollectes = p.MontantCollectes,
                    MontantCommissions = p.MontantCommissions,
                    NombreAdhesions = p.NombreAdhesions,
                    TauxCroissanceCA = 0, // Calculé dans GetTendancesFinancieresAsync
                    TauxCroissanceCollectes = 0,
                    TauxCroissanceCommissions = 0
                }).ToList(), ct);
        }

        public async Task<List<RevenuGeographiqueDto>> GetPerformanceParRegionAsync(CancellationToken ct = default)
        {
            return await GetRevenusParRegionAsync(ct);
        }

        public async Task<List<ProduitStatsDto>> GetRentabiliteParProduitAsync(CancellationToken ct = default)
        {
            return await GetProduitsStatsAsync(ct);
        }
    }
}
