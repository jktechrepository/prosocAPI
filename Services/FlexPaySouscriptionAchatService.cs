using System.Data;
using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Prosoc.Data;
using Prosoc.Utilities;
using ProsocAPI.Helpers;
using ProsocAPI.Models.Configuration;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.DTOs.FlexPay;

namespace ProsocAPI.Services
{
    public interface IFlexPaySouscriptionAchatService
    {
        Task<InitiateFlexPayResponseDto> InitiateAsync(
            SouscriptionPrestationPaiementElectroniqueCreateDto dto,
            CancellationToken ct = default);

        Task<(SouscriptionPrestation Souscription, Collecte Collecte)> FinalizeAsync(
            CollecteEnAttente enAttente,
            FlexPayCallbackDto callback,
            CancellationToken ct = default);
    }

    public class FlexPaySouscriptionAchatService : IFlexPaySouscriptionAchatService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly ProsocDbContext _db;
        private readonly IFlexPayService _flexPayService;
        private readonly IInfoPaiementMarchandService _marchandService;
        private readonly IPaiementHoldService _holdService;
        private readonly ICollecteMultideviseService _multidevise;
        private readonly IDeviseConversionService _conversion;
        private readonly ICommissionService _commissionService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly FlexPayOptions _options;
        private readonly ILogger<FlexPaySouscriptionAchatService> _logger;

        public FlexPaySouscriptionAchatService(
            ProsocDbContext db,
            IFlexPayService flexPayService,
            IInfoPaiementMarchandService marchandService,
            IPaiementHoldService holdService,
            ICollecteMultideviseService multidevise,
            IDeviseConversionService conversion,
            ICommissionService commissionService,
            IHttpContextAccessor httpContextAccessor,
            IOptions<FlexPayOptions> options,
            ILogger<FlexPaySouscriptionAchatService> logger)
        {
            _db = db;
            _flexPayService = flexPayService;
            _marchandService = marchandService;
            _holdService = holdService;
            _multidevise = multidevise;
            _conversion = conversion;
            _commissionService = commissionService;
            _httpContextAccessor = httpContextAccessor;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<InitiateFlexPayResponseDto> InitiateAsync(
            SouscriptionPrestationPaiementElectroniqueCreateDto dto,
            CancellationToken ct = default)
        {
            if (dto.Achat?.Collecte == null)
                throw new ArgumentException("Le bloc achat.collecte est obligatoire.");

            MethodePaiementHelper.EnsureFlexPayOnly(dto.ModePaiement);
            if (!_options.Enabled)
                throw new InvalidOperationException("Le paiement électronique FlexPay n'est pas activé.");

            var methode = MethodePaiementHelper.NormalizeForStorage(dto.ModePaiement);
            if (methode == MethodePaiementHelper.MobileMoney && string.IsNullOrWhiteSpace(dto.TelephonePaiement))
                throw new ArgumentException("Le numéro de téléphone est requis pour MOBILE_MONEY.");

            if (dto.DevisePaiementId != dto.Achat.Collecte.DeviseId)
                throw new ArgumentException("DevisePaiementId doit être égal à Achat.Collecte.DeviseId.");

            // Align nested collecte mode with root FlexPay mode
            dto.Achat.Collecte.ModePaiement = methode;

            var marchand = await _marchandService.GetActifAsync(ct)
                ?? throw new InvalidOperationException("Aucune configuration marchand FlexPay active.");

            if (methode == MethodePaiementHelper.MobileMoney && !marchand.ActifMobileMoney)
                throw new InvalidOperationException("Mobile Money FlexPay désactivé.");
            if (methode == MethodePaiementHelper.CarteBancaire && !marchand.ActifCarteBancaire)
                throw new InvalidOperationException("Carte bancaire FlexPay désactivée.");

            var (agentId, mois, annee) = await ValidateBusinessRulesAsync(dto.AffilieId, dto.Achat, ct);

            // PrestationId stored in SouscriptionPrestationId slot until finalize (same pattern as adhésion FlexPay)
            await _holdService.EnsureNoActiveHoldAsync(
                dto.AffilieId,
                TypeCollecte.Souscription,
                mois,
                annee,
                null,
                dto.Achat.PrestationId,
                null,
                null,
                ct);

            var nombreDependants = await _db.Dependants
                .CountAsync(d => d.AffilieId == dto.AffilieId && d.Statut, ct);

            var tempCollecte = new Collecte
            {
                TypeCollecte = TypeCollecte.Souscription,
                SouscriptionPrestationId = dto.Achat.PrestationId,
                AffilieId = dto.AffilieId,
                AgentId = agentId,
                Montant = dto.Achat.Collecte.Montant,
                Mois = mois,
                Annee = annee,
                DeviseId = dto.Achat.Collecte.DeviseId,
                ModePaiement = methode,
                DateCollecte = CollecteAdhesionHelper.ResolveDateCollecte(dto.Achat.Collecte)
            };

            await _multidevise.ValidateAndApplySnapshotAsync(tempCollecte, nombreDependants, ct);

            var (montantTarif, deviseTarifId) = await _multidevise.ResolveTarifAttenduAsync(
                tempCollecte, nombreDependants, ct);
            var deviseTarif = await _db.Devises.AsNoTracking().FirstAsync(d => d.IdDevise == deviseTarifId, ct);

            var devisePaiement = await _db.Devises.AsNoTracking()
                .FirstOrDefaultAsync(d => d.IdDevise == dto.DevisePaiementId && d.Statut, ct)
                ?? throw new ArgumentException($"Devise {dto.DevisePaiementId} introuvable.");

            var codeDevisePaiement = devisePaiement.Code.ToUpperInvariant();
            if (codeDevisePaiement is not ("CDF" or "USD"))
                throw new ArgumentException("FlexPay n'accepte que CDF ou USD comme devise de paiement.");

            var (montantFlexPay, taux) = deviseTarifId == dto.DevisePaiementId
                ? (montantTarif, 1m)
                : await _conversion.ConvertirAsync(montantTarif, deviseTarifId, dto.DevisePaiementId, DateTime.UtcNow, ct);

            if (codeDevisePaiement == "CDF")
                montantFlexPay = Math.Round(montantFlexPay, 0, MidpointRounding.AwayFromZero);

            var idEnAttente = Guid.NewGuid();
            var reference = $"SP-{idEnAttente:N}"[..20];
            var expireAt = DateTime.UtcNow.AddMinutes(_options.HoldMinutes);

            var operateurUtilisateurId = CurrentUserResolver.TryGetCurrentUtilisateurId(
                _httpContextAccessor.HttpContext?.User);

            var enAttente = new CollecteEnAttente
            {
                IdCollecteEnAttente = idEnAttente,
                SourceFlux = CollecteEnAttenteSourceFlux.SouscriptionAchatPaiementElectronique,
                AffilieId = dto.AffilieId,
                AgentId = agentId,
                IdUtilisateur = operateurUtilisateurId,
                TypeCollecte = TypeCollecte.Souscription,
                SouscriptionPrestationId = dto.Achat.PrestationId,
                Mois = mois,
                Annee = annee,
                MethodePaiement = methode,
                MontantTarif = montantTarif,
                DeviseTarifId = deviseTarifId,
                MontantFlexPay = montantFlexPay,
                CodeDevisePaiement = codeDevisePaiement,
                TauxVersDevisePaiement = taux,
                ReferenceFlexPay = reference,
                Phone = dto.TelephonePaiement,
                PayloadMetierJson = JsonSerializer.Serialize(dto, JsonOptions),
                DateExpiration = expireAt
            };

            _db.CollectesEnAttente.Add(enAttente);
            await _holdService.CreateHoldAsync(
                idEnAttente,
                dto.AffilieId,
                TypeCollecte.Souscription,
                mois,
                annee,
                null,
                dto.Achat.PrestationId,
                null,
                null,
                expireAt,
                ct);

            var callbackUrl = FlexPayUrlHelper.ResolveCallbackUrl(
                _httpContextAccessor.HttpContext,
                _options.CallbackBaseUrl,
                _options.ForceProductionCallbackInDev);

            FlexPayPaymentResponseDto fpResponse;
            if (methode == MethodePaiementHelper.CarteBancaire)
            {
                var approve = FlexPayUrlHelper.DeriveRedirectUrl(callbackUrl, "approve");
                var cancel = FlexPayUrlHelper.DeriveRedirectUrl(callbackUrl, "cancel");
                var decline = FlexPayUrlHelper.DeriveRedirectUrl(callbackUrl, "decline");
                fpResponse = await _flexPayService.InitierPaiementCarteV1Async(
                    marchand.CodeMarchand, marchand.ApiToken, reference,
                    montantFlexPay, codeDevisePaiement,
                    $"Souscription Prosoc prestation {dto.Achat.PrestationId}",
                    callbackUrl, approve, cancel, decline, ct);
            }
            else
            {
                fpResponse = await _flexPayService.InitierPaiementMobileMoneyAsync(
                    marchand.CodeMarchand, marchand.ApiToken, reference,
                    dto.TelephonePaiement!, montantFlexPay, codeDevisePaiement, callbackUrl, ct);
            }

            if (!fpResponse.IsSuccess)
            {
                await RollbackInitiationAsync(idEnAttente, ct);
                throw new InvalidOperationException(fpResponse.Message ?? "FlexPay a refusé l'initiation du paiement.");
            }

            enAttente.OrderNumberFlexPay = fpResponse.OrderNumber;
            _db.TransactionsFlexPay.Add(new TransactionFlexPay
            {
                OrderNumber = fpResponse.OrderNumber ?? reference,
                Reference = reference,
                TypePaiement = methode == MethodePaiementHelper.CarteBancaire ? "2" : "1",
                Amount = montantFlexPay,
                Currency = codeDevisePaiement,
                Phone = dto.TelephonePaiement,
                Merchant = marchand.CodeMarchand,
                CallbackUrl = callbackUrl,
                PaymentUrl = fpResponse.ResolvePaymentUrl(),
                CodeFlexPay = fpResponse.Code,
                MessageFlexPay = fpResponse.Message,
                IdCollecteEnAttente = idEnAttente,
                SourceFlux = CollecteEnAttenteSourceFlux.SouscriptionAchatPaiementElectronique,
                ReponseBruteFlexPay = JsonSerializer.Serialize(fpResponse)
            });
            await _db.SaveChangesAsync(ct);

            return new InitiateFlexPayResponseDto
            {
                IdCollecteEnAttente = idEnAttente,
                OrderNumberFlexPay = fpResponse.OrderNumber,
                ReferenceFlexPay = reference,
                MontantTarif = montantTarif,
                CodeDeviseTarif = deviseTarif.Code,
                MontantFlexPay = montantFlexPay,
                CodeDevisePaiement = codeDevisePaiement,
                TauxApplique = taux,
                HoldExpireAt = expireAt,
                PaymentUrl = fpResponse.ResolvePaymentUrl(),
                FlexPayAccepted = true,
                Message = methode == MethodePaiementHelper.CarteBancaire
                    ? "Redirigez le client vers l'URL de paiement carte."
                    : "Validez le paiement sur votre téléphone Mobile Money."
            };
        }

        public async Task<(SouscriptionPrestation Souscription, Collecte Collecte)> FinalizeAsync(
            CollecteEnAttente enAttente,
            FlexPayCallbackDto callback,
            CancellationToken ct = default)
        {
            if (enAttente.IdCollecteFinalisee.HasValue)
            {
                var existingCollecte = await _db.Collectes
                    .FirstOrDefaultAsync(c => c.IdCollecte == enAttente.IdCollecteFinalisee.Value, ct);
                if (existingCollecte?.SouscriptionPrestationId != null)
                {
                    var existingSub = await _db.SouscriptionsPrestations
                        .FirstAsync(s => s.IdSouscriptionPrestation == existingCollecte.SouscriptionPrestationId.Value, ct);
                    return (existingSub, existingCollecte);
                }
            }

            var payload = JsonSerializer.Deserialize<SouscriptionPrestationPaiementElectroniqueCreateDto>(
                    enAttente.PayloadMetierJson, JsonOptions)
                ?? throw new InvalidOperationException("Payload souscription FlexPay invalide.");

            if (payload.Achat?.Collecte == null)
                throw new InvalidOperationException("Payload souscription FlexPay incomplet (achat.collecte).");

            var affilieId = enAttente.AffilieId
                ?? throw new InvalidOperationException("AffilieId manquant sur la collecte en attente.");

            var agentId = enAttente.AgentId
                ?? throw new InvalidOperationException("AgentId manquant sur la collecte en attente.");

            var mode = MethodePaiementHelper.NormalizeForStorage(enAttente.MethodePaiement);
            var collecteDto = payload.Achat.Collecte;

            await using var transaction = await _db.Database.BeginTransactionAsync(
                IsolationLevel.ReadCommitted, ct);

            try
            {
                var existing = await _db.SouscriptionsPrestations
                    .FirstOrDefaultAsync(s => s.AffilieId == affilieId
                        && s.PrestationId == payload.Achat.PrestationId
                        && s.Statut, ct);
                if (existing != null)
                {
                    throw new InvalidOperationException(
                        $"L'affilié {affilieId} a déjà une souscription active pour la prestation {payload.Achat.PrestationId}.");
                }

                var souscription = new SouscriptionPrestation
                {
                    AffilieId = affilieId,
                    PrestationId = payload.Achat.PrestationId,
                    Statut = payload.Achat.Statut,
                    DateSouscription = payload.Achat.DateSouscription ?? DateTime.UtcNow,
                    DateCreation = DateTime.Now,
                    DateModification = DateTime.Now
                };

                _db.SouscriptionsPrestations.Add(souscription);
                await _db.SaveChangesAsync(ct);

                var collecte = new Collecte
                {
                    TypeCollecte = TypeCollecte.Souscription,
                    SouscriptionPrestationId = souscription.IdSouscriptionPrestation,
                    AffilieId = affilieId,
                    AgentId = agentId,
                    OperateurUtilisateurId = enAttente.IdUtilisateur,
                    Montant = collecteDto.Montant,
                    Mois = enAttente.Mois,
                    Annee = enAttente.Annee,
                    ReferencePaiement = callback.OrderNumber ?? enAttente.OrderNumberFlexPay,
                    OrderNumberFlexPay = callback.OrderNumber ?? enAttente.OrderNumberFlexPay,
                    ProviderReferenceFlexPay = callback.ProviderReference,
                    ModePaiement = mode,
                    Operateur = callback.Channel,
                    StatutPaiement = CollecteStatutPaiement.Valide,
                    MontantRecu = collecteDto.Montant,
                    MontantAttendu = enAttente.MontantTarif,
                    DeviseId = collecteDto.DeviseId,
                    Observation = collecteDto.Observation
                        ?? $"Souscription prestation {payload.Achat.PrestationId} — FlexPay",
                    DateCollecte = DateTime.UtcNow,
                    DateCreation = DateTime.UtcNow,
                    Statut = true
                };

                var nombreDependants = await _db.Dependants
                    .CountAsync(d => d.AffilieId == affilieId && d.Statut, ct);

                // Snapshot devises (montant déjà validé à l'initiation ; idempotence montant callback côté callback service)
                await _multidevise.ValidateAndApplySnapshotAsync(collecte, nombreDependants, ct);

                _db.Collectes.Add(collecte);
                await _db.SaveChangesAsync(ct);

                await _commissionService.ProcessCommissionAsync(collecte, ct);

                enAttente.StatutEnAttente = CollecteEnAttenteStatut.Finalise;
                enAttente.IdCollecteFinalisee = collecte.IdCollecte;
                enAttente.DateModification = DateTime.UtcNow;
                // Store real souscription id after finalize
                enAttente.SouscriptionPrestationId = souscription.IdSouscriptionPrestation;

                var txFlex = await _db.TransactionsFlexPay
                    .FirstOrDefaultAsync(t => t.IdCollecteEnAttente == enAttente.IdCollecteEnAttente, ct);
                if (txFlex != null)
                    txFlex.IdCollecte = collecte.IdCollecte;

                await _holdService.ReleaseHoldAsync(enAttente.IdCollecteEnAttente, ct);
                await _db.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);

                _logger.LogInformation(
                    "Souscription {SouscriptionId} + collecte {CollecteId} finalisées via FlexPay (en attente {EnAttenteId})",
                    souscription.IdSouscriptionPrestation, collecte.IdCollecte, enAttente.IdCollecteEnAttente);

                return (souscription, collecte);
            }
            catch
            {
                await transaction.RollbackAsync(ct);
                throw;
            }
        }

        private async Task<(int AgentId, int Mois, int Annee)> ValidateBusinessRulesAsync(
            int affilieId,
            SouscriptionPrestationAchatCreateDto achat,
            CancellationToken ct)
        {
            _ = await _db.Affilies.AsNoTracking()
                .FirstOrDefaultAsync(a => a.IdAffilie == affilieId && a.Statut, ct)
                ?? throw new ArgumentException($"Affilié {affilieId} introuvable ou inactif.");

            var adhesion = await _db.Adhesions.AsNoTracking()
                .Where(a => a.AffilieId == affilieId && a.Statut)
                .OrderByDescending(a => a.DateCreation)
                .FirstOrDefaultAsync(ct)
                ?? throw new ArgumentException(
                    "Aucune adhésion active trouvée. Impossible de souscrire à un produit.");

            var agentId = achat.Collecte.AgentId > 0
                ? achat.Collecte.AgentId
                : adhesion.AgentId;
            if (!agentId.HasValue || agentId.Value <= 0)
                throw new ArgumentException(
                    "Gestionnaire de compte non assigné. Impossible de poursuivre tant qu'un agent AT n'est pas affecté.");

            var agentExists = await _db.Agents.AsNoTracking()
                .AnyAsync(a => a.IdAgent == agentId.Value && a.Statut, ct);
            if (!agentExists)
                throw new ArgumentException($"Agent {agentId.Value} introuvable ou inactif.");

            var existing = await _db.SouscriptionsPrestations
                .AsNoTracking()
                .AnyAsync(s => s.AffilieId == affilieId
                    && s.PrestationId == achat.PrestationId
                    && s.Statut, ct);
            if (existing)
            {
                throw new InvalidOperationException(
                    $"L'affilié {affilieId} a déjà une souscription active pour la prestation {achat.PrestationId}.");
            }

            await ProduitEligibiliteRules.ValidateAchatProduitAsync(_db, affilieId, achat.PrestationId, ct);

            return (agentId.Value, achat.Collecte.Mois, achat.Collecte.Annee);
        }

        private async Task RollbackInitiationAsync(Guid idEnAttente, CancellationToken ct)
        {
            await _holdService.ReleaseHoldAsync(idEnAttente, ct);
            var enAttente = await _db.CollectesEnAttente
                .FirstOrDefaultAsync(c => c.IdCollecteEnAttente == idEnAttente, ct);
            if (enAttente != null)
            {
                enAttente.StatutEnAttente = CollecteEnAttenteStatut.Echec;
                await _db.SaveChangesAsync(ct);
            }
        }
    }
}
