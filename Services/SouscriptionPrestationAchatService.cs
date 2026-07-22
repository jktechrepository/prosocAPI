using System.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Prosoc.Data;
using Prosoc.Utilities;
using ProsocAPI.Helpers;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;

namespace ProsocAPI.Services
{
    public interface ISouscriptionPrestationAchatService
    {
        Task<(SouscriptionPrestation Souscription, Collecte Collecte)> CreateWithCollecteAsync(
            int affilieId,
            SouscriptionPrestationAchatCreateDto dto,
            CancellationToken ct = default);
    }

    public class SouscriptionPrestationAchatService : ISouscriptionPrestationAchatService
    {
        private readonly ProsocDbContext _db;
        private readonly ICollecteMultideviseService _multidevise;
        private readonly IWalletVirtuelPaymentService _walletVirtuelPayment;
        private readonly ICommissionService _commissionService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<SouscriptionPrestationAchatService> _logger;

        public SouscriptionPrestationAchatService(
            ProsocDbContext db,
            ICollecteMultideviseService multidevise,
            IWalletVirtuelPaymentService walletVirtuelPayment,
            ICommissionService commissionService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<SouscriptionPrestationAchatService> logger)
        {
            _db = db;
            _multidevise = multidevise;
            _walletVirtuelPayment = walletVirtuelPayment;
            _commissionService = commissionService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<(SouscriptionPrestation Souscription, Collecte Collecte)> CreateWithCollecteAsync(
            int affilieId,
            SouscriptionPrestationAchatCreateDto dto,
            CancellationToken ct = default)
        {
            if (dto.Collecte == null)
                throw new ArgumentException("Le bloc collecte est obligatoire pour souscrire à une prestation.");

            var collecteDto = dto.Collecte;
            var modePaiement = MethodePaiementHelper.NormalizeForStorage(collecteDto.ModePaiement);

            if (MethodePaiementHelper.IsFlexPay(modePaiement))
            {
                throw new ArgumentException(
                    "Les paiements MOBILE_MONEY et CARTE_BANCAIRE pour une nouvelle souscription doivent passer par " +
                    "POST /api/SouscriptionPrestation/paiement-electronique. " +
                    "Utilisez un mode synchrone (VIRTUAL_ACCOUNT, ESPECE, etc.) sur cet endpoint, " +
                    "ou payez une souscription déjà créée via FlexPay (/api/Affilie/paiement ou /api/Collecte).");
            }

            MethodePaiementHelper.EnsureGuichetSyncOnly(modePaiement);
            WalletVirtuelPaiementAutorisation.EnsureSiVirtualAccount(
                modePaiement,
                _httpContextAccessor.HttpContext?.User);

            _ = await _db.Affilies.AsNoTracking()
                .FirstOrDefaultAsync(a => a.IdAffilie == affilieId && a.Statut, ct)
                ?? throw new ArgumentException($"Affilié {affilieId} introuvable ou inactif.");

            var adhesion = await _db.Adhesions.AsNoTracking()
                .Where(a => a.AffilieId == affilieId && a.Statut)
                .OrderByDescending(a => a.DateCreation)
                .FirstOrDefaultAsync(ct)
                ?? throw new ArgumentException(
                    "Aucune adhésion active trouvée. Impossible de souscrire à un produit.");

            var agentId = collecteDto.AgentId > 0
                ? collecteDto.AgentId
                : adhesion.AgentId;
            if (!agentId.HasValue || agentId.Value <= 0)
                throw new ArgumentException(
                    "Gestionnaire de compte non assigné. Impossible de poursuivre tant qu'un agent AT n'est pas affecté.");

            var agentIdValue = agentId.Value;

            var agentExists = await _db.Agents.AsNoTracking()
                .AnyAsync(a => a.IdAgent == agentIdValue && a.Statut, ct);
            if (!agentExists)
                throw new ArgumentException($"Agent {agentIdValue} introuvable ou inactif.");

            var existing = await _db.SouscriptionsPrestations
                .FirstOrDefaultAsync(s => s.AffilieId == affilieId
                    && s.PrestationId == dto.PrestationId
                    && s.Statut, ct);
            if (existing != null)
            {
                throw new InvalidOperationException(
                    $"L'affilié {affilieId} a déjà une souscription active pour la prestation {dto.PrestationId}.");
            }

            await ProduitEligibiliteRules.ValidateAchatProduitAsync(_db, affilieId, dto.PrestationId, ct);

            var moisPaiement = collecteDto.Mois;
            var anneePaiement = collecteDto.Annee;

            await using var transaction = await _db.Database.BeginTransactionAsync(
                IsolationLevel.ReadCommitted, ct);

            try
            {
                var souscription = new SouscriptionPrestation
                {
                    AffilieId = affilieId,
                    PrestationId = dto.PrestationId,
                    Statut = dto.Statut,
                    DateSouscription = dto.DateSouscription ?? DateTime.UtcNow,
                    DateCreation = DateTime.Now,
                    DateModification = DateTime.Now
                };

                _db.SouscriptionsPrestations.Add(souscription);
                await _db.SaveChangesAsync(ct);

                var dateCollecte = CollecteAdhesionHelper.ResolveDateCollecte(collecteDto);
                var dateConversion = CollecteAdhesionHelper.ResolveDateConversionPaiement(
                    modePaiement, dateCollecte);

                var collecte = new Collecte
                {
                    TypeCollecte = TypeCollecte.Souscription,
                    SouscriptionPrestationId = souscription.IdSouscriptionPrestation,
                    AffilieId = affilieId,
                    AgentId = agentIdValue,
                    Montant = collecteDto.Montant,
                    Mois = moisPaiement,
                    Annee = anneePaiement,
                    ReferencePaiement = collecteDto.ReferencePaiement,
                    ModePaiement = modePaiement,
                    StatutPaiement = CollecteStatutPaiementRegles.NormaliserPourEcriture(
                        collecteDto.StatutPaiement ?? CollecteStatutPaiement.Valide),
                    MontantRecu = collecteDto.MontantRecu ?? collecteDto.Montant,
                    MontantAttendu = collecteDto.MontantAttendu ?? collecteDto.Montant,
                    DeviseId = collecteDto.DeviseId,
                    Observation = collecteDto.Observation
                        ?? $"Souscription prestation {dto.PrestationId} — période {moisPaiement:D2}/{anneePaiement}",
                    DateCollecte = dateCollecte,
                    DateCreation = DateTime.Now,
                    Statut = collecteDto.Statut
                };

                var nombreDependants = await _db.Dependants
                    .CountAsync(d => d.AffilieId == affilieId && d.Statut, ct);

                await _multidevise.ValidateAndApplySnapshotAsync(
                    collecte, nombreDependants, ct, dateConversion);

                if (CollecteAdhesionHelper.IsVirtualAccountPayment(modePaiement))
                {
                    var walletVirtuel = await _db.WalletsVirtuelsAgents
                        .AsNoTracking()
                        .Include(w => w.Devise)
                        .FirstOrDefaultAsync(w => w.AgentId == agentIdValue && w.Statut, ct)
                        ?? throw new ArgumentException(
                            $"Aucun wallet virtuel actif trouvé pour l'agent {agentIdValue}.");

                    var montantDebit = await _walletVirtuelPayment.ComputeMontantDebitAsync(
                        collecte, walletVirtuel, dateCollecte, ct);

                    await _walletVirtuelPayment.ValidateSoldeCumulSuffisantAsync(
                        walletVirtuel, montantDebit, ct);
                }

                _db.Collectes.Add(collecte);
                await _db.SaveChangesAsync(ct);

                await _commissionService.ProcessCommissionAsync(collecte, ct);

                await transaction.CommitAsync(ct);

                _logger.LogInformation(
                    "Souscription {SouscriptionId} et collecte {CollecteId} créées pour affilié {AffilieId}",
                    souscription.IdSouscriptionPrestation, collecte.IdCollecte, affilieId);

                return (souscription, collecte);
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }
    }
}
