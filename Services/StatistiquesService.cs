using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Extensions;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Statistiques;

namespace ProsocAPI.Services
{
    public class StatistiquesService : IStatistiquesService
    {
        private readonly ProsocDbContext _db;
        private readonly ILogger<StatistiquesService> _logger;

        public StatistiquesService(ProsocDbContext db, ILogger<StatistiquesService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<StatistiquesGeneralesDto> GetGeneralesAsync(StatistiquesFiltresDto filtres, CancellationToken ct = default)
        {
            var now = DateTime.Now;
            var (startMonth, endMonth) = StatistiquesPeriodBoundsHelper.CurrentMonth(now);
            var (startPreviousMonth, _) = StatistiquesPeriodBoundsHelper.PreviousMonth(now);

            var adhesions = await BuildAdhesionsQuery(filtres)
                .Where(a => a.Statut)
                .Select(a => new { a.AffilieId })
                .ToListAsync(ct);
            var totalAffilies = adhesions.Select(a => a.AffilieId).Distinct().Count();

            var collectesMois = await BuildCollectesQuery(filtres)
                .Where(c => c.Statut && c.DateCollecte >= startMonth && c.DateCollecte <= endMonth)
                .Select(c => new { c.StatutPaiement, Montant = c.MontantDevisePrincipale ?? c.Montant })
                .ToListAsync(ct);
            var collectesValidesMois = collectesMois.Where(c => CollecteStatutPaiementRegles.EstValide(c.StatutPaiement)).ToList();
            var totalCollectesMois = Round2(collectesValidesMois.Sum(x => x.Montant));
            var nombreCollectesMois = collectesValidesMois.Count;

            var arrieres = await BuildArrieresQuery(filtres)
                .Where(a => a.Statut && a.RestAPayer > 0)
                .Select(a => a.RestAPayer)
                .ToListAsync(ct);
            var totalArrieres = Round2(arrieres.Sum());

            var nombreObligationsMoisPrecedent = await BuildArrieresQuery(filtres)
                .Where(a => a.Statut && a.Annee == startPreviousMonth.Year && a.Mois == startPreviousMonth.Month)
                .CountAsync(ct);

            var montantObligationsM1List = await BuildArrieresQuery(filtres)
                .Where(a => a.Statut && a.Annee == startPreviousMonth.Year && a.Mois == startPreviousMonth.Month)
                .Select(a => a.MontantAttendu)
                .ToListAsync(ct);
            var montantObligationsM1 = montantObligationsM1List.Sum();

            var montantCollecteM = collectesValidesMois.Sum(x => x.Montant);
            var tauxRecouvrement = montantObligationsM1 > 0 ? Round2((montantCollecteM / montantObligationsM1) * 100m) : 0m;

            return new StatistiquesGeneralesDto
            {
                TotalAffilies = totalAffilies,
                NombreObligationsMoisPrecedent = nombreObligationsMoisPrecedent,
                TotalArrieres = totalArrieres,
                TotalCollectesMois = totalCollectesMois,
                TauxRecouvrement = tauxRecouvrement,
                NombreCollectesMois = nombreCollectesMois,
                DateGeneration = now
            };
        }

        public async Task<StatistiquesFinancieresDto> GetFinancieresAsync(StatistiquesFiltresDto filtres, CancellationToken ct = default)
        {
            var now = DateTime.Now;
            var (startMonth, endMonth) = StatistiquesPeriodBoundsHelper.CurrentMonth(now);
            var (customStart, customEnd) = StatistiquesPeriodBoundsHelper.CustomOrDefaultYearToDate(now, filtres.DateDebut, filtres.DateFin);

            var collectesMois = await BuildCollectesQuery(filtres)
                .Where(c => c.Statut && c.DateCollecte >= startMonth && c.DateCollecte <= endMonth)
                .Select(c => new { c.StatutPaiement, c.ModePaiement, Montant = c.MontantDevisePrincipale ?? c.Montant })
                .ToListAsync(ct);

            var chiffreAffaires = Round2(collectesMois
                .Where(c => CollecteStatutPaiementRegles.EstValide(c.StatutPaiement))
                .Sum(c => c.Montant));

            var collectesFenetre = await BuildCollectesQuery(filtres)
                .Where(c => c.Statut && c.DateCollecte >= customStart && c.DateCollecte <= customEnd)
                .Select(c => new { c.StatutPaiement, c.ModePaiement, Montant = c.MontantDevisePrincipale ?? c.Montant })
                .ToListAsync(ct);
            var collectesValidesFenetre = collectesFenetre.Where(c => CollecteStatutPaiementRegles.EstValide(c.StatutPaiement)).ToList();
            var montantPaye = Round2((filtres.DateDebut.HasValue || filtres.DateFin.HasValue)
                ? collectesValidesFenetre.Sum(c => c.Montant)
                : chiffreAffaires);

            var arrieresQuery = BuildArrieresQuery(filtres).Where(a => a.Statut);
            var montantArrieresList = await arrieresQuery.Where(a => a.RestAPayer > 0).Select(a => a.RestAPayer).ToListAsync(ct);
            var montantArrieres = Round2(montantArrieresList.Sum());

            var repartition = collectesValidesFenetre
                .GroupBy(x => string.IsNullOrWhiteSpace(x.ModePaiement) ? "INCONNU" : x.ModePaiement!)
                .Select(g => new RepartitionPaiementDto
                {
                    ModePaiement = g.Key,
                    MontantTotal = Round2(g.Sum(x => x.Montant)),
                    NombreCollectes = g.Count(),
                    Pourcentage = collectesValidesFenetre.Count == 0 ? 0 : Round2((decimal)g.Count() / collectesValidesFenetre.Count * 100m)
                })
                .OrderByDescending(r => r.MontantTotal)
                .ToList();

            var evolution = await BuildEvolutionMensuelleAsync(filtres, customStart, customEnd, ct);

            return new StatistiquesFinancieresDto
            {
                ChiffreAffaires = chiffreAffaires,
                MontantArrieres = montantArrieres,
                MontantPaye = montantPaye,
                MontantDu = montantArrieres,
                EvolutionMensuelle = evolution,
                RepartitionPaiements = repartition,
                DateGeneration = now
            };
        }

        public async Task<StatistiquesOperationnellesDto> GetOperationnellesAsync(StatistiquesFiltresDto filtres, CancellationToken ct = default)
        {
            var now = DateTime.Now;
            var adhesions = await BuildAdhesionsQuery(filtres)
                .Where(a => a.Statut)
                .Select(a => new
                {
                    a.AffilieId,
                    CategorieAdhesionId = a.TypeAdhesion.CategorieAdhesionId,
                    CategorieNom = a.TypeAdhesion.CategorieAdhesion.Libelle,
                    ZoneSocialeId = a.AgentCreateur != null ? a.AgentCreateur.ZoneSocialeId : null,
                    NomZone = a.AgentCreateur != null && a.AgentCreateur.Zone != null ? a.AgentCreateur.Zone.Nom : null,
                    NomCommune = a.AgentCreateur != null && a.AgentCreateur.Zone != null && a.AgentCreateur.Zone.Commune != null ? a.AgentCreateur.Zone.Commune.Nom : null
                })
                .ToListAsync(ct);

            var affilieIds = adhesions.Select(x => x.AffilieId).Distinct().ToList();
            var totalAffilies = affilieIds.Count;

            var arrieresParAffilie = await BuildArrieresQuery(filtres)
                .Where(a => a.Statut)
                .Select(a => new { a.AffilieId, a.MontantAttendu })
                .ToListAsync(ct);
            var arrieresMontantsParAffilie = arrieresParAffilie
                .GroupBy(a => a.AffilieId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.MontantAttendu));

            var repartitionCategorie = adhesions
                .GroupBy(a => new { a.CategorieAdhesionId, a.CategorieNom })
                .Select(g =>
                {
                    var ids = g.Select(x => x.AffilieId).Distinct().ToList();
                    var montantTotal = ids.Sum(id => arrieresMontantsParAffilie.GetValueOrDefault(id));
                    var count = ids.Count;
                    return new RepartitionAffilieParCategorieDto
                    {
                        CategorieAdhesionId = g.Key.CategorieAdhesionId,
                        NomCategorie = g.Key.CategorieNom,
                        NombreAffilies = count,
                        Pourcentage = totalAffilies == 0 ? 0 : Round2((decimal)count / totalAffilies * 100m),
                        MontantTotal = Round2(montantTotal)
                    };
                })
                .OrderByDescending(x => x.NombreAffilies)
                .ToList();

            var repartitionZone = adhesions
                .Where(a => a.ZoneSocialeId.HasValue)
                .GroupBy(a => new { ZoneSocialeId = a.ZoneSocialeId!.Value, a.NomZone, a.NomCommune })
                .Select(g =>
                {
                    var count = g.Select(x => x.AffilieId).Distinct().Count();
                    return new RepartitionAffilieParZoneDto
                    {
                        ZoneSocialeId = g.Key.ZoneSocialeId,
                        NomZone = g.Key.NomZone ?? "INCONNU",
                        NomCommune = g.Key.NomCommune ?? "INCONNU",
                        NombreAffilies = count,
                        Pourcentage = totalAffilies == 0 ? 0 : Round2((decimal)count / totalAffilies * 100m)
                    };
                })
                .OrderByDescending(x => x.NombreAffilies)
                .ToList();

            var obligations = await BuildArrieresQuery(filtres)
                .Where(a => a.Statut)
                .ToListAsync(ct);
            var statsObligationsMois = obligations
                .GroupBy(a => new { a.Annee, a.Mois })
                .Select(g => new StatistiqueObligationMoisDto
                {
                    Mois = FormatMonthLabel(g.Key.Annee, g.Key.Mois),
                    MontantTotal = Round2(g.Sum(x => x.MontantAttendu)),
                    NombreObligations = g.Count(),
                    MontantMoyen = g.Count() == 0 ? 0 : Round2(g.Sum(x => x.MontantAttendu) / g.Count())
                })
                .OrderBy(x => x.Mois)
                .ToList();

            var affilieStatuts = affilieIds.Count == 0
                ? new List<bool>()
                : await _db.Affilies
                    .AsNoTracking()
                    .Where(a => affilieIds.Contains(a.IdAffilie))
                    .Select(a => a.Statut)
                    .ToListAsync(ct);

            var nombreActifs = affilieStatuts.Count(s => s);
            var nombreInactifs = affilieStatuts.Count(s => !s);
            var total = affilieStatuts.Count;

            var affilieActivite = new AffilieActiviteDto
            {
                NombreAffiliesActifs = nombreActifs,
                NombreAffiliesInactifs = nombreInactifs,
                TotalAffilies = total,
                PourcentageActifs = total == 0 ? 0 : Round2((decimal)nombreActifs / total * 100m),
                PourcentageInactifs = total == 0 ? 0 : Round2((decimal)nombreInactifs / total * 100m)
            };

            return new StatistiquesOperationnellesDto
            {
                RepartitionAffiliesParCategorie = repartitionCategorie,
                RepartitionAffiliesParZone = repartitionZone,
                StatistiquesObligationsMois = statsObligationsMois,
                AffilieActivite = affilieActivite,
                DateGeneration = now
            };
        }

        public async Task<StatistiquesPerformanceDto> GetPerformanceAsync(StatistiquesFiltresDto filtres, CancellationToken ct = default)
        {
            var now = DateTime.Now;
            var (startMonth, endMonth) = StatistiquesPeriodBoundsHelper.CurrentMonth(now);
            var (startPreviousMonth, _) = StatistiquesPeriodBoundsHelper.PreviousMonth(now);

            var collectesMois = await BuildCollectesQuery(filtres)
                .Where(c => c.Statut && c.DateCollecte >= startMonth && c.DateCollecte <= endMonth)
                .Select(c => new { c.StatutPaiement, Montant = c.MontantDevisePrincipale ?? c.Montant, c.AgentId })
                .ToListAsync(ct);
            var collectesValidesMois = collectesMois.Where(p => CollecteStatutPaiementRegles.EstValide(p.StatutPaiement)).ToList();
            var montantCollecteMois = collectesValidesMois.Sum(x => x.Montant);

            var montantDuM1List = await BuildArrieresQuery(filtres)
                .Where(a => a.Statut && a.Annee == startPreviousMonth.Year && a.Mois == startPreviousMonth.Month)
                .Select(a => a.MontantAttendu)
                .ToListAsync(ct);
            var montantDuM1 = montantDuM1List.Sum();

            var tauxGlobal = montantDuM1 > 0 ? Round2((montantCollecteMois / montantDuM1) * 100m) : 0;

            var obligationsParCategorie = await BuildArrieresQuery(filtres)
                .Where(a => a.Statut)
                .Include(a => a.Affilie)
                .ThenInclude(af => af.Adhesion)
                .ThenInclude(ad => ad.TypeAdhesion)
                .ThenInclude(t => t.CategorieAdhesion)
                .ToListAsync(ct);

            var recouvrementCat = obligationsParCategorie
                .Where(x => x.Affilie?.Adhesion?.TypeAdhesion?.CategorieAdhesion != null)
                .GroupBy(x => new
                {
                    x.Affilie.Adhesion.TypeAdhesion.CategorieAdhesionId,
                    x.Affilie.Adhesion.TypeAdhesion.CategorieAdhesion.Libelle
                })
                .Select(g =>
                {
                    var montantDu = g.Sum(x => x.RestAPayer);
                    var montantPaye = g.Sum(x => x.MontantPaye);
                    return new TauxRecouvrementParCategorieDto
                    {
                        CategorieAdhesionId = g.Key.CategorieAdhesionId,
                        NomCategorie = g.Key.Libelle,
                        MontantDu = Round2(montantDu),
                        MontantPaye = Round2(montantPaye),
                        TauxRecouvrement = montantDu > 0 ? Round2((montantPaye / montantDu) * 100m) : 0
                    };
                })
                .OrderByDescending(x => x.MontantPaye)
                .ToList();

            var collectesTop = collectesValidesMois;

            var agentsCollecte = await _db.Agents
                .AsNoTracking()
                .Where(a => a.Statut && a.RoleAgent != null)
                .Select(a => new { a.IdAgent, a.NomComplet, a.RoleAgent })
                .ToListAsync(ct);

            var topAgents = agentsCollecte
                .Where(a => EstRoleCollecte(a.RoleAgent))
                .Select(a =>
                {
                    var agentCollectes = collectesTop.Where(c => c.AgentId == a.IdAgent).ToList();
                    var montant = agentCollectes.Sum(c => c.Montant);
                    var count = agentCollectes.Count;
                    return new TopAgentDto
                    {
                        IdAgent = a.IdAgent,
                        NomAgent = a.NomComplet ?? "",
                        RoleAgent = a.RoleAgent,
                        MontantCollecte = Round2(montant),
                        NombreCollectes = count,
                        TauxConversion = count > 0 ? Round2(100m / count) : 0
                    };
                })
                .Where(x => x.MontantCollecte > 0)
                .OrderByDescending(x => x.MontantCollecte)
                .Take(10)
                .ToList();

            var sixMonthsStart = new DateTime(now.Year, now.Month, 1).AddMonths(-5);
            var collectesPerf = await BuildCollectesQuery(filtres)
                .Where(c => c.Statut && c.DateCollecte >= sixMonthsStart && c.DateCollecte <= now)
                .Select(c => new { c.DateCollecte, c.StatutPaiement, Montant = c.MontantDevisePrincipale ?? c.Montant })
                .ToListAsync(ct);
            var perfs = collectesPerf
                .GroupBy(c => new { c.DateCollecte.Year, c.DateCollecte.Month })
                .Select(g =>
                {
                    var valides = g.Where(x => CollecteStatutPaiementRegles.EstValide(x.StatutPaiement)).ToList();
                    var montant = valides.Sum(x => x.Montant);
                    var count = valides.Count;
                    return new StatistiquesPerformanceMensuelleDto
                    {
                        Mois = FormatMonthLabel(g.Key.Year, g.Key.Month),
                        MontantCollecte = Round2(montant),
                        NombreCollectes = count,
                        TicketMoyen = count == 0 ? 0 : Round2(montant / count),
                        TauxRecouvrement = 0
                    };
                })
                .OrderBy(x => x.Mois)
                .ToList();

            return new StatistiquesPerformanceDto
            {
                TauxRecouvrementGlobal = tauxGlobal,
                TauxRecouvrementParCategorie = recouvrementCat,
                TopAgents = topAgents,
                PerformanceMensuelle = perfs,
                DateGeneration = now
            };
        }

        public async Task<StatistiquesConsolideesDto> GetConsolideesAsync(StatistiquesFiltresDto filtres, CancellationToken ct = default)
        {
            var generales = await GetGeneralesAsync(filtres, ct);
            var financieres = await GetFinancieresAsync(filtres, ct);
            var operationnelles = await GetOperationnellesAsync(filtres, ct);
            var performance = await GetPerformanceAsync(filtres, ct);

            var periode = new PeriodeStatistiquesDto
            {
                DateDebut = filtres.DateDebut,
                DateFin = filtres.DateFin,
                LibellePeriode = (filtres.DateDebut.HasValue || filtres.DateFin.HasValue) ? "Periode personnalisee" : "Periode par defaut"
            };

            return new StatistiquesConsolideesDto
            {
                Generales = generales,
                Financieres = financieres,
                Operationnelles = operationnelles,
                Performance = performance,
                Periode = periode,
                DateGeneration = DateTime.Now
            };
        }

        private IQueryable<Adhesion> BuildAdhesionsQuery(StatistiquesFiltresDto filtres)
        {
            return _db.Adhesions
                .AsNoTracking()
                .Include(a => a.TypeAdhesion)
                .ThenInclude(t => t.CategorieAdhesion)
                .Include(a => a.AgentCreateur)
                .ThenInclude(ag => ag!.Zone)
                .ThenInclude(z => z.Commune)
                .AppliquerFiltresStatistiques(filtres);
        }

        private IQueryable<Collecte> BuildCollectesQuery(StatistiquesFiltresDto filtres)
        {
            return _db.Collectes
                .AsNoTracking()
                .Include(c => c.Agent)
                .ThenInclude(a => a!.Zone)
                .ThenInclude(z => z.Commune)
                .AppliquerFiltresStatistiques(filtres);
        }

        private IQueryable<ArrieresAffilie> BuildArrieresQuery(StatistiquesFiltresDto filtres)
        {
            var query = _db.ArrieresAffilie
                .AsNoTracking()
                .AppliquerFiltresStatistiques(filtres);

            if (filtres.CategorieAdhesionId.HasValue || filtres.TypeAdhesionId.HasValue || filtres.ZoneSocialeId.HasValue || filtres.CommuneId.HasValue)
            {
                var adhesions = BuildAdhesionsQuery(filtres).Select(a => a.AffilieId).Distinct();
                query = query.Where(a => adhesions.Contains(a.AffilieId));
            }

            return query;
        }

        private async Task<List<EvolutionMensuelleDto>> BuildEvolutionMensuelleAsync(
            StatistiquesFiltresDto filtres,
            DateTime start,
            DateTime end,
            CancellationToken ct)
        {
            var obligations = await BuildArrieresQuery(filtres)
                .Where(a => a.Statut)
                .Select(a => new { a.Annee, a.Mois, a.MontantAttendu, a.RestAPayer })
                .ToListAsync(ct);

            var collectes = await BuildCollectesQuery(filtres)
                .Where(c => c.Statut && c.DateCollecte >= start && c.DateCollecte <= end)
                .Select(c => new { c.DateCollecte, c.StatutPaiement, Montant = c.MontantDevisePrincipale ?? c.Montant })
                .ToListAsync(ct);

            var periodMonths = EnumerateMonths(start, end).ToList();
            var result = new List<EvolutionMensuelleDto>();

            foreach (var month in periodMonths)
            {
                var obligationsMois = obligations.Where(a => a.Annee == month.Year && a.Mois == month.Month).ToList();
                var collectesMois = collectes
                    .Where(c => c.DateCollecte.Year == month.Year && c.DateCollecte.Month == month.Month)
                    .Where(c => CollecteStatutPaiementRegles.EstValide(c.StatutPaiement))
                    .ToList();

                result.Add(new EvolutionMensuelleDto
                {
                    Mois = FormatMonthLabel(month.Year, month.Month),
                    MontantObligations = Round2(obligationsMois.Sum(x => x.MontantAttendu)),
                    MontantCollectes = Round2(collectesMois.Sum(x => x.Montant)),
                    MontantArrieres = Round2(obligationsMois.Sum(x => x.RestAPayer)),
                    NombreObligations = obligationsMois.Count,
                    NombreCollectes = collectesMois.Count
                });
            }

            return result;
        }

        private static bool EstRoleCollecte(string? roleAgent)
        {
            if (string.IsNullOrWhiteSpace(roleAgent))
                return false;

            var role = roleAgent.ToLowerInvariant();
            return role.Contains("caiss")
                || role.Contains("percept")
                || role == "at"
                || role == "aa"
                || role.Contains("agent terrain")
                || role.Contains("agent d");
        }

        private static IEnumerable<DateTime> EnumerateMonths(DateTime start, DateTime end)
        {
            var cursor = new DateTime(start.Year, start.Month, 1);
            var finish = new DateTime(end.Year, end.Month, 1);
            while (cursor <= finish)
            {
                yield return cursor;
                cursor = cursor.AddMonths(1);
            }
        }

        private static string FormatMonthLabel(int year, int month)
        {
            return new DateTime(year, month, 1).ToString("MMMM yyyy");
        }

        private static decimal Round2(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}
