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
    public interface IFlexPayCollecteService
    {
        Task<InitiateFlexPayResponseDto> InitiateAgentCollecteAsync(
            CollecteCreateDto dto,
            string? phone,
            CollecteEnAttenteSourceFlux sourceFlux = CollecteEnAttenteSourceFlux.CollecteAgent,
            CancellationToken ct = default);
    }

    public class FlexPayCollecteService : IFlexPayCollecteService
    {
        private readonly ProsocDbContext _db;
        private readonly IFlexPayService _flexPayService;
        private readonly IInfoPaiementMarchandService _marchandService;
        private readonly IPaiementHoldService _holdService;
        private readonly ICollecteMultideviseService _multidevise;
        private readonly IDeviseConversionService _conversion;
        private readonly ICotisationAffilieMetierService _cotisationMetier;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly FlexPayOptions _options;
        private readonly ILogger<FlexPayCollecteService> _logger;

        public FlexPayCollecteService(
            ProsocDbContext db,
            IFlexPayService flexPayService,
            IInfoPaiementMarchandService marchandService,
            IPaiementHoldService holdService,
            ICollecteMultideviseService multidevise,
            IDeviseConversionService conversion,
            ICotisationAffilieMetierService cotisationMetier,
            IHttpContextAccessor httpContextAccessor,
            IOptions<FlexPayOptions> options,
            ILogger<FlexPayCollecteService> logger)
        {
            _db = db;
            _flexPayService = flexPayService;
            _marchandService = marchandService;
            _holdService = holdService;
            _multidevise = multidevise;
            _conversion = conversion;
            _cotisationMetier = cotisationMetier;
            _httpContextAccessor = httpContextAccessor;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<InitiateFlexPayResponseDto> InitiateAgentCollecteAsync(
            CollecteCreateDto dto,
            string? phone,
            CollecteEnAttenteSourceFlux sourceFlux = CollecteEnAttenteSourceFlux.CollecteAgent,
            CancellationToken ct = default)
        {
            MethodePaiementHelper.EnsureFlexPayOnly(dto.ModePaiement);
            if (!_options.Enabled)
                throw new InvalidOperationException("Le paiement électronique FlexPay n'est pas activé.");

            var methode = MethodePaiementHelper.NormalizeForStorage(dto.ModePaiement);
            if (methode == MethodePaiementHelper.MobileMoney && string.IsNullOrWhiteSpace(phone))
                throw new ArgumentException("Le numéro de téléphone est requis pour MOBILE_MONEY.");

            if (!dto.IsValid())
                throw new ArgumentException("Données de collecte invalides pour le type spécifié.");

            var marchand = await _marchandService.GetActifAsync(ct)
                ?? throw new InvalidOperationException("Aucune configuration marchand FlexPay active.");

            if (methode == MethodePaiementHelper.MobileMoney && !marchand.ActifMobileMoney)
                throw new InvalidOperationException("Mobile Money FlexPay désactivé.");
            if (methode == MethodePaiementHelper.CarteBancaire && !marchand.ActifCarteBancaire)
                throw new InvalidOperationException("Carte bancaire FlexPay désactivée.");

            await ValidateStructureAsync(dto, ct);

            await _holdService.EnsureNoActiveHoldAsync(
                dto.AffilieId, dto.TypeCollecte, dto.Mois, dto.Annee,
                dto.FraisId, dto.SouscriptionPrestationId, dto.CotisationAffilieId,
                null, ct);

            var devisePaiement = await _db.Devises.AsNoTracking()
                .FirstOrDefaultAsync(d => d.IdDevise == dto.DeviseId && d.Statut, ct)
                ?? throw new ArgumentException($"Devise {dto.DeviseId} introuvable.");

            var codeDevisePaiement = devisePaiement.Code.ToUpperInvariant();
            if (codeDevisePaiement is not ("CDF" or "USD"))
                throw new ArgumentException("FlexPay n'accepte que CDF ou USD comme devise de paiement.");

            var nombreDependants = await _db.Dependants.CountAsync(d => d.AffilieId == dto.AffilieId && d.Statut, ct);
            var tempCollecte = MapTempCollecte(dto);
            await _multidevise.ValidateAndApplySnapshotAsync(tempCollecte, nombreDependants, ct);

            var (montantTarif, deviseTarifId) = await _multidevise.ResolveTarifAttenduAsync(tempCollecte, nombreDependants, ct);
            var deviseTarif = await _db.Devises.AsNoTracking().FirstAsync(d => d.IdDevise == deviseTarifId, ct);

            var (montantFlexPay, taux) = deviseTarifId == dto.DeviseId
                ? (montantTarif, 1m)
                : await _conversion.ConvertirAsync(montantTarif, deviseTarifId, dto.DeviseId, DateTime.UtcNow, ct);

            if (codeDevisePaiement == "CDF")
                montantFlexPay = Math.Round(montantFlexPay, 0, MidpointRounding.AwayFromZero);

            var idEnAttente = Guid.NewGuid();
            var reference = $"PS-{idEnAttente:N}"[..20];
            var expireAt = DateTime.UtcNow.AddMinutes(_options.HoldMinutes);

            var operateurUtilisateurId = CurrentUserResolver.TryGetCurrentUtilisateurId(
                _httpContextAccessor.HttpContext?.User);

            var enAttente = new CollecteEnAttente
            {
                IdCollecteEnAttente = idEnAttente,
                SourceFlux = sourceFlux,
                AffilieId = dto.AffilieId,
                AgentId = dto.AgentId,
                IdUtilisateur = operateurUtilisateurId,
                TypeCollecte = dto.TypeCollecte,
                FraisId = dto.FraisId,
                CotisationAffilieId = dto.CotisationAffilieId,
                SouscriptionPrestationId = dto.SouscriptionPrestationId,
                Mois = dto.Mois,
                Annee = dto.Annee,
                MethodePaiement = methode,
                MontantTarif = montantTarif,
                DeviseTarifId = deviseTarifId,
                MontantFlexPay = montantFlexPay,
                CodeDevisePaiement = codeDevisePaiement,
                TauxVersDevisePaiement = taux,
                ReferenceFlexPay = reference,
                Phone = phone,
                PayloadMetierJson = JsonSerializer.Serialize(dto),
                DateExpiration = expireAt
            };

            _db.CollectesEnAttente.Add(enAttente);
            await _holdService.CreateHoldAsync(
                idEnAttente, dto.AffilieId, dto.TypeCollecte, dto.Mois, dto.Annee,
                dto.FraisId, dto.SouscriptionPrestationId, dto.CotisationAffilieId,
                null, expireAt, ct);

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
                    $"Collecte Prosoc {dto.TypeCollecte}",
                    callbackUrl, approve, cancel, decline, ct);
            }
            else
            {
                fpResponse = await _flexPayService.InitierPaiementMobileMoneyAsync(
                    marchand.CodeMarchand, marchand.ApiToken, reference,
                    phone!, montantFlexPay, codeDevisePaiement, callbackUrl, ct);
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
                Phone = phone,
                Merchant = marchand.CodeMarchand,
                CallbackUrl = callbackUrl,
                PaymentUrl = fpResponse.ResolvePaymentUrl(),
                CodeFlexPay = fpResponse.Code,
                MessageFlexPay = fpResponse.Message,
                IdCollecteEnAttente = idEnAttente,
                SourceFlux = sourceFlux,
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

        private async Task ValidateStructureAsync(CollecteCreateDto dto, CancellationToken ct)
        {
            if (dto.TypeCollecte == TypeCollecte.Souscription && dto.SouscriptionPrestationId.HasValue)
            {
                await ProduitEligibiliteRules.ValidateAchatProduitBySouscriptionAsync(
                    _db, dto.AffilieId, dto.SouscriptionPrestationId.Value, ct);
            }

            if (dto.TypeCollecte == TypeCollecte.Cotisation && dto.CotisationAffilieId.HasValue)
            {
                var adhesion = await _db.Adhesions.AsNoTracking()
                    .FirstOrDefaultAsync(a => a.AffilieId == dto.AffilieId, ct)
                    ?? throw new ArgumentException($"Aucune adhésion pour l'affilié {dto.AffilieId}.");

                var nombreDependants = await _db.Dependants.CountAsync(d => d.AffilieId == dto.AffilieId && d.Statut, ct);
                await _cotisationMetier.ValidateCollecteCotisationStructureAsync(
                    dto.CotisationAffilieId.Value, adhesion.TypeAdhesionId, nombreDependants, ct);
            }
        }

        private static Collecte MapTempCollecte(CollecteCreateDto dto) => new()
        {
            TypeCollecte = dto.TypeCollecte,
            FraisId = dto.FraisId,
            CotisationAffilieId = dto.CotisationAffilieId,
            AffilieId = dto.AffilieId,
            AgentId = dto.AgentId,
            Montant = dto.Montant,
            Mois = dto.Mois,
            Annee = dto.Annee,
            DeviseId = dto.DeviseId,
            SouscriptionPrestationId = dto.SouscriptionPrestationId,
            DateCollecte = DateTime.UtcNow
        };

        private async Task RollbackInitiationAsync(Guid idEnAttente, CancellationToken ct)
        {
            await _holdService.ReleaseHoldAsync(idEnAttente, ct);
            var enAttente = await _db.CollectesEnAttente.FirstOrDefaultAsync(c => c.IdCollecteEnAttente == idEnAttente, ct);
            if (enAttente != null)
            {
                enAttente.StatutEnAttente = CollecteEnAttenteStatut.Echec;
                await _db.SaveChangesAsync(ct);
            }
        }
    }
}
