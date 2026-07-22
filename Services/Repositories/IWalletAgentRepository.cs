using ProsocAPI.Models.Core;

namespace ProsocAPI.Services.Repositories
{
    public interface IWalletAgentRepository
    {
        Task<List<WalletAgent>> GetAllAsync(CancellationToken ct = default);
        Task<WalletAgent?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<WalletAgent?> GetByAgentIdAsync(int agentId, CancellationToken ct = default);
        Task<WalletAgent?> GetPrincipalWalletByAgentIdAsync(int agentId, CancellationToken ct = default);
        Task<WalletAgent?> GetByAgentAndDeviseAsync(int agentId, int deviseId, CancellationToken ct = default);
        Task<List<WalletAgent>> GetByAgentIdAllAsync(int agentId, CancellationToken ct = default);
        Task<WalletAgent> GetOrCreateForAgentAndDeviseAsync(int agentId, int deviseId, CancellationToken ct = default);
        Task<WalletAgent> CreateAsync(WalletAgent entity, CancellationToken ct = default);
        Task<WalletAgent?> UpdateAsync(int id, WalletAgent entity, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    }
}
