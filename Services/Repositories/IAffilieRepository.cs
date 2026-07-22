using ProsocAPI.Models.Core;

namespace ProsocAPI.Services.Repositories
{
    public interface IAffilieRepository
    {
        Task<List<Affilie>> GetAllAsync(CancellationToken ct = default);
        Task<Affilie?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<Affilie> CreateAsync(Affilie entity, CancellationToken ct = default);
        Task<Affilie?> UpdateAsync(int id, Affilie entity, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    }
}
