using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Prosoc.Data;
using ProsocAPI.Helpers;
using ProsocAPI.Models.Configuration;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.DTOs.FlexPay;
using ProsocAPI.Utilities;

namespace ProsocAPI.Services
{
    public interface IFlexPayAdhesionService
    {
        Task<InitiateFlexPayResponseDto> InitiateAsync(
            AdhesionWithAffilieCreateDto input,
            int? utilisateurId,
            string? phone,
            string? expectedModePaiement = null,
            int? expectedDevisePaiementId = null,
            CancellationToken ct = default);
    }

    public class FlexPayAdhesionService : IFlexPayAdhesionService
    {
        private readonly ProsocDbContext _db;
        private readonly IFlexPayService _flexPayService;
        private readonly IInfoPaiementMarchandService _marchandService;
        private readonly IPaiementHoldService _holdService;
        private readonly ICollecteMultideviseService _multidevise;
        private readonly ITypeAdhesionDependantsValidationService _typeAdhesionDependantsValidation;
        private readonly IDeviseConversionService _conversion;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly FlexPayOptions _options;
        private readonly ILogger<FlexPayAdhesionService> _logger;

        public FlexPayAdhesionService(
            ProsocDbContext db,
            IFlexPayService flexPayService,
            IInfoPaiementMarchandService marchandService,
            IPaiementHoldService holdService,
            ICollecteMultideviseService multidevise,
            ITypeAdhesionDependantsValidationService typeAdhesionDependantsValidation,
            IDeviseConversionService conversion,
            IHttpContextAccessor httpContextAccessor,
            IOptions<FlexPayOptions> options,
            ILogger<FlexPayAdhesionService> logger)
        {
            _db = db;
            _flexPayService = flexPayService;
            _marchandService = marchandService;
            _holdService = holdService;
            _multidevise = multidevise;
            _typeAdhesionDependantsValidation = typeAdhesionDependantsValidation;
            _conversion = conversion;
            _httpContextAccessor = httpContextAccessor;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<InitiateFlexPayResponseDto> InitiateAsync(
            AdhesionWithAffilieCreateDto input,
            int? utilisateurId,
            string? phone,
            string? expectedModePaiement = null,
            int? expectedDevisePaiementId = null,
            CancellationToken ct = default)
        {
            if (input.Collectes == null || !input.Collectes.Any())
                throw new ArgumentException("Au moins une collecte est requise.");

            if (!input.Collectes.All(c => c != null && MethodePaiementHelper.IsFlexPay(c.ModePaiement)))
                throw new InvalidOperationException(
                    "L'adhésion FlexPay exige que toutes les collectes utilisent MOBILE_MONEY ou CARTE_BANCAIRE.");

            var modes = input.Collectes
                .Select(c => MethodePaiementHelper.NormalizeForStorage(c.ModePaiement))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (modes.Count > 1)
                throw new InvalidOperationException("Une seule méthode FlexPay par adhésion.");

            var methode = modes[0];
            var phoneToUse = !string.IsNullOrWhiteSpace(phone) ? phone : input.Telephone;
            if (!string.IsNullOrWhiteSpace(phoneToUse))
            {
                phoneToUse = phoneToUse.Trim();
            }

            if (!string.IsNullOrWhiteSpace(expectedModePaiement))
            {
                var expectedMode = MethodePaiementHelper.NormalizeForStorage(expectedModePaiement);
                if (!string.Equals(methode, expectedMode, StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException(
                        $"Mode de paiement incohérent: attendu {expectedMode}, collectes configurées en {methode}.");
            }

            if (!_options.Enabled)
                throw new InvalidOperationException("Le paiement électronique FlexPay n'est pas activé.");

            if (methode == MethodePaiementHelper.MobileMoney && string.IsNullOrWhiteSpace(phoneToUse))
                throw new ArgumentException("Le numéro de téléphone est requis pour MOBILE_MONEY.");

            var marchand = await _marchandService.GetActifAsync(ct)
                ?? throw new InvalidOperationException("Aucune configuration marchand FlexPay active.");

            if (methode == MethodePaiementHelper.MobileMoney && !marchand.ActifMobileMoney)
                throw new InvalidOperationException("Mobile Money FlexPay désactivé.");
            if (methode == MethodePaiementHelper.CarteBancaire && !marchand.ActifCarteBancaire)
                throw new InvalidOperationException("Carte bancaire FlexPay désactivée.");

            var deviseIds = input.Collectes.Select(c => c.DeviseId).Distinct().ToList();
            if (deviseIds.Count > 1)
                throw new ArgumentException("Toutes les collectes d'une adhésion FlexPay doivent utiliser la même devise de paiement.");

            var devisePaiementId = deviseIds[0];
            if (expectedDevisePaiementId.HasValue && devisePaiementId != expectedDevisePaiementId.Value)
                throw new ArgumentException(
                    $"Devise de paiement incohérente: attendu {expectedDevisePaiementId.Value}, collectes configurées en {devisePaiementId}.");

            var devisePaiement = await _db.Devises.AsNoTracking()
                .FirstOrDefaultAsync(d => d.IdDevise == devisePaiementId && d.Statut, ct)
                ?? throw new ArgumentException($"Devise {devisePaiementId} introuvable.");

            var codeDevisePaiement = devisePaiement.Code.ToUpperInvariant();
            if (codeDevisePaiement is not ("CDF" or "USD"))
                throw new ArgumentException("FlexPay n'accepte que CDF ou USD comme devise de paiement.");

            var nombreDependants = input.Dependants?.Count ?? 0;
            await _typeAdhesionDependantsValidation.ValidateDependantsCountAsync(
                input.TypeAdhesionId, nombreDependants, ct);

            var adhesionAgentId = AdhesionAgentIdHelper.ResolveAdhesionAgentId(input.AgentId, isOnlineFlexPay: true);
            var collecteAgentId = AdhesionAgentIdHelper.ResolveCollecteAgentId(adhesionAgentId);

            var collectesDto = input.Collectes.Select(c => new CollecteCreateDto
            {
                TypeCollecte = c.TypeCollecte,
                FraisId = c.FraisId,
                CotisationAffilieId = c.CotisationAffilieId,
                Montant = c.Montant,
                Mois = c.Mois,
                Annee = c.Annee,
                DeviseId = c.DeviseId,
                SouscriptionPrestationId = c.Souscription?.PrestationId,
                AffilieId = 0,
                AgentId = collecteAgentId
            }).ToList();

            decimal montantTarifTotal = 0;
            int? deviseTarifIdRef = null;
            decimal montantFlexPayTotal = 0;
            decimal? tauxRef = null;

            foreach (var dto in collectesDto)
            {
                var tempCollecte = new Collecte
                {
                    TypeCollecte = dto.TypeCollecte,
                    FraisId = dto.FraisId,
                    CotisationAffilieId = dto.CotisationAffilieId,
                    AffilieId = 0,
                    AgentId = dto.AgentId,
                    Montant = dto.Montant,
                    Mois = dto.Mois,
                    Annee = dto.Annee,
                    DeviseId = dto.DeviseId,
                    SouscriptionPrestationId = dto.SouscriptionPrestationId,
                    DateCollecte = DateTime.UtcNow
                };

                await _multidevise.ValidateAndApplySnapshotAsync(tempCollecte, nombreDependants, ct);
                var (montantTarif, deviseTarifId) = await _multidevise.ResolveTarifAttenduAsync(
                    tempCollecte, nombreDependants, ct);

                deviseTarifIdRef ??= deviseTarifId;
                montantTarifTotal += montantTarif;

                var (montantFlexPay, taux) = deviseTarifId == dto.DeviseId
                    ? (montantTarif, 1m)
                    : await _conversion.ConvertirAsync(montantTarif, deviseTarifId, dto.DeviseId, DateTime.UtcNow, ct);

                montantFlexPayTotal += montantFlexPay;
                tauxRef = taux;

                if (Math.Abs(montantTarif - dto.Montant) > _options.MontantTolerance)
                {
                    throw new InvalidOperationException(
                        $"Paiement partiel interdit : montant attendu {montantTarif}, reçu {dto.Montant} pour {dto.TypeCollecte}.");
                }
            }

            if (codeDevisePaiement == "CDF")
                montantFlexPayTotal = Math.Round(montantFlexPayTotal, 0, MidpointRounding.AwayFromZero);

            var telephoneHold = phoneToUse?.Trim();
            await _holdService.EnsureNoActiveHoldAsync(
                null, TypeCollecte.Souscription, 0, 0, null, null, null, telephoneHold, ct);

            var idEnAttente = Guid.NewGuid();
            var reference = $"AD-{idEnAttente:N}"[..20];
            var expireAt = DateTime.UtcNow.AddMinutes(_options.HoldMinutes);

            var payload = new AdhesionFlexPayPayload
            {
                Input = input,
                UtilisateurId = utilisateurId,
                DevisePaiementId = devisePaiementId
            };

            var deviseTarif = await _db.Devises.AsNoTracking()
                .FirstAsync(d => d.IdDevise == deviseTarifIdRef!.Value, ct);

            var enAttente = new CollecteEnAttente
            {
                IdCollecteEnAttente = idEnAttente,
                SourceFlux = CollecteEnAttenteSourceFlux.AdhesionWithAffilie,
                AgentId = adhesionAgentId,
                IdUtilisateur = utilisateurId,
                TypeCollecte = TypeCollecte.Souscription,
                Mois = DateTime.UtcNow.Month,
                Annee = DateTime.UtcNow.Year,
                MethodePaiement = methode,
                MontantTarif = montantTarifTotal,
                DeviseTarifId = deviseTarifIdRef!.Value,
                MontantFlexPay = montantFlexPayTotal,
                CodeDevisePaiement = codeDevisePaiement,
                TauxVersDevisePaiement = tauxRef,
                ReferenceFlexPay = reference,
                Phone = phoneToUse,
                TelephoneAffilie = telephoneHold,
                PayloadMetierJson = JsonSerializer.Serialize(payload),
                DateExpiration = expireAt
            };

            _db.CollectesEnAttente.Add(enAttente);
            await _holdService.CreateHoldAsync(
                idEnAttente, null, TypeCollecte.Souscription, 0, 0,
                null, null, null, telephoneHold, expireAt, ct);

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
                    montantFlexPayTotal, codeDevisePaiement,
                    $"Adhésion Prosoc {input.Nom} {input.Prenom}",
                    callbackUrl, approve, cancel, decline, ct);
            }
            else
            {
                fpResponse = await _flexPayService.InitierPaiementMobileMoneyAsync(
                    marchand.CodeMarchand, marchand.ApiToken, reference,
                    phoneToUse!, montantFlexPayTotal, codeDevisePaiement, callbackUrl, ct);
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
                Amount = montantFlexPayTotal,
                Currency = codeDevisePaiement,
                Phone = phoneToUse,
                Merchant = marchand.CodeMarchand,
                CallbackUrl = callbackUrl,
                PaymentUrl = fpResponse.ResolvePaymentUrl(),
                CodeFlexPay = fpResponse.Code,
                MessageFlexPay = fpResponse.Message,
                IdCollecteEnAttente = idEnAttente,
                SourceFlux = CollecteEnAttenteSourceFlux.AdhesionWithAffilie,
                ReponseBruteFlexPay = JsonSerializer.Serialize(fpResponse)
            });
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Adhésion FlexPay initiée {EnAttenteId} — montant {Montant} {Devise}",
                idEnAttente, montantFlexPayTotal, codeDevisePaiement);

            return new InitiateFlexPayResponseDto
            {
                IdCollecteEnAttente = idEnAttente,
                OrderNumberFlexPay = fpResponse.OrderNumber,
                ReferenceFlexPay = reference,
                MontantTarif = montantTarifTotal,
                CodeDeviseTarif = deviseTarif.Code,
                MontantFlexPay = montantFlexPayTotal,
                CodeDevisePaiement = codeDevisePaiement,
                TauxApplique = tauxRef,
                HoldExpireAt = expireAt,
                PaymentUrl = fpResponse.ResolvePaymentUrl(),
                FlexPayAccepted = true,
                Message = methode == MethodePaiementHelper.CarteBancaire
                    ? "Adhésion en attente — redirigez vers l'URL de paiement carte."
                    : "Adhésion en attente — validez le paiement Mobile Money."
            };
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
