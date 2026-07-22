using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Services.Repositories;
using Prosoc.Data;
using Prosoc.Utilities;

namespace ProsocAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardAffilieController : ControllerBase
    {
        private readonly IDashboardAffilieRepository _dashboardAffilieRepository;
        private readonly ProsocDbContext _db;
        private readonly ILogger<DashboardAffilieController> _logger;

        public DashboardAffilieController(
            IDashboardAffilieRepository dashboardAffilieRepository,
            ProsocDbContext db,
            ILogger<DashboardAffilieController> logger)
        {
            _dashboardAffilieRepository = dashboardAffilieRepository;
            _db = db;
            _logger = logger;
        }

        private async Task<ActionResult?> EnsureOwnAffilieScopeAsync(int affilieId, CancellationToken ct = default) =>
            await AffilieMemberScopeHelper.EnsureOwnAffilieScopeAsync(User, _db, affilieId, ct);

        /// <summary>
        /// Récupère le dashboard complet de l'affilié
        /// </summary>
        [HttpGet("resume/{affilieId}")]
        public async Task<ActionResult<AffilieDashboardResumeDto>> GetDashboardResume(int affilieId, int annee = 0, CancellationToken ct = default)
        {
            try
            {
                var scopeError = await EnsureOwnAffilieScopeAsync(affilieId, ct);
                if (scopeError != null)
                    return scopeError;

                if (annee == 0)
                    annee = DateTime.Now.Year;

                var dashboard = await _dashboardAffilieRepository.GetDashboardResumeAsync(affilieId, annee);
                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du dashboard de l'affilié {AffilieId}", affilieId);
                return this.TechnicalErrorResponse("Erreur lors de la récupération du dashboard", ex);
            }
        }

        /// <summary>
        /// Récupère les KPIs principaux de l'affilié
        /// </summary>
        /// <param name="affilieId">ID de l'affilié</param>
        /// <returns>KPIs de l'affilié</returns>
        [HttpGet("kpis/{affilieId}")]
        public async Task<ActionResult<AffilieKpisDto>> GetAffilieKpis(int affilieId, CancellationToken ct = default)
        {
            try
            {
                var scopeError = await EnsureOwnAffilieScopeAsync(affilieId, ct);
                if (scopeError != null)
                    return scopeError;

                var kpis = await _dashboardAffilieRepository.GetAffilieKpisAsync(affilieId);
                return Ok(kpis);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des KPIs de l'affilié {AffilieId}", affilieId);
                return this.TechnicalErrorResponse("Erreur lors de la récupération des KPIs", ex);
            }
        }

        /// <summary>
        /// Récupère les informations de base de l'affilié
        /// </summary>
        /// <param name="affilieId">ID de l'affilié</param>
        /// <returns>Informations de l'affilié</returns>
        [HttpGet("info/{affilieId}")]
        public async Task<ActionResult<AffilieInfoDto>> GetAffilieInfo(int affilieId, CancellationToken ct = default)
        {
            try
            {
                var scopeError = await EnsureOwnAffilieScopeAsync(affilieId, ct);
                if (scopeError != null)
                    return scopeError;

                var info = await _dashboardAffilieRepository.GetAffilieInfoAsync(affilieId);
                return Ok(info);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des informations de l'affilié {AffilieId}", affilieId);
                return this.TechnicalErrorResponse("Erreur lors de la récupération des informations", ex);
            }
        }

        /// <summary>
        /// Récupère les cotisations de l'affilié pour une période donnée
        /// </summary>
        /// <param name="affilieId">ID de l'affilié</param>
        /// <param name="mois">Mois (1-12)</param>
        /// <param name="annee">Année</param>
        /// <returns>Liste des cotisations</returns>
        [HttpGet("cotisations/{affilieId}")]
        public async Task<ActionResult<List<AffilieCotisationDto>>> GetCotisations(int affilieId, int mois, int annee, CancellationToken ct = default)
        {
            try
            {
                var scopeError = await EnsureOwnAffilieScopeAsync(affilieId, ct);
                if (scopeError != null)
                    return scopeError;

                var cotisations = await _dashboardAffilieRepository.GetCotisationsAsync(affilieId, mois, annee);
                return Ok(cotisations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des cotisations de l'affilié {AffilieId}", affilieId);
                return this.TechnicalErrorResponse("Erreur lors de la récupération des cotisations", ex);
            }
        }

        /// <summary>
        /// Récupère les cotisations récentes de l'affilié
        /// </summary>
        /// <param name="affilieId">ID de l'affilié</param>
        /// <param name="limit">Nombre maximum de résultats</param>
        /// <returns>Liste des cotisations récentes</returns>
        [HttpGet("cotisations/recentes/{affilieId}")]
        public async Task<ActionResult<List<AffilieCotisationDto>>> GetCotisationsRecentes(int affilieId, int limit = 10, CancellationToken ct = default)
        {
            try
            {
                var scopeError = await EnsureOwnAffilieScopeAsync(affilieId, ct);
                if (scopeError != null)
                    return scopeError;

                var cotisations = await _dashboardAffilieRepository.GetCotisationsRecentesAsync(affilieId, limit);
                return Ok(cotisations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des cotisations récentes de l'affilié {AffilieId}", affilieId);
                return this.TechnicalErrorResponse("Erreur lors de la récupération des cotisations récentes", ex);
            }
        }

        /// <summary>
        /// Récupère les prestations de l'affilié pour une période donnée
        /// </summary>
        /// <param name="affilieId">ID de l'affilié</param>
        /// <param name="mois">Mois (1-12)</param>
        /// <param name="annee">Année</param>
        /// <returns>Liste des prestations</returns>
        [HttpGet("prestations/{affilieId}")]
        public async Task<ActionResult<List<AffiliePrestationDto>>> GetPrestations(int affilieId, int mois, int annee, CancellationToken ct = default)
        {
            try
            {
                var scopeError = await EnsureOwnAffilieScopeAsync(affilieId, ct);
                if (scopeError != null)
                    return scopeError;

                var prestations = await _dashboardAffilieRepository.GetPrestationsAsync(affilieId, mois, annee);
                return Ok(prestations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des prestations de l'affilié {AffilieId}", affilieId);
                return this.TechnicalErrorResponse("Erreur lors de la récupération des prestations", ex);
            }
        }

        /// <summary>
        /// Récupère les prestations récentes de l'affilié
        /// </summary>
        /// <param name="affilieId">ID de l'affilié</param>
        /// <param name="limit">Nombre maximum de résultats</param>
        /// <returns>Liste des prestations récentes</returns>
        [HttpGet("prestations/recentes/{affilieId}")]
        public async Task<ActionResult<List<AffiliePrestationDto>>> GetPrestationsRecentes(int affilieId, int limit = 10, CancellationToken ct = default)
        {
            try
            {
                var scopeError = await EnsureOwnAffilieScopeAsync(affilieId, ct);
                if (scopeError != null)
                    return scopeError;

                var prestations = await _dashboardAffilieRepository.GetPrestationsRecentesAsync(affilieId, limit);
                return Ok(prestations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des prestations récentes de l'affilié {AffilieId}", affilieId);
                return this.TechnicalErrorResponse("Erreur lors de la récupération des prestations récentes", ex);
            }
        }

        /// <summary>
        /// Récupère les bénéficiaires de l'affilié
        /// </summary>
        /// <param name="affilieId">ID de l'affilié</param>
        /// <returns>Liste des bénéficiaires</returns>
        [HttpGet("beneficiaires/{affilieId}")]
        public async Task<ActionResult<List<AffilieBeneficiaireDto>>> GetBeneficiaires(int affilieId, CancellationToken ct = default)
        {
            try
            {
                var scopeError = await EnsureOwnAffilieScopeAsync(affilieId, ct);
                if (scopeError != null)
                    return scopeError;

                var beneficiaires = await _dashboardAffilieRepository.GetBeneficiairesAsync(affilieId);
                return Ok(beneficiaires);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des bénéficiaires de l'affilié {AffilieId}", affilieId);
                return this.TechnicalErrorResponse("Erreur lors de la récupération des bénéficiaires", ex);
            }
        }

        /// <summary>
        /// Récupère les graphiques et statistiques de l'affilié
        /// </summary>
        /// <param name="affilieId">ID de l'affilié</param>
        /// <param name="annee">Année des données</param>
        /// <returns>Graphiques de l'affilié</returns>
        [HttpGet("graphiques/{affilieId}")]
        public async Task<ActionResult<AffilieGraphsDto>> GetGraphiques(int affilieId, int annee, CancellationToken ct = default)
        {
            try
            {
                var scopeError = await EnsureOwnAffilieScopeAsync(affilieId, ct);
                if (scopeError != null)
                    return scopeError;

                var graphiques = await _dashboardAffilieRepository.GetGraphiquesAsync(affilieId, annee);
                return Ok(graphiques);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des graphiques de l'affilié {AffilieId}", affilieId);
                return this.TechnicalErrorResponse("Erreur lors de la récupération des graphiques", ex);
            }
        }

        /// <summary>
        /// Récupère les notifications de l'affilié
        /// </summary>
        /// <param name="affilieId">ID de l'affilié</param>
        /// <param name="limit">Nombre maximum de résultats</param>
        /// <returns>Liste des notifications</returns>
        [HttpGet("notifications/{affilieId}")]
        public async Task<ActionResult<List<AffilieNotificationDto>>> GetNotifications(int affilieId, int limit = 20, CancellationToken ct = default)
        {
            try
            {
                var scopeError = await EnsureOwnAffilieScopeAsync(affilieId, ct);
                if (scopeError != null)
                    return scopeError;

                var notifications = await _dashboardAffilieRepository.GetNotificationsAsync(affilieId, limit);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des notifications de l'affilié {AffilieId}", affilieId);
                return this.TechnicalErrorResponse("Erreur lors de la récupération des notifications", ex);
            }
        }

        /// <summary>
        /// Marque une notification comme lue
        /// </summary>
        /// <param name="idNotification">ID de la notification</param>
        /// <returns>Résultat de l'opération</returns>
        [HttpPut("notifications/{idNotification}/lire")]
        public async Task<ActionResult<bool>> MarquerNotificationLue(int idNotification)
        {
            try
            {
                var resultat = await _dashboardAffilieRepository.MarquerNotificationLueAsync(idNotification);
                return Ok(resultat);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du marquage de la notification {IdNotification} comme lue", idNotification);
                return this.TechnicalErrorResponse("Erreur lors du marquage de la notification", ex);
            }
        }

        /// <summary>
        /// Récupère le nombre de notifications non lues
        /// </summary>
        /// <param name="affilieId">ID de l'affilié</param>
        /// <returns>Nombre de notifications non lues</returns>
        [HttpGet("notifications/non-lues/{affilieId}")]
        public async Task<ActionResult<int>> GetNotificationsNonLuesCount(int affilieId, CancellationToken ct = default)
        {
            try
            {
                var scopeError = await EnsureOwnAffilieScopeAsync(affilieId, ct);
                if (scopeError != null)
                    return scopeError;

                var count = await _dashboardAffilieRepository.GetNotificationsNonLuesCountAsync(affilieId);
                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du nombre de notifications non lues de l'affilié {AffilieId}", affilieId);
                return this.TechnicalErrorResponse("Erreur lors de la récupération du nombre de notifications", ex);
            }
        }

        /// <summary>
        /// Récupère les documents de l'affilié
        /// </summary>
        /// <param name="affilieId">ID de l'affilié</param>
        /// <returns>Liste des documents</returns>
        [HttpGet("documents/{affilieId}")]
        public async Task<ActionResult<List<AffilieDocumentDto>>> GetDocuments(int affilieId, CancellationToken ct = default)
        {
            try
            {
                var scopeError = await EnsureOwnAffilieScopeAsync(affilieId, ct);
                if (scopeError != null)
                    return scopeError;

                var documents = await _dashboardAffilieRepository.GetDocumentsAsync(affilieId);
                return Ok(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des documents de l'affilié {AffilieId}", affilieId);
                return this.TechnicalErrorResponse("Erreur lors de la récupération des documents", ex);
            }
        }

        /// <summary>
        /// Récupère les documents en attente de validation
        /// </summary>
        /// <param name="affilieId">ID de l'affilié</param>
        /// <returns>Liste des documents en attente</returns>
        [HttpGet("documents/en-attente/{affilieId}")]
        public async Task<ActionResult<List<AffilieDocumentDto>>> GetDocumentsEnAttente(int affilieId, CancellationToken ct = default)
        {
            try
            {
                var scopeError = await EnsureOwnAffilieScopeAsync(affilieId, ct);
                if (scopeError != null)
                    return scopeError;

                var documents = await _dashboardAffilieRepository.GetDocumentsEnAttenteAsync(affilieId);
                return Ok(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des documents en attente de l'affilié {AffilieId}", affilieId);
                return this.TechnicalErrorResponse("Erreur lors de la récupération des documents en attente", ex);
            }
        }

        /// <summary>
        /// Récupère les préférences de l'affilié
        /// </summary>
        /// <param name="affilieId">ID de l'affilié</param>
        /// <returns>Préférences de l'affilié</returns>
        [HttpGet("preferences/{affilieId}")]
        public async Task<ActionResult<AffiliePreferencesDto>> GetPreferences(int affilieId, CancellationToken ct = default)
        {
            try
            {
                var scopeError = await EnsureOwnAffilieScopeAsync(affilieId, ct);
                if (scopeError != null)
                    return scopeError;

                var preferences = await _dashboardAffilieRepository.GetPreferencesAsync(affilieId);
                return Ok(preferences);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des préférences de l'affilié {AffilieId}", affilieId);
                return this.TechnicalErrorResponse("Erreur lors de la récupération des préférences", ex);
            }
        }

        /// <summary>
        /// Met à jour les préférences de l'affilié
        /// </summary>
        /// <param name="affilieId">ID de l'affilié</param>
        /// <param name="preferences">Nouvelles préférences</param>
        /// <returns>Résultat de l'opération</returns>
        [HttpPut("preferences/{affilieId}")]
        public async Task<ActionResult<bool>> UpdatePreferences(int affilieId, [FromBody] AffiliePreferencesDto preferences, CancellationToken ct = default)
        {
            try
            {
                var scopeError = await EnsureOwnAffilieScopeAsync(affilieId, ct);
                if (scopeError != null)
                    return scopeError;

                var resultat = await _dashboardAffilieRepository.UpdatePreferencesAsync(affilieId, preferences);
                return Ok(resultat);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour des préférences de l'affilié {AffilieId}", affilieId);
                return this.TechnicalErrorResponse("Erreur lors de la mise à jour des préférences", ex);
            }
        }

        /// <summary>
        /// Récupère le résumé annuel de l'affilié
        /// </summary>
        /// <param name="affilieId">ID de l'affilié</param>
        /// <param name="annee">Année</param>
        /// <returns>Résumé annuel</returns>
        [HttpGet("resume-annuel/{affilieId}")]
        public async Task<ActionResult<AffilieResumeAnnuelDto>> GetResumeAnnuel(int affilieId, int annee, CancellationToken ct = default)
        {
            try
            {
                var scopeError = await EnsureOwnAffilieScopeAsync(affilieId, ct);
                if (scopeError != null)
                    return scopeError;

                var resume = await _dashboardAffilieRepository.GetResumeAnnuelAsync(affilieId, annee);
                return Ok(resume);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du résumé annuel de l'affilié {AffilieId}", affilieId);
                return this.TechnicalErrorResponse("Erreur lors de la récupération du résumé annuel", ex);
            }
        }

        /// <summary>
        /// Exporte les cotisations de l'affilié
        /// </summary>
        /// <param name="affilieId">ID de l'affilié</param>
        /// <param name="mois">Mois</param>
        /// <param name="annee">Année</param>
        /// <param name="format">Format d'export (PDF, Excel, CSV)</param>
        /// <returns>Fichier exporté</returns>
        [HttpGet("export/cotisations/{affilieId}")]
        public async Task<ActionResult<byte[]>> ExporterCotisations(int affilieId, int mois, int annee, string format = "PDF", CancellationToken ct = default)
        {
            try
            {
                var scopeError = await EnsureOwnAffilieScopeAsync(affilieId, ct);
                if (scopeError != null)
                    return scopeError;

                var data = await _dashboardAffilieRepository.ExporterCotisationsAsync(affilieId, mois, annee, format);
                var contentType = format.ToUpper() switch
                {
                    "PDF" => "application/pdf",
                    "EXCEL" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "CSV" => "text/csv",
                    _ => "application/octet-stream"
                };
                
                return File(data, contentType, $"cotisations_{affilieId}_{mois}_{annee}.{format.ToLower()}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'export des cotisations de l'affilié {AffilieId}", affilieId);
                return this.TechnicalErrorResponse("Erreur lors de l'export", ex);
            }
        }

        /// <summary>
        /// Exporte les prestations de l'affilié
        /// </summary>
        /// <param name="affilieId">ID de l'affilié</param>
        /// <param name="mois">Mois</param>
        /// <param name="annee">Année</param>
        /// <param name="format">Format d'export (PDF, Excel, CSV)</param>
        /// <returns>Fichier exporté</returns>
        [HttpGet("export/prestations/{affilieId}")]
        public async Task<ActionResult<byte[]>> ExporterPrestations(int affilieId, int mois, int annee, string format = "PDF", CancellationToken ct = default)
        {
            try
            {
                var scopeError = await EnsureOwnAffilieScopeAsync(affilieId, ct);
                if (scopeError != null)
                    return scopeError;

                var data = await _dashboardAffilieRepository.ExporterPrestationsAsync(affilieId, mois, annee, format);
                var contentType = format.ToUpper() switch
                {
                    "PDF" => "application/pdf",
                    "EXCEL" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "CSV" => "text/csv",
                    _ => "application/octet-stream"
                };
                
                return File(data, contentType, $"prestations_{affilieId}_{mois}_{annee}.{format.ToLower()}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'export des prestations de l'affilié {AffilieId}", affilieId);
                return this.TechnicalErrorResponse("Erreur lors de l'export", ex);
            }
        }

        /// <summary>
        /// Récupère les alertes de cotisation
        /// </summary>
        /// <param name="affilieId">ID de l'affilié</param>
        /// <returns>Liste des alertes</returns>
        [HttpGet("alertes/cotisation/{affilieId}")]
        public async Task<ActionResult<List<AffilieNotificationDto>>> GetAlertesCotisation(int affilieId, CancellationToken ct = default)
        {
            try
            {
                var scopeError = await EnsureOwnAffilieScopeAsync(affilieId, ct);
                if (scopeError != null)
                    return scopeError;

                var alertes = await _dashboardAffilieRepository.GetAlertesCotisationAsync(affilieId);
                return Ok(alertes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des alertes de cotisation de l'affilié {AffilieId}", affilieId);
                return this.TechnicalErrorResponse("Erreur lors de la récupération des alertes", ex);
            }
        }

        /// <summary>
        /// Récupère les alertes de prestation
        /// </summary>
        /// <param name="affilieId">ID de l'affilié</param>
        /// <returns>Liste des alertes</returns>
        [HttpGet("alertes/prestation/{affilieId}")]
        public async Task<ActionResult<List<AffilieNotificationDto>>> GetAlertesPrestation(int affilieId, CancellationToken ct = default)
        {
            try
            {
                var scopeError = await EnsureOwnAffilieScopeAsync(affilieId, ct);
                if (scopeError != null)
                    return scopeError;

                var alertes = await _dashboardAffilieRepository.GetAlertesPrestationAsync(affilieId);
                return Ok(alertes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des alertes de prestation de l'affilié {AffilieId}", affilieId);
                return this.TechnicalErrorResponse("Erreur lors de la récupération des alertes", ex);
            }
        }

        /// <summary>
        /// Récupère les alertes de document
        /// </summary>
        /// <param name="affilieId">ID de l'affilié</param>
        /// <returns>Liste des alertes</returns>
        [HttpGet("alertes/document/{affilieId}")]
        public async Task<ActionResult<List<AffilieNotificationDto>>> GetAlertesDocument(int affilieId, CancellationToken ct = default)
        {
            try
            {
                var scopeError = await EnsureOwnAffilieScopeAsync(affilieId, ct);
                if (scopeError != null)
                    return scopeError;

                var alertes = await _dashboardAffilieRepository.GetAlertesDocumentAsync(affilieId);
                return Ok(alertes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des alertes de document de l'affilié {AffilieId}", affilieId);
                return this.TechnicalErrorResponse("Erreur lors de la récupération des alertes", ex);
            }
        }

        /// <summary>
        /// Récupère les alertes d'expiration
        /// </summary>
        /// <param name="affilieId">ID de l'affilié</param>
        /// <returns>Liste des alertes</returns>
        [HttpGet("alertes/expiration/{affilieId}")]
        public async Task<ActionResult<List<AffilieNotificationDto>>> GetAlertesExpiration(int affilieId, CancellationToken ct = default)
        {
            try
            {
                var scopeError = await EnsureOwnAffilieScopeAsync(affilieId, ct);
                if (scopeError != null)
                    return scopeError;

                var alertes = await _dashboardAffilieRepository.GetAlertesExpirationAsync(affilieId);
                return Ok(alertes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des alertes d'expiration de l'affilié {AffilieId}", affilieId);
                return this.TechnicalErrorResponse("Erreur lors de la récupération des alertes", ex);
            }
        }
    }
}
