using ProsocAPI.Models.Core;

namespace ProsocAPI.Services
{
    public interface ICommissionNotificationService
    {
        /// <summary>
        /// Notifie un agent qu'il a gagné une commission
        /// </summary>
        /// <param name="agentId">ID de l'agent</param>
        /// <param name="commissionAmount">Montant de la commission</param>
        /// <param name="collecte">Collecte concernée</param>
        /// <param name="ancienSolde">Ancien solde du wallet</param>
        /// <param name="nouveauSolde">Nouveau solde du wallet</param>
        /// <param name="ct">Token d'annulation</param>
        Task NotifyCommissionEarnedAsync(
            int agentId, 
            decimal commissionAmount, 
            Collecte collecte,
            decimal ancienSolde,
            decimal nouveauSolde,
            CancellationToken ct = default);
    }
}
