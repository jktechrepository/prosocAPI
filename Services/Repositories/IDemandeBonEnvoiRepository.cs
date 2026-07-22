using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;

namespace ProsocAPI.Services.Repositories
{
    public interface IDemandeBonEnvoiRepository
    {
        Task<List<DemandeBonEnvoi>> GetAllAsync(CancellationToken ct = default);
        Task<DemandeBonEnvoi?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<List<DemandeBonEnvoi>> GetByAffilieAsync(int affilieId, CancellationToken ct = default);
        Task<List<DemandeBonEnvoi>> GetByStatutAsync(string statut, CancellationToken ct = default);
        Task<DemandeBonEnvoi> CreateAsync(DemandeBonEnvoi entity, CancellationToken ct = default);
        Task<DemandeBonEnvoi?> UpdateAsync(int id, DemandeBonEnvoi entity, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    }
}
