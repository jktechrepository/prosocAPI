using ProsocAPI.Models.Core;

namespace ProsocAPI.Services.Repositories
{
    public interface IBonEnvoiRepository
    {
        Task<List<BonEnvoi>> GetAllAsync(CancellationToken ct = default);
        Task<BonEnvoi?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<BonEnvoi?> GetByNumeroBonAsync(string numeroBon, CancellationToken ct = default);
        Task<List<BonEnvoi>> GetByAffilieAsync(int affilieId, CancellationToken ct = default);
        Task<List<BonEnvoi>> GetByPrestationAsync(int prestationId, CancellationToken ct = default);
        Task<List<BonEnvoi>> GetNonUtilisesAsync(CancellationToken ct = default);
        Task<List<BonEnvoi>> GetUtilisesAsync(CancellationToken ct = default);
        Task<BonEnvoi> CreateAsync(BonEnvoi entity, CancellationToken ct = default);
        Task<BonEnvoi?> UpdateAsync(int id, BonEnvoi entity, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
        Task<bool> MarquerCommeUtiliseAsync(int id, CancellationToken ct = default);
    }
}
