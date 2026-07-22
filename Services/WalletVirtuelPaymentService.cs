using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using Prosoc.Utilities;
using ProsocAPI.Models.Core;

namespace ProsocAPI.Services
{
    public interface IWalletVirtuelPaymentService
    {
        Task<decimal> ComputeMontantDebitAsync(
            Collecte collecte,
            WalletVirtuelAgent wallet,
            DateTime dateReference,
            CancellationToken ct = default);

        Task ValidateSoldeSuffisantAsync(
            Collecte collecte,
            WalletVirtuelAgent wallet,
            DateTime dateReference,
            CancellationToken ct = default);

        Task ValidateSoldeCumulSuffisantAsync(
            WalletVirtuelAgent wallet,
            decimal montantDebitCumule,
            CancellationToken ct = default);

        Task DebitAsync(Collecte collecte, int agentId, CancellationToken ct = default);
    }

    public class WalletVirtuelPaymentService : IWalletVirtuelPaymentService
    {
        private readonly ProsocDbContext _db;
        private readonly IDeviseConversionService _conversion;
        private readonly IWalletVirtuelMouvementService _mouvementService;

        public WalletVirtuelPaymentService(
            ProsocDbContext db,
            IDeviseConversionService conversion,
            IWalletVirtuelMouvementService mouvementService)
        {
            _db = db;
            _conversion = conversion;
            _mouvementService = mouvementService;
        }

        public async Task<decimal> ComputeMontantDebitAsync(
            Collecte collecte,
            WalletVirtuelAgent wallet,
            DateTime dateReference,
            CancellationToken ct = default)
        {
            if (collecte.DeviseId == wallet.DeviseId)
                return collecte.Montant;

            var datePaiement = CollecteAdhesionHelper.ResolveDateConversionPaiement(
                collecte.ModePaiement, dateReference);

            var (montantConverti, _) = await _conversion.ConvertirAsync(
                collecte.Montant,
                collecte.DeviseId,
                wallet.DeviseId,
                datePaiement,
                ct);

            return montantConverti;
        }

        public async Task ValidateSoldeSuffisantAsync(
            Collecte collecte,
            WalletVirtuelAgent wallet,
            DateTime dateReference,
            CancellationToken ct = default)
        {
            var montantDebit = await ComputeMontantDebitAsync(collecte, wallet, dateReference, ct);
            try
            {
                await ValidateSoldeCumulSuffisantAsync(wallet, montantDebit, ct);
            }
            catch (InvalidOperationException)
            {
                var collecteDevise = await _db.Devises.AsNoTracking()
                    .FirstOrDefaultAsync(d => d.IdDevise == collecte.DeviseId, ct);
                throw new InvalidOperationException(
                    $"Solde virtuel insuffisant. Solde disponible : {wallet.SoldeVirtuel:F2} {wallet.Devise?.Code ?? wallet.DeviseId.ToString()}, " +
                    $"montant requis : {montantDebit:F2} {wallet.Devise?.Code ?? wallet.DeviseId.ToString()} " +
                    $"(paiement {collecte.Montant:F2} {collecteDevise?.Code ?? collecte.DeviseId.ToString()}).");
            }
        }

        public Task ValidateSoldeCumulSuffisantAsync(
            WalletVirtuelAgent wallet,
            decimal montantDebitCumule,
            CancellationToken ct = default)
        {
            if (wallet.SoldeVirtuel < montantDebitCumule)
            {
                throw new InvalidOperationException(
                    $"Solde virtuel insuffisant. Solde disponible : {wallet.SoldeVirtuel:F2} {wallet.Devise?.Code ?? wallet.DeviseId.ToString()}, " +
                    $"montant total requis : {montantDebitCumule:F2} {wallet.Devise?.Code ?? wallet.DeviseId.ToString()}.");
            }

            return Task.CompletedTask;
        }

        public async Task DebitAsync(Collecte collecte, int agentId, CancellationToken ct = default)
        {
            var walletVirtuel = await _db.WalletsVirtuelsAgents
                .Include(w => w.Devise)
                .FirstOrDefaultAsync(w => w.AgentId == agentId && w.Statut, ct);

            if (walletVirtuel == null)
                throw new InvalidOperationException($"Aucun wallet virtuel actif trouvé pour l'agent {agentId}.");

            var montantDebit = await ComputeMontantDebitAsync(
                collecte, walletVirtuel, collecte.DateCollecte, ct);
            await ValidateSoldeCumulSuffisantAsync(walletVirtuel, montantDebit, ct);

            var ancienSolde = walletVirtuel.SoldeVirtuel;
            walletVirtuel.SoldeVirtuel -= montantDebit;
            walletVirtuel.DateModification = DateTime.Now;

            var collecteDevise = await _db.Devises.AsNoTracking()
                .FirstOrDefaultAsync(d => d.IdDevise == collecte.DeviseId, ct);

            var description = collecte.DeviseId == walletVirtuel.DeviseId
                ? $"Collecte {collecte.IdCollecte}"
                : $"Collecte {collecte.IdCollecte} ({collecte.Montant:F2} {collecteDevise?.Code} → {montantDebit:F2} {walletVirtuel.Devise?.Code})";

            await _mouvementService.EnregistrerMouvementAsync(
                walletVirtuel.IdWalletVirtuelAgent,
                montantDebit,
                "DEBIT",
                WalletVirtuelMouvementSources.CollecteCompteVirtuel,
                ancienSolde,
                walletVirtuel.SoldeVirtuel,
                collecte.OperateurUtilisateurId,
                walletVirtuel.DeviseId,
                description,
                collecte.IdCollecte,
                ct);

            await _db.SaveChangesAsync(ct);
        }
    }
}
