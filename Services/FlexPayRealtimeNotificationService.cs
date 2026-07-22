using Microsoft.AspNetCore.SignalR;
using ProsocAPI.Hubs;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.FlexPay;

namespace ProsocAPI.Services
{
    public interface IFlexPayRealtimeNotificationService
    {
        Task NotifyPaymentUpdatedAsync(
            CollecteEnAttente enAttente,
            FlexPayCallbackProcessResultDto result,
            FlexPayCallbackDto callback,
            CancellationToken ct = default);
    }

    public class FlexPayRealtimeNotificationService : IFlexPayRealtimeNotificationService
    {
        private readonly IHubContext<FlexPayHub> _flexPayHub;
        private readonly IHubContext<NotificationHub> _notificationHub;
        private readonly ILogger<FlexPayRealtimeNotificationService> _logger;

        public FlexPayRealtimeNotificationService(
            IHubContext<FlexPayHub> flexPayHub,
            IHubContext<NotificationHub> notificationHub,
            ILogger<FlexPayRealtimeNotificationService> logger)
        {
            _flexPayHub = flexPayHub;
            _notificationHub = notificationHub;
            _logger = logger;
        }

        public async Task NotifyPaymentUpdatedAsync(
            CollecteEnAttente enAttente,
            FlexPayCallbackProcessResultDto result,
            FlexPayCallbackDto callback,
            CancellationToken ct = default)
        {
            var payload = BuildPayload(enAttente, result, callback);
            var group = FlexPayHub.GroupName(enAttente.IdCollecteEnAttente);

            await _flexPayHub.Clients.Group(group)
                .SendAsync(FlexPayHub.PaymentUpdatedEvent, payload, ct);

            if (enAttente.IdUtilisateur.HasValue)
            {
                await _notificationHub.Clients.Group($"user_{enAttente.IdUtilisateur.Value}")
                    .SendAsync(FlexPayHub.PaymentUpdatedEvent, payload, ct);
            }

            _logger.LogInformation(
                "SignalR FlexPayPaymentUpdated — groupe {Group}, success={Success}, source={Source}",
                group, payload.Success, payload.SourceFlux);
        }

        public static FlexPayPaymentUpdatedDto BuildPayload(
            CollecteEnAttente enAttente,
            FlexPayCallbackProcessResultDto result,
            FlexPayCallbackDto callback)
        {
            var failed = callback.Code != null && callback.Code != "0";
            var paymentSucceeded = result.AlreadyProcessed
                || (!failed && result.Success && (result.IdAdhesion.HasValue || result.IdCollecte.HasValue));

            return new FlexPayPaymentUpdatedDto
            {
                IdCollecteEnAttente = enAttente.IdCollecteEnAttente,
                OrderNumberFlexPay = callback.OrderNumber ?? enAttente.OrderNumberFlexPay,
                ReferenceFlexPay = callback.Reference ?? enAttente.ReferenceFlexPay,
                Success = paymentSucceeded,
                AlreadyProcessed = result.AlreadyProcessed,
                Failed = failed && !result.AlreadyProcessed,
                CodeFlexPay = callback.Code,
                Message = result.Message,
                SourceFlux = enAttente.SourceFlux.ToString(),
                IdAdhesion = result.IdAdhesion ?? enAttente.IdAdhesionFinalisee,
                IdCollecte = result.IdCollecte ?? enAttente.IdCollecteFinalisee,
                TimestampUtc = DateTime.UtcNow
            };
        }
    }
}
