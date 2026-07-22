using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Prosoc.Data;
using Prosoc.Utilities;
using ProsocAPI.Models.Core;
using ProsocAPI.Services.Queue;
using ProsocAPI.Services.Repositories;
using System.Globalization;
using System.Text;

namespace ProsocAPI.Services
{
    public interface ICommissionService
    {
        Task ProcessCommissionAsync(Collecte collecte, CancellationToken ct = default);
    }

    public class CommissionService : ICommissionService
    {
        private readonly ProsocDbContext _db;
        private readonly ILogger<CommissionService> _logger;
        private readonly ICommissionNotificationService _commissionNotificationService;
        private readonly INotificationQueueService _notificationQueueService;
        private readonly IArrieresAffilieService _arrieresService;
        private readonly IPenaliteAffilieService _penaliteService;
        private readonly IWalletVirtuelPaymentService _walletVirtuelPaymentService;
        private readonly IWalletAgentRepository _walletAgentRepository;
        private readonly IDeviseConversionService _deviseConversionService;
        private const decimal DEFAULT_COMMISSION_RATE_PERCENT = 25m;

        public CommissionService(
            ProsocDbContext db, 
            ILogger<CommissionService> logger,
            ICommissionNotificationService commissionNotificationService,
            INotificationQueueService notificationQueueService,
            IArrieresAffilieService arrieresService,
            IPenaliteAffilieService penaliteService,
            IWalletVirtuelPaymentService walletVirtuelPaymentService,
            IWalletAgentRepository walletAgentRepository,
            IDeviseConversionService deviseConversionService)
        {
            _db = db;
            _logger = logger;
            _commissionNotificationService = commissionNotificationService;
            _notificationQueueService = notificationQueueService;
            _arrieresService = arrieresService;
            _penaliteService = penaliteService;
            _walletVirtuelPaymentService = walletVirtuelPaymentService;
            _walletAgentRepository = walletAgentRepository;
            _deviseConversionService = deviseConversionService;
        }

        public async Task ProcessCommissionAsync(Collecte collecte, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("=== DÉBUT TRAITEMENT COMMISSION ===");
                _logger.LogInformation("Traitement de la commission pour la collecte {CollecteId}", collecte.IdCollecte);
                _logger.LogInformation("AffilieId: {AffilieId}, Montant: {Montant}", collecte.AffilieId, collecte.Montant);

                // 1. Trouver l'agent qui a adhéré l'affilié
                _logger.LogInformation("Recherche de l'adhésion pour l'affilié {AffilieId}...", collecte.AffilieId);
                var adhesion = await _db.Adhesions
                    .FirstOrDefaultAsync(a => a.AffilieId == collecte.AffilieId, ct);

                if (adhesion == null)
                {
                    _logger.LogWarning("Aucune adhésion trouvée pour l'affilié {AffilieId}", collecte.AffilieId);
                    return;
                }

                _logger.LogInformation("Adhésion trouvée: AgentId {AgentId}", adhesion.AgentId);

                if (!adhesion.AgentId.HasValue)
                {
                    _logger.LogWarning(
                        "Aucun gestionnaire AT pour l'affilié {AffilieId} — commission non créditée (adhésion en ligne).",
                        collecte.AffilieId);
                    await _arrieresService.ProcessCollecteForArrieresAsync(collecte, ct);
                    if (collecte.PenaliteAffilieId.HasValue)
                        await _penaliteService.ProcessCollecteForPenaliteAsync(collecte, ct);
                    return;
                }

                // 2. Résoudre le taux de commission selon le workflow hybride
                var (tauxCommission, sourceTaux) = await ResolveCommissionRateAsync(collecte, adhesion.AgentId.Value, ct);
                var montantBaseCommission = collecte.Montant;
                var commission = montantBaseCommission * (tauxCommission / 100m);

                _logger.LogInformation(
                    "Commission calculée: {Commission} ({Taux}% de {Montant}) - source: {Source}",
                    commission, tauxCommission, montantBaseCommission, sourceTaux);

                // 3. Créditer le wallet de l'agent qui a fait l'adhésion
                _logger.LogInformation("Créditation du wallet de l'agent {AgentId}...", adhesion.AgentId);
                await CreditWalletAgentAsync(adhesion.AgentId.Value, commission, collecte, ct);

                // 4. Gérer le wallet virtuel si paiement par compte virtuel
                if (collecte.ModePaiement?.ToUpperInvariant() == "VIRTUAL_ACCOUNT" || collecte.ModePaiement?.ToUpperInvariant() == "COMPTE VIRTUEL")
                {
                    _logger.LogInformation("Mode de paiement: Compte Virtuel/VIRTUAL_ACCOUNT");
                    await ProcessWalletVirtuelAsync(collecte, ct);
                }
                else
                {
                    _logger.LogInformation("Mode de paiement: {ModePaiement}", collecte.ModePaiement);
                }

                // 5. Traiter les arriérés
                _logger.LogInformation("Traitement des arriérés pour la collecte {CollecteId}", collecte.IdCollecte);
                await _arrieresService.ProcessCollecteForArrieresAsync(collecte, ct);

                // 6. Traiter les pénalités
                if (collecte.PenaliteAffilieId.HasValue)
                {
                    _logger.LogInformation("Traitement pénalité pour la collecte {CollecteId}", collecte.IdCollecte);
                    await _penaliteService.ProcessCollecteForPenaliteAsync(collecte, ct);
                }

                _logger.LogInformation("=== FIN TRAITEMENT COMMISSION ===");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du traitement de la commission pour la collecte {CollecteId}", collecte.IdCollecte);
                throw;
            }
        }

        private async Task CreditWalletAgentAsync(int agentId, decimal commission, Collecte collecte, CancellationToken ct)
        {
            _logger.LogInformation("=== DÉBUT CRÉDITATION WALLET ===");
            _logger.LogInformation(
                "Agent {AgentId}, devise collecte {DeviseId}, commission {Commission}...",
                agentId, collecte.DeviseId, commission);

            var devisePrincipale = await _deviseConversionService.GetDevisePrincipaleAsync(ct);
            var dateReference = collecte.DateCollecte == default ? DateTime.Now : collecte.DateCollecte;

            decimal commissionPrincipale;
            if (collecte.DeviseId == devisePrincipale.IdDevise)
            {
                commissionPrincipale = commission;
            }
            else
            {
                var (montantConverti, taux) = await _deviseConversionService.ConvertirAsync(
                    commission,
                    collecte.DeviseId,
                    devisePrincipale.IdDevise,
                    dateReference,
                    ct);
                commissionPrincipale = montantConverti;
                _logger.LogInformation(
                    "Commission convertie en devise principale {DeviseCode}: {MontantSource} → {MontantCible} (taux {Taux})",
                    devisePrincipale.Code, commission, commissionPrincipale, taux);
            }

            var wallet = await _walletAgentRepository.GetOrCreateForAgentAndDeviseAsync(
                agentId, devisePrincipale.IdDevise, ct);

            _logger.LogInformation(
                "Wallet Id {WalletId} (devise principale {DeviseCode}), solde actuel: {Solde}",
                wallet.IdWalletAgent, devisePrincipale.Code, wallet.SoldeCourant);

            var ancienSolde = wallet.SoldeCourant;
            wallet.SoldeCourant += commissionPrincipale;
            wallet.SoldeDisponible += commissionPrincipale;
            wallet.DateModification = DateTime.Now;

            _logger.LogInformation(
                "Mise à jour du solde: {AncienSolde} → {NouveauSolde} (disponible synchronisé)",
                ancienSolde, wallet.SoldeCourant);

            await _db.SaveChangesAsync(ct);

            var affilieNom = collecte.Affilie?.NomComplet;
            if (string.IsNullOrWhiteSpace(affilieNom))
            {
                affilieNom = await _db.Affilies
                    .Where(a => a.IdAffilie == collecte.AffilieId)
                    .Select(a => a.NomComplet)
                    .FirstOrDefaultAsync(ct);
            }

            var description = collecte.DeviseId == devisePrincipale.IdDevise
                ? WalletMouvementDescriptionBuilder.BuildStoredCommissionDescription(
                    collecte.IdCollecte, affilieNom ?? $"affilié {collecte.AffilieId}")
                : WalletMouvementDescriptionBuilder.BuildStoredCommissionDescription(
                    collecte.IdCollecte,
                    affilieNom ?? $"affilié {collecte.AffilieId}",
                    devisePrincipale.Code);

            var mouvement = new WalletMouvement
            {
                WalletId = wallet.IdWalletAgent,
                DeviseId = devisePrincipale.IdDevise,
                Montant = commissionPrincipale,
                TypeOperation = "CREDIT",
                Source = WalletMouvementSources.CommissionCollecte,
                Description = description,
                DateOperation = DateTime.Now,
                Statut = true,
                DateCreation = DateTime.Now
            };

            _db.WalletMouvements.Add(mouvement);
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("Mouvement créé: Id {MouvementId}", mouvement.IdWalletMouvement);
            _logger.LogInformation(
                "Wallet principal de l'agent {AgentId} crédité de {Commission}. Ancien solde: {AncienSolde}, Nouveau solde: {NouveauSolde}",
                agentId, commissionPrincipale, ancienSolde, wallet.SoldeCourant);
            _logger.LogInformation("=== FIN CRÉDITATION WALLET ===");

            await _notificationQueueService.QueueCommissionNotificationAsync(
                agentId,
                commissionPrincipale,
                collecte.IdCollecte,
                ancienSolde,
                wallet.SoldeCourant);
        }

        private async Task ProcessWalletVirtuelAsync(Collecte collecte, CancellationToken ct)
        {
            _logger.LogInformation("=== DÉBUT TRAITEMENT WALLET VIRTUEL ===");
            _logger.LogInformation("Paiement par compte virtuel pour la collecte {CollecteId} - Montant: {Montant}", 
                collecte.IdCollecte, collecte.Montant);

            try
            {
                var adhesion = await _db.Adhesions
                    .FirstOrDefaultAsync(a => a.AffilieId == collecte.AffilieId, ct);

                if (adhesion == null)
                {
                    _logger.LogWarning("Aucune adhésion trouvée pour l'affilié {AffilieId}", collecte.AffilieId);
                    return;
                }

                if (!adhesion.AgentId.HasValue)
                {
                    _logger.LogWarning(
                        "Wallet virtuel ignoré : aucun gestionnaire AT pour l'affilié {AffilieId}",
                        collecte.AffilieId);
                    return;
                }

                await _walletVirtuelPaymentService.DebitAsync(collecte, adhesion.AgentId.Value, ct);

                _logger.LogInformation("=== FIN TRAITEMENT WALLET VIRTUEL ===");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du traitement du wallet virtuel pour la collecte {CollecteId}", collecte.IdCollecte);
                throw;
            }
        }

        private async Task<(decimal taux, string source)> ResolveCommissionRateAsync(Collecte collecte, int agentId, CancellationToken ct)
        {
            if (collecte.TypeCollecte == TypeCollecte.Frais && collecte.FraisId.HasValue)
            {
                var frais = await _db.Frais
                    .AsNoTracking()
                    .FirstOrDefaultAsync(f => f.IdFrais == collecte.FraisId.Value, ct);

                if (frais != null)
                {
                    return (frais.TauxCommission, $"Frais:{frais.IdFrais}");
                }
            }

            if (collecte.TypeCollecte == TypeCollecte.Souscription && collecte.SouscriptionPrestationId.HasValue)
            {
                var roleAgent = await _db.Agents
                    .AsNoTracking()
                    .Where(a => a.IdAgent == agentId)
                    .Select(a => a.RoleAgent)
                    .FirstOrDefaultAsync(ct);

                var souscription = await _db.SouscriptionsPrestations
                    .AsNoTracking()
                    .Include(sp => sp.Prestation)
                        .ThenInclude(p => p.ProduitMutuel)
                    .Include(sp => sp.Prestation)
                        .ThenInclude(p => p.ProduitAssureur)
                    .FirstOrDefaultAsync(sp => sp.IdSouscriptionPrestation == collecte.SouscriptionPrestationId.Value, ct);

                if (souscription?.Prestation?.ProduitMutuel != null)
                {
                    var pm = souscription.Prestation.ProduitMutuel;
                    if (pm.EstGratuit)
                        return (0m, $"ProduitMutuel:{souscription.Prestation.ProduitMutuelId}:Gratuit");
                    var (taux, groupeRole) = ResolveCommissionRateByRole(
                        roleAgent,
                        pm.TauxCommissionAT,
                        pm.TauxCommissionAA,
                        pm.TauxCommissionAAMash,
                        pm.TauxCommissionAAStructure);
                    return (taux, $"ProduitMutuel:{souscription.Prestation.ProduitMutuelId}:{groupeRole}");
                }

                if (souscription?.Prestation?.ProduitAssureur != null)
                {
                    var pa = souscription.Prestation.ProduitAssureur;
                    if (pa.EstGratuit)
                        return (0m, $"ProduitAssureur:{souscription.Prestation.ProduitAssureurId}:Gratuit");
                    var (taux, groupeRole) = ResolveCommissionRateByRole(
                        roleAgent,
                        pa.TauxCommissionAT,
                        pa.TauxCommissionAA,
                        pa.TauxCommissionAAMash,
                        pa.TauxCommissionAAStructure);
                    return (taux, $"ProduitAssureur:{souscription.Prestation.ProduitAssureurId}:{groupeRole}");
                }
            }

            _logger.LogWarning(
                "Taux commission introuvable pour collecte {CollecteId} (Type={TypeCollecte}, FraisId={FraisId}, CotisationAffilieId={CotisationAffilieId}, SouscriptionPrestationId={SouscriptionId}). Fallback {Fallback}%",
                collecte.IdCollecte, collecte.TypeCollecte, collecte.FraisId, collecte.CotisationAffilieId, collecte.SouscriptionPrestationId, DEFAULT_COMMISSION_RATE_PERCENT);
            return (DEFAULT_COMMISSION_RATE_PERCENT, "FallbackDefault");
        }

        private (decimal taux, string groupeRole) ResolveCommissionRateByRole(
            string? roleAgent,
            decimal tauxAT,
            decimal tauxAA,
            decimal tauxAAMash,
            decimal tauxAAStructure)
        {
            var roleNormalise = NormalizeRole(roleAgent);

            if (IsAtRole(roleNormalise))
                return (tauxAT, "AT");
            if (IsAaRole(roleNormalise))
                return (tauxAA, "AA");
            if (IsAaMashRole(roleNormalise))
                return (tauxAAMash, "AAMash");
            if (IsAaStructureRole(roleNormalise))
                return (tauxAAStructure, "AAStructure");

            _logger.LogWarning(
                "RoleAgent non reconnu ({RoleAgent}) pour le calcul de commission. Fallback sur taux AT.",
                roleAgent);
            return (tauxAT, "AT_Fallback");
        }

        private static string NormalizeRole(string? roleAgent)
        {
            if (string.IsNullOrWhiteSpace(roleAgent))
                return string.Empty;

            var formD = roleAgent.Trim().Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(formD.Length);
            foreach (var c in formD)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
                    continue;
                sb.Append(char.ToLowerInvariant(c));
            }

            return sb.ToString()
                .Replace('’', '\'')
                .Replace("'", " ")
                .Replace("-", " ");
        }

        private static bool IsAtRole(string roleNormalise)
        {
            return roleNormalise == "at"
                || roleNormalise.Contains("agent at")
                || roleNormalise.Contains("(at)")
                || roleNormalise.Contains("superviseur")
                || (roleNormalise.Contains("chef") && roleNormalise.Contains("equipe"));
        }

        private static bool IsAaRole(string roleNormalise)
        {
            return roleNormalise == "aa"
                || roleNormalise.Contains("agent aa")
                || roleNormalise.Contains("(aa)")
                || roleNormalise.Contains("percepteur")
                || roleNormalise.Contains("caissier")
                || roleNormalise.Contains("financier")
                || roleNormalise == "it"
                || roleNormalise.Contains("admin");
        }

        private static bool IsAaMashRole(string roleNormalise)
        {
            return roleNormalise.Contains("aamash")
                || roleNormalise.Contains("aa mash")
                || roleNormalise.Contains("mash");
        }

        private static bool IsAaStructureRole(string roleNormalise)
        {
            return roleNormalise.Contains("aastructure")
                || roleNormalise.Contains("aa structure")
                || roleNormalise.Contains("structure");
        }
    }
}
