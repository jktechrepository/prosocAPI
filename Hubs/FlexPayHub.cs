using Microsoft.AspNetCore.SignalR;

namespace ProsocAPI.Hubs
{
    /// <summary>
    /// Hub SignalR dédié aux paiements FlexPay (connexion sans JWT pour flux publics).
    /// </summary>
    public class FlexPayHub : Hub
    {
        public const string PaymentUpdatedEvent = "FlexPayPaymentUpdated";

        public static string GroupName(Guid idCollecteEnAttente) =>
            $"flexpay_{idCollecteEnAttente}";

        /// <summary>
        /// Rejoindre le groupe temps réel d'une initiation FlexPay (id retourné dans InitiateFlexPayResponseDto).
        /// </summary>
        public async Task JoinFlexPayPayment(Guid idCollecteEnAttente)
        {
            if (idCollecteEnAttente == Guid.Empty)
                throw new HubException("IdCollecteEnAttente invalide.");

            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(idCollecteEnAttente));
        }

        public async Task LeaveFlexPayPayment(Guid idCollecteEnAttente)
        {
            if (idCollecteEnAttente == Guid.Empty)
                return;

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(idCollecteEnAttente));
        }
    }
}
