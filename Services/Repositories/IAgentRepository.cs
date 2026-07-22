using ProsocAPI.Models.Core;

namespace ProsocAPI.Services.Repositories
{
    public interface IAgentRepository
    {
        Task<List<Agent>> GetAllAsync(CancellationToken ct = default);
        Task<Agent?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<Agent> CreateAsync(Agent entity, CancellationToken ct = default);
        Task<Agent?> UpdateAsync(int id, Agent entity, CancellationToken ct = default);
        Task<Agent?> AffecterZoneSocialeAsync(int agentId, int? zoneSocialeId, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
        Task<List<Affilie>> GetAffiliesByAgentAsync(int agentId, CancellationToken ct = default);
    }
}
