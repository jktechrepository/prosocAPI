using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.Pagination;

namespace ProsocAPI.Services.Repositories
{
    public interface IDashboardCaissierRepository
    {
        Task<CaissierKpisDto> GetKpisAsync(int utilisateurId, CancellationToken ct = default);
        Task<List<CaissierCollecteDto>> GetCollectesRecentesAsync(int utilisateurId, int limit = 50, CancellationToken ct = default);
        Task<PaginatedResponse<CaissierCollecteDto>> GetCollectesHistoriqueAsync(
            int utilisateurId,
            GuichetCollecteHistoriqueFiltreDto filtres,
            PaginationRequest pagination,
            CancellationToken ct = default);
        Task<List<CaissierRepartitionDto>> GetRepartitionParTypeAsync(int utilisateurId, CancellationToken ct = default);
        Task<List<CaissierRepartitionDto>> GetRepartitionParModeAsync(int utilisateurId, CancellationToken ct = default);
        Task<List<CaissierAdhesionDuJourDto>> GetAdhesionsDuJourAsync(int utilisateurId, CancellationToken ct = default);
        Task<DashboardCaissierDto> GetDashboardSummaryAsync(int utilisateurId, CancellationToken ct = default);
    }
}
