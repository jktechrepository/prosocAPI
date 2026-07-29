using ProsocAPI.Models.Configuration;
using ProsocAPI.Models.DTOs.Core;

namespace ProsocAPI.Services
{
    public interface IParametresMetierProvider
    {
        Task<RetraitAgentOptions> GetRetraitAgentAsync(CancellationToken ct = default);
        Task<RetraitAgentParametresReadDto> GetRetraitAgentReadAsync(CancellationToken ct = default);
        Task<RetraitAgentParametresReadDto> UpdateRetraitAgentAsync(
            RetraitAgentParametresUpdateDto dto,
            int utilisateurId,
            CancellationToken ct = default);

        Task<AgentMaashOptions> GetAgentMaashAsync(CancellationToken ct = default);
        Task<AgentMaashParametresReadDto> GetAgentMaashReadAsync(CancellationToken ct = default);
        Task<AgentMaashParametresReadDto> UpdateAgentMaashAsync(
            AgentMaashParametresUpdateDto dto,
            int utilisateurId,
            CancellationToken ct = default);

        Task<ArrieresOptions> GetArrieresAsync(CancellationToken ct = default);
        Task<ArrieresParametresReadDto> GetArrieresReadAsync(CancellationToken ct = default);
        Task<ArrieresParametresReadDto> UpdateArrieresAsync(
            ArrieresParametresUpdateDto dto,
            int utilisateurId,
            CancellationToken ct = default);

        Task<PenaliteOptions> GetPenaliteAsync(CancellationToken ct = default);
        Task<PenaliteParametresReadDto> GetPenaliteReadAsync(CancellationToken ct = default);
        Task<PenaliteParametresReadDto> UpdatePenaliteAsync(
            PenaliteParametresUpdateDto dto,
            int utilisateurId,
            CancellationToken ct = default);

        Task<WalletVirtuelOptions> GetWalletVirtuelAsync(CancellationToken ct = default);
        Task<WalletVirtuelParametresReadDto> GetWalletVirtuelReadAsync(CancellationToken ct = default);
        Task<WalletVirtuelParametresReadDto> UpdateWalletVirtuelAsync(
            WalletVirtuelParametresUpdateDto dto,
            int utilisateurId,
            CancellationToken ct = default);

        void InvalidateCache(string code);
    }
}
