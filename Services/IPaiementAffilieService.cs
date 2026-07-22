using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Services;

namespace ProsocAPI.Services
{
    public interface IPaiementAffilieService
    {
        /// <summary>
        /// Récupérer les souscriptions payables par un affilié
        /// </summary>
        Task<List<SouscriptionPrestation>> GetSouscriptionsPayablesAsync(int affilieId, CancellationToken ct = default);

        /// <summary>
        /// Permettre à un affilié de payer sa souscription
        /// </summary>
        Task<Collecte> PayerSouscriptionAsync(int affilieId, PayerSouscriptionDto dto, CancellationToken ct = default);

        /// <summary>
        /// Requête paginable : historique des paiements (souscriptions) d'un affilié.
        /// </summary>
        IQueryable<Collecte> GetHistoriquePaiementsQuery(int affilieId);

        /// <summary>
        /// Récupérer l'historique des paiements d'un affilié
        /// </summary>
        Task<List<Collecte>> GetHistoriquePaiementsAsync(int affilieId, CancellationToken ct = default);

        /// <summary>
        /// Valider automatiquement un paiement d'affilié
        /// </summary>
        Task<bool> ValiderPaiementAutomatiqueAsync(Collecte collecte, CancellationToken ct = default);

        /// <summary>
        /// Calculer la commission pour un paiement d'affilié
        /// </summary>
        Task<Commission> CalculerCommissionAsync(Collecte collecte, CancellationToken ct = default);

        /// <summary>
        /// Vérifier si une souscription appartient à un affilié
        /// </summary>
        Task<bool> VerifierProprieteSouscriptionAsync(int souscriptionId, int affilieId, CancellationToken ct = default);

        /// <summary>
        /// Vérifier si une souscription est déjà payée
        /// </summary>
        Task<bool> EstSouscriptionDejaPayeeAsync(int souscriptionId, CancellationToken ct = default);
    }
}
