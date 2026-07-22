using Prosoc.Data;
using ProsocAPI.Models.Core;

namespace ProsocAPI.Services
{
    public interface IWalletVirtuelMouvementService
    {
        Task EnregistrerMouvementAsync(
            int walletVirtuelId,
            decimal montant,
            string typeOperation,
            string source,
            decimal soldeAvant,
            decimal soldeApres,
            int? operateurUtilisateurId = null,
            int? deviseId = null,
            string? description = null,
            int? referenceExterne = null,
            CancellationToken ct = default);

        Task EnregistrerDeltaSoldeAsync(
            int walletVirtuelId,
            decimal ancienSolde,
            decimal nouveauSolde,
            string source,
            int? operateurUtilisateurId = null,
            int? deviseId = null,
            string? description = null,
            int? referenceExterne = null,
            CancellationToken ct = default);
    }

    public class WalletVirtuelMouvementService : IWalletVirtuelMouvementService
    {
        private readonly ProsocDbContext _db;

        public WalletVirtuelMouvementService(ProsocDbContext db)
        {
            _db = db;
        }

        public Task EnregistrerMouvementAsync(
            int walletVirtuelId,
            decimal montant,
            string typeOperation,
            string source,
            decimal soldeAvant,
            decimal soldeApres,
            int? operateurUtilisateurId = null,
            int? deviseId = null,
            string? description = null,
            int? referenceExterne = null,
            CancellationToken ct = default)
        {
            if (montant <= 0)
                throw new ArgumentOutOfRangeException(nameof(montant), "Le montant du mouvement doit être positif.");

            _db.WalletVirtuelMouvements.Add(new WalletVirtuelMouvement
            {
                WalletVirtuelId = walletVirtuelId,
                DeviseId = deviseId,
                Montant = montant,
                TypeOperation = typeOperation,
                Source = source,
                Description = description,
                ReferenceExterne = referenceExterne,
                SoldeAvant = soldeAvant,
                SoldeApres = soldeApres,
                OperateurUtilisateurId = operateurUtilisateurId,
                DateOperation = DateTime.Now,
                DateCreation = DateTime.Now,
                Statut = true
            });

            return Task.CompletedTask;
        }

        public Task EnregistrerDeltaSoldeAsync(
            int walletVirtuelId,
            decimal ancienSolde,
            decimal nouveauSolde,
            string source,
            int? operateurUtilisateurId = null,
            int? deviseId = null,
            string? description = null,
            int? referenceExterne = null,
            CancellationToken ct = default)
        {
            var delta = nouveauSolde - ancienSolde;
            if (delta == 0)
                return Task.CompletedTask;

            var typeOperation = delta > 0 ? "CREDIT" : "DEBIT";
            return EnregistrerMouvementAsync(
                walletVirtuelId,
                Math.Abs(delta),
                typeOperation,
                source,
                ancienSolde,
                nouveauSolde,
                operateurUtilisateurId,
                deviseId,
                description,
                referenceExterne,
                ct);
        }
    }
}
