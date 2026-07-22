using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;

namespace ProsocAPI.Services.Repositories
{
    public interface ICollecteRepository
    {
        Task<List<Collecte>> GetAllAsync(CancellationToken ct = default);
        Task<Collecte?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<List<Collecte>> GetByAffilieAsync(int affilieId, CancellationToken ct = default);
        Task<List<Collecte>> GetByAgentAsync(int agentId, CancellationToken ct = default);
        Task<List<Collecte>> GetByDeviseAsync(int deviseId, CancellationToken ct = default);
        Task<List<Collecte>> GetByDateRangeAsync(DateTime debut, DateTime fin, CancellationToken ct = default);
        Task<Collecte> CreateAsync(Collecte entity, CancellationToken ct = default);
        Task<Collecte?> UpdateAsync(int id, Collecte entity, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
        Task<CollecteStatsDto> GetStatsAsync(CancellationToken ct = default);
    }
}
