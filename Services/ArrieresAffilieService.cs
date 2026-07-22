using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Prosoc.Data;
using ProsocAPI.Models.Configuration;
using ProsocAPI.Models.Core;

namespace ProsocAPI.Services
{
    public class ArrieresAffilieService : IArrieresAffilieService
    {
        private readonly ProsocDbContext _db;
        private readonly ICotisationAffilieMetierService _cotisationMetier;
        private readonly IParametresMetierProvider _parametresMetierProvider;
        private readonly ILogger<ArrieresAffilieService> _logger;

        public ArrieresAffilieService(
            ProsocDbContext db,
            ICotisationAffilieMetierService cotisationMetier,
            IParametresMetierProvider parametresMetierProvider,
            ILogger<ArrieresAffilieService> logger)
        {
            _db = db;
            _cotisationMetier = cotisationMetier;
            _parametresMetierProvider = parametresMetierProvider;
            _logger = logger;
        }

        public async Task<ArrieresAffilie> ProcessCollecteForArrieresAsync(Collecte collecte, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Traitement arriéré pour collecte {CollecteId}, type {Type}",
                    collecte.IdCollecte, collecte.TypeCollecte);

                if (collecte.TypeCollecte == 0 || !collecte.IsValid())
                {
                    _logger.LogDebug("Collecte {CollecteId} invalide pour arriérés", collecte.IdCollecte);
                    return null!;
                }

                var arriere = await FindExistingAsync(collecte, ct);
                if (arriere == null)
                    arriere = await CreateFromCollecteAsync(collecte, ct);

                ArrieresAffilieRules.AppliquerPaiement(arriere, collecte.Montant);
                collecte.ArrieresAffilieId = arriere.IdArrieresAffilie;

                await _db.SaveChangesAsync(ct);

                _logger.LogInformation("Arriéré {ArriereId} mis à jour pour collecte {CollecteId}",
                    arriere.IdArrieresAffilie, collecte.IdCollecte);

                return arriere;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur traitement arriéré pour collecte {CollecteId}", collecte.IdCollecte);
                throw;
            }
        }

        public async Task<List<ArrieresAffilie>> GetArrieresByAffilieAsync(int affilieId, CancellationToken ct = default)
        {
            return await _db.ArrieresAffilie
                .Include(a => a.Affilie)
                .Include(a => a.Frais)
                .Include(a => a.SouscriptionPrestation)!.ThenInclude(sp => sp!.Prestation)
                .Include(a => a.CotisationAffilie)
                .Include(a => a.Devise)
                .Where(a => a.AffilieId == affilieId && a.Statut)
                .OrderByDescending(a => a.Annee)
                .ThenByDescending(a => a.Mois)
                .ToListAsync(ct);
        }

        public async Task<List<ArrieresAffilie>> GetArrieresByPeriodeAsync(int mois, int annee, CancellationToken ct = default)
        {
            return await _db.ArrieresAffilie
                .Include(a => a.Affilie)
                .Include(a => a.Frais)
                .Include(a => a.SouscriptionPrestation)!.ThenInclude(sp => sp!.Prestation)
                .Include(a => a.CotisationAffilie)
                .Where(a => a.Mois == mois && a.Annee == annee && a.Statut)
                .OrderBy(a => a.Affilie.NomComplet)
                .ToListAsync(ct);
        }

        public async Task<List<ArrieresAffilie>> GetArrieresByStatutPaiementAsync(string statutPaiement, CancellationToken ct = default)
        {
            return await _db.ArrieresAffilie
                .Include(a => a.Affilie)
                .Include(a => a.Frais)
                .Include(a => a.SouscriptionPrestation)!.ThenInclude(sp => sp!.Prestation)
                .Include(a => a.CotisationAffilie)
                .Where(a => a.StatutPaiement == statutPaiement && a.Statut)
                .OrderByDescending(a => a.Annee)
                .ThenByDescending(a => a.Mois)
                .ToListAsync(ct);
        }

        public async Task<ArrieresStatsDto> GetArrieresStatsByAffilieAsync(int affilieId, CancellationToken ct = default)
        {
            var arrieres = await _db.ArrieresAffilie
                .Include(a => a.Affilie)
                .Where(a => a.AffilieId == affilieId && a.Statut)
                .ToListAsync(ct);

            return BuildStats(affilieId, arrieres);
        }

        public async Task<List<ArrieresAffilie>> GenerateArrieresForDateAsync(DateTime date, CancellationToken ct = default)
        {
            var options = await _parametresMetierProvider.GetArrieresAsync(ct);
            var jourEcheance = options.JourEcheanceMensuelle;
            var dateReference = date.Date;
            var nouvelles = new List<ArrieresAffilie>();

            var adhesions = await GetAdhesionsEligiblesAsync(ct);
            foreach (var adhesion in adhesions)
                nouvelles.AddRange(await GenererCotisationsAsync(adhesion, dateReference, jourEcheance, ct));

            var souscriptions = await GetSouscriptionsActivesAsync(ct);
            foreach (var souscription in souscriptions)
            {
                var created = await GenererSouscriptionAsync(souscription, dateReference, jourEcheance, ct);
                if (created != null)
                    nouvelles.Add(created);
            }

            var fraisRecurrents = await _db.Frais
                .AsNoTracking()
                .Where(f => f.Statut && !f.EstSupprime)
                .ToListAsync(ct);

            foreach (var frais in fraisRecurrents.Where(f => ArrieresAffilieRules.EstPeriodiciteRecurrente(f.Periodicite)))
            {
                if (!ArrieresAffilieRules.DoitGenererPourDate(frais.Periodicite, dateReference, jourEcheance))
                    continue;

                foreach (var adhesion in adhesions)
                {
                    var created = await GenererFraisAsync(adhesion.AffilieId, frais, dateReference, jourEcheance, ct);
                    if (created != null)
                        nouvelles.Add(created);
                }
            }

            if (nouvelles.Count > 0)
                await _db.SaveChangesAsync(ct);

            _logger.LogInformation("Génération arriérés pour {Date}: {Count} créés", dateReference, nouvelles.Count);
            return nouvelles;
        }

        public async Task<List<ArrieresAffilie>> GenerateMonthlyArrieresAsync(int mois, int annee, CancellationToken ct = default)
        {
            var options = await _parametresMetierProvider.GetArrieresAsync(ct);
            var date = ArrieresAffilieRules.CalculerDateEcheance("Mensuel", mois, annee, options.JourEcheanceMensuelle);
            return await GenerateArrieresForDateAsync(date, ct);
        }

        public async Task<int> UpdateStatutsRetardAsync(CancellationToken ct = default)
        {
            var today = DateTime.Today;
            var arrieres = await _db.ArrieresAffilie
                .Where(a => a.Statut
                            && a.RestAPayer > 0
                            && a.StatutPaiement == ArrieresAffilieStatuts.EnAttente
                            && a.DateEcheance.Date < today)
                .ToListAsync(ct);

            foreach (var arriere in arrieres)
            {
                arriere.StatutPaiement = ArrieresAffilieStatuts.EnRetard;
                arriere.DateModification = DateTime.Now;
            }

            if (arrieres.Count > 0)
                await _db.SaveChangesAsync(ct);

            return arrieres.Count;
        }

        public async Task<ArrieresAffilie> UpdateStatutArriereAsync(int id, string statutPaiement, CancellationToken ct = default)
        {
            var arriere = await _db.ArrieresAffilie
                .FirstOrDefaultAsync(a => a.IdArrieresAffilie == id, ct)
                ?? throw new ArgumentException($"Arriéré {id} non trouvé");

            arriere.StatutPaiement = statutPaiement;
            arriere.DateModification = DateTime.Now;
            await _db.SaveChangesAsync(ct);
            return arriere;
        }

        public async Task<ArrieresResumeDto> GetArrieresResumeAsync(CancellationToken ct = default)
        {
            var arrieres = await _db.ArrieresAffilie
                .Include(a => a.Affilie)
                .Where(a => a.Statut)
                .ToListAsync(ct);

            var resume = new ArrieresResumeDto
            {
                TotalAffiliesAvecArrieres = arrieres.Select(a => a.AffilieId).Distinct().Count(),
                TotalMontantAttendu = arrieres.Sum(a => a.MontantAttendu),
                TotalMontantPaye = arrieres.Sum(a => a.MontantPaye),
                TotalRestantAPayer = arrieres.Sum(a => a.RestAPayer),
                TotalNombreArrieres = arrieres.Count,
                TotalNombrePayes = arrieres.Count(a => a.StatutPaiement == ArrieresAffilieStatuts.Paye),
                TotalNombreEnRetard = arrieres.Count(a => a.StatutPaiement == ArrieresAffilieStatuts.EnRetard),
                TauxPaiementGlobal = arrieres.Sum(a => a.MontantAttendu) > 0
                    ? (arrieres.Sum(a => a.MontantPaye) / arrieres.Sum(a => a.MontantAttendu)) * 100
                    : 0
            };

            resume.ArrieresParMois = arrieres
                .GroupBy(a => a.Periode)
                .Select(g => new ArrieresParMoisDto
                {
                    Periode = g.Key,
                    MontantAttendu = g.Sum(a => a.MontantAttendu),
                    MontantPaye = g.Sum(a => a.MontantPaye),
                    RestantAPayer = g.Sum(a => a.RestAPayer),
                    NombreArrieres = g.Count(),
                    NombrePayes = g.Count(a => a.StatutPaiement == ArrieresAffilieStatuts.Paye),
                    TauxPaiement = g.Sum(a => a.MontantAttendu) > 0
                        ? (g.Sum(a => a.MontantPaye) / g.Sum(a => a.MontantAttendu)) * 100
                        : 0
                })
                .OrderByDescending(x => x.Periode)
                .ToList();

            resume.ArrieresParStatut = arrieres
                .GroupBy(a => a.StatutPaiement)
                .Select(g => new ArrieresParStatutDto
                {
                    StatutPaiement = g.Key,
                    NombreArrieres = g.Count(),
                    MontantTotal = g.Sum(a => a.RestAPayer),
                    PourcentageTotal = resume.TotalRestantAPayer > 0
                        ? (g.Sum(a => a.RestAPayer) / resume.TotalRestantAPayer) * 100
                        : 0
                })
                .OrderByDescending(x => x.MontantTotal)
                .ToList();

            return resume;
        }

        public async Task<bool> DoitExecuterGenerationAutomatiqueAsync(CancellationToken ct = default)
        {
            var options = await _parametresMetierProvider.GetArrieresAsync(ct);
            if (!options.GenerationAutomatiqueActivee)
                return false;

            var now = DateTime.Now;
            return now.Hour > options.HeureExecution ||
                   (now.Hour == options.HeureExecution && now.Minute >= options.MinuteExecution);
        }

        public async Task ExecuterGenerationAutomatiqueAsync(CancellationToken ct = default)
        {
            await GenerateArrieresForDateAsync(DateTime.Today, ct);
            await UpdateStatutsRetardAsync(ct);
        }

        private async Task<List<Adhesion>> GetAdhesionsEligiblesAsync(CancellationToken ct)
        {
            var adhesions = await _db.Adhesions
                .Include(a => a.Affilie)
                .Include(a => a.TypeAdhesion)
                .Where(a => a.Statut && a.Affilie.Statut)
                .ToListAsync(ct);

            return adhesions.Where(ArrieresAffilieRules.AdhesionEstValidee).ToList();
        }

        private async Task<List<SouscriptionPrestation>> GetSouscriptionsActivesAsync(CancellationToken ct)
        {
            return await _db.SouscriptionsPrestations
                .Include(sp => sp.Affilie)
                .Include(sp => sp.Prestation)!.ThenInclude(p => p!.ProduitMutuel)
                .Include(sp => sp.Prestation)!.ThenInclude(p => p!.ProduitAssureur)
                .Where(sp => sp.Statut && sp.Affilie.Statut)
                .ToListAsync(ct);
        }

        private async Task<List<ArrieresAffilie>> GenererCotisationsAsync(
            Adhesion adhesion,
            DateTime dateReference,
            int jourEcheanceMensuelle,
            CancellationToken ct)
        {
            var result = new List<ArrieresAffilie>();
            var cotisations = await _db.CotisationsAffilie
                .AsNoTracking()
                .Where(c => c.TypeAdhesionId == adhesion.TypeAdhesionId && c.Statut)
                .ToListAsync(ct);

            foreach (var cotisation in cotisations)
            {
                if (!ArrieresAffilieRules.DoitGenererPourDate(
                        cotisation.Periodicite, dateReference, jourEcheanceMensuelle))
                    continue;

                var (mois, annee) = ArrieresAffilieRules.GetPeriodeComptable(cotisation.Periodicite, dateReference);
                if (await ExistsAsync(adhesion.AffilieId, TypeCollecte.Cotisation, mois, annee,
                        cotisationAffilieId: cotisation.IdCotisationAffilie, ct: ct))
                    continue;

                var nombreDependants = await _db.Dependants
                    .CountAsync(d => d.AffilieId == adhesion.AffilieId && d.Statut, ct);
                var calcul = await _cotisationMetier.CalculerMontantTotalAsync(
                    cotisation.IdCotisationAffilie, nombreDependants, ct);

                var arriere = BuildArriere(
                    adhesion.AffilieId,
                    TypeCollecte.Cotisation,
                    mois,
                    annee,
                    cotisation.Periodicite,
                    calcul.MontantTotal,
                    1,
                    $"Cotisation {cotisation.Periodicite} - {mois:D2}/{annee}",
                    jourEcheanceMensuelle,
                    cotisationAffilieId: cotisation.IdCotisationAffilie);

                _db.ArrieresAffilie.Add(arriere);
                result.Add(arriere);
            }

            return result;
        }

        private async Task<ArrieresAffilie?> GenererSouscriptionAsync(
            SouscriptionPrestation souscription,
            DateTime dateReference,
            int jourEcheanceMensuelle,
            CancellationToken ct)
        {
            var produit = souscription.Prestation?.ProduitMutuel as ProduitBase
                          ?? souscription.Prestation?.ProduitAssureur;
            if (produit == null)
                return null;

            var periodicite = produit.Periodicite;
            if (!ArrieresAffilieRules.DoitGenererPourDate(periodicite, dateReference, jourEcheanceMensuelle))
                return null;

            var (mois, annee) = ArrieresAffilieRules.GetPeriodeComptable(periodicite, dateReference);
            if (await ExistsAsync(souscription.AffilieId, TypeCollecte.Souscription, mois, annee,
                    souscriptionPrestationId: souscription.IdSouscriptionPrestation, ct: ct))
                return null;

            var montant = produit.EstGratuit ? 0m : souscription.Prestation!.Montant;
            var deviseId = souscription.Prestation!.DeviseId;

            var arriere = BuildArriere(
                souscription.AffilieId,
                TypeCollecte.Souscription,
                mois,
                annee,
                periodicite,
                montant,
                deviseId,
                $"Souscription {souscription.Prestation.NomPrestation} - {mois:D2}/{annee}",
                jourEcheanceMensuelle,
                souscriptionPrestationId: souscription.IdSouscriptionPrestation);

            _db.ArrieresAffilie.Add(arriere);
            return arriere;
        }

        private async Task<ArrieresAffilie?> GenererFraisAsync(
            int affilieId,
            Frais frais,
            DateTime dateReference,
            int jourEcheanceMensuelle,
            CancellationToken ct)
        {
            var (mois, annee) = ArrieresAffilieRules.GetPeriodeComptable(frais.Periodicite, dateReference);
            if (await ExistsAsync(affilieId, TypeCollecte.Frais, mois, annee, fraisId: frais.IdFrais, ct: ct))
                return null;

            var arriere = BuildArriere(
                affilieId,
                TypeCollecte.Frais,
                mois,
                annee,
                frais.Periodicite,
                (decimal)frais.Montant,
                frais.DeviseId,
                $"Frais {frais.Libelle} - {mois:D2}/{annee}",
                jourEcheanceMensuelle,
                fraisId: frais.IdFrais);

            _db.ArrieresAffilie.Add(arriere);
            return arriere;
        }

        private ArrieresAffilie BuildArriere(
            int affilieId,
            TypeCollecte typeObligation,
            int mois,
            int annee,
            string periodicite,
            decimal montantAttendu,
            int deviseId,
            string description,
            int jourEcheanceMensuelle,
            int? fraisId = null,
            int? souscriptionPrestationId = null,
            int? cotisationAffilieId = null)
        {
            return new ArrieresAffilie
            {
                AffilieId = affilieId,
                TypeObligation = typeObligation,
                FraisId = fraisId,
                SouscriptionPrestationId = souscriptionPrestationId,
                CotisationAffilieId = cotisationAffilieId,
                Mois = mois,
                Annee = annee,
                Periodicite = ArrieresAffilieRules.NormalizePeriodicite(periodicite),
                DateEcheance = ArrieresAffilieRules.CalculerDateEcheance(
                    periodicite, mois, annee, jourEcheanceMensuelle),
                MontantAttendu = montantAttendu,
                MontantPaye = 0,
                RestAPayer = montantAttendu,
                DeviseId = deviseId,
                Description = description,
                StatutPaiement = montantAttendu <= 0
                    ? ArrieresAffilieStatuts.Paye
                    : ArrieresAffilieStatuts.EnAttente,
                Statut = true,
                DateCreation = DateTime.Now
            };
        }

        private async Task<bool> ExistsAsync(
            int affilieId,
            TypeCollecte typeObligation,
            int mois,
            int annee,
            int? fraisId = null,
            int? souscriptionPrestationId = null,
            int? cotisationAffilieId = null,
            CancellationToken ct = default)
        {
            return await _db.ArrieresAffilie.AnyAsync(a =>
                a.AffilieId == affilieId &&
                a.TypeObligation == typeObligation &&
                a.Mois == mois &&
                a.Annee == annee &&
                a.FraisId == fraisId &&
                a.SouscriptionPrestationId == souscriptionPrestationId &&
                a.CotisationAffilieId == cotisationAffilieId, ct);
        }

        private async Task<ArrieresAffilie?> FindExistingAsync(Collecte collecte, CancellationToken ct)
        {
            return collecte.TypeCollecte switch
            {
                TypeCollecte.Frais => await _db.ArrieresAffilie.FirstOrDefaultAsync(a =>
                    a.AffilieId == collecte.AffilieId &&
                    a.TypeObligation == TypeCollecte.Frais &&
                    a.Mois == collecte.Mois &&
                    a.Annee == collecte.Annee &&
                    a.FraisId == collecte.FraisId, ct),
                TypeCollecte.Souscription => await _db.ArrieresAffilie.FirstOrDefaultAsync(a =>
                    a.AffilieId == collecte.AffilieId &&
                    a.TypeObligation == TypeCollecte.Souscription &&
                    a.Mois == collecte.Mois &&
                    a.Annee == collecte.Annee &&
                    a.SouscriptionPrestationId == collecte.SouscriptionPrestationId, ct),
                TypeCollecte.Cotisation => await _db.ArrieresAffilie.FirstOrDefaultAsync(a =>
                    a.AffilieId == collecte.AffilieId &&
                    a.TypeObligation == TypeCollecte.Cotisation &&
                    a.Mois == collecte.Mois &&
                    a.Annee == collecte.Annee &&
                    a.CotisationAffilieId == collecte.CotisationAffilieId, ct),
                _ => null
            };
        }

        private async Task<ArrieresAffilie> CreateFromCollecteAsync(Collecte collecte, CancellationToken ct)
        {
            var options = await _parametresMetierProvider.GetArrieresAsync(ct);
            var (periodicite, deviseId, description, montantAttendu) = await ResolveCollecteMetadataAsync(collecte, ct);
            montantAttendu = collecte.MontantAttendu ?? montantAttendu;

            var arriere = BuildArriere(
                collecte.AffilieId,
                collecte.TypeCollecte,
                collecte.Mois,
                collecte.Annee,
                periodicite,
                montantAttendu,
                deviseId,
                description,
                options.JourEcheanceMensuelle,
                fraisId: collecte.FraisId,
                souscriptionPrestationId: collecte.SouscriptionPrestationId,
                cotisationAffilieId: collecte.CotisationAffilieId);

            arriere.MontantPaye = 0;
            arriere.RestAPayer = montantAttendu;

            _db.ArrieresAffilie.Add(arriere);
            await _db.SaveChangesAsync(ct);
            return arriere;
        }

        private async Task<(string Periodicite, int DeviseId, string Description, decimal MontantAttendu)> ResolveCollecteMetadataAsync(
            Collecte collecte,
            CancellationToken ct)
        {
            switch (collecte.TypeCollecte)
            {
                case TypeCollecte.Frais:
                {
                    var frais = await _db.Frais.AsNoTracking()
                        .FirstOrDefaultAsync(f => f.IdFrais == collecte.FraisId, ct)
                        ?? throw new ArgumentException($"Frais {collecte.FraisId} introuvable");
                    return (frais.Periodicite, frais.DeviseId, $"Frais {frais.Libelle}", (decimal)frais.Montant);
                }
                case TypeCollecte.Souscription:
                {
                    var souscription = await _db.SouscriptionsPrestations
                        .AsNoTracking()
                        .Include(sp => sp.Prestation)!.ThenInclude(p => p!.ProduitMutuel)
                        .Include(sp => sp.Prestation)!.ThenInclude(p => p!.ProduitAssureur)
                        .FirstOrDefaultAsync(sp => sp.IdSouscriptionPrestation == collecte.SouscriptionPrestationId, ct)
                        ?? throw new ArgumentException($"Souscription {collecte.SouscriptionPrestationId} introuvable");

                    var produit = souscription.Prestation?.ProduitMutuel as ProduitBase
                                  ?? souscription.Prestation?.ProduitAssureur;
                    var montant = produit?.EstGratuit == true ? 0m : souscription.Prestation!.Montant;
                    return (produit?.Periodicite ?? "Mensuel", souscription.Prestation!.DeviseId,
                        $"Souscription {souscription.Prestation.NomPrestation}", montant);
                }
                case TypeCollecte.Cotisation:
                {
                    var cotisation = await _db.CotisationsAffilie.AsNoTracking()
                        .Include(c => c.TypeAdhesion)
                        .FirstOrDefaultAsync(c => c.IdCotisationAffilie == collecte.CotisationAffilieId, ct)
                        ?? throw new ArgumentException($"Cotisation {collecte.CotisationAffilieId} introuvable");

                    var nombreDependants = await _db.Dependants
                        .CountAsync(d => d.AffilieId == collecte.AffilieId && d.Statut, ct);
                    var calcul = await _cotisationMetier.CalculerMontantTotalAsync(
                        cotisation.IdCotisationAffilie, nombreDependants, ct);

                    return (cotisation.Periodicite, collecte.DeviseId,
                        $"Cotisation {cotisation.Periodicite}", calcul.MontantTotal);
                }
                default:
                    throw new ArgumentException($"Type de collecte non supporté: {collecte.TypeCollecte}");
            }
        }

        private static ArrieresStatsDto BuildStats(int affilieId, List<ArrieresAffilie> arrieres)
        {
            return new ArrieresStatsDto
            {
                AffilieId = affilieId,
                NomAffilie = arrieres.FirstOrDefault()?.Affilie?.NomComplet ?? "Inconnu",
                TotalMontantAttendu = arrieres.Sum(a => a.MontantAttendu),
                TotalMontantPaye = arrieres.Sum(a => a.MontantPaye),
                TotalRestantAPayer = arrieres.Sum(a => a.RestAPayer),
                NombreArrieres = arrieres.Count,
                NombreArrieresPayes = arrieres.Count(a => a.StatutPaiement == ArrieresAffilieStatuts.Paye),
                NombreArrieresEnRetard = arrieres.Count(a => a.StatutPaiement == ArrieresAffilieStatuts.EnRetard),
                TauxPaiementGlobal = arrieres.Sum(a => a.MontantAttendu) > 0
                    ? (arrieres.Sum(a => a.MontantPaye) / arrieres.Sum(a => a.MontantAttendu)) * 100
                    : 0,
                DerniereMiseAJour = arrieres.Max(a => a.DateModification ?? a.DateCreation)
            };
        }
    }
}
