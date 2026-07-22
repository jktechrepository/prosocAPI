using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ProsocAPI.Hubs;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.FlexPay;
using ProsocAPI.Services;
using Xunit;

namespace Prosoc.Tests.Unit;

public class FlexPayRealtimeNotificationServiceTests
{
    [Fact]
    public async Task NotifyPaymentUpdatedAsync_SendsFlexPayPaymentUpdatedToFlexPayGroup()
    {
        var enAttenteId = Guid.Parse("11111111-2222-3333-4444-555555555555");
        var flexPayClient = new Mock<IClientProxy>();
        flexPayClient
            .Setup(c => c.SendCoreAsync(
                FlexPayHub.PaymentUpdatedEvent,
                It.IsAny<object[]>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var flexPayClients = new Mock<IHubClients>();
        flexPayClients
            .Setup(c => c.Group($"flexpay_{enAttenteId}"))
            .Returns(flexPayClient.Object);

        var flexPayHub = new Mock<IHubContext<FlexPayHub>>();
        flexPayHub.Setup(h => h.Clients).Returns(flexPayClients.Object);

        var notificationHub = new Mock<IHubContext<NotificationHub>>();

        var service = new FlexPayRealtimeNotificationService(
            flexPayHub.Object,
            notificationHub.Object,
            NullLogger<FlexPayRealtimeNotificationService>.Instance);

        var enAttente = new CollecteEnAttente
        {
            IdCollecteEnAttente = enAttenteId,
            SourceFlux = CollecteEnAttenteSourceFlux.AdhesionWithAffilie,
            ReferenceFlexPay = "AD-REF",
            OrderNumberFlexPay = "ORD-1"
        };

        var result = new FlexPayCallbackProcessResultDto
        {
            Success = true,
            Message = "Adhésion créée",
            IdAdhesion = 42,
            IdCollecteEnAttente = enAttenteId
        };

        var callback = new FlexPayCallbackDto { Code = "0", OrderNumber = "ORD-1", Reference = "AD-REF" };

        await service.NotifyPaymentUpdatedAsync(enAttente, result, callback);

        flexPayClient.Verify(
            c => c.SendCoreAsync(
                FlexPayHub.PaymentUpdatedEvent,
                It.Is<object[]>(args => args.Length == 1 && args[0] is FlexPayPaymentUpdatedDto),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public void BuildPayload_WhenPaymentRefused_SetsFailedAndNotSuccess()
    {
        var enAttente = new CollecteEnAttente
        {
            IdCollecteEnAttente = Guid.NewGuid(),
            SourceFlux = CollecteEnAttenteSourceFlux.CollecteAgent
        };

        var payload = FlexPayRealtimeNotificationService.BuildPayload(
            enAttente,
            new FlexPayCallbackProcessResultDto { Success = true, Message = "Paiement refusé" },
            new FlexPayCallbackDto { Code = "1" });

        Assert.True(payload.Failed);
        Assert.False(payload.Success);
    }
}
