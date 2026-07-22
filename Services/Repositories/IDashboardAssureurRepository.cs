using ProsocAPI.Models.DTOs.DashboardAssureur;

namespace ProsocAPI.Services.Repositories
{
    public interface IDashboardAssureurRepository
    {
        Task<AssureurKpisDto> GetKpisAsync(int assureurId, CancellationToken ct = default);
        Task<List<AssureurAffilieDto>> GetAffiliesAsync(int assureurId, int limit = 50, CancellationToken ct = default);
        Task<List<AssureurDependantDto>> GetDependantsAsync(int assureurId, int limit = 100, CancellationToken ct = default);
        Task<List<AssureurAntecedentDto>> GetAntecedentsAsync(int assureurId, int limit = 100, CancellationToken ct = default);
        Task<List<AssureurRepartitionProduitDto>> GetRepartitionProduitsAsync(int assureurId, CancellationToken ct = default);
        Task<DashboardAssureurDto> GetDashboardSummaryAsync(int assureurId, CancellationToken ct = default);
    }
}
