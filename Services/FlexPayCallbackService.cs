using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Prosoc.Data;
using ProsocAPI.Models.Configuration;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.FlexPay;

namespace ProsocAPI.Services
{
    public interface IFlexPayCallbackService
    {
        Task<FlexPayCallbackProcessResultDto> ProcessCallbackAsync(
            FlexPayCallbackDto callback,
            string? payloadComplet,
            string? headers,
            string? ipSource,
            CancellationToken ct = default);

        Task<FlexPayCallbackProcessResultDto> VerifyAndFinalizeAsync(
            string orderNumber,
            CancellationToken ct = default);
    }

    public class FlexPayCallbackService : IFlexPayCallbackService
    {
        private readonly ProsocDbContext _db;
        private readonly IFlexPayService _flexPayService;
        private readonly IInfoPaiementMarchandService _marchandService;
        private readonly IFlexPayFinalizationService _finalization;
        private readonly IFlexPaySouscriptionAchatService _souscriptionAchatFlexPay;
        private readonly IPaiementHoldService _holdService;
        private readonly IFlexPayRealtimeNotificationService _realtimeNotification;
        private readonly FlexPayOptions _options;
        private readonly ILogger<FlexPayCallbackService> _logger;

        public FlexPayCallbackService(
            ProsocDbContext db,
            IFlexPayService flexPayService,
            IInfoPaiementMarchandService marchandService,
            IFlexPayFinalizationService finalization,
            IFlexPaySouscriptionAchatService souscriptionAchatFlexPay,
            IPaiementHoldService holdService,
            IFlexPayRealtimeNotificationService realtimeNotification,
            IOptions<FlexPayOptions> options,
            ILogger<FlexPayCallbackService> logger)
        {
            _db = db;
            _flexPayService = flexPayService;
            _marchandService = marchandService;
            _finalization = finalization;
            _souscriptionAchatFlexPay = souscriptionAchatFlexPay;
            _holdService = holdService;
            _realtimeNotification = realtimeNotification;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<FlexPayCallbackProcessResultDto> ProcessCallbackAsync(
            FlexPayCallbackDto callback,
            string? payloadComplet,
            string? headers,
            string? ipSource,
            CancellationToken ct = default)
        {
            var audit = new CallbackFlexPay
            {
                OrderNumber = callback.OrderNumber,
                Code = callback.Code,
                Reference = callback.Reference,
                ProviderReference = callback.ProviderReference,
                Amount = callback.Amount,
                AmountCustomer = callback.AmountCustomer,
                Phone = callback.Phone,
                Currency = callback.Currency,
                Channel = callback.Channel,
                CreatedAt = callback.CreatedAt,
                PayloadComplet = payloadComplet,
                Headers = headers,
                IpSource = ipSource
            };

            CollecteEnAttente? enAttente = null;

            try
            {
                if (string.IsNullOrWhiteSpace(callback.OrderNumber) && string.IsNullOrWhiteSpace(callback.Reference))
                {
                    audit.MessageErreur = "OrderNumber ou Reference requis.";
                    return await SaveAuditAsync(audit, false, audit.MessageErreur, ct);
                }

                var transaction = await FindTransactionAsync(callback, ct);
                audit.IdTransaction = transaction?.IdTransaction;

                enAttente = await FindEnAttenteAsync(callback, transaction, ct);
                if (enAttente == null)
                {
                    audit.MessageErreur = "Collecte en attente introuvable.";
                    return await SaveAuditAsync(audit, false, audit.MessageErreur, ct);
                }

                if (enAttente.IdAdhesionFinalisee.HasValue || enAttente.IdCollecteFinalisee.HasValue)
                {
                    audit.TraiteAvecSucces = true;
                    audit.DetailsTraitement = "Déjà finalisé (idempotence).";
                    await PersistAuditAsync(audit, ct);
                    return await ReturnWithNotifyAsync(enAttente, callback, new FlexPayCallbackProcessResultDto
                    {
                        Success = true,
                        AlreadyProcessed = true,
                        Message = audit.DetailsTraitement,
                        IdCollecte = enAttente.IdCollecteFinalisee,
                        IdAdhesion = enAttente.IdAdhesionFinalisee,
                        IdCollecteEnAttente = enAttente.IdCollecteEnAttente
                    }, ct);
                }

                if (transaction != null)
                {
                    transaction.NombreCallbacks++;
                    transaction.DateCallback = DateTime.UtcNow;
                    transaction.ProviderReference = callback.ProviderReference ?? transaction.ProviderReference;
                    transaction.Channel = callback.Channel ?? transaction.Channel;
                    transaction.CodeFlexPay = callback.Code;
                }

                if (callback.Code != "0")
                {
                    await MarkFailureAsync(enAttente, transaction, ct);
                    audit.TraiteAvecSucces = true;
                    audit.DetailsTraitement = "Paiement refusé par FlexPay.";
                    await PersistAuditAsync(audit, ct);
                    await _db.SaveChangesAsync(ct);
                    return await ReturnWithNotifyAsync(enAttente, callback, new FlexPayCallbackProcessResultDto
                    {
                        Success = true,
                        Pending = false,
                        Message = audit.DetailsTraitement,
                        IdCollecteEnAttente = enAttente.IdCollecteEnAttente
                    }, ct);
                }

                ValidateCallbackAmount(callback, enAttente);

                if (enAttente.SourceFlux == CollecteEnAttenteSourceFlux.AdhesionWithAffilie)
                {
                    var adhesion = await _finalization.FinalizeAdhesionWithAffilieAsync(enAttente, callback, ct);
                    audit.TraiteAvecSucces = true;
                    audit.DetailsTraitement = $"Adhésion {adhesion.Id} créée.";
                    await PersistAuditAsync(audit, ct);
                    return await ReturnWithNotifyAsync(enAttente, callback, new FlexPayCallbackProcessResultDto
                    {
                        Success = true,
                        Message = audit.DetailsTraitement,
                        IdAdhesion = adhesion.Id,
                        IdCollecte = enAttente.IdCollecteFinalisee,
                        IdCollecteEnAttente = enAttente.IdCollecteEnAttente
                    }, ct);
                }

                if (enAttente.SourceFlux == CollecteEnAttenteSourceFlux.SouscriptionAchatPaiementElectronique)
                {
                    var (souscription, collecteSouscription) =
                        await _souscriptionAchatFlexPay.FinalizeAsync(enAttente, callback, ct);
                    audit.TraiteAvecSucces = true;
                    audit.DetailsTraitement =
                        $"Souscription {souscription.IdSouscriptionPrestation} et collecte {collecteSouscription.IdCollecte} créées.";
                    await PersistAuditAsync(audit, ct);
                    return await ReturnWithNotifyAsync(enAttente, callback, new FlexPayCallbackProcessResultDto
                    {
                        Success = true,
                        Message = audit.DetailsTraitement,
                        IdCollecte = collecteSouscription.IdCollecte,
                        IdCollecteEnAttente = enAttente.IdCollecteEnAttente
                    }, ct);
                }

                var collecte = enAttente.SourceFlux switch
                {
                    CollecteEnAttenteSourceFlux.CollecteAgent or CollecteEnAttenteSourceFlux.CollectePaiementElectroniquePublic =>
                        await _finalization.FinalizeCollecteAgentAsync(enAttente, callback, ct),
                    CollecteEnAttenteSourceFlux.PaiementAffilie =>
                        await _finalization.FinalizePaiementAffilieAsync(enAttente, callback, ct),
                    _ => throw new InvalidOperationException("Source flux inconnue.")
                };

                audit.TraiteAvecSucces = true;
                audit.DetailsTraitement = $"Collecte {collecte.IdCollecte} créée.";
                await PersistAuditAsync(audit, ct);
                await _db.SaveChangesAsync(ct);

                return await ReturnWithNotifyAsync(enAttente, callback, new FlexPayCallbackProcessResultDto
                {
                    Success = true,
                    Message = audit.DetailsTraitement,
                    IdCollecte = collecte.IdCollecte,
                    IdCollecteEnAttente = enAttente.IdCollecteEnAttente
                }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur traitement callback FlexPay");
                audit.MessageErreur = ex.Message;
                audit.TraiteAvecSucces = false;
                await PersistAuditAsync(audit, ct);

                var errorResult = new FlexPayCallbackProcessResultDto
                {
                    Success = false,
                    Message = ex.Message,
                    IdCollecteEnAttente = enAttente?.IdCollecteEnAttente
                };

                if (enAttente != null)
                    await ReturnWithNotifyAsync(enAttente, callback, errorResult, ct);

                return errorResult;
            }
        }

        public async Task<FlexPayCallbackProcessResultDto> VerifyAndFinalizeAsync(
            string orderNumber,
            CancellationToken ct = default)
        {
            var transaction = await _db.TransactionsFlexPay
                .FirstOrDefaultAsync(t => t.OrderNumber == orderNumber, ct)
                ?? throw new InvalidOperationException($"Transaction FlexPay {orderNumber} introuvable.");

            var marchand = await _marchandService.GetActifAsync(ct)
                ?? throw new InvalidOperationException("Configuration marchand introuvable.");

            var check = await _flexPayService.VerifierStatutTransactionAsync(marchand.ApiToken, orderNumber, ct);
            transaction.NombreVerifications++;
            transaction.DateDerniereVerification = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);

            var status = check.Transaction?.Status ?? check.Code;
            _logger.LogInformation(
                "FlexPay check {OrderNumber}: code={Code}, transaction.status={TxStatus}, resolved={Resolved}, message={Message}",
                orderNumber, check.Code, check.Transaction?.Status, status, check.Message);

            var enAttente = await _db.CollectesEnAttente
                .FirstOrDefaultAsync(e => e.IdCollecteEnAttente == transaction.IdCollecteEnAttente, ct);

            if (enAttente != null
                && (enAttente.IdAdhesionFinalisee.HasValue || enAttente.IdCollecteFinalisee.HasValue))
            {
                return new FlexPayCallbackProcessResultDto
                {
                    Success = true,
                    AlreadyProcessed = true,
                    Pending = false,
                    Message = "Déjà finalisé (idempotence).",
                    IdCollecte = enAttente.IdCollecteFinalisee,
                    IdAdhesion = enAttente.IdAdhesionFinalisee,
                    IdCollecteEnAttente = enAttente.IdCollecteEnAttente
                };
            }

            // Succès confirmé chez FlexPay → même pipeline que le callback.
            if (string.Equals(status, "0", StringComparison.Ordinal))
            {
                var callback = new FlexPayCallbackDto
                {
                    Code = "0",
                    OrderNumber = orderNumber,
                    Reference = transaction.Reference,
                    ProviderReference = transaction.ProviderReference,
                    Amount = transaction.Amount.ToString(CultureInfo.InvariantCulture),
                    Currency = transaction.Currency
                };

                var paid = await ProcessCallbackAsync(callback, JsonSerializer.Serialize(check), null, null, ct);
                paid.Pending = false;
                return paid;
            }

            // Statut non final (pending / en cours) : ne PAS marquer Echec (évite faux « Paiement refusé » au poll).
            return new FlexPayCallbackProcessResultDto
            {
                Success = false,
                Pending = true,
                Message = "Paiement en cours de confirmation auprès de FlexPay.",
                IdCollecteEnAttente = transaction.IdCollecteEnAttente
            };
        }

        private async Task<FlexPayCallbackProcessResultDto> ReturnWithNotifyAsync(
            CollecteEnAttente enAttente,
            FlexPayCallbackDto callback,
            FlexPayCallbackProcessResultDto result,
            CancellationToken ct)
        {
            try
            {
                await _realtimeNotification.NotifyPaymentUpdatedAsync(enAttente, result, callback, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "SignalR FlexPayPaymentUpdated non envoyé pour {EnAttenteId}",
                    enAttente.IdCollecteEnAttente);
            }

            return result;
        }

        private void ValidateCallbackAmount(FlexPayCallbackDto callback, CollecteEnAttente enAttente)
        {
            if (!decimal.TryParse(callback.Amount, NumberStyles.Any, CultureInfo.InvariantCulture, out var montantCallback)
                && !decimal.TryParse(callback.Amount, out montantCallback))
            {
                return;
            }

            if (Math.Abs(montantCallback - enAttente.MontantFlexPay) > _options.MontantTolerance)
            {
                throw new InvalidOperationException(
                    $"Montant callback ({montantCallback}) différent du montant attendu ({enAttente.MontantFlexPay}).");
            }
        }

        private async Task MarkFailureAsync(
            CollecteEnAttente enAttente,
            TransactionFlexPay? transaction,
            CancellationToken ct)
        {
            enAttente.StatutEnAttente = CollecteEnAttenteStatut.Echec;
            enAttente.DateModification = DateTime.UtcNow;
            if (transaction != null)
                transaction.MessageErreur = "Paiement refusé";
            await _holdService.ReleaseHoldAsync(enAttente.IdCollecteEnAttente, ct);
            await _db.SaveChangesAsync(ct);
        }

        private async Task<TransactionFlexPay?> FindTransactionAsync(FlexPayCallbackDto callback, CancellationToken ct)
        {
            if (!string.IsNullOrWhiteSpace(callback.OrderNumber))
            {
                var t = await _db.TransactionsFlexPay
                    .FirstOrDefaultAsync(x => x.OrderNumber == callback.OrderNumber, ct);
                if (t != null) return t;
            }

            if (!string.IsNullOrWhiteSpace(callback.Reference))
            {
                return await _db.TransactionsFlexPay
                    .FirstOrDefaultAsync(x => x.Reference == callback.Reference, ct);
            }

            return null;
        }

        private async Task<CollecteEnAttente?> FindEnAttenteAsync(
            FlexPayCallbackDto callback,
            TransactionFlexPay? transaction,
            CancellationToken ct)
        {
            if (transaction?.IdCollecteEnAttente != null)
            {
                return await _db.CollectesEnAttente
                    .FirstOrDefaultAsync(c => c.IdCollecteEnAttente == transaction.IdCollecteEnAttente, ct);
            }

            if (!string.IsNullOrWhiteSpace(callback.Reference))
            {
                return await _db.CollectesEnAttente
                    .FirstOrDefaultAsync(c => c.ReferenceFlexPay == callback.Reference, ct);
            }

            if (!string.IsNullOrWhiteSpace(callback.OrderNumber))
            {
                return await _db.CollectesEnAttente
                    .FirstOrDefaultAsync(c => c.OrderNumberFlexPay == callback.OrderNumber, ct);
            }

            return null;
        }

        private async Task PersistAuditAsync(CallbackFlexPay audit, CancellationToken ct)
        {
            _db.CallbacksFlexPay.Add(audit);
            await _db.SaveChangesAsync(ct);
        }

        private async Task<FlexPayCallbackProcessResultDto> SaveAuditAsync(
            CallbackFlexPay audit, bool success, string message, CancellationToken ct)
        {
            audit.TraiteAvecSucces = success;
            audit.MessageErreur = success ? null : message;
            await PersistAuditAsync(audit, ct);
            return new FlexPayCallbackProcessResultDto { Success = success, Message = message };
        }
    }
}
