using ProsocAPI.Models.DTOs.Core;

namespace ProsocAPI.Services.Repositories
{
    public interface IDashboardAffilieRepository
    {
        // KPIs principaux de l'affilié
        Task<AffilieKpisDto> GetAffilieKpisAsync(int affilieId, CancellationToken ct = default);
        
        // Informations de base de l'affilié
        Task<AffilieInfoDto> GetAffilieInfoAsync(int affilieId, CancellationToken ct = default);
        
        // Historique des cotisations
        Task<List<AffilieCotisationDto>> GetCotisationsAsync(int affilieId, int mois, int annee, CancellationToken ct = default);
        Task<List<AffilieCotisationDto>> GetCotisationsRecentesAsync(int affilieId, int limit = 10, CancellationToken ct = default);
        
        // Historique des prestations
        Task<List<AffiliePrestationDto>> GetPrestationsAsync(int affilieId, int mois, int annee, CancellationToken ct = default);
        Task<List<AffiliePrestationDto>> GetPrestationsRecentesAsync(int affilieId, int limit = 10, CancellationToken ct = default);
        
        // Bénéficiaires de l'affilié
        Task<List<AffilieBeneficiaireDto>> GetBeneficiairesAsync(int affilieId, CancellationToken ct = default);
        
        // Graphiques et statistiques
        Task<AffilieGraphsDto> GetGraphiquesAsync(int affilieId, int annee, CancellationToken ct = default);
        
        // Notifications et alertes
        Task<List<AffilieNotificationDto>> GetNotificationsAsync(int affilieId, int limit = 20, CancellationToken ct = default);
        Task<bool> MarquerNotificationLueAsync(int idNotification, CancellationToken ct = default);
        Task<int> GetNotificationsNonLuesCountAsync(int affilieId, CancellationToken ct = default);
        
        // Documents de l'affilié
        Task<List<AffilieDocumentDto>> GetDocumentsAsync(int affilieId, CancellationToken ct = default);
        Task<List<AffilieDocumentDto>> GetDocumentsEnAttenteAsync(int affilieId, CancellationToken ct = default);
        Task<bool> TelechargerDocumentAsync(int idDocument, CancellationToken ct = default);
        
        // Paramètres et préférences
        Task<AffiliePreferencesDto> GetPreferencesAsync(int affilieId, CancellationToken ct = default);
        Task<bool> UpdatePreferencesAsync(int affilieId, AffiliePreferencesDto preferences, CancellationToken ct = default);
        
        // Résumé complet du dashboard
        Task<AffilieDashboardResumeDto> GetDashboardResumeAsync(int affilieId, int annee, CancellationToken ct = default);
        
        // Statistiques avancées
        Task<AffilieResumeAnnuelDto> GetResumeAnnuelAsync(int affilieId, int annee, CancellationToken ct = default);
        Task<decimal> GetTauxUtilisationMoyenAsync(int affilieId, int mois, int annee, CancellationToken ct = default);
        Task<decimal> GetTauxCouvertureMoyenAsync(int affilieId, int mois, int annee, CancellationToken ct = default);
        
        // Recherche et filtrage
        Task<List<AffilieCotisationDto>> RechercherCotisationsAsync(int affilieId, string? typeCotisation, DateTime? dateDebut, DateTime? dateFin, CancellationToken ct = default);
        Task<List<AffiliePrestationDto>> RechercherPrestationsAsync(int affilieId, string? typePrestation, string? statut, DateTime? dateDebut, DateTime? dateFin, CancellationToken ct = default);
        
        // Export et rapports
        Task<byte[]> ExporterCotisationsAsync(int affilieId, int mois, int annee, string format, CancellationToken ct = default);
        Task<byte[]> ExporterPrestationsAsync(int affilieId, int mois, int annee, string format, CancellationToken ct = default);
        Task<byte[]> ExporterDashboardAsync(int affilieId, int annee, string format, CancellationToken ct = default);
        
        // Alertes et notifications personnalisées
        Task<List<AffilieNotificationDto>> GetAlertesCotisationAsync(int affilieId, CancellationToken ct = default);
        Task<List<AffilieNotificationDto>> GetAlertesPrestationAsync(int affilieId, CancellationToken ct = default);
        Task<List<AffilieNotificationDto>> GetAlertesDocumentAsync(int affilieId, CancellationToken ct = default);
        Task<List<AffilieNotificationDto>> GetAlertesExpirationAsync(int affilieId, CancellationToken ct = default);
        
        // Méthodes utilitaires
        Task<int> GetAgeAffilieAsync(int affilieId, CancellationToken ct = default);
        Task<bool> EstAffilieActifAsync(int affilieId, CancellationToken ct = default);
        Task<decimal> GetPlafondRestantAsync(int affilieId, int annee, CancellationToken ct = default);
        Task<DateTime> GetDateDerniereActiviteAsync(int affilieId, CancellationToken ct = default);
    }
}
