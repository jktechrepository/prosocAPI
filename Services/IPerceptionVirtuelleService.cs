using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.Pagination;

namespace ProsocAPI.Services
{
    public interface IPerceptionVirtuelleService
    {
        Task<PaginatedResponse<CollecteVirtuelleEnAttenteDto>> GetCollectesEnAttenteAsync(
            int? agentId,
            DateTime? dateDebut,
            DateTime? dateFin,
            PaginationRequest pagination,
            CancellationToken ct = default);

        Task<List<PerceptionVirtuelleSyntheseAgentDto>> GetSyntheseAgentsAsync(CancellationToken ct = default);

        Task<PerceptionVirtuelleReadDto?> GetByIdAsync(int id, CancellationToken ct = default);

        Task<PaginatedResponse<PerceptionVirtuelleReadDto>> GetHistoriqueAsync(
            int percepteurUtilisateurId,
            PaginationRequest pagination,
            CancellationToken ct = default);

        Task<PaginatedResponse<PerceptionVirtuelleReadDto>> GetHistoriqueGlobalAsync(
            PerceptionVirtuelleHistoriqueFiltreDto filtres,
            PaginationRequest pagination,
            CancellationToken ct = default);

        Task<PerceptionReconciliationDto> GetReconciliationAsync(
            int? agentId,
            DateTime? dateDebut,
            DateTime? dateFin,
            CancellationToken ct = default);

        Task<PerceptionVirtuelleConfirmerResultDto> ConfirmerPerceptionAsync(
            int percepteurUtilisateurId,
            PerceptionVirtuelleConfirmerDto dto,
            CancellationToken ct = default);

        Task<(decimal Montant, int Nombre)> GetTotauxVirtuelsEnAttenteAsync(CancellationToken ct = default);
    }
}
