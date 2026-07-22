using ProsocAPI.Models.Core;

namespace ProsocAPI.Services.Repositories
{
    public interface IRetraitAgentRepository
    {
        Task<List<RetraitAgent>> GetAllAsync(CancellationToken ct = default);
        Task<RetraitAgent?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<List<RetraitAgent>> GetByAgentAsync(int agentId, CancellationToken ct = default);
        Task<List<RetraitAgent>> GetNonValidesAsync(CancellationToken ct = default);
        Task<List<RetraitAgent>> GetValidesAsync(CancellationToken ct = default);
        Task<RetraitAgent> CreateAsync(RetraitAgent entity, CancellationToken ct = default);
        Task<RetraitAgent?> UpdateAsync(int id, RetraitAgent entity, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
        Task<bool> ValiderRetraitAsync(int id, CancellationToken ct = default);
    }
}
