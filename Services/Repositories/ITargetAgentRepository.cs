using ProsocAPI.Models.Core;

namespace ProsocAPI.Services.Repositories
{
    public interface ITargetAgentRepository
    {
        Task<List<TargetAgent>> GetAllAsync(CancellationToken ct = default);
        Task<TargetAgent?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<List<TargetAgent>> GetByRoleAsync(int roleId, CancellationToken ct = default);
        Task<List<TargetAgent>> GetActifsAsync(CancellationToken ct = default);
        Task<bool> HasActiveConflictAsync(int roleId, PeriodiciteTarget periodicite, int? excludeId = null, CancellationToken ct = default);
        Task<TargetAgent> CreateAsync(TargetAgent entity, CancellationToken ct = default);
        Task<TargetAgent?> UpdateAsync(int id, TargetAgent entity, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    }
}
