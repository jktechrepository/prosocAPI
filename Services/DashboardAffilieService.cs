using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Prosoc.Data;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Services.Repositories;
using Prosoc.Utilities;

namespace ProsocAPI.Services
{
    public class DashboardAffilieService : IDashboardAffilieRepository
    {
        private readonly ProsocDbContext _db;
        private readonly IAffilieConformiteService _conformiteService;
        private readonly ILogger<DashboardAffilieService> _logger;

        public DashboardAffilieService(
            ProsocDbContext db,
            IAffilieConformiteService conformiteService,
            ILogger<DashboardAffilieService> logger)
        {
            _db = db;
            _conformiteService = conformiteService;
            _logger = logger;
        }

        public async Task<AffilieKpisDto> GetAffilieKpisAsync(int affilieId, CancellationToken ct = default)
        {
            try
            {
                var affilie = await _db.Affilies
                    .Include(a => a.Adhesion)
                    .FirstOrDefaultAsync(a => a.IdAffilie == affilieId, ct);

                if (affilie == null)
                    return new AffilieKpisDto();

                var dateActuelle = DateTime.Now;
                var debutAnnee = new DateTime(dateActuelle.Year, 1, 1);
                var finAnnee = new DateTime(dateActuelle.Year, 12, 31);

                var devisePrincipaleCode = await _db.Devises
                    .AsNoTracking()
                    .Where(d => d.EstDevisePrincipale && d.Statut)
                    .Select(d => d.Code)
                    .FirstOrDefaultAsync(ct);

                // Cotisations affilié (hors collectes liées à une souscription prestation)
                var cotisationsCollectes = await _db.Collectes
                    .AsNoTracking()
                    .Where(c => c.AffilieId == affilieId
                        && c.SouscriptionPrestationId == null
                        && c.DateCollecte >= debutAnnee
                        && c.DateCollecte <= finAnnee
                        && c.StatutPaiement != null
                        && CollecteStatutPaiementRegles.ValeursSqlValideEtLegacy.Contains(c.StatutPaiement))
                    .ToListAsync(ct);
                var totalCotisations = cotisationsCollectes.Sum(CollecteStatutPaiementRegles.MontantEnDevisePrincipale);

                var souscriptionsPrestations = await _db.SouscriptionsPrestations
                    .AsNoTracking()
                    .Where(sp => sp.AffilieId == affilieId && sp.DateCreation >= debutAnnee && sp.DateCreation <= finAnnee)
                    .Select(sp => sp.IdSouscriptionPrestation)
                    .ToListAsync(ct);

                var prestationsCollectes = souscriptionsPrestations.Count == 0
                    ? new List<Collecte>()
                    : await _db.Collectes
                        .AsNoTracking()
                        .Where(c => c.SouscriptionPrestationId != null
                            && souscriptionsPrestations.Contains(c.SouscriptionPrestationId.Value))
                        .ToListAsync(ct);
                var totalPrestations = prestationsCollectes.Sum(CollecteStatutPaiementRegles.MontantEnDevisePrincipale);

                var nombrePrestations = souscriptionsPrestations.Count;

                var derniereCotisation = await _db.Collectes
                    .Where(c => c.AffilieId == affilieId && c.SouscriptionPrestationId == null)
                    .OrderByDescending(c => c.DateCollecte)
                    .FirstOrDefaultAsync(ct);

                var dernierePrestationCollecte = await (from c in _db.Collectes
                                        join sp in _db.SouscriptionsPrestations on c.SouscriptionPrestationId equals sp.IdSouscriptionPrestation
                                        where sp.AffilieId == affilieId
                                        orderby c.DateCollecte descending
                                        select c).FirstOrDefaultAsync(ct);

                var ancienneteMois = (dateActuelle.Year - affilie.DateCreation.Year) * 12 + 
                                   (dateActuelle.Month - affilie.DateCreation.Month);

                var tauxUtilisation = totalCotisations > 0 ? (totalPrestations / totalCotisations) * 100 : 0;

                var conformite = await _conformiteService.GetConformiteAffilieAsync(affilieId, ct)
                    ?? new AffilieConformiteDto { AffilieId = affilieId };

                return new AffilieKpisDto
                {
                    IdAffilie = affilieId,
                    CodeAdhesion = affilie.CodeAdhesion,
                    NomComplet = affilie.NomComplet ?? string.Empty,
                    SoldeTotal = totalCotisations - totalPrestations,
                    SoldeDisponible = totalCotisations - totalPrestations,
                    TotalCotisations = totalCotisations,
                    TotalPrestations = totalPrestations,
                    DevisePrincipaleCode = devisePrincipaleCode,
                    NombrePrestations = nombrePrestations,
                    MontantDerniereCotisation = derniereCotisation != null
                        ? CollecteStatutPaiementRegles.MontantEnDevisePrincipale(derniereCotisation)
                        : 0,
                    DateDerniereCotisation = derniereCotisation?.DateCollecte,
                    MontantDernierePrestation = dernierePrestationCollecte != null
                        ? CollecteStatutPaiementRegles.MontantEnDevisePrincipale(dernierePrestationCollecte)
                        : 0,
                    DateDernierePrestation = dernierePrestationCollecte?.DateCollecte,
                    StatutAdhesion = affilie.Statut ? "Actif" : "Inactif",
                    DateAdhesion = affilie.DateCreation,
                    AncienneteMois = Math.Max(0, ancienneteMois),
                    TauxUtilisation = tauxUtilisation,
                    TauxCouverture = 100, // À calculer selon logique métier
                    EstActif = affilie.Statut,
                    NombreBeneficiaires = 1, // À calculer selon bénéficiaires
                    MontantPlafond = 1000000, // À configurer selon type d'adhésion
                    RestePlafond = 1000000 - totalPrestations,
                    StatutGlobal = conformite.StatutGlobal,
                    StatutCotisation = conformite.StatutCotisation,
                    StatutPrestation = conformite.StatutPrestation,
                    NombreArrieresOuverts = conformite.NombreArrieresOuverts,
                    MontantRestantDu = conformite.MontantRestantDu
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des KPIs de l'affilié {AffilieId}", affilieId);
                return new AffilieKpisDto();
            }
        }

        public async Task<AffilieInfoDto> GetAffilieInfoAsync(int affilieId, CancellationToken ct = default)
        {
            try
            {
                var affilie = await _db.Affilies
                    .Include(a => a.Adhesion)
                        .ThenInclude(ad => ad!.TypeAdhesion)
                    .FirstOrDefaultAsync(a => a.IdAffilie == affilieId, ct);

                if (affilie == null)
                    return new AffilieInfoDto();

                var adhesion = affilie.Adhesion;

                return new AffilieInfoDto
                {
                    IdAffilie = affilie.IdAffilie,
                    CodeAdhesion = affilie.CodeAdhesion,
                    NomComplet = affilie.NomComplet ?? string.Empty,
                    Telephone = affilie.Telephone ?? string.Empty,
                    Email = string.Empty, // À ajouter si disponible
                    DateNaissance = affilie.DateNaissance,
                    PhotoUrl = AffilieFichierHelper.ADesDonnees(affilie.PhotoData)
                        ? $"/api/affilie/{affilie.IdAffilie}/photo"
                        : null,
                    DateAdhesion = affilie.DateCreation,
                    StatutAdhesion = affilie.Statut ? "Actif" : "Inactif",
                    EstActif = affilie.Statut,
                    ProvinceResidence = affilie.ProvinceResidence,
                    CommuneResidence = affilie.CommuneResidence,
                    QuartierResidence = affilie.QuartierResidence,
                    AvenueResidence = affilie.AvenueResidence,
                    NumeroResidence = affilie.NumeroResidence,
                    CommuneActivite = affilie.CommuneActivite,
                    QuartierActivite = affilie.QuartierActivite,
                    AvenueActivite = affilie.AvenueActivite,
                    NumeroActivite = affilie.NumeroActivite,
                    NombreBeneficiaires = 1, // À calculer
                    TypeAdhesion = adhesion?.TypeAdhesion?.Libelle ?? string.Empty,
                    CategorieAdhesion = string.Empty // À ajouter
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des informations de l'affilié {AffilieId}", affilieId);
                return new AffilieInfoDto();
            }
        }

        public async Task<AffilieDashboardResumeDto> GetDashboardResumeAsync(int affilieId, int annee, CancellationToken ct = default)
        {
            try
            {
                var kpis = await GetAffilieKpisAsync(affilieId, ct);
                var infos = await GetAffilieInfoAsync(affilieId, ct);
                var cotisations = await GetCotisationsRecentesAsync(affilieId, 5, ct);
                var prestations = await GetPrestationsRecentesAsync(affilieId, 5, ct);
                var beneficiaires = await GetBeneficiairesAsync(affilieId, ct);
                var graphiques = await GetGraphiquesAsync(affilieId, annee, ct);
                var notifications = await GetNotificationsAsync(affilieId, 10, ct);
                var documents = await GetDocumentsEnAttenteAsync(affilieId, ct);
                var preferences = await GetPreferencesAsync(affilieId, ct);

                return new AffilieDashboardResumeDto
                {
                    Kpis = kpis,
                    Informations = infos,
                    CotisationsRecentes = cotisations,
                    PrestationsRecentes = prestations,
                    Beneficiaires = beneficiaires,
                    Graphiques = graphiques,
                    NotificationsRecentes = notifications,
                    DocumentsEnAttente = documents,
                    Preferences = preferences
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du dashboard de l'affilié {AffilieId}", affilieId);
                return new AffilieDashboardResumeDto();
            }
        }

        // Implémentation des autres méthodes (simplifiées pour l'exemple)
        public async Task<List<AffilieCotisationDto>> GetCotisationsAsync(int affilieId, int mois, int annee, CancellationToken ct = default)
        {
            return new List<AffilieCotisationDto>();
        }

        public async Task<List<AffilieCotisationDto>> GetCotisationsRecentesAsync(int affilieId, int limit = 10, CancellationToken ct = default)
        {
            return new List<AffilieCotisationDto>();
        }

        public async Task<List<AffiliePrestationDto>> GetPrestationsAsync(int affilieId, int mois, int annee, CancellationToken ct = default)
        {
            return new List<AffiliePrestationDto>();
        }

        public async Task<List<AffiliePrestationDto>> GetPrestationsRecentesAsync(int affilieId, int limit = 10, CancellationToken ct = default)
        {
            return new List<AffiliePrestationDto>();
        }

        public async Task<List<AffilieBeneficiaireDto>> GetBeneficiairesAsync(int affilieId, CancellationToken ct = default)
        {
            return new List<AffilieBeneficiaireDto>();
        }

        public async Task<AffilieGraphsDto> GetGraphiquesAsync(int affilieId, int annee, CancellationToken ct = default)
        {
            return new AffilieGraphsDto();
        }

        public async Task<List<AffilieNotificationDto>> GetNotificationsAsync(int affilieId, int limit = 20, CancellationToken ct = default)
        {
            return new List<AffilieNotificationDto>();
        }

        public async Task<bool> MarquerNotificationLueAsync(int idNotification, CancellationToken ct = default)
        {
            return true;
        }

        public async Task<int> GetNotificationsNonLuesCountAsync(int affilieId, CancellationToken ct = default)
        {
            return 0;
        }

        public async Task<List<AffilieDocumentDto>> GetDocumentsAsync(int affilieId, CancellationToken ct = default)
        {
            return new List<AffilieDocumentDto>();
        }

        public async Task<List<AffilieDocumentDto>> GetDocumentsEnAttenteAsync(int affilieId, CancellationToken ct = default)
        {
            return new List<AffilieDocumentDto>();
        }

        public async Task<bool> TelechargerDocumentAsync(int idDocument, CancellationToken ct = default)
        {
            return true;
        }

        public async Task<AffiliePreferencesDto> GetPreferencesAsync(int affilieId, CancellationToken ct = default)
        {
            return new AffiliePreferencesDto { IdAffilie = affilieId };
        }

        public async Task<bool> UpdatePreferencesAsync(int affilieId, AffiliePreferencesDto preferences, CancellationToken ct = default)
        {
            return true;
        }

        public async Task<AffilieResumeAnnuelDto> GetResumeAnnuelAsync(int affilieId, int annee, CancellationToken ct = default)
        {
            return new AffilieResumeAnnuelDto { Annee = annee };
        }

        public async Task<decimal> GetTauxUtilisationMoyenAsync(int affilieId, int mois, int annee, CancellationToken ct = default)
        {
            return 0;
        }

        public async Task<decimal> GetTauxCouvertureMoyenAsync(int affilieId, int mois, int annee, CancellationToken ct = default)
        {
            return 0;
        }

        public async Task<List<AffilieCotisationDto>> RechercherCotisationsAsync(int affilieId, string? typeCotisation, DateTime? dateDebut, DateTime? dateFin, CancellationToken ct = default)
        {
            return new List<AffilieCotisationDto>();
        }

        public async Task<List<AffiliePrestationDto>> RechercherPrestationsAsync(int affilieId, string? typePrestation, string? statut, DateTime? dateDebut, DateTime? dateFin, CancellationToken ct = default)
        {
            return new List<AffiliePrestationDto>();
        }

        public async Task<byte[]> ExporterCotisationsAsync(int affilieId, int mois, int annee, string format, CancellationToken ct = default)
        {
            return Array.Empty<byte>();
        }

        public async Task<byte[]> ExporterPrestationsAsync(int affilieId, int mois, int annee, string format, CancellationToken ct = default)
        {
            return Array.Empty<byte>();
        }

        public async Task<byte[]> ExporterDashboardAsync(int affilieId, int annee, string format, CancellationToken ct = default)
        {
            return Array.Empty<byte>();
        }

        public async Task<List<AffilieNotificationDto>> GetAlertesCotisationAsync(int affilieId, CancellationToken ct = default)
        {
            return new List<AffilieNotificationDto>();
        }

        public async Task<List<AffilieNotificationDto>> GetAlertesPrestationAsync(int affilieId, CancellationToken ct = default)
        {
            return new List<AffilieNotificationDto>();
        }

        public async Task<List<AffilieNotificationDto>> GetAlertesDocumentAsync(int affilieId, CancellationToken ct = default)
        {
            return new List<AffilieNotificationDto>();
        }

        public async Task<List<AffilieNotificationDto>> GetAlertesExpirationAsync(int affilieId, CancellationToken ct = default)
        {
            return new List<AffilieNotificationDto>();
        }

        public async Task<int> GetAgeAffilieAsync(int affilieId, CancellationToken ct = default)
        {
            return 0;
        }

        public async Task<bool> EstAffilieActifAsync(int affilieId, CancellationToken ct = default)
        {
            return true;
        }

        public async Task<decimal> GetPlafondRestantAsync(int affilieId, int annee, CancellationToken ct = default)
        {
            return 0;
        }

        public async Task<DateTime> GetDateDerniereActiviteAsync(int affilieId, CancellationToken ct = default)
        {
            return DateTime.Now;
        }
    }
}
